
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


        public LevelsControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            LogoName2.Content = HelpMe.GetVersion();

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
                _selectedRooms = AllRooms();
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
            //var levelsControl = new LevelsControl(_UIDoc);
            string _roomType = txtBoxType.Text;

            IList<Room> filteredSelectedRooms = new List<Room>();

            IList<Room> ModelRooms = new FilteredElementCollector(_doc)
                    .OfClass(typeof(SpatialElement))
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .Cast<Room>()
                    .Where(room => room.Area > 0 && room.LevelId != null)
                    .Where(room => room.LookupParameter(_roomType).AsInteger() != 5)
                    .ToList();
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