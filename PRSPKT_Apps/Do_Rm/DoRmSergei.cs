﻿#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
            t.Start("Простановка Rm_Этаж для Сергея ОВ");
            //IList<Level> _levels = new FilteredElementCollector(_doc)
            //    .OfCategory(BuiltInCategory.OST_Levels).Cast<Level>().ToList();

            IList<Room> _rooms = new FilteredElementCollector(_doc)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(x => x.Area>0)
                .ToList();

            foreach (Room tempRoom in _rooms)
            {
                String _lvl = tempRoom.Level.Name;
                Parameter _rm = tempRoom.LookupParameter("Rm_Этаж");
                Parameter _gp = tempRoom.LookupParameter("GP_Этаж");
                _rm.Set(_lvl);
                _gp.Set(_lvl);
            }
            t.Commit();
        }
    }
}