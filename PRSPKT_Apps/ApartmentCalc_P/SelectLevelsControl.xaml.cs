
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    /// <summary>
    /// Логика взаимодействия для SelectLevels.xaml
    /// </summary>
    public partial class SelectLevelsControl : Window
    {
        LinearGradientBrush eBrush = null;
        SolidColorBrush lBrush = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(0, 0, 0, 0));

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

        private LinearGradientBrush EnterBrush()
        {
            LinearGradientBrush b = new LinearGradientBrush()
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 1)
            };
            b.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 195, 195, 195), 0.0));
            b.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(255, 245, 245, 245), 1.0));
            return b;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OK_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (eBrush == null)
            {
                eBrush = EnterBrush();
            }
            okRect.Fill = eBrush;
        }

        private void OK_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            okRect.Fill = lBrush;
        }

        private void Cancel_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (eBrush == null)
            {
                eBrush = EnterBrush();
            }
            cancelRect.Fill = eBrush;
        }

        private void Cancel_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            cancelRect.Fill = lBrush;
        }
    }
}