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
namespace ApartmentCalc_P
{
    [Transaction(TransactionMode.Manual)]
    public class ApartmentCalc_P : IExternalCommand
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
        // TODO: Запилить возможность подсчёта по двухуровневым квартирам
        // TODO: Запилить возможность дополнительных рулек (округление, выбор параметров)
        // TODO: Может, запилить возможность сохранения настроек?

        private void RoomCalc(UIDocument UIDdoc, Transaction t)
        {
            Document _doc = UIDdoc.Document;
            t.Start("Квартирография");
            //string msg = "";

            // Load user form
            PRSPKT_Apps.ApartmentCalc_P.LevelsControl userControl = new PRSPKT_Apps.ApartmentCalc_P.LevelsControl(UIDdoc);
            userControl.InitializeComponent();

            //LevelsWindow.ShowDialog();

            if (userControl.ShowDialog() == true)
            {

                IList<Room> ModelRooms = userControl.SelectedRooms;
                //			double koef = 1;

                int roundCount = 2; // Округлить до __ знаков
                string lookingFor = userControl.SelectedLevel.Name;

                var query =
                    from element in ModelRooms
                    let myGroup = element.LookupParameter("П_Номер квартиры").AsString()
                    group element by myGroup into groupGroup
                    from room in groupGroup
                    group room by groupGroup.Key;

                foreach (var apart in query)
                {
                    //msg += "Квартира " + apart.Key + "\r\n";

                    List<double> area_list = ApartAreas(apart.ToList(), roundCount);
                    double area_L = area_list[0]; // Жилая площадь
                    double area_A = area_list[1]; // Площадь квартиры
                    double area_C = area_list[2]; // Общая площадь квартиры
                    int count_r = (int)area_list[3]; // Количество жилых комнат

                    double area_L_Converted = UnitUtils.ConvertToInternalUnits(area_L, DisplayUnitType.DUT_SQUARE_METERS);
                    double area_A_Converted = UnitUtils.ConvertToInternalUnits(area_A, DisplayUnitType.DUT_SQUARE_METERS);
                    double area_C_Converted = UnitUtils.ConvertToInternalUnits(area_C, DisplayUnitType.DUT_SQUARE_METERS);


                    foreach (var _room in apart)
                    {
                        Parameter Area_L = _room.LookupParameter("Площадь квартиры Жилая");
                        Parameter Area_A = _room.LookupParameter("Площадь квартиры");
                        Parameter Area_C = _room.LookupParameter("Площадь квартиры Общая");
                        Parameter Count_R = _room.LookupParameter("Число комнат");
                        try
                        {
                            int _type = _room.LookupParameter("П_Тип помещения").AsInteger();
                            Area_L.Set(area_L_Converted);
                            Area_A.Set(area_A_Converted);
                            Area_C.Set(area_C_Converted);
                            Count_R.Set(count_r);

                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Квартирография", "Ошибочка: " + ex.Message);
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

        private const double METERS_IN_FEET = 0.3048;

        private List<double> ApartAreas(List<Room> list, int roundCount)
        {
            double room_living_sum = 0;
            double room_apart_sum = 0;
            double room_common_sum = 0;
            double room_count = 0;
            var outlist = new List<double>();

            foreach (Room tempRoom in list)
            {
                int _type = tempRoom.LookupParameter("П_Тип помещения").AsInteger();
                double koef = 1;
                double area_inn = UnitUtils.ConvertFromInternalUnits(tempRoom.Area, DisplayUnitType.DUT_SQUARE_METERS);

                double area = Math.Round(area_inn, roundCount);

                switch (_type)
                {
                    case 1:
                        room_living_sum += area;
                        room_apart_sum += area;
                        room_count += 1;
                        break;
                    case 2:
                        room_apart_sum += area;
                        break;
                    case 5:
                        koef = 0;
                        break;
                    case 3:
                        koef = 0.5;
                        break;
                    case 4:
                    case 6:
                        koef = 0.3;
                        break;
                    default:
                        koef = 1;
                        break;
                }
                double karea = Math.Round(area_inn * koef, roundCount);
                double karea_converted = UnitUtils.ConvertToInternalUnits(karea, DisplayUnitType.DUT_SQUARE_METERS);
                room_common_sum += karea;

                tempRoom.LookupParameter("Площадь с коэффициентом").Set(karea_converted);
                tempRoom.LookupParameter("Коэффициент площади").Set(koef);
            }
            outlist.Add(room_living_sum);
            outlist.Add(room_apart_sum);
            outlist.Add(room_common_sum);
            outlist.Add(room_count);
            return outlist;
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
