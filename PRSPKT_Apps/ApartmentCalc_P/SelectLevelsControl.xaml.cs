
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

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

        private IList<Level> _selectedLevels;
        public IList<Level> SelectedLevels
        {
            get { return _selectedLevels; }
        }

        public SelectLevelsControl(UIDocument UIDoc)
        {
            InitializeComponent();
            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            // Find a room
            //IList<Room> roomList = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>().ToList();

            LogoName2.Content = HelpMe.GetVersion();

            _levels = new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(lev => lev.Elevation);

            LevelsListBox.ItemsSource = _levels;
            LevelsListBox.DisplayMemberPath = "Name";
        }


        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            if (LevelsListBox.SelectedItems.Count > 0)
            {
                _selectedLevels = LevelsListBox.SelectedItems.Cast<Level>().ToList();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                TaskDialog.Show("Квартирография", "Ошибочка с выбором уровней", TaskDialogCommonButtons.Close, TaskDialogResult.Close);
                this.Activate();
            }
        }
  
        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}