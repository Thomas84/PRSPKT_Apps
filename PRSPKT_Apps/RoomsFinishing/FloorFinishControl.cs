using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PRSPKT_Apps;

namespace PRSPKT_Apps.RoomsFinishing
{
    public partial class FloorFinishControl : System.Windows.Forms.Form
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



        public FloorFinishControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            // FIll out Text in form

            this.allrooms_radio.Text = Tools.GetResourceManager("floorFinishes_all_rooms_radio");
            this.floorheight_radio.Text = Tools.GetResourceManager("floorFinishes_height_label");
            this.heightparam_radio.Text = Tools.GetResourceManager("floorFinishes_height_param_label");
            this.groupBoxName.Text = Tools.GetResourceManager("floorFinishes_groupboxName");
            this.selectfloor_label.Text = Tools.GetResourceManager("floorFinishes_select_floor_label");
            this.selectedrooms_radio.Text = Tools.GetResourceManager("floorFinishes_SelectedRooms");
            this.OKButton.Text = Tools.GetResourceManager("roomFinishes_OK_Button");
            this.CancelButton.Text = Tools.GetResourceManager("roomFinishes_Cancel_Button");

            // Select the floor type in the document
            IEnumerable<FloorType> floorTypes = from elem in new FilteredElementCollector(_doc).OfClass(typeof(FloorType))
                                           let type = elem as FloorType
                                           let typeName = type.Name
                                           where type.IsFoundationSlab == false
                                           //orderby typeName
                                           //where typeName.Contains("Пол")
                                           select type;
            floorTypes = floorTypes.OrderBy(floorType => floorType.Name);

            // Bind ArrayList with the ListBox
            foreach (FloorType floortype in floorTypes)
            {
                FloorTypeListBox.Items.Add(floortype);
            }
            //FloorTypeListBox.DataSource = floorTypes;
            //FloorTypeListBox.DisplayMember = 
            FloorTypeListBox.SelectedItem = FloorTypeListBox.Items[0];

            // Find a room
            IList<Element> roomList = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Rooms).ToList();

            if (roomList.Count != 0)
            {
                // Get all double parameters
                Room room = roomList.First() as Room;

                IEnumerable<Parameter> doubleParam = from Parameter p in room.Parameters
                                                where p.Definition.ParameterType == ParameterType.Length
                                                select p;

                foreach (Parameter parameter in doubleParam)
                {
                    paramSelector.Items.Add(parameter.Definition.Name);
                }
                //paramSelector.DataSource = doubleParam;
                paramSelector.DisplayMember = "Definition.Name";
                paramSelector.SelectedIndex = 0;
            }
            else
            {
                paramSelector.Enabled = false;
                heightparam_radio.Enabled = false;
            }
        }

        private void Ok_Button_Click(object sender, EventArgs e)
        {
            if (floorheight_radio.Checked == true)
            {
                _roomParameter = null;
                if (Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits()) != null)
                {
                    _floorHeight = (double)Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits());
                    if (FloorTypeListBox.SelectedItem != null)
                    {
                        // Select floor type
                        _selectedFloorType = FloorTypeListBox.SelectedItem as FloorType;
                        this.DialogResult = DialogResult.OK;
                        this.Close();

                        // Select the rooms
                        _selectedRooms = SelectRooms();
                    }
                }
                else
                {
                    TaskDialog.Show(Tools.GetResourceManager("floorFinishes_TaskDialogName"),
                        Tools.GetResourceManager("floorFinishes_heightValueError"), TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                    this.Activate();
                }
            }
            else
            {
                _roomParameter = paramSelector.SelectedItem as Parameter;
                if (FloorTypeListBox.SelectedItem != null)
                {
                    _selectedFloorType = FloorTypeListBox.SelectedItem as FloorType;
                    this.DialogResult = DialogResult.OK;
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

            if (allrooms_radio.Checked)
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

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void Height_Text_LostFocus(object sender, EventArgs e)
        {
            if (Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits()) != null)
            {
                _floorHeight = (double)Tools.GetValueFromString(Height_TextBox.Text, _doc.GetUnits());
                Height_TextBox.Text = UnitFormatUtils.Format(_doc.GetUnits(), UnitType.UT_Length, _floorHeight, true, true);
            }
            else
            {
                TaskDialog.Show(Tools.GetResourceManager("floorFinishes_TaskDialogName"),
                    Tools.GetResourceManager("floorFinishes_heightValueError"));
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
