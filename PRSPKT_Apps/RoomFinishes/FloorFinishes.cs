using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRSPKT_Apps;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace PRSPKT_Apps.RoomFinishes
{
    [Transaction(TransactionMode.Manual)]
    public class FloorFinishes : IExternalCommand
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
            }
        }

        private void FloorFinish(UIDocument UIdoc, Transaction t)
        {
            Document _doc = UIdoc.Document;
            t.Start(Tools.LangResMan.GetString("floorFinishes_transactionName", Tools.Cult));

            // Load the selection form

            FloorsFinishesControl userControl = new FloorsFinishesControl(UIdoc);
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
                        if (tempRoom.UnboundedHeight != 0)
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
                            IList<IList<BoundarySegment>> boundarySegment = tempRoom.GetBoundarySegments(opt);
                        }
                    }
                }
            }
        }
    }
}
