// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    /// <summary>
    /// Логика взаимодействия для Levels.xaml
    /// </summary>
    public partial class LevelsControl : Window
    {
        LinearGradientBrush eBrush = null;
        SolidColorBrush lBrush = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(0, 0, 0, 0));

        private Document _doc;
        private UIDocument _UIDoc;

        private IList<Room> _selectedRooms;
        public IList<Room> SelectedRooms
        {
            get { return _selectedRooms; }
        }

        private bool _isOk;
        public bool IsOk { get => _isOk; set => _isOk = value; }

        public LevelsControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            _isOk = true;
            LogoName2.Content = Common.MiscUtils.GetVersion();

            Cancel_Button.Content = "Отмена";
            OK_Button.Content = "OK";
        }

        private LinearGradientBrush EnterBrush()
        {
            LinearGradientBrush b = new LinearGradientBrush()
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 1)
            };
            b.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 195, 195, 195), 0.0));
            b.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 245, 245, 245), 1.0));
            return b;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            SelectLevelsControl userLevelsControl = new SelectLevelsControl(_UIDoc);
            userLevelsControl.InitializeComponent();


            if (radioSelectedLevels.IsChecked == true)
            {

                userLevelsControl.ShowDialog();
                _selectedRooms = SelectRooms(userLevelsControl);
            }
            else if (radioActiveView.IsChecked == true)
            {
                _selectedRooms = ActiveViewRooms();
            }
            else if (radioAllLevels.IsChecked == true)
            {
                var msg = ErrorMessage();
                msg += "\n\n  Продолжить?";

                if (this.IsOk)
                {
                    _selectedRooms = AllRooms();
                }
                else
                {
                    MessageBoxResult res = MessageBox.Show(msg, "Обратите внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                    {
                        _selectedRooms = AllRooms();
                    }
                    else
                    {
                        this.DialogResult = false;
                        return;
                    }
                }
            }
            this.DialogResult = true;
            this.Close();


        }
        /// <summary>
        /// Collect rooms in entire project file
        /// </summary>
        /// <returns></returns>
        private IList<Room> AllRooms()
        {
            IList<Room> ModelRooms = new FilteredElementCollector(_doc)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(room => room.Area > 0 && room.LevelId != null)
                .Where(room => room.LookupParameter(txtBoxType.Text).AsInteger() != 5)
                .ToList();

            return ModelRooms;
        }

        /// <summary>
        /// Collect rooms in active view only
        /// </summary>
        /// <returns></returns>
        private IList<Room> ActiveViewRooms()
        {

            IList<Room> ModelRooms = new FilteredElementCollector(_doc, _UIDoc.ActiveView.Id)
                .OfClass(typeof(SpatialElement))
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(room => room.Area > 0 && room.LevelId != null)
                .Where(room => room.LookupParameter(txtBoxType.Text).AsInteger() != 5)
                .ToList();

            return ModelRooms;
        }

        /// <summary>
        /// Collect rooms in user selected levels only
        /// </summary>
        /// <param name="levelControl"></param>
        /// <returns></returns>
        private IList<Room> SelectRooms(SelectLevelsControl levelControl)
        {
            IList<Room> filteredSelectedRooms = new List<Room>();
            var ModelRooms = AllRooms();

            //var filteredModelRooms = ModelRooms.Where(item => SelectedLevels.Contains(item.Name));
            if (levelControl.SelectedLevels != null)
            {
                foreach (var tempLevel in levelControl.SelectedLevels)
                {
                    foreach (var tempRoom in ModelRooms)
                    {
                        if (tempRoom.Level.Name == tempLevel.Name)
                        {
                            filteredSelectedRooms.Add(tempRoom);
                        }
                    }
                }
            }
            return filteredSelectedRooms;

        }



        private string ErrorMessage()
        {
            var levels = SelectLevelsControl.LevelsInProject(_doc).ToList();
            var roomsList = AllRooms();
            var dict = new Dictionary<string, List<string>>();
            var errorDict = new Dictionary<string, List<string>>();
            var message = string.Empty;

            foreach (var level in levels)
            {
                var numberList = new List<string>();
                var errorList = new List<string>();

                foreach (var tempRoom in roomsList.Where(room => room.Level.Name == level.Name))
                {
                    var apartNumber = tempRoom.LookupParameter(txtBoxApartNum.Text).AsString();
                    var apartName = tempRoom.LookupParameter(txtBoxRoomName.Text).AsString();
                    var apartType = tempRoom.LookupParameter(txtBoxType.Text).AsValueString();

                    if (!string.IsNullOrEmpty(apartNumber))
                    {
                        if (!numberList.Contains(apartNumber))
                        {
                            numberList.Add(apartNumber);
                        }
                    }
                    else
                    {
                        errorList.Add(string.Format("{0}[{1}]", apartName, apartType));
                    }
                }
                if (numberList.Count > 0)
                {
                    dict.Add(level.Name, numberList.OrderBy(q => q).ToList());
                }
                if (errorList.Count > 0)
                {
                    errorDict.Add(level.Name, errorList.OrderBy(q => q).ToList());
                }
            }
            message += "Номера квартир по помещениям: \n";


            foreach (KeyValuePair<string, List<string>> pair in dict)
            {
                message += string.Format("{0} ({1}) шт. : {2} \n", pair.Key, pair.Value.Count.ToString(), string.Join(", ", pair.Value));
            }

            if (errorDict.Count > 0)
            {
                _isOk = false;
                message += "\n\n Ошибки: Следующие помещения без номера квартиры" + "\n";
                foreach (KeyValuePair<string, List<string>> pair in errorDict)
                {
                    message += pair.Key + "-------" + string.Join(", ", pair.Value) + "\n";
                }
            }

            var duplicateValuesDict = new Dictionary<string, List<string>>();
            var duplicateValues = dict.Values.SelectMany(q => q).GroupBy(q => q).Where(q => q.Count() > 1).Select(q => q.Key);

            if (duplicateValues.Count() > 0)
            {
                message += "\n\n Помещения с одинаковыми номерами квартир" + "\n";
                _isOk = false;
                foreach (KeyValuePair<string, List<string>> item in dict)
                {
                    var ddd = new List<string>();
                    foreach (string str in item.Value)
                    {
                        if (duplicateValues.Contains(str))
                        {
                            ddd.Add(str);
                        }
                    }
                    if (ddd.Count > 0)
                    {
                        duplicateValuesDict[item.Key] = ddd;
                        message += string.Format("{0} ({1}) шт. : {2} \n", item.Key, ddd.Count.ToString(), string.Join(", ", ddd));
                    }
                }
            }

            return message;

        }

        /// <summary>
        /// Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OK_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (eBrush == null)
            {
                eBrush = EnterBrush();
            }
            okRect.Fill = eBrush;
        }

        private void OK_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            okRect.Fill = lBrush;
        }

        private void Cancel_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (eBrush == null)
            {
                eBrush = EnterBrush();
            }
            cancelRect.Fill = eBrush;
        }

        private void Cancel_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            cancelRect.Fill = lBrush;
        }
    }
}