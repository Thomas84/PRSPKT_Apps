
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.ApartmentCalc_P;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    /// <summary>
    /// Логика взаимодействия для SelectLevels.xaml
    /// </summary>
    public partial class SelectLevelsControl : Window
    {
        private Document _doc;
        private UIDocument _UIDoc;
        private IOrderedEnumerable<Level> _levels;

        private IList<Room> _selectedRooms;
        public IList<Room> SelectedRooms
        {
            get { return _selectedRooms; }
        }

        private Level _selectedLevel;
        public Level SelectedLevel
        {
            get { return _selectedLevel; }
        }

        private Level _nextLevel;
        public Level NextLevel
        {
            get { return _nextLevel; }
        }

        public SelectLevelsControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            // Find a room
            //IList<Room> roomList = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>().ToList();

            _levels = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(lev => lev.Elevation);

            LevelsListBox.ItemsSource = _levels;
            LevelsListBox.SelectedItem = LevelsListBox.Items[0];
            LevelsListBox.DisplayMemberPath = "Name";


        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (LevelsListBox.SelectedItem != null)
            {
                _selectedLevel = LevelsListBox.SelectedItem as Level;

                if (Yes_Checkbox.IsChecked == true)
                {
                    _nextLevel = LevelsListBox.Items[LevelsListBox.SelectedIndex + 1] as Level;
                }
                else
                {
                    _nextLevel = LevelsListBox.SelectedItem as Level;
                }

                this.DialogResult = true;
                this.Close();

                // Select rooms
                _selectedRooms = SelectRooms();
            }
            else
            {
                TaskDialog.Show("Квартирография", "Ошибочка с уровнем", TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                this.Activate();
            }
        }

        private IList<Room> SelectRooms()
        {
            IList<Room> ModelRooms = new FilteredElementCollector(_doc)
                    .OfClass(typeof(SpatialElement))
                    .OfCategory(BuiltInCategory.OST_Rooms)
                    .Cast<Room>()
                    .Where(room => room.Area > 0 && room.LevelId != null)
                    .Where(room => room.Level.Name == SelectedLevel.Name || room.Level.Name == NextLevel.Name)
                    .Where(room => room.LookupParameter("П_Тип помещения").AsInteger() != 5)
                    .ToList();
            return ModelRooms;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
