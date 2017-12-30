// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PRSPKT_Apps.FloorEdit
{
    /// <summary>
    /// Логика взаимодействия для FloorEditControl.xaml
    /// </summary>
    public partial class FloorEditControl : Window
    {
        LinearGradientBrush eBrush = null;
        SolidColorBrush lBrush = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(0, 0, 0, 0));

        private Document _doc;
        private UIDocument _UIDoc;
        private double userNumber;

        public double UserNumber { get => userNumber; set => userNumber = value; }

        public FloorEditControl(UIDocument UIDoc)
        {
            InitializeComponent();

            _doc = UIDoc.Document;
            _UIDoc = UIDoc;

            btn_Cancel.Text = "Отмена";
            btn_OK.Text = "OK";

        }
        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            userNumber = double.Parse(txtBox_userNumber.Text, CultureInfo.InvariantCulture);
            this.DialogResult = true;
            this.Close();
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
