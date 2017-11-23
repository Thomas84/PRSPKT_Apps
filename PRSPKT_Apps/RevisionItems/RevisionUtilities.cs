using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Globalization;
using PRSPKT_Apps.Common;
using System.Collections.ObjectModel;

namespace RevisionItems
{
    public class RevisionUtilities
    {

        public static void ExportCloudInfo(Document doc, Dictionary<string, RevisionItem> dictionary, string exportFileName)
        {
            if (doc == null || dictionary == null)
            {
                TaskDialog.Show("Error", "could not export cloud information");
                return;
            }

            string ExportFileName = string.IsNullOrEmpty(exportFileName) ? @"C:\Temp\clouds" : exportFileName;
            Application excelApp;
            Worksheet excelWorksheet;
            Workbook excelWorkbook;

            excelApp = new Application()
            {
                Visible = false
            };
            excelWorkbook = (Workbook)excelApp.Workbooks.Add(Missing.Value);
            excelWorksheet = (Worksheet)excelWorkbook.ActiveSheet;

            int cloudNumber = 0;

            SortableBindingListCollection<RevisionCloudItem> allRevisionClouds = GetRevisionClouds(doc);

            string[,] data = new string[allRevisionClouds.Count +1, 8];
            data[0, 0] = "Sheet Number";
            data[0, 1] = "Revision Number";
            data[0, 2] = "Sheet Name";
            data[0, 3] = "Revision Mark";
            data[0, 4] = "Revision Comment";
            data[0, 5] = "Revision Data";
            data[0, 6] = "Revision Description";
            data[0, 7] = "GUID";

            foreach (RevisionCloudItem revCloud in allRevisionClouds)
            {
                if (dictionary.ContainsKey(revCloud.Date + revCloud.Description))
                {
                    cloudNumber++;
                    data[cloudNumber, 0] = revCloud.SheetNumber;
                    data[cloudNumber, 1] = revCloud.Revision;
                    data[cloudNumber, 2] = revCloud.SheetName;
                    data[cloudNumber, 3] = revCloud.Mark;
                    data[cloudNumber, 4] = revCloud.Comments;
                    data[cloudNumber, 5] = revCloud.Date;
                    data[cloudNumber, 6] = revCloud.Description;
                    data[cloudNumber, 7] = revCloud.Id.IntegerValue.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (cloudNumber < 1)
            {
                TaskDialog.Show("WARNING", "no clouds to export");
            }
            else
            {
                WriteArray(data, cloudNumber, 8, excelWorksheet);
                TaskDialog.Show("Finished", cloudNumber + @" revision clouds scheduled in the file " + ExportFileName);
                excelWorkbook.SaveAs(ExportFileName, XlFileFormat.xlWorkbookNormal);
                excelWorkbook.Close();
            }

        }

        private static void WriteArray(string[,] data, int rows, int columns, Worksheet excelWorksheet)
        {
            if (excelWorksheet != null)
            {
                var startCell = excelWorksheet.Cells[1, 1] as Range;
                var endCell = excelWorksheet.Cells[rows + 1, columns] as Range;
                var writeRange = excelWorksheet.Range[startCell, endCell];
                writeRange.Value2 = data;
            }
        }

        public static SortableBindingListCollection<RevisionCloudItem> GetRevisionClouds(Document doc)
        {
            var revisionClouds = new SortableBindingListCollection<RevisionCloudItem>();
            if (doc != null)
            {
                using (FilteredElementCollector fec = new FilteredElementCollector(doc))
                {
                    fec.OfCategory(BuiltInCategory.OST_RevisionClouds);
                    fec.OfClass(typeof(RevisionCloud));
                    foreach (RevisionCloud revCl in fec)
                    {
                        revisionClouds.Add(new RevisionCloudItem(doc, revCl));
                    }
                }
            }
            return revisionClouds;
        }

        public static SortableBindingListCollection<RevisionItem> GetRevisions(Document doc)
        {
            var revisions = new SortableBindingListCollection<RevisionItem>();
            using (var fec = new FilteredElementCollector(doc))
            {
                fec.OfCategory(BuiltInCategory.OST_Revisions);
                foreach (Revision rev in fec)
                {
                    if (rev.IsValidObject)
                    {
                        revisions.Add(new RevisionItem(rev));
                    }
                }
            }
            return revisions;
        }

        public static void AssignRevisionToClouds(Document doc, Collection<RevisionCloudItem> revisionClouds)
        {
            throw new NotImplementedException();
            //if (doc == null || revisionClouds == null)
            //{
            //    TaskDialog.Show("ERROR", "Could not assign revisions to clouds");
            //    return;
            //}
            //ElementId cloudId = null;
            //using (var r = PRSPKT_Apps.ExportManager.RevisionSelectionDialog(doc))
            //{
            //    var result = r.ShowDialog();
            //    if (result != DialogResult.OK)
            //    {
            //        return;
            //    }
            //    cloudId = r.Id;
            //}
            //if (cloudId == null)
            //{
            //    TaskDialog.Show("ERROR", "Selected cloud is not valid");
            //    return;
            //}
            //using (var t = new Transaction(doc, "Assign Revisions to Clouds"))
            //{
            //    t.Start();
            //    foreach (RevisionCloudItem rc in revisionClouds)
            //    {
            //        if (rc != null)
            //        {
            //            rc.SetCloudId(cloudId);
            //        }
            //    }
            //    t.Commit();
            //}
        }

        public static void DeleteRevisionClouds(Document doc, Collection<RevisionCloudItem> revisionClouds)
        {
            if (doc == null || revisionClouds == null)
            {
                TaskDialog.Show("ERROR", "Could not delete revision clouds");
                return;
            }
            using (var t = new Transaction(doc, "Deleting Revision Clouds"))
            {
                t.Start();
                foreach (RevisionCloudItem rc in revisionClouds)
                {
                    if (rc != null)
                    {
                        doc.Delete(rc.Id);
                    }
                }
                t.Commit();
            }
        }

    }
}
