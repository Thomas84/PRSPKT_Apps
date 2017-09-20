
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
    /// Логика взаимодействия для Levels.xaml
    /// </summary>
    public partial class LevelsControl : Window
    {
        private Document _doc;
        private UIDocument _UIDoc;
        private IOrderedEnumerable<Level> _levels;

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

            // Find a room
            //IList<Room> roomList = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>().ToList();

            _levels = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(lev => lev.Elevation);
            
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (radioSelectedLevels.IsChecked == true)
            {
                PRSPKT_Apps.ApartmentCalc_P.SelectLevelsControl userLevelsControl = new PRSPKT_Apps.ApartmentCalc_P.SelectLevelsControl(_UIDoc);
                userLevelsControl.InitializeComponent();
                if (userLevelsControl.ShowDialog() == true)
                {
                    
                }
            }
            else if (radioActiveView.IsChecked == true)
            {

            }
            else if (radioAllLevels.IsChecked == true)
            {

            }

                this.DialogResult = true;
                this.Close();

                // Select rooms
                _selectedRooms = SelectRooms();
        }

        private IList<Room> SelectRooms()
        {
            IList<Room> ModelRooms = new List<Room>();

            return ModelRooms;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        
    }
}
