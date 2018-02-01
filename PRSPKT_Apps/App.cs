// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

#region Namespaces

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

#endregion

namespace PRSPKT_Apps
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                var dllPath = new Uri(Assembly.GetAssembly(typeof(App)).CodeBase).LocalPath;

                // Define localisation values
                Tools.GetLocalisationValues();

                // Create a custom ribbon tab
                const string tabName = "PRSPKT Apps";
                application.CreateRibbonTab(tabName);
                // Create the panel for PRSPKT Tools;
                var aboutPanel = application.CreateRibbonPanel(tabName, "About");
                var roomPanel = application.CreateRibbonPanel(tabName,
                    Tools.GetResourceManager("roomFinishes_ribbon_panel_name"));
                var toolsPanel =
                    application.CreateRibbonPanel(tabName, Tools.GetResourceManager("tools_ribbon_panel_name"));
                var sheetPanel =
                    application.CreateRibbonPanel(tabName, Tools.GetResourceManager("sheets_ribbon_panel_name"));
                var cleanUpToolsPanel =
                    application.CreateRibbonPanel(tabName, "Зачистка");


                // Create icons in this panel
                aboutPanel.AddItem(AboutBox(dllPath));

                Icons.RoomsPanel(roomPanel);

                toolsPanel.AddItem(GetDimensionAxies(dllPath));
                toolsPanel.AddItem(DeleteCorruptFile(dllPath));
                toolsPanel.AddItem(GetElementsByWorksets(dllPath));
                toolsPanel.AddItem(FloorEditButton(dllPath));
                toolsPanel.AddStackedItems(
                    MakeGridWallDim(dllPath),
                    GetWindowFromLegend(dllPath),
                    MakeDimensionTolerance(dllPath));
                toolsPanel.AddSeparator();
                toolsPanel.AddStackedItems(
                    CreatePerspUserView(dllPath),
                    CreateUserView(dllPath),
                    AuditViewNamesPushButtonData(dllPath));
                toolsPanel.AddItem(CalcWallsArea(dllPath));
                toolsPanel.AddItem(CalcHatchesArea(dllPath));
                toolsPanel.AddItem(GetTotalLength(dllPath));
                toolsPanel.AddSeparator();
                toolsPanel.AddItem(OpenProjectFolder(dllPath));


                sheetPanel.AddItem(ElementInfo(dllPath));
                sheetPanel.AddItem(PrintMe(dllPath));

                PulldownButtonData cleanUpToolsPullDownButtonData = new PulldownButtonData(
                    name: "CleanUpToolsPulldown",
                    text: "Clean Up Tools");

                RibbonItem stackOne = cleanUpToolsPanel.AddItem(cleanUpToolsPullDownButtonData);

                PulldownButton cleanupPulldownButton = (PulldownButton)stackOne;

                cleanupPulldownButton.AddPushButton(PurgeViews(dllPath));
                cleanupPulldownButton.AddPushButton(PurgeLinePatterns(dllPath));


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
            var elementInfoButtonText = Tools.GetResourceManager("element_info");
            var pbd = new PushButtonData(
                "cmdElementInfo", elementInfoButtonText, dll, "Utils.ElementInfo");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.information.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("element_info_tooltip");
            return pbd;
        }

        private static PushButtonData PrintMe(string dll)
        {
            var pushButtonData = new PushButtonData(
                name: "cmdPrintMe",
                text: Tools.GetResourceManager("printme_button_name"),
                assemblyName: dll,
                className: "PrintMe.PrintMe");
            AssignPushButtonImage(pushButtonData, "PRSPKT_Apps.Resources.printme.png", 32, dll);
            pushButtonData.ToolTip = Tools.GetResourceManager("printme_tooltip");
            return pushButtonData;
        }

        private static PushButtonData GetTotalLength(string dll)
        {
            var elementInfoButtonText = Tools.GetResourceManager("totalLength_button_name");
            var pbd = new PushButtonData("cmdCurveTotalLength", elementInfoButtonText, dll, "Utils.CurveTotalLength");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.totalLength.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("totalLength_toolTip");
            return pbd;
        }

        private static PushButtonData GetDimensionAxies(string dll)
        {
            var elementInfoButtonText = Tools.GetResourceManager("dimAxies_button_name");
            var pbd = new PushButtonData("cmdDimAxies", elementInfoButtonText, dll, "DimAxies.DimAxies");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.dimAxies.png", 32, dll);
            pbd.ToolTip = Tools.GetResourceManager("dimAxies_toolTip");
            return pbd;
        }

        private static PushButtonData AuditViewNamesPushButtonData(string dll)
        {
            var elementInfoButtonText = "Названия видов";
            var pbd = new PushButtonData("cmdAuditViewNames", elementInfoButtonText, dll,
                "PRSPKT_Apps.Commands.AuditViewNamesCommand.AuditViewNamesCommand");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.dimensionTol_16.png", 16, dll);
            pbd.ToolTip = "Красиво причесать названия видов";
            return pbd;
        }

        private static PushButtonData MakeDimensionTolerance(string dll)
        {
            var elementInfoButtonText = "Раскидать размеры";
            var pbd = new PushButtonData("cmdMakeDimTolerance", elementInfoButtonText, dll,
                "Dimensions.DimensionTolerance");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.dimensionTol_16.png", 16, dll);
            pbd.ToolTip = "Красиво раскидать размеры";
            return pbd;
        }

        private static PushButtonData GetWindowFromLegend(string dll)
        {
            var elementInfoButtonText = "Дверь/окно с легенды";
            var pbd = new PushButtonData("cmdGetWinFromLgnd", elementInfoButtonText, dll, "PRSPKT_Apps.Commands.GetFromLegend.GetWindowFromLegendCommand");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.window_16.png", 16, dll);
            pbd.ToolTip = "Выделить окно с легенды";
            return pbd;
        }


        private static PushButtonData MakeGridWallDim(string dll)
        {
            var elementInfoButtonText = "Образмерить стены";
            var pbd = new PushButtonData("cmdMakeGridWallDim", elementInfoButtonText, dll, "Dimensions.WallDimension");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.dimension_16.png", 16, dll);
            pbd.ToolTip = "Образмерить стены и оси";
            return pbd;
        }

        private static PushButtonData CreateUserView(string dll)
        {
            var elementInfoButtonText = "Создать вид";
            var pbd = new PushButtonData("cmdCreateUserView", elementInfoButtonText, dll, "UserView.CreateUserView");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.userview_16.png", 16, dll);
            pbd.ToolTip = "Создать пользовательский вид для Задания";
            return pbd;
        }

        private static PushButtonData CreatePerspUserView(string dll)
        {
            const string elementInfoButtonText = "Создать перспективу";
            var pbd = new PushButtonData("cmdCreatePerspUserView", elementInfoButtonText, dll, "UserView.CreateCameraFromView");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.perspuserview_16.png", 16, dll);
            pbd.ToolTip = "Создать пользовательский перспективный вид";
            return pbd;
        }

        private static PushButtonData DeleteCorruptFile(string dll)
        {
            var elementInfoButtonText = "Удалить Corrupt";
            var pbd = new PushButtonData("cmdDeleteFile", elementInfoButtonText, dll, "Utils.DeleteCorruptFile");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.deleteCorrupt.png", 32, dll);
            pbd.ToolTip = "Попытаться удалить файл corrupt с сервера";
            return pbd;
        }

        private static PushButtonData GetElementsByWorksets(string dll)
        {
            var elementInfoButtonText = "Рабочие Наборы";
            var pbd = new PushButtonData("cmdObjectsOnWorkSets", elementInfoButtonText, dll,
                "ElementsOnWorkset.WorksetExplorer");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.objectsOnWorkset.png", 32, dll);
            pbd.ToolTip = "Просмотр списка элементов (Id) в рабочих наборах";
            return pbd;
        }

        private static PushButtonData PurgeViews(string dll)
        {
            var elementInfoButtonText = "Зачистка видов";
            var pbd = new PushButtonData("cmdPurgeViews", elementInfoButtonText, dll,
                "PRSPKT_Apps.Commands.PurgeViewsCommand.PurgeViewsCommand");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.objectsOnWorkset.png", 32, dll);
            pbd.ToolTip = "Зачистка видов по фильтру";
            return pbd;
        }

        private static PushButtonData PurgeLinePatterns(string dll)
        {
            var elementInfoButtonText = "Зачистка линий";
            var pbd = new PushButtonData("cmdPurgeLinePatterns", elementInfoButtonText, dll,
                "PRSPKT_Apps.Commands.PurgeLinePatternsCommand.PurgeLinePatternsCommand");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.objectsOnWorkset.png", 32, dll);
            pbd.ToolTip = "Зачистка типов линий по фильтру";
            return pbd;
        }

        private static PushButtonData FloorEditButton(string dll)
        {
            var elementInfoButtonText = "Уклоны";
            var pbd = new PushButtonData("cmdFloorEdit", elementInfoButtonText, dll, "FloorEdit.Main");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.floorEdit_32.png", 32, dll);
            pbd.ToolTip = "Редактирование уклона";
            return pbd;
        }

        private static PushButtonData CalcWallsArea(string dll)
        {
            var elementInfoButtonText = "Площадь стен";
            var pbd = new PushButtonData("cmdWallsAreaCalc", elementInfoButtonText, dll, "Utils.WallsTotalArea");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.brickwall_32.png", 32, dll);
            pbd.ToolTip = "Подсчет площади выделенных стен";
            return pbd;
        }

        private static PushButtonData CalcHatchesArea(string dll)
        {
            var elementInfoButtonText = "Площадь штриховок";
            var pbd = new PushButtonData("cmdHatchesAreaCalc", elementInfoButtonText, dll, "Utils.HatchesTotalArea");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.hatch_32.png", 32, dll);
            pbd.ToolTip = "Подсчет площади штриховок";
            return pbd;
        }

        private static PushButtonData OpenProjectFolder(string dll)
        {
            var elementInfoButtonText = "Папка проекта";
            var pbd = new PushButtonData("cmdOpenProjectFolder", elementInfoButtonText, dll, "Utils.OpenProjectFolder");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.OpenProject_32.png", 32, dll);
            pbd.ToolTip = "Открыть папку проекта";
            return pbd;
        }

        private static PushButtonData AboutBox(string dll)
        {
            var elementInfoButtonText = "About Apps";
            var pbd = new PushButtonData("cmdAboutBox", elementInfoButtonText, dll, "Common.About");
            AssignPushButtonImage(pbd, "PRSPKT_Apps.Resources.about_32.png", 32, dll);
            pbd.ToolTip = "О приложении";
            return pbd;
        }

        private static void AssignPushButtonImage(ButtonData pushButtonData, string iconName, int size, string dll)
        {
            if (size == -1) size = 32;
            var image = LoadPngImageSource(iconName, dll);
            if (image == null || pushButtonData == null) return;
            if (size == 32)
                pushButtonData.LargeImage = image;
            else
                pushButtonData.Image = image;
        }

        private static ImageSource LoadPngImageSource(string sourceName, string path)
        {
            try
            {
                var mAssembly = Assembly.LoadFrom(Path.Combine(path));
                var mIcon = mAssembly.GetManifestResourceStream(sourceName);
                var mDecoder = new PngBitmapDecoder(mIcon, BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.Default);
                ImageSource mSource = mDecoder.Frames[0];
                return mSource;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }

    internal class Icons
    {
        private static readonly string DllPath = Assembly.GetExecutingAssembly().Location;

        public static void RoomsPanel(RibbonPanel panel)
        {
            // Add PRSPKT Rename Apart Rooms Button
            var apartRoomsButtonText = "Переименовать \n помещения";
            var apartRoomsData = new PushButtonData("cmdRenameApartRooms", apartRoomsButtonText, DllPath,
                "RenameApartRooms.RenameApartRooms")
            {
                ToolTip = Tools.GetResourceManager("apartRoomRename_toolTip"),
                LargeImage =
                    new BitmapImage(
                        new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/renameApartRooms.png"))
            };
            panel.AddItem(apartRoomsData);

            // Add PRSPKT Rename Apart Rooms Button
            var doRmButtonText = "Rm Сергею ОВ";
            var doRMdata = new PushButtonData("cmdDoRM", doRmButtonText, DllPath, "Do_Rm.DoRmSergei")
            {
                ToolTip = "Проставить Rm_Этаж для Сергея ОВ",
                LargeImage = new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/DoRM.png"))
            };
            panel.AddItem(doRMdata);

            // Add PRSPKT ApartmentCalc Button New
            var apartCalcPButtonText = "Квартирография";
            var apartCalcPData = new PushButtonData("cmdApartCalc_P", apartCalcPButtonText, DllPath,
                "ApartmentCalc_P.ApartmentCalc_P")
            {
                ToolTip = Tools.GetResourceManager("apartCalc_toolTip"),
                LargeImage =
                    new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/apartCalc_new.png"))
            };
            panel.AddItem(apartCalcPData);

            // Add PRSPKT FloorFinish button
            var floorFinishButtonText = Tools.GetResourceManager("floorfinish_button_name");
            var floorFinishData = new PushButtonData("cmdFloorsFinish", floorFinishButtonText, DllPath,
                "RoomsFinishes.FloorsFinishesClass")
            {
                ToolTip = Tools.LangResMan.GetString("floorfinish_toolTip", Tools.Cult),
                LargeImage =
                    new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/floorFinish.png"))
            };

            // Add PRSPKT RoomFinish button
            var roomFinishesButtonText = Tools.GetResourceManager("roomFinishes_button_name");
            var roomsFinishesData = new PushButtonData("cmdRoomFinish", roomFinishesButtonText, DllPath,
                "RoomsFinishes.RoomFinishesClass")
            {
                ToolTip = Tools.LangResMan.GetString("roomFinishes_toolTip", Tools.Cult),
                LargeImage =
                    new BitmapImage(new Uri("pack://application:,,,/PRSPKT_Apps;component/Resources/roomFinish.png"))
            };

            // Group RoomsFinishes button
            var sbRoomData = new SplitButtonData("room", "PRSPKT_Apps");
            if (!(panel.AddItem(sbRoomData) is SplitButton sbRoom)) return;
            sbRoom.AddPushButton(floorFinishData);
            sbRoom.AddPushButton(roomsFinishesData);
        }
    }
}