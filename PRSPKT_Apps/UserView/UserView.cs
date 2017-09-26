using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.UserView
{
    /// <summary>
    /// Copy a view, give it a user name, remove any view templates and
    /// categorize it nicely
    /// </summary>
    public static class UserView
    {
        public static bool Create(View sourceView, Document doc)
        {
            if (sourceView == null || doc == null)
            {
                return false;
            }
            if (sourceView.ViewType == ViewType.DrawingSheet)
            {
                return true;
            }

            if (ValidViewType(sourceView.ViewType))
            {
                return CreateView(sourceView, doc);
            }

            ShowErrorDialog(sourceView);
            return false;
        }

        private static bool CreateView(View sourceView, Document doc)
        {
            ElementId destViewId = sourceView.Duplicate(ViewDuplicateOption.Duplicate);
            var newView = doc.GetElement(destViewId) as View;
            newView.Name = GetNewViewName(doc, sourceView);
            newView.ViewTemplateId = ElementId.InvalidElementId;
            var p = newView.GetParameters("Подкатегория");
            if (p.Count < 1)
            {
                return true;
            }
            Parameter param = p[0];
            if (param == null)
            {
                return true;
            }
            if (param.IsReadOnly)
            {
                TaskDialog.Show("Error", "Подкатегория только для чтения!");
                return false;
            }
            else
            {
                if (!param.Set("Пользовательский"))
                {
                    TaskDialog.Show("Error", "Ошибка в настройках параметра Подкатегории");
                    return false;
                }
            }
            return true;
        }

        private static bool ValidViewType(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.FloorPlan:
                case ViewType.Elevation:
                case ViewType.CeilingPlan:
                case ViewType.Section:
                case ViewType.AreaPlan:
                case ViewType.ThreeD:
                    return true;
            }
            return false;
        }

        private static void ShowErrorDialog(View sourceView)
        {
            if (sourceView == null)
            {
                return;
            }
            using (var td = new TaskDialog("Create User View"))
            {
                td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                td.MainInstruction = "Error creating user view for view:";
                td.MainContent = sourceView.Name;
                td.Show();
            }
        }

        public static void Create(ICollection<PRSPKT_Apps.ExportManager.ExportSheet> sheets, Document doc)
        {
            string message = "";
            if (sheets == null || doc == null)
            {
                message += "Could not create user view";
            }
            else
            {
                using (var t = new Transaction(doc, "Copy User Views"))
                {
                    if (t.Start() == TransactionStatus.Started)
                    {
                        foreach (PRSPKT_Apps.ExportManager.ExportSheets sheet in sheets)
                        {
                            message += Create(sheet.Sheet, doc);
                        }
                        t.Commit();
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Could not start user view transaction");
                    }
                }
            }
            ShowSummaryDialog(message);
        }

        private static void ShowSummaryDialog(string message)
        {
            using (var td = new TaskDialog("Create User Views"))
            {
                td.MainIcon = TaskDialogIcon.TaskDialogIconNone;
                td.MainInstruction = "Summary of users view created:";
                td.MainContent = message;
                td.Show();
            }
        }

        public static string GetNewViewName(Document doc, Element sourceView)
        {
            if (doc == null || sourceView == null)
            {
                return string.Empty;
            }
            string name = sourceView.Name;

            name = name.Replace(@"{", string.Empty).Replace(@"}", string.Empty);
            name = Environment.UserName + "-" + name + "-" + MiscUtils.GetDateString;
            return MiscUtils.GetNiceViewName(doc, name);
        }
    }
}
