#region Namespaces
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
using System.Windows.Media;
using System.IO;
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
                // Create the panel for PRSPKT Tools;
                RibbonPanel PRSPKTpanel = application.CreateRibbonPanel(Tools.LangResMan.GetString("roomFinishes_ribbon_panel_name", Tools.Cult));

                // Create icons in this panel
                Icons.CreateIcons(PRSPKTpanel);

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
        public static void CreateIcons(RibbonPanel PRSPKTpanel)
        {
            //Get dll assembly path
            string DllPath = Assembly.GetExecutingAssembly().Location;

            // Add PRSPKT TotalLength Button
            string ButtonText = Tools.LangResMan.GetString("totalLength_button_name", Tools.Cult);
            PushButtonData totalLengthData = new PushButtonData("cmdCurveTotalLength", ButtonText, DllPath, "TotalLength.CurveTotalLength");

            totalLengthData.ToolTip = Tools.LangResMan.GetString("totalLength_toolTip", Tools.Cult);
            totalLengthData.LargeImage = RetriveImage("PRSPKT_Apps.Resources.totalLength.png");
            PRSPKTpanel.AddItem(totalLengthData);

            // Add PRSPKT dimAxies Button
            string dimAxiesButtonText = Tools.LangResMan.GetString("dimAxies_button_name", Tools.Cult);
            PushButtonData dimAxiesData = new PushButtonData("cmdDimAxies", dimAxiesButtonText, DllPath, "DimAxies.DimAxies");
            dimAxiesData.ToolTip = Tools.LangResMan.GetString("dimAxies_toolTip", Tools.Cult);
            dimAxiesData.LargeImage = RetriveImage("PRSPKT_Apps.Resources.dimAxies.png");
            PRSPKTpanel.AddItem(dimAxiesData);

            // Add PRSPKT ApartmentCalc Button
            string ApartCalcButtonText = Tools.LangResMan.GetString("apartCalc_button_name", Tools.Cult);
            PushButtonData apartCalcData = new PushButtonData("cmdApartCalc", ApartCalcButtonText, DllPath, "ApartmentCalc.ApartmentCalc");
            apartCalcData.ToolTip = Tools.LangResMan.GetString("dimAxies_toolTip", Tools.Cult);
            apartCalcData.LargeImage = RetriveImage("PRSPKT_Apps.Resources.dimAxies.png");


            // Add PRSPKT Rename Apart Rooms Button
            string ApartRoomsButtonText = Tools.LangResMan.GetString("apartRoomRename_button_name", Tools.Cult);
            PushButtonData ApartRoomsData = new PushButtonData("cmdRenameApartRooms", ApartRoomsButtonText, DllPath, "RenameApartRooms.RenameApartRooms");
            ApartRoomsData.ToolTip = Tools.LangResMan.GetString("apartRoomRename_toolTip", Tools.Cult);
            ApartRoomsData.LargeImage = RetriveImage("PRSPKT_Apps.Resources.renameApartRooms.png");
            PRSPKTpanel.AddItem(ApartRoomsData);

            // Add PRSPKT FloorFinish button
            string FloorFinishButtonText = Tools.LangResMan.GetString("floorfinish_button_name", Tools.Cult);
            PushButtonData FloorFinishData = new PushButtonData("cmdFloorFinish", FloorFinishButtonText, DllPath, "RoomFinishes.FloorFinishes");
            FloorFinishData.ToolTip = Tools.LangResMan.GetString("apartRoomRename_toolTip", Tools.Cult);
            FloorFinishData.LargeImage = RetriveImage("PRSPKT_Apps.Resources.renameApartRooms.png");

            // Add PRSPKT RoomFinish button
            string RoomFinishesButtonText = Tools.LangResMan.GetString("roomFinishes_button_name", Tools.Cult);
            PushButtonData RoomsFinishesData = new PushButtonData("cmdRoomFinish",RoomFinishesButtonText, DllPath, "RoomFinishes.RoomsFinishes");
            RoomsFinishesData.ToolTip = Tools.LangResMan.GetString("apartRoomRename_toolTip", Tools.Cult);
            RoomsFinishesData.LargeImage = RetriveImage("PRSPKT_Apps.Resources.renameApartRooms.png");

            // Group RoomsFinishes button
            SplitButtonData sbRoomData = new SplitButtonData("room", "PRSPKT_Apps");
            SplitButton sbRoom = PRSPKTpanel.AddItem(sbRoomData) as SplitButton;
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
