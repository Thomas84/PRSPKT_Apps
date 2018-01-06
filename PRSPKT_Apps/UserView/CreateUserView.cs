// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* CreateUserView.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */

#region Namespaces

using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.UserView;
using View = Autodesk.Revit.DB.View;

// ReSharper disable All

#endregion

namespace UserView
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CreateUserView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                return Result.Failed;
            }

            var uiApp = commandData.Application;
            var uiDoc = uiApp?.ActiveUIDocument;
            var doc = uiDoc?.Document;
            if (doc != null)
            {
                var view = doc.ActiveView;
                var PartName = "";
                var Description = "";
                var newUserViews = new List<View>();

                using (var t = new Transaction(doc))
                {
                    if (t.Start("Дубликат вида") == TransactionStatus.Started)
                    {
                        GetNameUserForm dialog = new GetNameUserForm();
                        dialog.InitializeComponent();
                        dialog.ShowDialog();
                        if (dialog.DialogResult == true)
                        {
                            PartName = dialog.PartName;
                            Description = dialog.Description;
                            newUserViews = UserView.Create(uiDoc, view, PartName, Description);
                        }
                        else
                        {
                            newUserViews = UserView.Create(uiDoc, view);
                        }

                        if (t.Commit() != TransactionStatus.Committed)
                        {
                            TaskDialog.Show("Создать виды", "Сведения о созданном(ых) виде(ах)");
                        }
                    }

                    if (newUserViews == null || newUserViews.Count > 0)
                    {
                        UserView.ShowSummaryDialog(newUserViews);
                        if (newUserViews != null)
                        {
                            var uiapp = new UIApplication(doc.Application);
                            uiapp.ActiveUIDocument.ActiveView = newUserViews[0];
                        }
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}