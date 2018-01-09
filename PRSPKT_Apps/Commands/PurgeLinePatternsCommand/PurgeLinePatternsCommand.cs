// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* PurgeLinePatternsCommand.cs
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


namespace PRSPKT_Apps.Commands.PurgeLinePatternsCommand
{
    /// <summary>
    /// Revit external command.
    /// </summary>	
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public sealed partial class PurgeLinePatternsCommand : IExternalCommand
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
                    PurgeLinePattern(uiDoc, t);
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

        private static void PurgeLinePattern(UIDocument uiDoc, Transaction t)
        {
            var doc = uiDoc.Document;

            IEnumerable<LinePatternElement> docLinePatternElements = new FilteredElementCollector(doc)
                .OfClass(typeof(LinePatternElement))
                .AsEnumerable()
                .Cast<LinePatternElement>();

            PurgeElementsWindow purgeWindow =
                new PurgeElementsWindow(doc, typeof(LinePatternElement))
                { PurgeRegExString = @"^IMPORT-.*$" };

            purgeWindow.ShowDialog();
            if (false == purgeWindow.DialogResult)
            {
                return;
            }

            ICollection<ElementId> patternsToDelete = new List<ElementId>();
            ElementId match;
            foreach (var patternName in purgeWindow.MatchingElementsListBox.Items)
            {
                match = docLinePatternElements
                    .FirstOrDefault(p => (string)patternName == p.Name)?.Id;
                if (null != match)
                {
                    patternsToDelete.Add(match);
                }

            }

            t.Start("Purge LinePatterns");
            doc.Delete(patternsToDelete);
            t.Commit();
        }
    }
}