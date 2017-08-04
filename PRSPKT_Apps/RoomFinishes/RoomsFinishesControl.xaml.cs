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
using RoomFinishes;
#endregion

namespace RoomFinishes
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

        private double _boardHeight;
        public double BoardHeight
        {
            get { return _boardHeight; }
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
            this.Title = Tools.LangResMan.GetString("roomFinishes_TaskDialogName", Tools.Cult);
            this.all_rooms_radio.Content = Tools.LangResMan.GetString("roomFinishes_all_rooms_radio", Tools.Cult);
            this.board_height_label.Content = Tools.LangResMan.GetString("roomFinishes_board_height_label", Tools.Cult);
            this.select_wall_label.Content = Tools.LangResMan.GetString("roomFinishes_select_wall_label", Tools.Cult);
            this.selected_rooms_radio.Content = Tools.LangResMan.GetString("roomFinishes_selected_rooms_radio", Tools.Cult);
            this.Cancel_Button.Content = Tools.LangResMan.GetString("roomFinishes_Cancel_Button", Tools.Cult);
            this.Ok_Button.Content = Tools.LangResMan.GetString("roomFinishes_OK_Button", Tools.Cult);
            this.join_checkbox_label.Content = Tools.LangResMan.GetString("roomFinishes_joinWalls", Tools.Cult);
            this.Height_TextBox.Text = "2950.0";
        }
    }
}
