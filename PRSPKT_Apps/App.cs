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
                RibbonPanel SheetPanel = application.CreateRibbonPanel(tabName, Tools.GetResourceManager("sheets_ribbon_panel_name"));


                // Create icons in this panel
                Icons.ToolsPanel(ToolsPanel);
                Icons.RoomsPanel(RoomPanel);
                Icons.SheetsPanel(SheetPanel);

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

            // Add PRSPKT Delete Corrupt File
            string DeleteFileButtonText = "Удалить Corrupt";
            PushButtonData DeleteFileData = new PushButtonData("cmdDeleteFile", DeleteFileButtonText, DllPath, "DeleteCorruptFile.Command")
            {
                ToolTip = "Попытаться удалить файл corrupt с сервера",
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/deleteCorrupt.png"))
            };
            panel.AddItem(DeleteFileData);

            // Add PRSPKT Worksets
            string objectsOnWorksetsButtonText = "Элементы в Рабочих наборах";
            PushButtonData objectsOnWorksetsData = new PushButtonData("cmdObjectsOnWorkSets", objectsOnWorksetsButtonText, DllPath, "ElementsOnWorkset.WorksetExplorer")
            {
                ToolTip = "Просмотр списка элементов (Id) в рабочих наборах",
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/objectsOnWorkset.png"))
            };
            panel.AddItem(objectsOnWorksetsData);

        }
        public static void RoomsPanel(RibbonPanel panel)
        {
            //Get dll assembly path
            //string DllPath = Assembly.GetExecutingAssembly().Location;

            // Add PRSPKT Rename Apart Rooms Button
            string ApartRoomsButtonText = "Переименовать \n помещения";
            PushButtonData ApartRoomsData = new PushButtonData("cmdRenameApartRooms", ApartRoomsButtonText, DllPath, "RenameApartRooms.RenameApartRooms")
            {
                ToolTip = Tools.LangResMan.GetString("apartRoomRename_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/renameApartRooms.png"))
            };
            panel.AddItem(ApartRoomsData);

            // Add PRSPKT Rename Apart Rooms Button
            string DoRM_ButtonText = "Rm Сергею ОВ";
            PushButtonData DoRMdata = new PushButtonData("cmdDoRM", DoRM_ButtonText, DllPath, "Do_Rm.DoRmSergei")
            {
                ToolTip = "Проставить Rm_Этаж для Сергея ОВ",
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/DoRM.png"))
            };
            panel.AddItem(DoRMdata);

            // Add PRSPKT ApartmentCalc Button OLD
            string ApartCalcButtonText = "Квартирография \n (old)";
            PushButtonData apartCalcData = new PushButtonData("cmdApartCalc", ApartCalcButtonText, DllPath, "ApartmentCalc.ApartmentCalc")
            {
                ToolTip = Tools.LangResMan.GetString("apartCalc_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/apartCalc_old.png"))
            };
            panel.AddItem(apartCalcData);


            // Add PRSPKT ApartmentCalc Button New
            string ApartCalc_P_ButtonText = "Квартирография \n (new)";
            PushButtonData apartCalc_P_Data = new PushButtonData("cmdApartCalc_P", ApartCalc_P_ButtonText, DllPath, "ApartmentCalc_P.ApartmentCalc_P")
            {
                ToolTip = Tools.LangResMan.GetString("apartCalc_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/apartCalc_new.png"))
            };
            panel.AddItem(apartCalc_P_Data);

            // Add PRSPKT FloorFinish button
            string FloorFinishButtonText = Tools.LangResMan.GetString("floorfinish_button_name", Tools.Cult);
            PushButtonData FloorFinishData = new PushButtonData("cmdFloorsFinish", FloorFinishButtonText, DllPath, "RoomsFinishes.FloorsFinishesClass")
            {
                ToolTip = Tools.LangResMan.GetString("floorfinish_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/floorFinish.png"))
            };

            // Add PRSPKT RoomFinish button
            string RoomFinishesButtonText = Tools.LangResMan.GetString("roomFinishes_button_name", Tools.Cult);
            PushButtonData RoomsFinishesData = new PushButtonData("cmdRoomFinish", RoomFinishesButtonText, DllPath, "RoomsFinishes.RoomFinishesClass")
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
        public static void SheetsPanel(RibbonPanel panel)
        {
            //Add PrintMe button
            string PrintMeButtonText = Tools.LangResMan.GetString("printme_button_name", Tools.Cult);
            PushButtonData PrintMeData = new PushButtonData("cmdPrintMe", PrintMeButtonText, DllPath, "PrintMe.PrintMe")
            {
                ToolTip = Tools.LangResMan.GetString("printme_tooltip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/printme.png"))
            };
            panel.AddItem(PrintMeData);
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
