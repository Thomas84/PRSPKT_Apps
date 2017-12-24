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
                string dllPath = new Uri(Assembly.GetAssembly(typeof(App)).CodeBase).LocalPath;

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
                Icons.RoomsPanel(RoomPanel);
                ToolsPanel.AddItem(GetTotalLength(dllPath));
                ToolsPanel.AddItem(GetDimensionAxies(dllPath));
                ToolsPanel.AddItem(DeleteCorruptFile(dllPath));
                ToolsPanel.AddItem(GetElementsByWorksets(dllPath));
                ToolsPanel.AddItem(FloorEditButton(dllPath));
                SheetPanel.AddItem(ElementInfo(dllPath));
                SheetPanel.AddItem(PrintMe(dllPath));

                return Result.Succeeded;
            }
            catch
            {
                // Return Failure
                return Result.Failed;
            }
        }

        private static PushButtonData ElementInfo(string dll)
        {
            var ElementInfoButtonText = Tools.GetResourceManager("element_info");
            var pbd = new PushButtonData(
                "cmdElementInfo", ElementInfoButtonText, dll, "PRSPKT_Apps.ElementInfo");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.information.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("element_info_tooltip");
            return pbd;
        }

        private static PushButtonData PrintMe(string dll)
        {
            var ElementInfoButtonText = Tools.GetResourceManager("printme_button_name");
            var pbd = new PushButtonData(
                "cmdPrintMe", ElementInfoButtonText, dll, "PrintMe.PrintMe");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.printme.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("printme_tooltip");
            return pbd;
        }

        private static PushButtonData GetTotalLength(string dll)
        {
            var ElementInfoButtonText = Tools.GetResourceManager("totalLength_button_name");
            var pbd = new PushButtonData("cmdCurveTotalLength", ElementInfoButtonText, dll, "TotalLength.CurveTotalLength");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.totalLength.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("totalLength_toolTip");
            return pbd;
        }

        private static PushButtonData GetDimensionAxies(string dll)
        {
            var ElementInfoButtonText = Tools.GetResourceManager("dimAxies_button_name");
            var pbd = new PushButtonData("cmdDimAxies", ElementInfoButtonText, dll, "DimAxies.DimAxies");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.dimAxies.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("dimAxies_toolTip");
            return pbd;
        }

        private static PushButtonData DeleteCorruptFile(string dll)
        {
            var ElementInfoButtonText = "Удалить Corrupt";
            var pbd = new PushButtonData("cmdDeleteFile", ElementInfoButtonText, dll, "DeleteCorruptFile.Command");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.deleteCorrupt.png", 32, dll);
            pbd.ToolTip = "Попытаться удалить файл corrupt с сервера";
            return pbd;
        }

        private static PushButtonData GetElementsByWorksets(string dll)
        {
            var ElementInfoButtonText = "Рабочие Наборы";
            var pbd = new PushButtonData("cmdObjectsOnWorkSets", ElementInfoButtonText, dll, "ElementsOnWorkset.WorksetExplorer");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.objectsOnWorkset.png", 32, dll);
            pbd.ToolTip = "Просмотр списка элементов (Id) в рабочих наборах";
            return pbd;
        }

        private static PushButtonData FloorEditButton(string dll)
        {
            var ElementInfoButtonText = "Уклоны";
            var pbd = new PushButtonData("cmdFloorEdit", ElementInfoButtonText, dll, "FloorEdit.Main");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.objectsOnWorkset.png", 32, dll);
            pbd.ToolTip = "Редактирование уклона";
            return pbd;
        }

        private static void AssignPushButtonImage(ButtonData pushButtonData, string iconName, int size, string dll)
        {
            if (size == -1)
            {
                size = 32;
            }
            ImageSource image = LoadPNGImageSource(iconName, dll);
            if (image != null && pushButtonData != null)
            {
                if (size == 32)
                {
                    pushButtonData.LargeImage = image;
                }
                else
                {
                    pushButtonData.Image = image;
                }
            }
        }

        private static ImageSource LoadPNGImageSource(string sourceName, string path)
        {
            try
            {
                Assembly m_assembly = Assembly.LoadFrom(Path.Combine(path));
                Stream m_icon = m_assembly.GetManifestResourceStream(sourceName);
                PngBitmapDecoder m_decoder = new PngBitmapDecoder(m_icon, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                ImageSource m_source = m_decoder.Frames[0];
                return m_source;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }

    class Icons
    {
        static string DllPath = Assembly.GetExecutingAssembly().Location;

        public static void RoomsPanel(RibbonPanel panel)
        {
            // Add PRSPKT Rename Apart Rooms Button
            string ApartRoomsButtonText = "Переименовать \n помещения";
            PushButtonData ApartRoomsData = new PushButtonData("cmdRenameApartRooms", ApartRoomsButtonText, DllPath, "RenameApartRooms.RenameApartRooms")
            {
                ToolTip = Tools.GetResourceManager("apartRoomRename_toolTip"),
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

            // Add PRSPKT ApartmentCalc Button New
            string ApartCalc_P_ButtonText = "Квартирография";
            PushButtonData apartCalc_P_Data = new PushButtonData("cmdApartCalc_P", ApartCalc_P_ButtonText, DllPath, "ApartmentCalc_P.ApartmentCalc_P")
            {
                ToolTip = Tools.GetResourceManager("apartCalc_toolTip"),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/apartCalc_new.png"))
            };
            panel.AddItem(apartCalc_P_Data);

            // Add PRSPKT FloorFinish button
            string FloorFinishButtonText = Tools.GetResourceManager("floorfinish_button_name");
            PushButtonData FloorFinishData = new PushButtonData("cmdFloorsFinish", FloorFinishButtonText, DllPath, "RoomsFinishes.FloorsFinishesClass")
            {
                ToolTip = Tools.LangResMan.GetString("floorfinish_toolTip", Tools.Cult),
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/floorFinish.png"))
            };

            // Add PRSPKT RoomFinish button
            string RoomFinishesButtonText = Tools.GetResourceManager("roomFinishes_button_name");
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
    }
}
