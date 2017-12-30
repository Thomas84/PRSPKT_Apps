// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com


using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.IO;

namespace Utils
{
    [Transaction(TransactionMode.ReadOnly)]
    public class OpenProjectFolder : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;
            try
            {
                string userVisiblePath = string.Empty;
                if (doc.GetWorksharingCentralModelPath() != null)
                {
                    userVisiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath());
                }
                string _filePath = doc.PathName;
                string filePath = userVisiblePath == null ? _filePath : userVisiblePath;


                string _filedir = Path.GetDirectoryName(filePath);

                Process.Start(_filedir);
                return Result.Succeeded;
            }
            catch (PRSPKT_Apps.ErrorMessageException errorException)
            {
                message = errorException.Message;
                return Result.Failed;
            }
            catch (Exception ex)
            {
                message = "Неожиданная ошибка: " + ex.Message;
                return Result.Failed;
            }
        }
    }
}
