﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PRSPKT_Apps;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PRSPKT_Apps.ApartmentCalc
{
    /// <summary>
    /// Логика взаимодействия для Levels.xaml
    /// </summary>
    public partial class LevelsForm : Window
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


        public LevelsForm(UIDocument UIDoc)
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

            Levels_ComboBox.ItemsSource = _levels;
            Levels_ComboBox.SelectedItem = Levels_ComboBox.Items[0];
            Levels_ComboBox.DisplayMemberPath = "Name";
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Levels_ComboBox.SelectedItem != null)
            {
                _selectedLevel = Levels_ComboBox.SelectedItem as Level;
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
                    .Where(room => room.Name == _selectedLevel.Name)
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