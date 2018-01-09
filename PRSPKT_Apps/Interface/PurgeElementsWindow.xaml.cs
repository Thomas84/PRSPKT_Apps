// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* PurgeElementsWindow.xaml.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */
using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace PRSPKT_Apps.Interface
{
    /// <summary>
    /// Логика взаимодействия для PurgeElementsWindow.xaml
    /// </summary>
    public partial class PurgeElementsWindow : Window
    {
        private IEnumerable<Element> docElements;

        // wheter or not to run a purge on the matching element once the dialog closes
        public bool DoPurge = false;

        public string PurgeRegExString
        {
            get => (string) GetValue(PurgeRegExStringProperty);
            set
            {
                string oldValue = PurgeRegExString;
                SetValue(PurgeRegExStringProperty, value);
                OnPropertyChanged(
                    new DependencyPropertyChangedEventArgs(PurgeRegExStringProperty, oldValue, value));
            }
        }

        public static readonly DependencyProperty PurgeRegExStringProperty =
            DependencyProperty.Register(
                "PurgeRegExString",
                typeof(String),
                typeof(Window),
                new UIPropertyMetadata(null));

        private PurgeElementsWindow()
        {
            InitializeComponent();
        }

        public PurgeElementsWindow(Document doc, Type elementType) : this()
        {
            docElements = new FilteredElementCollector(doc).OfClass(elementType).AsEnumerable();
            MatchingElementsListBox.ItemsSource = GetMatchingDocLinePatternNames();
        }

        private IEnumerable<string> GetMatchingDocLinePatternNames()
        {
            Regex regEx;
            try
            {
                regEx = new Regex(PurgeRegExString);
                return docElements.Select(e => e.Name).Where(n => regEx.IsMatch(n));
            }
            catch
            {
                return new List<string>();
            }
        }

        private void PurgeRegExTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            IEnumerable<string> matchingLinePatterns = GetMatchingDocLinePatternNames();
            PurgeElementCountLabel.Content =
                1 == matchingLinePatterns.Count()
                    ? matchingLinePatterns.Count() + " element will be purged"
                    : matchingLinePatterns.Count() + " elements will be purged";
            MatchingElementsListBox.ItemsSource = matchingLinePatterns;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
