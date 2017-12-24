#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
#endregion

namespace DeleteCorruptFile
{
    [Transaction(TransactionMode.Manual)]
    class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    DeleteFile(UIdoc, t);
                    return Result.Succeeded;
                }
                catch (ErrorMessageException errorException)
                {
                    message = errorException.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
                catch (Exception ex)
                {
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
            }
        }

        private void DeleteFile(UIDocument UIdoc, Transaction t)
        {
            Document _doc = UIdoc.Document;
            string _filePath = BasicFileInfo.Extract(_doc.PathName).CentralPath;
            string _filename = Path.GetFileNameWithoutExtension(_filePath);
            string _filedir = Path.GetDirectoryName(_filePath);
            string _backupDir = _filename + "_backup";
            string _corruptFileName = "corrupt";
            string fullPath = Path.Combine(_filedir, _backupDir, _corruptFileName);
            if (File.Exists(fullPath))
            {
                MessageBoxResult question = MessageBox.Show(string.Format("Удалить файл {0} ?", fullPath), "Удалить коррупцию", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (question == MessageBoxResult.OK)
                {
                    t.Start("Удалить файл Corrupt");
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (IOException exception)
                    {

                        MessageBox.Show("Ошибка", exception.Message);
                    }
                    TaskDialog.Show("Удалить файл", "Файл " + _corruptFileName + " удален.");
                    t.Commit();
                }
            }
            else
            {
                MessageBox.Show(string.Format("Файл {0} не найден по пути {1}", _corruptFileName, fullPath), "Ошибка");
            }
        }

    }
}
