// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* GetWindowFromLegendCommand.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */

#region Namespaces

using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WPF = System.Windows;
using System.Linq;

#endregion


namespace PRSPKT_Apps.Commands.GetFromLegend
{
    /// <inheritdoc />
    /// <summary>
    /// Revit external command.
    /// </summary>	
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class GetWindowFromLegendCommand : IExternalCommand
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
                    CmdGetWindow(uiDoc, t);
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

        private static void CmdGetWindow(UIDocument uiDoc, Transaction t)
        {
            var doc = uiDoc.Document;
            t.Start("Get Window from Legend");

            var selection = doc.GetElement(uiDoc.Selection.GetElementIds().FirstOrDefault());
            var getFamilyId = selection.get_Parameter(BuiltInParameter.LEGEND_COMPONENT).AsElementId();
            var instancesList = new List<ElementId>();

            var categories = new List<BuiltInCategory> {BuiltInCategory.OST_Windows, BuiltInCategory.OST_Doors};
            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(categories);
            

            var elementList = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .WherePasses(elementMulticategoryFilter)
                .Cast<FamilyInstance>()
                .ToList();

            foreach (var element in elementList)
            {
                var typeId = element.GetTypeId();
                if (typeId == getFamilyId)
                {
                    instancesList.Add(element.Id);
                }
            }
            uiDoc.Selection.SetElementIds(instancesList);

            t.Commit();
        }
        
    }
    
}