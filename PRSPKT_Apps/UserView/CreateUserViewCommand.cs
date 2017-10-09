using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PRSPKT_Apps.UserView
{
    public class CreateUserViewCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                return Result.Failed;
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            View view = doc.ActiveView;

            using (var t = new Transaction(doc))
            {
                if (t.Start("Создание пользовательского вида") == TransactionStatus.Started)
                {
                    if (UserView.Create(view, doc))
                    {
                        UserView.ShowSummaryDialog(UserView.GetNewViewName(doc, view));
                    }
                    else
                    {
                        UserView.ShowErrorDialog(view);
                    }
                    if (t.Commit() != TransactionStatus.Committed)
                    {
                        TaskDialog.Show("Failed", "Невозможно создать пользовательский вид(ы)");
                    }
                }
            }
            return Result.Succeeded;
        }
    }
}
