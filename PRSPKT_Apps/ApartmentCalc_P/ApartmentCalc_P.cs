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
namespace ApartmentCalc_P
{
    [Transaction(TransactionMode.Manual)]
    public class ApartmentCalc_P : IExternalCommand
    {
        #region initialize variables
        private string _apartNumber;
        private int _roundCount;
        private string _apartAreaL;
        private string _apartAreaA;
        private string _apartAreaC;
        private string _apartCount;
        private string _apartAreaLwCoef;
        private string _apartRoomType;
        private double _koef;
        private double _userKoef1;
        private double _userKoef2;
        private double _userKoef3;
        private string _apartAreaKoef;

        public int RoundCount { get => _roundCount; }
        public string ApartNumber { get => _apartNumber; }
        public string ApartArea_L { get => _apartAreaL; }
        public string ApartArea_A { get => _apartAreaA; }
        public string ApartArea_C { get => _apartAreaC; }
        public string ApartCount { get => _apartCount; }
        public string ApartAreaLwCoef { get => _apartAreaLwCoef; }
        public string ApartRoomType { get => _apartRoomType; }
        public double Koeff { get => _koef; }
        public double UserKoef1 { get => _userKoef1; }
        public double UserKoef2 { get => _userKoef2; }
        public double UserKoef3 { get => _userKoef3; }
        public string ApartAreaKoef { get => _apartAreaKoef; }
        #endregion

        private const double METERS_IN_FEET = 0.3048;

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
        // TODO: Может, запилить возможность сохранения настроек?

        private void RoomCalc(UIDocument UIDdoc, Transaction t)
        {
            Document _doc = UIDdoc.Document;
            t.Start("Квартирография");

            var userControl = new PRSPKT_Apps.ApartmentCalc_P.LevelsControl(UIDdoc);
            //var userLevelsControl = new PRSPKT_Apps.ApartmentCalc_P.SelectLevelsControl(UIDdoc);
            userControl.InitializeComponent();


            if (userControl.ShowDialog() == true)
            {
                _apartNumber = userControl.txtBoxApartNum.Text;
                _roundCount = Int32.Parse(userControl.txtBoxRound.Text);
                _apartAreaL = userControl.txtBoxAreaLivingApart.Text;
                _apartAreaA = userControl.txtBoxAreaApart.Text;
                _apartAreaC = userControl.txtBoxAreaApartC.Text;
                _apartCount = userControl.txtBoxApartRoomsCount.Text;
                _apartAreaLwCoef = userControl.txtBoxAreaApartWithKoef.Text;
                _apartRoomType = userControl.txtBoxType.Text;
                _userKoef1 = Double.Parse(userControl.txtBoxRow1Koef.Text);
                _userKoef2 = Double.Parse(userControl.txtBoxRow2Koef.Text);
                _userKoef3 = Double.Parse(userControl.txtBoxRow3Koef.Text);
                _apartAreaKoef = userControl.txtBoxAreaKoef.Text;

                IList<Room> ModelRooms = userControl.SelectedRooms;

                var query =
                    from element in ModelRooms
                    let myGroup = element.LookupParameter(ApartNumber).AsString()
                    group element by myGroup into groupGroup
                    from room in groupGroup
                    group room by groupGroup.Key;

                foreach (var apart in query)
                {
                    //msg += "Квартира " + apart.Key + "\r\n";

                    List<double> area_list = ApartAreas(apart.ToList(), RoundCount);
                    double area_L = area_list[0]; // Жилая площадь
                    double area_A = area_list[1]; // Площадь квартиры
                    double area_C = area_list[2]; // Общая площадь квартиры
                    int count_r = (int)area_list[3]; // Количество жилых комнат

                    double area_L_Converted = UnitUtils.ConvertToInternalUnits(area_L, DisplayUnitType.DUT_SQUARE_METERS);
                    double area_A_Converted = UnitUtils.ConvertToInternalUnits(area_A, DisplayUnitType.DUT_SQUARE_METERS);
                    double area_C_Converted = UnitUtils.ConvertToInternalUnits(area_C, DisplayUnitType.DUT_SQUARE_METERS);


                    foreach (var _room in apart)
                    {
                        Parameter Area_L = _room.LookupParameter(ApartArea_L);
                        Parameter Area_A = _room.LookupParameter(ApartArea_A);
                        Parameter Area_C = _room.LookupParameter(ApartArea_C);
                        Parameter Count_R = _room.LookupParameter(ApartCount);
                        try
                        {
                            int _type = _room.LookupParameter(ApartRoomType).AsInteger();
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


        private List<double> ApartAreas(List<Room> list, int roundCount)
        {
            double room_living_sum = 0;
            double room_apart_sum = 0;
            double room_common_sum = 0;
            double room_count = 0;
            var outlist = new List<double>();

            foreach (Room tempRoom in list)
            {
                int _type = tempRoom.LookupParameter(ApartRoomType).AsInteger();
                _koef = 1;
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
                        _koef = 0;
                        break;
                    case 3:
                        _koef = UserKoef1;
                        break;
                    case 4:
                        _koef = UserKoef2;
                        break;
                    case 6:
                        _koef = UserKoef3;
                        break;
                    default:
                        _koef = 1;
                        break;
                }
                double karea = Math.Round(area_inn * _koef, roundCount);
                double karea_converted = UnitUtils.ConvertToInternalUnits(karea, DisplayUnitType.DUT_SQUARE_METERS);
                room_common_sum += karea;

                tempRoom.LookupParameter(ApartAreaLwCoef).Set(karea_converted);
                tempRoom.LookupParameter(ApartAreaKoef).Set(_koef);
            }
            outlist.Add(room_living_sum);
            outlist.Add(room_apart_sum);
            outlist.Add(room_common_sum);
            outlist.Add(room_count);
            return outlist;
        }
    }
}