using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRSPKT_Apps;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.Attributes;

namespace RoomFinishes
{
    [Transaction(TransactionMode.Manual)]
    public class RoomsFinishes : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // Subscribe to the FailureProcessing Event
            uiApp.Application.FailuresProcessing += new EventHandler<Autodesk.Revit.DB.Events.FailuresProcessingEventArgs>(FailuresProcessing);
            return Result.Succeeded;
        }

        private void FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            String transactionName = failuresAccessor.GetTransactionName();
            IList<FailureMessageAccessor> failures = failuresAccessor.GetFailureMessages();

            if (failures.Count != 0)
            {
                foreach (FailureMessageAccessor f in failures)
                {
                    FailureDefinitionId id = f.GetFailureDefinitionId();

                    if (id == BuiltInFailures.JoinElementsFailures.CannotJoinElementsError)
                    {
                        failuresAccessor.ResolveFailure(f);
                        e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
                    }

                    return;
                }
            }
        }
    }
}
