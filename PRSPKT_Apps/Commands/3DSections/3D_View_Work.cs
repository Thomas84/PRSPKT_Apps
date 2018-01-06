// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* 3D_View_Work.cs
 * PRSPKT.ru
 * © PRSPKT Architects, 2017
 *
 * This file contains the methods which are used by the 
 * command.
 */
#region Namespaces

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WPF = System.Windows;

#endregion


namespace PRSPKT_Apps.Commands._3DSections
{

    public sealed partial class _3D_View
    {

        private bool DoWork(ExternalCommandData commandData,
            ref String message, ElementSet elements)
        {

            if (null == commandData)
            {

                throw new ArgumentNullException(nameof(
                    commandData));
            }

            if (null == message)
            {

                throw new ArgumentNullException(nameof(message)
                    );
            }

            if (null == elements)
            {

                throw new ArgumentNullException(nameof(elements
                    ));
            }

            UIApplication ui_app = commandData.Application;
            UIDocument ui_doc = ui_app.ActiveUIDocument;
            Application app = ui_app.Application;
            Document doc = ui_doc.Document;

            var tr_name = "Make 3D View";

            try
            {
                using (var tr = new Transaction(doc, tr_name))
                {

                    if (TransactionStatus.Started == tr.Start())
                    {

                        if (doc.ActiveView.ViewType != ViewType.FloorPlan)
                        {
                            TaskDialog.Show("Error", "Please activate a floor plan view to use this tool", TaskDialogCommonButtons.Ok);
                        }
                        else
                        {
                            var elementList = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).ToElements();
                            ElementId id;
                            try
                            {
                                foreach (Element element in elementList)
                                {
                                    ViewFamilyType viewFamilyType = element as ViewFamilyType;
                                    if (viewFamilyType.ViewFamily == ViewFamily.ThreeDimensional)
                                    {
                                        id = viewFamilyType.Id;
                                        break;
                                    }
                                }
                            }
                            finally
                            {
                                ((IDisposable)elementList).Dispose();
                            }
                        }

                        return TransactionStatus.Committed ==
                            tr.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                /* TODO: Handle the exception here if you need 
                 * or throw the exception again if you need. */
                throw ex;
            }

            return false;
        }
    }
}
