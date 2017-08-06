using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PRSPKT_Apps.ApartmentCalc;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace ApartmentCalc
{
    [Transaction(TransactionMode.Manual)]
    public class ApartmentCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string msg = "";
            var askUser = new askUser();

            foreach (Level level in new FilteredElementCollector(doc)
                     .OfClass(typeof(Level))
                     .Cast<Level>()
                     .ToList())
            {
                askUser.cb_Levels.Items.Add(level.Name);
            }
            IList<Room> roomList = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .ToList();
            askUser.ShowDialog();
            if (askUser.DialogResult == DialogResult.OK)
            {
                //			double koef = 1;

                int roundCount = 2; // Округлить до __ знаков

                //			int count = 0;
                //
                //			string outApartNumber = "Номер квартиры";
                //			string outApartAreaLiving = "Площадь квартиры Жилая";
                //			string outApartAreaCommon = "Площадь квартиры Общая";
                //			string outApartRoomCount = "Число комнат";
                //			string outRoomIndex = "Индекс помещения";
                //			string outRoomKoef = "Площадь с коэффициентом";
                //			string outApartArea = "Площадь квартиры";

                //			double karea;

                string lookingFor = askUser.cb_Levels.SelectedItem.ToString();

                // Список комнат на __ уровне
                var query =
                    from element in roomList
                    where element.Level != null
                    where element.Level.Name == lookingFor
                    where element.LookupParameter("Тип помещения").AsInteger() != 5
                    where element.Area > 0
                    let myGroup = element.LookupParameter("Номер квартиры").AsString()
                    group element by myGroup into groupGroup
                    orderby groupGroup.Key
                    select new
                    {
                        // Номер квартиры
                        m_Group = groupGroup.Key,
                        // Площадь квартиры ОБЩАЯ
                        m_CommonArea = (from element in groupGroup
                                        let m_type = element.LookupParameter("Тип помещения").AsInteger()
                                        select acceptKoef(m_type, element.Area, roundCount)).Sum(),

                        // Площадь квартиры ЖИЛАЯ
                        m_LivingArea = (from element in groupGroup
                                        let m_type = element.LookupParameter("Тип помещения").AsInteger()
                                        where m_type == 1
                                        select acceptKoef(m_type, element.Area, roundCount)).Sum(),
                        //				                select Math.Round(koef*Math.Round(element.Area*0.09290304,roundCount),roundCount)).Sum(),
                        // Количество жилых комнат в квартире
                        m_Count = (from element in groupGroup
                                   where element.LookupParameter("Тип помещения").AsInteger() == 1
                                   select element).Count(),
                        // Площадь квартиры (без балконов и лоджий)
                        m_Area = (from element in groupGroup
                                  let m_type = element.LookupParameter("Тип помещения").AsInteger()
                                  where m_type < 3
                                  select acceptKoef(m_type, element.Area, roundCount)).Sum(),
                    };
                //TODO Добавить внесение полученных данных в Revit обратно
                using (Transaction t = new Transaction(doc, "Квартирография"))
                {
                    t.Start();
                    foreach (var outerGroup in query.GroupBy(x => x.m_Group))
                    {
                        msg += "\r\n" + "Квартира " + outerGroup.Key + "\r\n";
                        foreach (var element in outerGroup)
                        {

                            msg += "Количество комнат = " + element.m_Count + " шт. \r\n";
                            msg += "Жилая площадь = " + element.m_LivingArea + " м2 \r\n";
                            msg += "Площадь квартиры = " + element.m_Area + " м2 \r\n";
                            msg += "Общая площадь = " + element.m_CommonArea + " м2 \r\n";
                        }
                    }

                    // Назначить помещениям коэффициент и площадь с коэффициентом
                    foreach (Room room in roomList.Where(r => r.Level.Name == lookingFor && r.LookupParameter("Тип помещения").AsInteger() != 5 && r.Area > 0))
                    {
                        int RoomType = room.LookupParameter("Тип помещения").AsInteger();
                        room.LookupParameter("Площадь с коэффициентом").Set(acceptKoef(RoomType, room.Area, roundCount));
                        room.LookupParameter("Коэффициент площади").Set(RoomKoef(RoomType));
                    }
                    t.Commit();
                }



                TaskDialog.Show("Message", msg);


            }
            else
                askUser.Close();


        }

        private double acceptKoef(int type, double area, int round)
        {
            return Math.Round(RoomKoef(type) * Math.Round(area * 0.09290304, round), round);
        }

        private double RoomKoef(int type)
        {
            double k = 1;
            switch (type)
            {
                case 5:
                    k = 0;
                    break;
                case 3:
                    k = 0.5;
                    break;
                case 4:
                    k = 0.3;
                    break;
                case 6:
                    k = 0.7;
                    break;
                default:
                    k = 1;
                    break;
            }
            return k;

        }
            try
            {

                return Result.Succeeded;

            }
            catch (Exception)
            {

                return Result.Failed;
            }
        }
    }
}
