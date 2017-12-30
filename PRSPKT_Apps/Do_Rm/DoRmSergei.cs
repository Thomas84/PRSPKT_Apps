// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Do_Rm
{
    [Transaction(TransactionMode.Manual)]
    class DoRmSergei : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    DoRm(UIdoc, t);
                    return Result.Succeeded;
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
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
            }
        }


        private void DoRm(UIDocument UIdoc, Transaction t)
        {
            Document _doc = UIdoc.Document;
            int wall_count = 0;
            int room_count = 0;
            t.Start("Простановка Rm_Этаж для Сергея ОВ");
            //IList<Level> _levels = new FilteredElementCollector(_doc)
            //    .OfCategory(BuiltInCategory.OST_Levels).Cast<Level>().ToList();

            IList<Room> _rooms = new FilteredElementCollector(_doc)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(x => x.Area > 0)
                .ToList();

            var walls = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .ToList();


            try
            {
                foreach (Room tempRoom in _rooms)
                {
                    String _lvl = tempRoom.Level.Name;
                    Parameter _rm = tempRoom.LookupParameter("Rm_Этаж");
                    Parameter _gp = tempRoom.LookupParameter("GP_Этаж");
                    room_count += 1;
                    _rm.Set(_lvl);
                    _gp.Set(_lvl);
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }

            try
            {
                foreach (Wall wall in walls)
                {
                    var _lvl = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsValueString();
                    wall_count += 1;
                    wall.LookupParameter("GP_Этаж").Set(_lvl);
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }

            t.Commit();
            TaskDialog.Show("Результат работы Сергей_ОВ",
                "В стены вписан параметр GP_Этаж в количестве: " + wall_count.ToString() + " шт." + "\n" +
                "В помещения вписан параметр Rm_Этаж и GP_Этаж: " + room_count.ToString() + " шт."
                );
        }
    }
}
