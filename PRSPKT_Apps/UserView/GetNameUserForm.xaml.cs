using System.Windows;
using System.Windows.Media;
using Autodesk.Revit.UI;

namespace PRSPKT_Apps.UserView
{

    /// <summary>
    /// Логика взаимодействия для GetNameUserForm.xaml
    /// </summary>
    public partial class GetNameUserForm : Window
    {
        public string PartName { get; set; }
        public string Description { get; set; }

        LinearGradientBrush eBrush = null;
        SolidColorBrush lBrush = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(0, 0, 0, 0));

        public GetNameUserForm()
        {
            InitializeComponent();
            PartName = "";
            Description = "";
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

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void OK_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (eBrush == null)
            {
                eBrush = EnterBrush();
            }
            OkRect.Fill = eBrush;
        }

        private void OK_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            OkRect.Fill = lBrush;
        }

        private void Cancel_Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (eBrush == null)
            {
                eBrush = EnterBrush();
            }
            CancelRect.Fill = eBrush;
        }

        private void Cancel_Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CancelRect.Fill = lBrush;
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            PartName = TxtBoxTo.Text;
            Description = TxtBoxFrom.Text;
            if (string.IsNullOrWhiteSpace(PartName) || string.IsNullOrWhiteSpace(Description))
            {
                TaskDialog.Show("Ошибка", "Заполните, пожалуйста, все поля", TaskDialogCommonButtons.Ok, TaskDialogResult.Ok);
                this.Activate();
            }
            DialogResult = true;
            this.Close();
        }
    }
}
