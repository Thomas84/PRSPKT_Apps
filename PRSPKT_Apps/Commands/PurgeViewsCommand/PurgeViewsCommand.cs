// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* PurgeViewsCommand.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */

#region Namespaces

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

#endregion


namespace PRSPKT_Apps.Commands.PurgeViewsCommand
{
    /// <summary>
    /// Revit external command.
    /// </summary>	
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public sealed partial class PurgeViewsCommand : IExternalCommand
    {
        /// <summary>
        /// This method implements the external command within 
        /// Revit.
        /// </summary>
        /// <param name="commandData">An ExternalCommandData 
        /// object which contains reference to Application and 
        /// View needed by external command.</param>
        /// <param name="message">Error message can be returned
        /// by external command. This will be displayed only if
        /// the command status was "Failed". There is a limit 
        /// of 1023 characters for this message; strings longer
        /// than this will be truncated.</param>
        /// <param name="elements">Element set indicating 
        /// problem elements to display in the failure dialog. 
        /// This will be used only if the command status was 
        /// "Failed".</param>
        /// <returns>The result indicates if the execution 
        /// fails, succeeds, or was canceled by user. If it 
        /// does not succeed, Revit will undo any changes made 
        /// by the external command.</returns>	
        public Result Execute
        (
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            Result result = Result.Failed;

            if (commandData == null)
            {
                result = Result.Failed;
            }

            UIApplication uiApp = commandData?.Application;
            UIDocument uiDoc = uiApp?.ActiveUIDocument;
            Document doc = uiDoc?.Document;

            using (var t = new Transaction(doc))
            {
                try
                {
                    PurgeViews(uiDoc, t);
                    result = Result.Succeeded;
                }
                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }

                    result = Result.Cancelled;
                }
                catch (ErrorMessageException errorException)
                {
                    message = errorException.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }

                    result = Result.Failed;
                }
                catch (Exception ex)
                {
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }

                    result = Result.Failed;
                }
            }

            return result;

        }

        private void PurgeViews(UIDocument uiDoc, Transaction t)
        {
            var doc = uiDoc.Document;
            IEnumerable<View> docViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .AsEnumerable()
                .Cast<View>();

            PurgeElementsWindow purgeWindow = new PurgeElementsWindow(doc, typeof(View))
            {
                PurgeRegExString =
                    @"(^Фасад ([1-9][0-9]|[1-9]) - [a-d]$)|(^3D вид ([1-9][0-9]|[1-9])$)|(^Разрез ([1-9][0-9]|[1-9])$)|(^Чертеж [1-9]?[0-9]$)|(^Узел [1-9]?[0-9]$)|(^Фрагмент of .*)|(^Копия .*)"
            };
            purgeWindow.ShowDialog();
            if (false == purgeWindow.DialogResult)
            {
                return;
            }

            ICollection<ElementId> viewsToDelete = new List<ElementId>();
            ElementId match;
            foreach (var viewName in purgeWindow.MatchingElementsListBox.Items)
            {
                match = docViews
                    .Where(v => (string)viewName == v.Name)
                    .Select(v => v.Id)
                    .FirstOrDefault();
                if (null != match)
                {
                    viewsToDelete.Add(match);
                }
            }

            t.Start("Purge Views");
            doc.Delete(viewsToDelete);
            t.Commit();
        }
    }
}