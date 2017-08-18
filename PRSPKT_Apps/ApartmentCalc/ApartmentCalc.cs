#region Namespaces
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
namespace ApartmentCalc
{
    [Transaction(TransactionMode.Manual)]
    public class ApartmentCalc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    RoomCalc(UIdoc, t);
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
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
            }
        }

        private void RoomCalc(UIDocument UIDdoc, Transaction t)
        {
            Document _doc = UIDdoc.Document;
            t.Start("Квартирография");
            string msg = "";

            // Load user form
            PRSPKT_Apps.ApartmentCalc.LevelsForm userControl = new PRSPKT_Apps.ApartmentCalc.LevelsForm(UIDdoc);
            userControl.InitializeComponent();

            //LevelsWindow.ShowDialog();

            if (userControl.ShowDialog() == true)
            {

                IList<Room> ModelRooms = userControl.SelectedRooms;
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
                string lookingFor = userControl.SelectedLevel.Name;

                // Список комнат на __ уровне
                var query =
                    from element in ModelRooms
                        //where element.Level.Name == lookingFor
                    where element.LookupParameter("Тип помещения").AsInteger() != 5
                    //where element.Area > 0
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
                                        select AcceptKoef(m_type, element.Area, roundCount)).Sum(),

                        // Площадь квартиры ЖИЛАЯ
                        m_LivingArea = (from element in groupGroup
                                        let m_type = element.LookupParameter("Тип помещения").AsInteger()
                                        where m_type == 1
                                        select AcceptKoef(m_type, element.Area, roundCount)).Sum(),
                        //				                select Math.Round(koef*Math.Round(element.Area*0.09290304,roundCount),roundCount)).Sum(),
                        // Количество жилых комнат в квартире
                        m_Count = (from element in groupGroup
                                   where element.LookupParameter("Тип помещения").AsInteger() == 1
                                   select element).Count(),
                        // Площадь квартиры (без балконов и лоджий)
                        m_Area = (from element in groupGroup
                                  let m_type = element.LookupParameter("Тип помещения").AsInteger()
                                  where m_type < 3
                                  select AcceptKoef(m_type, element.Area, roundCount)).Sum(),
                    };
                //TODO Добавить внесение полученных данных в Revit обратно
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
                foreach (Room room in ModelRooms
                    .Where(r => r.Level.Name == lookingFor && r.LookupParameter("Тип помещения").AsInteger() != 5 && r.Area > 0))
                {
                    int RoomType = room.LookupParameter("Тип помещения").AsInteger();
                    room.LookupParameter("Площадь с коэффициентом").Set(AcceptKoef(RoomType, room.Area, roundCount));
                    room.LookupParameter("Коэффициент площади").Set(RoomKoef(RoomType));
                }
                t.Commit();
                TaskDialog.Show("Message", msg);
            }
            else
            {
                t.RollBack();
            }
            //LevelsWindow.Close();
        }

        private double AcceptKoef(int type, double area, int round)
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
                    k = 0.3;
                    break;
                default:
                    k = 1;
                    break;
            }
            return k;
        }
    }
}
