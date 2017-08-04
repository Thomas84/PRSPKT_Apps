#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Globalization;
using System.Resources;
using PRSPKT_Apps;
#endregion

namespace RoomFinishes
{
    /// <summary>
    /// Логика взаимодействия для FloorFinishesControl.xaml
    /// </summary>
    public partial class FloorFinishesControl : Window
    {
        private Document _doc;
        private UIDocument _UIDoc;

        private FloorType _selectedFloorType;
        public FloorType SelectedWallType { get { return _selectedFloorType; } }

        private double _floorHeight;
        public double FloorHeight { get { return _floorHeight; } }

        private IEnumerable<Room> _selectedRooms;
        public IEnumerable<Room> SelectedRooms { get { return _selectedRooms; } }

        private Parameter _roomParameter;
        public Parameter RoomParameter { get { return _roomParameter; } }


        //private IEnumerable<WallType> _wallTypes;

        public FloorFinishesControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            // Fill out Text in form

            this.Title = "Rooms Finishes";
            this.all_rooms_radio.Content = "Все помещения";
            this.floor_height_radio.Content = "Высота ";
            this.height_param_radio.Content = "Параметр";
            this.groupboxName.Header = "Группа параметров";
            this.select_floor_label.Content = "Пол";
            this.selected_rooms_radio.Content = "Выбранные помещения";
            this.Ok_Button.Content = "OK";
            this.Cancel_Button.Content = "Cancel";

            IEnumerable<FloorType> floorTypes = from elem in new FilteredElementCollector(_doc).OfClass(typeof(FloorType))
                                                let type = elem as FloorType
                                                where type.IsFoundationSlab == false
                                                select type;
            floorTypes = floorTypes.OrderBy(floorType => floorType.Name);

            // Bind ArrayList with the ListBox
            FloorTypeListBox.ItemsSource = floorTypes;
            FloorTypeListBox.SelectedItem = FloorTypeListBox.Items[0];

            // Find a room
            IList<Element> roomList = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Rooms).ToList();

            if (roomList.Count != 0)
            {
                // Get all double parameters
                Room room = roomList.First() as Room;

                List<Parameter> doubleParam = (from Parameter p in room.Parameters
                                               where p.Definition.ParameterType == ParameterType.Length
                                               select p).ToList();
                paramSelector.ItemsSource = doubleParam;
                paramSelector.DisplayMemberPath = "Definition.Name";
                paramSelector.SelectedIndex = 0;
            }
            else
            {
                paramSelector.IsEnabled = false;
                height_param_radio.IsEnabled = false;
            }


        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (floor_height_radio.IsChecked == true)
            {
                _roomParameter = null;
// _floorHeight = (double)
                if (FloorTypeListBox.SelectedItem != null)
                {
                    _selectedFloorType = FloorTypeListBox.SelectedItem as FloorType;
                }
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

}
