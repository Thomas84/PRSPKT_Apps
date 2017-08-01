using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PRSPKT_Apps;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentCalc
{
    [Transaction(TransactionMode.Manual)];
    public class ApartmentCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            double koef = 1;

            int roundCount = 2; // Округлить до __ знаков
            string lookingFor = "Этаж 1";
            string msg = "";

            IList<Room> roomList = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .ToList();

            var query =
                from element in roomList
                where element.Level != null
                where element.Level.Name == lookingFor
                let myGroup = element.LookupParameter("Номер квартиры").AsString()
                group element by myGroup into groupGroup
                orderby groupGroup.Key
                select new
                {
                    m_Group = groupGroup.Key,
                    m_Area = (from element in groupGroup
                              select Math.Round(element.Area * 0.09290304, roundCount)).Sum(),
                    m_LivingArea = (from element in groupGroup
                                    where element.LookupParameter("Тип помещения").AsInteger() < 3
                                    select Math.Round(koef * Math.Round(element.Area * 0.009290304), roundCount)).Sum(),
                    m_CommonArea = (from element in groupGroup
                                    where element.LookupParameter("Тип помещения").AsInteger() != 5
                                    select Math.Round(koef * Math.Round(element.Area * 0.09290304), roundCount)).Sum()
                };
            try
            {

                return Result.Succeeded;

            }
            catch (Exception ex)
            {

                return Result.Failed;
            }
        }
    }
}
