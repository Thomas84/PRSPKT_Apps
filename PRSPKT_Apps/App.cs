#region Namespaces
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endregion

namespace PRSPKT_Apps
{
    class App : IExternalApplication
    {
        /*
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
        */

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Define localisation values
                Tools.GetLocalisationValues();

                // Create a custom ribbon tab
                const string tabName = "PRSPKT Apps";
                application.CreateRibbonTab(tabName);

                // Create the panel for PRSPKT Tools;
                RibbonPanel RoomPanel = application.CreateRibbonPanel(tabName, Tools.GetResourceManager("roomFinishes_ribbon_panel_name"));
                RibbonPanel ToolsPanel = application.CreateRibbonPanel(tabName, Tools.GetResourceManager("tools_ribbon_panel_name"));

                // Create icons in this panel
                Icons.ToolsPanel(ToolsPanel);
                Icons.RoomsPanel(RoomPanel);

                return Result.Succeeded;
            }
            catch
            {
                // Return Failure
                return Result.Failed;
            }

        }
    }

    class Icons
    {
        static string DllPath = Assembly.GetExecutingAssembly().Location;
        public static void ToolsPanel(RibbonPanel panel)
        {
            // Add PRSPKT TotalLength Button
            string ButtonText = Tools.LangResMan.GetString("totalLength_button_name", Tools.Cult);
            PushButtonData totalLengthData = new PushButtonData("cmdCurveTotalLength", ButtonText, DllPath, "TotalLength.CurveTotalLength")
            {
                ToolTip = Tools.LangResMan.GetString("totalLength_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/totalLength.png"))
            };
            panel.AddItem(totalLengthData);

            // Add PRSPKT dimAxies Button
            string dimAxiesButtonText = Tools.LangResMan.GetString("dimAxies_button_name", Tools.Cult);
            PushButtonData dimAxiesData = new PushButtonData("cmdDimAxies", dimAxiesButtonText, DllPath, "DimAxies.DimAxies")
            {
                ToolTip = Tools.LangResMan.GetString("dimAxies_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/dimAxies.png"))
            };
            panel.AddItem(dimAxiesData);
        }
        public static void RoomsPanel(RibbonPanel panel)
        {
            //Get dll assembly path
            //string DllPath = Assembly.GetExecutingAssembly().Location;

            // Add PRSPKT Rename Apart Rooms Button
            string ApartRoomsButtonText = Tools.LangResMan.GetString("apartRoomRename_button_name", Tools.Cult);
            PushButtonData ApartRoomsData = new PushButtonData("cmdRenameApartRooms", ApartRoomsButtonText, DllPath, "RenameApartRooms.RenameApartRooms")
            {
                ToolTip = Tools.LangResMan.GetString("apartRoomRename_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/renameApartRooms.png"))
            };
            panel.AddItem(ApartRoomsData);

            // Add PRSPKT ApartmentCalc Button
            string ApartCalcButtonText = Tools.LangResMan.GetString("apartCalc_button_name", Tools.Cult);
            PushButtonData apartCalcData = new PushButtonData("cmdApartCalc", ApartCalcButtonText, DllPath, "ApartmentCalc.ApartmentCalc")
            {
                ToolTip = Tools.LangResMan.GetString("apartCalc_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/test_OK.png"))
            };
            panel.AddItem(apartCalcData);



            // Add PRSPKT FloorFinish button
            string FloorFinishButtonText = Tools.LangResMan.GetString("floorfinish_button_name", Tools.Cult);
            PushButtonData FloorFinishData = new PushButtonData("cmdFloorsFinish", FloorFinishButtonText, DllPath, "PRSPKT_Apps.RoomsFinishes.FloorsFinishesClass")
            {
                ToolTip = Tools.LangResMan.GetString("floorfinish_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/floorFinish.png"))
            };

            // Add PRSPKT RoomFinish button
            string RoomFinishesButtonText = Tools.LangResMan.GetString("roomFinishes_button_name", Tools.Cult);
            PushButtonData RoomsFinishesData = new PushButtonData("cmdRoomFinish", RoomFinishesButtonText, DllPath, "PRSPKT_Apps.RoomsFinishes.RoomFinishesClass")
            {
                ToolTip = Tools.LangResMan.GetString("roomFinishes_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/roomFinish.png"))
            };

            // Group RoomsFinishes button
            SplitButtonData sbRoomData = new SplitButtonData("room", "PRSPKT_Apps");
            SplitButton sbRoom = panel.AddItem(sbRoomData) as SplitButton;
            sbRoom.AddPushButton(FloorFinishData);
            sbRoom.AddPushButton(RoomsFinishesData);

        }

        private static ImageSource RetriveImage(string imagePath)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(imagePath);

            switch (imagePath.Substring(imagePath.Length - 3))
            {
                case "jpg":
                    var jpgDecoder = new System.Windows.Media.Imaging.JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return jpgDecoder.Frames[0];
                case "bmp":
                    var bmpDecoder = new System.Windows.Media.Imaging.BmpBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return bmpDecoder.Frames[0];
                case "png":
                    var pngDecoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return pngDecoder.Frames[0];
                case "ico":
                    var icoDecoder = new System.Windows.Media.Imaging.IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return icoDecoder.Frames[0];
                default:
                    return null;
            }
        }
    }
}
