using Autodesk.Revit.DB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using RevisionItems;

namespace PRSPKT_Apps.RevisionItems
{
    /// <summary>
    /// Логика взаимодействия для RevisionUserControl.xaml
    /// </summary>
    public partial class RevisionUserControl : Window
    {
        private Document doc;

        public RevisionUserControl(Document doc)
        {
            this.doc = doc;
            InitializeComponent();
            RefreshDataGridList();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            DataGridList.SelectAll();
        }


        private void btnSelectNone_Click(object sender, RoutedEventArgs e)
        {
            DataGridList.UnselectAll();
        }

        private void btnDeleteCloud_Click(object sender, RoutedEventArgs e)
        {
            RevisionUtilities.DeleteRevisionClouds(doc, SelectedRevisionCloudItems());
            RefreshDataGridList();
        }

        private void btnAssignCloud_Click(object sender, RoutedEventArgs e)
        {
            RevisionUtilities.AssignRevisionToClouds(doc, SelectedRevisionCloudItems());
            RefreshDataGridList();
        }

        private void btnExportSchedule_Click(object sender, RoutedEventArgs e)
        {
            var dict = new Dictionary<string, RevisionItem>();
            foreach (RevisionItem rev in SelectedRevisionItems())
            {
                if (rev != null)
                {
                    string s = rev.Date + rev.Description;
                    if (!dict.ContainsKey(s))
                    {
                        dict.Add(s, rev);
                    }
                }
            }

            SaveFileDialog SaveFileDialog = new SaveFileDialog();
            var fileDialogResult = SaveFileDialog.ShowDialog();

            var saveFileName = "";
            if (fileDialogResult == true)
            {
                saveFileName = SaveFileDialog.FileName;
            }
            RevisionUtilities.ExportCloudInfo(doc, dict, saveFileName);
        }

        private Collection<RevisionCloudItem> SelectedRevisionCloudItems()
        {
            Collection<RevisionCloudItem> cloudSelection = new Collection<RevisionCloudItem>();
            foreach (DataGridRow row in DataGridList.Items)
            {
                RevisionCloudItem rev = row.Item as RevisionCloudItem;
                if (rev.Export)
                {
                    cloudSelection.Add(rev);
                }
            }
            return cloudSelection;
        }

        private Collection<RevisionItem> SelectedRevisionItems()
        {
            Collection<RevisionItem> revSelection = new Collection<RevisionItem>();
            foreach (DataGridRow row in DataGridList.Items)
            {
                RevisionItem rev = row.Item as RevisionItem;
                if (rev.Export)
                {
                    revSelection.Add(rev);
                }
            }

            return revSelection;
        }

        private void RefreshDataGridList()
        {
            if (rbRevisions.IsChecked == true)
            {
                DataGridList.ItemsSource = RevisionUtilities.GetRevisions(doc);
            }
            else
            {
                DataGridList.ItemsSource = RevisionUtilities.GetRevisionClouds(doc);
                AdjustColumnOrder();
            }
        }

        private void AdjustColumnOrder()
        {
            DataGridList.Columns[0].Header = "Экспорт";
            DataGridList.Columns[1].Header = "Пояснение";
            DataGridList.Columns[2].Header = "Дата";
            DataGridList.Columns[3].Header = "Номер листа";
            DataGridList.Columns[4].Header = "ИЗМ";
            DataGridList.Columns[5].Header = "Имя листа";
            DataGridList.Columns[6].Header = "Имя вида";
            DataGridList.Columns[7].Header = "Марка";
            DataGridList.Columns[8].Header = "Комментарии";
            DataGridList.Columns[9].Header = "ID";
            DataGridList.Columns[10].Header = "Сек";
            DataGridList.Columns[11].Header = "Статус";
        }


    }
}
