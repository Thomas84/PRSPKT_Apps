using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.DB;
using PRSPKT_Apps;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;

namespace PRSPKT_Apps
{
    public class App : IExternalApplication
    {
        static void AddRibbonPanel(UIControlledApplication application)
        {
            // Create a custom ribbon tab
            const string tabName = "PRSPKT Apps";
            application.CreateRibbonTab(tabName);

            //Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Tools");

            //Get dll assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            //Create push button for CurveTotalLength
            PushButtonData pbData = new PushButtonData(
                "cmdCurveTotalLength",
                "Общая \n длина ",
                thisAssemblyPath,
                "TotalLength.CurveTotalLength");

            PushButton pb1 = ribbonPanel.AddItem(pbData) as PushButton;
            pb1.ToolTip = "Подсчет общей длины линий";
            BitmapImage pb1Image = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/totalLength.png"));
            pb1.LargeImage = pb1Image;

            PushButtonData pbData2 = new PushButtonData(
                "cmdRenameApartRooms",
                "Переименовать \n помещения ",
                thisAssemblyPath,
                "RenameApartRooms.RenameApartRooms");

            PushButton pb2 = ribbonPanel.AddItem(pbData2) as PushButton;
            pb2.ToolTip = "Переименовать помещения квартиры на текущем виде";
            BitmapImage pb2Image = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/renameApartRooms.png"));
            pb2.LargeImage = pb2Image;

            PushButtonData pbData3 = new PushButtonData(
                "cmdDimAxies",
                "Проставить \n размеры ",
                thisAssemblyPath,
                "DimAxies.DimAxies");

            PushButton pb3 = ribbonPanel.AddItem(pbData3) as PushButton;
            pb3.ToolTip = "Проставить размеры в осях";
            BitmapImage pb3Image = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/dimAxies.png"));
            pb3.LargeImage = pb3Image;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // do nothing
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            AddRibbonPanel(application);
            return Result.Succeeded;
        }
    }
}
