
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    /// <summary>
    /// Логика взаимодействия для Levels.xaml
    /// </summary>
    public partial class LevelsControl : Window
    {
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

            Cancel_Button.Content = "Отмена";
            OK_Button.Content = "OK";
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (radioSelectedLevels.IsChecked == true)
            {
                this.DialogResult = true;
                this.Close();
                var userLevelsControl = new PRSPKT_Apps.ApartmentCalc_P.SelectLevelsControl(_UIDoc);
                userLevelsControl.InitializeComponent();
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
        /// Cancel button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

    }
}