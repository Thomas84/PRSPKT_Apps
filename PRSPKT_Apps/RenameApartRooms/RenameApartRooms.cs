using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RenameApartRooms
{
    [Transaction(TransactionMode.Manual)]
    public class RenameApartRooms : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Room> roomList = new FilteredElementCollector(doc, uidoc.ActiveView.Id)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Where(q => q.LookupParameter("Тип помещения").AsInteger() != 5)
                .Cast<Room>()
                .ToList();

            try
            {
                using (Transaction t = new Transaction(doc, "Переименовать помещения квартиры"))
                {
                    t.Start();
                    foreach (var room in roomList)
                    {
                        string _roomName = room.LookupParameter("Имя").AsString();
                        Parameter _roomNumber = room.LookupParameter("Номер");
                        switch (_roomName)
                        {
                            case "Кухня":
                                _roomNumber.Set("К-я");
                                break;
                            case "Гардероб":
                            case "Гардеробная":
                                _roomNumber.Set("Гр");
                                break;
                            case "Гостиная":
                                _roomNumber.Set("Г-я");
                                break;
                            case "Спальня":
                            case "Спальная":
                                _roomNumber.Set("Сп");
                                break;
                            case "Прихожая":
                                _roomNumber.Set("П-я");
                                break;
                            case "Ванная":
                                _roomNumber.Set("В-я");
                                break;
                            case "Кладовая":
                                _roomNumber.Set("Кл");
                                break;
                            case "Коридор":
                                _roomNumber.Set("К-р");
                                break;
                            case "Санузел":
                                _roomNumber.Set("С/у");
                                break;
                            case "Балкон":
                                _roomNumber.Set("Б-н");
                                break;
                            case "Лоджия":
                                _roomNumber.Set("Л-я");
                                break;
                            default:
                                _roomNumber.Set("ХХХ");
                                break;
                        }
                    }
                    t.Commit();

                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception e)
            {

                TaskDialog.Show("Error", e.Message);
                return Result.Failed;
            }
        }
    }
}
