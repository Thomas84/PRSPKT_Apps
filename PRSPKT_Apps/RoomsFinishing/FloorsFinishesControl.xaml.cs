#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PRSPKT_Apps;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
#endregion

namespace PRSPKT_Apps.RoomsFinishes
{
    /// <summary>
    /// Логика взаимодействия для FloorFinishesControl.xaml
    /// </summary>
    public partial class FloorsFinishesControl : Window
    {
        private Document _doc;
        private UIDocument _UIDoc;

        private FloorType _selectedFloorType;
        public FloorType SelectedFloorType
        {
            get { return _selectedFloorType; }
        }

        private double _floorHeight;
        public double FloorHeight
        {
            get { return _floorHeight; }
        }

        private IEnumerable<Room> _selectedRooms;
        public IEnumerable<Room> SelectedRooms
        {
            get { return _selectedRooms; }
        }

        private Parameter _roomParameter;
        public Parameter RoomParameter
        {
            get { return _roomParameter; }
        }


        public FloorsFinishesControl(UIDocument UIDoc)
        {

            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            // Fill out Text in form

            this.Title = Tools.GetResourceManager("floorFinishes_TaskDialogName");
            this.all_rooms_radio.Content = Tools.GetResourceManager("floorFinishes_all_rooms_radio");
            this.floor_height_radio.Content = Tools.GetResourceManager("floorFinishes_height_label");
            this.height_param_radio.Content = Tools.GetResourceManager("floorFinishes_height_param_label");
            this.groupboxName.Header = Tools.GetResourceManager("floorFinishes_groupboxName");
            this.select_floor_label.Content = Tools.GetResourceManager("floorFinishes_select_floor_label");
            this.selected_rooms_radio.Content = Tools.GetResourceManager("floorFinishes_SelectedRooms");
            this.Ok_Button.Content = Tools.GetResourceManager("roomFinishes_OK_Button");
            this.Cancel_Button.Content = Tools.GetResourceManager("roomFinishes_Cancel_Button");

            // Select the floor type in the document
            IEnumerable<FloorType> floorTypes = from elem in new FilteredElementCollector(_doc).OfClass(typeof(FloorType))
                                                let type = elem as FloorType
                                                let typeName = type.Name
                                                where type.IsFoundationSlab == false
                                                where typeName.Contains("Пол")
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
                if (Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits()) != null)
                {
                    _floorHeight = (double)Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits());
                    if (FloorTypeListBox.SelectedItem != null)
                    {
                        // Select floor type
                        _selectedFloorType = FloorTypeListBox.SelectedItem as FloorType;

                        this.DialogResult = true;
                        this.Close();

                        // Select the rooms
                        _selectedRooms = SelectRooms();
                    }
                }
                else
                {
                    TaskDialog.Show(Tools.LangResMan.GetString("floorFinishes_TaskDialogName", Tools.Cult),
                        Tools.LangResMan.GetString("floorFinishes_heightValueError", Tools.Cult), TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                    this.Activate();
                }
            }
            else
            {
                _roomParameter = paramSelector.SelectedItem as Parameter;

                if (FloorTypeListBox.SelectedItem != null)
                {
                    // Select floor type
                    _selectedFloorType = FloorTypeListBox.SelectedItem as FloorType;

                    this.DialogResult = true;
                    this.Close();

                    // Select the rooms
                    _selectedRooms = SelectRooms();
                }
            }
        }

        private IEnumerable<Room> SelectRooms()
        {
            // Create a set of selected elements ids
            ICollection<ElementId> selectedObjectIds = _UIDoc.Selection.GetElementIds();

            // Create a set of rooms
            IEnumerable<Room> ModelRooms = null;
            IList<Room> tempList = new List<Room>();

            if (all_rooms_radio.IsChecked.Value)
            {
                // Find all rooms in current view
                ModelRooms =
                    from elem in new FilteredElementCollector(_doc, _doc.ActiveView.Id).OfClass(typeof(SpatialElement))
                    let room = elem as Room
                    select room;
            }
            else
            {
                if (selectedObjectIds.Count != 0)
                {
                    // Find all rooms in selection
                    ModelRooms =
                        from elem in new FilteredElementCollector(_doc, selectedObjectIds).OfClass(typeof(SpatialElement))
                        let room = elem as Room
                        select room;
                    tempList = ModelRooms.ToList();
                }

                if (tempList.Count == 0)
                {
                    // Create a selection filter on rooms
                    ISelectionFilter filter = new RoomSelectionFilter();
                    IList<Reference> refList = _UIDoc.Selection.PickObjects(ObjectType.Element, filter,
                        Tools.LangResMan.GetString("roomFinishes_SelectRooms", Tools.Cult));
                    foreach (Reference r in refList)
                    {
                        tempList.Add(_doc.GetElement(r) as Room);
                    }

                    ModelRooms = tempList;

                }
            }

            return ModelRooms;
        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();

        }

        private void Height_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits()) != null)
            {
                _floorHeight = (double)Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits());
                Height_TextBox.Text = UnitFormatUtils.Format(_doc.GetUnits(), UnitType.UT_Length, _floorHeight, true, true);
            }
            else
            {
                TaskDialog.Show(Tools.LangResMan.GetString("floorFinishes_TaskDialogName", Tools.Cult),
                    Tools.LangResMan.GetString("floorFinishes_heightValueError", Tools.Cult), TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                this.Activate();
            }
        }
    }
}
