using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using PRSPKT_Apps.RoomFinishes;
using System;
using System.Collections.Generic;

namespace RoomFinishes
{
    [Transaction(TransactionMode.Manual)]
    public class RoomsFinishes : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Subscribe to the FailureProcessing Event
            uiApp.Application.FailuresProcessing += new EventHandler<Autodesk.Revit.DB.Events.FailuresProcessingEventArgs>(FailuresProcessing);

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    RoomFinish(uiDoc, t);
                    uiApp.Application.FailuresProcessing -= FailuresProcessing;
                    return Result.Succeeded;

                }
                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted()) t.RollBack();
                    uiApp.Application.FailuresProcessing -= FailuresProcessing;
                    return Result.Cancelled;
                }
                catch (ErrorMessageException errorEx)
                {
                    message = errorEx.Message;
                    if (t.HasStarted()) t.RollBack();
                    uiApp.Application.FailuresProcessing -= FailuresProcessing;
                    return Result.Failed;
                }
                catch (Exception)
                {
                    message = Tools.LangResMan.GetString("roomFinishes_unexpectedError", Tools.Cult);
                    if (t.HasStarted()) t.RollBack();
                    uiApp.Application.FailuresProcessing -= FailuresProcessing;
                    return Result.Failed;
                }
            }
        }

        private void RoomFinish(UIDocument uiDoc, Transaction t)
        {
            Document doc = uiDoc.Document;
            t.Start(Tools.LangResMan.GetString("roomFinishes_transactionName", Tools.Cult));

            // Load the selection form
            RoomsFinishesControl userControl = new RoomsFinishesControl(uiDoc);
            userControl.InitializeComponent();

            if (userControl.ShowDialog() == true)
            {
                // Select wall types
                WallType finishLayer = userControl.SelectedWallType;
                WallType newWallType = userControl.DuplicatedWallType;

                // Get all finish properties
                double height = userControl.FinishHeight;

                IEnumerable<Room> modelRooms = userControl.SelectedRooms;

                Dictionary<ElementId, ElementId> finishesDictionary = new Dictionary<ElementId, ElementId>();
                List<KeyValuePair<Wall, Wall>> addedWalls = new List<KeyValuePair<Wall, Wall>>();

                // Loop on all rooms to get boundaries
                foreach (Room currentRoom in modelRooms)
                {
                    ElementId roomLevelId = currentRoom.LevelId;

                    SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
                    opt.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;
                    IList<IList<BoundarySegment>> boundarySegmentArray = currentRoom.GetBoundarySegments(opt);
                    if (null == boundarySegmentArray)
                    {
                        continue;
                    }

                    foreach (IList<BoundarySegment> boundarySegmentArr in boundarySegmentArray)
                    {
                        if (0 == boundarySegmentArr.Count)
                        {
                            continue;
                        }
                        else
                        {
                            foreach (BoundarySegment bSeg in boundarySegmentArr)
                            {
                                // Check if the boundary is a room separation lines
                                Element boundaryElement = doc.GetElement(bSeg.ElementId);

                                if (boundaryElement == null) { continue; }

                                Categories categories = doc.Settings.Categories;
                                Category roomSeparationLineCat = categories.get_Item(BuiltInCategory.OST_RoomSeparationLines);

                                if (boundaryElement.Category.Id != roomSeparationLineCat.Id)
                                {
                                    Wall currentWall = Wall.Create(doc, bSeg.GetCurve(), newWallType.Id, roomLevelId, height, 0, false, false);
                                    Parameter wallJustification = currentWall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM);
                                    wallJustification.Set(2);

                                    finishesDictionary.Add(currentWall.Id, bSeg.ElementId);
                                }
                            }
                        }
                    }
                }
                FailureHandlingOptions options = t.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new PrintPreprocessor());
                // New, showing of any eventual mini-warning will be postponed until
                // the following transaction
                t.Commit(options);

                t.Start(Tools.LangResMan.GetString("roomFinishes_transactionName", Tools.Cult));

                List<ElementId> addedIds = new List<ElementId>(finishesDictionary.Keys);
                foreach (ElementId addedFinishesId in addedIds)
                {
                    if (doc.GetElement(addedFinishesId) == null)

                    {
                        finishesDictionary.Remove(addedFinishesId);
                    }
                }

                Wall.ChangeTypeId(doc, finishesDictionary.Keys, finishLayer.Id);

                // Joint both wall
                if (userControl.JoinWall)
                {
                    foreach (ElementId finishId in finishesDictionary.Keys)
                    {
                        Wall finishWall = doc.GetElement(finishId) as Wall;
                        if (finishWall != null)
                        {
                            Parameter wallJustification = finishWall.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM);
                            wallJustification.Set(3);
                            Wall baseWall = doc.GetElement(finishesDictionary[finishId]) as Wall;

                            if (baseWall != null)
                            {
                                JoinGeometryUtils.JoinGeometry(doc, finishWall, baseWall);
                            }
                        }
                    }
                }

                doc.Delete(newWallType.Id);
                t.Commit();
            }
            else
            {
                t.RollBack();
            }

        }

        /// <summary>
        /// Implements the FailureProcessing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            String transactionName = failuresAccessor.GetTransactionName();
            IList<FailureMessageAccessor> failures = failuresAccessor.GetFailureMessages();

            if (failures.Count != 0)
            {
                foreach (FailureMessageAccessor f in failures)
                {
                    FailureDefinitionId id = f.GetFailureDefinitionId();

                    if (id == BuiltInFailures.JoinElementsFailures.CannotJoinElementsError)
                    {
                        failuresAccessor.ResolveFailure(f);
                        e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
                    }

                    return;
                }
            }
        }
    }
}
