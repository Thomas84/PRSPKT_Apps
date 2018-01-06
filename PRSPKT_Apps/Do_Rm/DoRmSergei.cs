// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* DoRmSergei.cs
 * PRSPKT.ru
 * © PRSPKT Architects, 2017
 *
 * This file contains the methods which are used by the 
 * command.
 */
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


        private static void DoRm(UIDocument UIdoc, Transaction t)
        {
            Document _doc = UIdoc.Document;
            int wall_count = 0;
            int room_count = 0;
            var errorTypeList = new List<string>();

            t.Start("Простановка Rm_Этаж для Сергея ОВ");

            var rooms = new FilteredElementCollector(_doc)
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
                if (rooms.Count > 0)
                {
                    foreach (Room tempRoom in rooms)
                    {
                        String lvl = tempRoom.Level.Name;
                        bool flag = false;

                        if (tempRoom.LookupParameter("Rm_Этаж") != null)
                        {
                            Parameter rm = tempRoom.LookupParameter("Rm_Этаж");
                            rm.Set(lvl);
                            flag = true;
                        }
                        else
                        {
                            var e = Enum.GetName(typeof(ErrorEnum), ErrorEnum.Помещение_Rm_Этаж);
                            if (!errorTypeList.Contains(e))
                            {
                                errorTypeList.Add(e);
                            }

                            if (tempRoom.LookupParameter("GP_Этаж") != null)
                            {
                                Parameter gp = tempRoom.LookupParameter("GP_Этаж");
                                gp.Set(lvl);
                                flag = true;
                            }
                            else
                            {
                                var e1 = Enum.GetName(typeof(ErrorEnum), ErrorEnum.Помещение_GP_Этаж);
                                if (!errorTypeList.Contains(e1))
                                {
                                    errorTypeList.Add(e1);
                                }
                            }
                            if (flag)
                            {
                                room_count += 1;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }

            try
            {
                if (walls.Count > 0)
                {
                    foreach (var wall in walls)
                    {
                        var flag = false;
                        var lvl = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsValueString();
                        if (wall.LookupParameter("GP_Этаж") != null)
                        {
                            wall.LookupParameter("GP_Этаж").Set(lvl);
                            flag = true;
                        }
                        else
                        {
                            var e = Enum.GetName(typeof(ErrorEnum), ErrorEnum.Стены_GP_Этаж);
                            if (!errorTypeList.Contains(e))
                            {
                                errorTypeList.Add(e);
                            }
                        }
                        if (flag)
                        {
                            wall_count += 1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
            }

            t.Commit();

            string message_wall = string.Empty;
            string message_room = string.Empty;
            string message_error = string.Empty;

            if (errorTypeList.Count > 0)
            {
                message_error = "Возможные ошибки: " + string.Join(", ", errorTypeList);
            }

            message_wall = wall_count == 0 ? "В стены ничего не вписано" : $"В стены вписан параметр GP_Этаж в количестве: {wall_count} шт.";
            message_room = room_count == 0 ? "В помещения ничего не вписано" : $"В помещения вписан параметр Rm_Этаж и GP_Этаж: {room_count} шт.";

            TaskDialog.Show("Результат работы Сергей_ОВ",
                message_wall + "\n" +
                message_room + "\n" +
                message_error,
                TaskDialogCommonButtons.Ok
                );
        }
    }
}
