// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using PRSPKT_Apps.Common;
using System.Windows;
using System.Windows.Media;

namespace Common
{
    /// <summary>
    /// Логика взаимодействия для AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        LinearGradientBrush eBrush = null;
        SolidColorBrush lBrush = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(0, 0, 0, 0));

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

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
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


        public AboutBox()
        {
            InitializeComponent();
            this.labelName.Text = MiscUtils.GetProductName();
            this.labelVersion.Text = MiscUtils.Version.ToString();
        }
    }
}
