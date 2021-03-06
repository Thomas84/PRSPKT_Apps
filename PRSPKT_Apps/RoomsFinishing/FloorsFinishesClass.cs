﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoomsFinishes
{
    [Transaction(TransactionMode.Manual)]
    public class FloorsFinishesClass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    FloorFinish(UIdoc, t);
                    return Result.Succeeded;
                }
                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Cancelled;
                }
                catch (ErrorMessageException errorException)
                {
                    message = errorException.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
                catch (Exception ex)
                {
                    message = Tools.GetResourceManager("floorFinishes_unexpectedError") + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
            }
        }

        private void FloorFinish(UIDocument UIdoc, Transaction t)
        {
            Document _doc = UIdoc.Document;

            t.Start(Tools.GetResourceManager("floorFinishes_transactionName"));

            // Load the selection form

            PRSPKT_Apps.RoomsFinishes.FloorsFinishesControl userControl = new PRSPKT_Apps.RoomsFinishes.FloorsFinishesControl(UIdoc);
            userControl.InitializeComponent();

            if (userControl.ShowDialog() == true)
            {

                // Select floor types
                FloorType flType = userControl.SelectedFloorType;

                // Select Rooms in model
                IEnumerable<Room> ModelRooms = userControl.SelectedRooms;

                foreach (Room tempRoom in ModelRooms)
                {
                    if (tempRoom != null)
                    {
                        if (Tools.IsZero(tempRoom.UnboundedHeight))
                        {
                            double height;
                            if (userControl.RoomParameter == null)
                            {
                                height = userControl.FloorHeight;
                            }
                            else
                            {
                                Parameter tempRoomParam = tempRoom.get_Parameter(userControl.RoomParameter.Definition);
                                height = tempRoomParam.AsDouble();
                            }

                            string name = tempRoom.Name;

                            SpatialElementBoundaryOptions opt = new SpatialElementBoundaryOptions();
                            IList<IList<BoundarySegment>> boundarySegments = tempRoom.GetBoundarySegments(opt);

                            CurveArray curveArray = new CurveArray();
                            if (boundarySegments.Count != 0)
                            {
                                foreach (BoundarySegment boundSegment in boundarySegments.First())
                                {
                                    curveArray.Append(boundSegment.GetCurve());
                                }

                                // Retrieve room info
                                Level roomLevel = _doc.GetElement(tempRoom.LevelId) as Level;
                                Parameter param = tempRoom.get_Parameter(BuiltInParameter.ROOM_HEIGHT);
                                double rmHeight = param.AsDouble();

                                if (curveArray.Size != 0)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    Floor floor = _doc.Create.NewFloor(curveArray, flType, roomLevel, false);

                                    // Change some param on the floor
                                    param = floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM);
                                    param.Set(height);
                                }
                            }
                        }
                    }
                }
                t.Commit();
            }
            else
            {
                t.RollBack();
            }
        }



    }

}
