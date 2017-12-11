#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using PRSPKT_Apps.Common;
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
        private double _coef;
        private double _userCoef1;
        private double _userCoef2;
        private double _userCoef3;


        public int RoundCount { get => _roundCount; }
        public string ApartNumber { get => _apartNumber; }
        public string ApartArea_L { get => _apartAreaL; }
        public string ApartArea_A { get => _apartAreaA; }
        public string ApartArea_C { get => _apartAreaC; }
        public string ApartCount { get => _apartCount; }
        public string ApartAreaLwCoef { get => _apartAreaLwCoef; }
        public string ApartRoomType { get => _apartRoomType; }
        public double Koeff { get => _coef; }
        public double UserCoef1 { get => _userCoef1; }
        public double UserCoef2 { get => _userCoef2; }
        public double UserCoef3 { get => _userCoef3; }

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

        private void RoomCalc(UIDocument UIDdoc, Transaction t)
        {
            Document _doc = UIDdoc.Document;
            t.Start("Квартирография");

            var userControl = new PRSPKT_Apps.ApartmentCalc_P.LevelsControl(UIDdoc);
            userControl.InitializeComponent();
            userControl.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            string rSelectedLevels = UserSettings.Get("rSelectedLevels");
            string rAllLevels = UserSettings.Get("rAllLevels");
            string rActiveLevel = UserSettings.Get("rActiveLevel");
            if (rSelectedLevels == "1")
            {
                userControl.radioSelectedLevels.IsChecked = true;
            }
            else if (rAllLevels == "1")
            {
                userControl.radioAllLevels.IsChecked = true;
            }
            else
            {
                userControl.radioActiveView.IsChecked = true;
            }


            userControl.txtBoxRow1Coef.Text = UserSettings.Get("userCoef1");
            userControl.txtBoxRow2Coef.Text = UserSettings.Get("userCoef2");
            userControl.txtBoxRow3Coef.Text = UserSettings.Get("userCoef3");
            userControl.txtBoxType.Text = UserSettings.Get("type");
            userControl.txtBoxAreaLivingApart.Text = UserSettings.Get("area_living");
            userControl.txtBoxAreaApart.Text = UserSettings.Get("area");
            userControl.txtBoxAreaApartC.Text = UserSettings.Get("area_common");
            userControl.txtBoxAreaApartWithCoef.Text = UserSettings.Get("area_w_coef");
            userControl.txtBoxApartRoomsCount.Text = UserSettings.Get("rooms_count");
            userControl.txtBoxApartNum.Text = UserSettings.Get("rooms_number");
            userControl.txtBoxRoomsApart.Text = UserSettings.Get("apartment_rooms");
            userControl.txtBoxLiveApart.Text = UserSettings.Get("apartment_living_rooms");
            userControl.txtBoxRound.Text = UserSettings.Get("roundNumber");



            if (userControl.ShowDialog() == true)
            {
                _apartNumber = userControl.txtBoxApartNum.Text;
                _roundCount = Int32.Parse(userControl.txtBoxRound.Text);
                _apartAreaL = userControl.txtBoxAreaLivingApart.Text;
                _apartAreaA = userControl.txtBoxAreaApart.Text;
                _apartAreaC = userControl.txtBoxAreaApartC.Text;
                _apartCount = userControl.txtBoxApartRoomsCount.Text;
                _apartAreaLwCoef = userControl.txtBoxAreaApartWithCoef.Text;
                _apartRoomType = userControl.txtBoxType.Text;
                _userCoef1 = Double.Parse(userControl.txtBoxRow1Coef.Text);
                _userCoef2 = Double.Parse(userControl.txtBoxRow2Coef.Text);
                _userCoef3 = Double.Parse(userControl.txtBoxRow3Coef.Text);

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

                UserSettings.Set("rSelectedLevels", (userControl.radioSelectedLevels.IsChecked.Value) ? "1" : "0");
                UserSettings.Set("rAllLevels", (userControl.radioAllLevels.IsChecked.Value) ? "1" : "0");
                UserSettings.Set("rActiveLevel", (userControl.radioActiveView.IsChecked.Value) ? "1" : "0");
                UserSettings.Set("userCoef1", userControl.txtBoxRow1Coef.Text);
                UserSettings.Set("userCoef2", userControl.txtBoxRow2Coef.Text);
                UserSettings.Set("userCoef3", userControl.txtBoxRow3Coef.Text);
                UserSettings.Set("type", userControl.txtBoxType.Text);
                UserSettings.Set("area_living", userControl.txtBoxAreaLivingApart.Text);
                UserSettings.Set("area", userControl.txtBoxAreaApart.Text);
                UserSettings.Set("area_common", userControl.txtBoxAreaApartC.Text);
                UserSettings.Set("area_w_coef", userControl.txtBoxAreaApartWithCoef.Text);
                UserSettings.Set("rooms_count", userControl.txtBoxApartRoomsCount.Text);
                UserSettings.Set("rooms_number", userControl.txtBoxApartNum.Text);
                UserSettings.Set("apartment_rooms", userControl.txtBoxRoomsApart.Text);
                UserSettings.Set("apartment_living_rooms", userControl.txtBoxLiveApart.Text);
                UserSettings.Set("roundNumber", userControl.txtBoxRound.Text);
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
                _coef = 1;
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
                        _coef = 0;
                        break;
                    case 3:
                        _coef = UserCoef1;
                        break;
                    case 4:
                        _coef = UserCoef2;
                        break;
                    case 6:
                        _coef = UserCoef3;
                        break;
                    default:
                        _coef = 1;
                        break;
                }
                double karea = Math.Round(area_inn * _coef, roundCount);
                double karea_converted = UnitUtils.ConvertToInternalUnits(karea, DisplayUnitType.DUT_SQUARE_METERS);
                room_common_sum += karea;

                tempRoom.LookupParameter(ApartAreaLwCoef).Set(karea_converted);
                tempRoom.LookupParameter("Коэффициент площади").Set(_coef);
            }
            outlist.Add(room_living_sum);
            outlist.Add(room_apart_sum);
            outlist.Add(room_common_sum);
            outlist.Add(room_count);
            return outlist;
        }
    }
}