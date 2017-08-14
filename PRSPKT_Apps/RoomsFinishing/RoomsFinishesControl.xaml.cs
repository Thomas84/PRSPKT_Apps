#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
#endregion

namespace PRSPKT_Apps.RoomsFinishes
{
    /// <summary>
    /// Логика взаимодействия для RoomsFinishesControl.xaml
    /// </summary>
    public partial class RoomsFinishesControl : Window
    {
        private Document _doc;
        private UIDocument _UIDoc;

        private WallType _selectedWallType;
        public WallType SelectedWallType
        {
            get { return _selectedWallType; }
        }

        private WallType _duplicatedWallType;
        public WallType DuplicatedWallType
        {
            get { return _duplicatedWallType; }
        }

        private double _finishHeight;
        public double FinishHeight
        {
            get { return _finishHeight; }
        }

        private bool _joinWall;
        public bool JoinWall
        {
            get { return _joinWall; }
        }

        private IEnumerable<Room> _selectedRooms;
        public IEnumerable<Room> SelectedRooms
        {
            get { return _selectedRooms; }
        }

        private IEnumerable<WallType> _wallTypes;

        public RoomsFinishesControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            //Fill out Text in form
            this.Title = Tools.GetResourceManager("roomFinishes_TaskDialogName");
            this.all_rooms_radio.Content = Tools.GetResourceManager("roomFinishes_all_rooms_radio");
            this.board_height_label.Content = Tools.GetResourceManager("roomFinishes_board_height_label");
            this.select_wall_label.Content = Tools.GetResourceManager("roomFinishes_select_wall_label");
            this.selected_rooms_radio.Content = Tools.GetResourceManager("roomFinishes_selected_rooms_radio");
            this.Cancel_Button.Content = Tools.GetResourceManager("roomFinishes_Cancel_Button");
            this.Ok_Button.Content = Tools.GetResourceManager("roomFinishes_OK_Button");
            this.join_checkbox_label.Content = Tools.GetResourceManager("roomFinishes_joinWalls");
            this.Height_TextBox.Text = "2950.0";


            // Select the wall type in the document
            _wallTypes = from elem in new FilteredElementCollector(_doc).OfClass(typeof(WallType))
                         let type = elem as WallType
                         //let typeName = type.Name
                         where type.Kind == WallKind.Basic
                         //where typeName.Contains("Отделка") || typeName.Contains("(ОС-")
                         select type;

            _wallTypes = _wallTypes.OrderBy(wallType => wallType.Name);

            // Bind ArrayList with the ListBox
            WallTypeListBox.ItemsSource = _wallTypes;
            WallTypeListBox.SelectedItem = WallTypeListBox.Items[0];
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits()) != null)
            {
                _joinWall = (bool)join_checkbox.IsChecked;
                _finishHeight = (double)Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits());

                if (WallTypeListBox.SelectedItem != null)
                {
                    // Select wall type for finish walls
                    _selectedWallType = WallTypeListBox.SelectedItem as WallType;

                    // duplicate and double thickness of the wall type
                    _duplicatedWallType = CreateNewWallType(_selectedWallType);

                    this.DialogResult = true;
                    this.Close();

                    // Select the rooms
                    _selectedRooms = SelectRooms();
                }
            }
            else
            {
                TaskDialog.Show(Tools.GetResourceManager("roomFinishes_TaskDialogName"),
                    Tools.GetResourceManager("roomFinishes_heightValueError"), TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                this.Activate();
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
                        Tools.GetResourceManager("roomFinishes_SelectRooms"));
                    foreach (Reference r in refList)
                    {
                        tempList.Add(_doc.GetElement(r) as Room);
                    }
                    ModelRooms = tempList;
                }
            }

            return ModelRooms;
        }

        private WallType CreateNewWallType(WallType selectedWallType)
        {
            WallType newWallType;
            List<string> wallTypesNames = _wallTypes.Select(o => o.Name).ToList();

            if (!wallTypesNames.Contains("newWallTypeName"))
            {
                newWallType = selectedWallType.Duplicate("newWallTypeName") as WallType;
            }
            else
            {
                newWallType = selectedWallType.Duplicate("newWallTypeName2") as WallType;
            }

            CompoundStructure compoundStructure = newWallType.GetCompoundStructure();

            IList<CompoundStructureLayer> layers = compoundStructure.GetLayers();
            int layerIndex = 0;

            foreach (CompoundStructureLayer csl in layers)
            {
                double layerWidth = csl.Width * 2;

                if (compoundStructure.GetRegionsAssociatedToLayer(layerIndex).Count == 1)
                {
                    try
                    {
                        compoundStructure.SetLayerWidth(layerIndex, layerWidth);
                    }
                    catch
                    {

                        throw new ErrorMessageException(Tools.GetResourceManager("roomFinishes_verticalCompoundError"));
                    }
                }
                else
                { throw new ErrorMessageException(Tools.GetResourceManager("roomFinishes_verticalCompoundError")); }
                layerIndex++;
            }
            newWallType.SetCompoundStructure(compoundStructure);
            return newWallType;
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
                _finishHeight = (double)Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits());
                Height_TextBox.Text = UnitFormatUtils.Format(_doc.GetUnits(), UnitType.UT_Length, _finishHeight, true, true);
            }
            else
            {
                TaskDialog.Show(Tools.GetResourceManager("roomFinishes_TaskDialogName"),
                    Tools.GetResourceManager("roomFinishes_heightValueError"), TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                this.Activate();
            }
        }
    }

    internal class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Rooms) { return true; }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
