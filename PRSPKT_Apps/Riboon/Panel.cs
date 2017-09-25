using Autodesk.Revit.UI;
using System.IO;
using System.Linq;
using System.Reflection;


namespace PRSPKT_Apps.Riboon
{
    public class Panel
    {
        public static void Initialize(UIControlledApplication app)
        {
            // Create PRSPKT Tab
            app.CreateRibbonTab("tabName");
            var panelManagement = app.CreateRibbonPanel("tabname", "name");

            var pbAddinManager = RibbonUtilities.CreatePushButton(
                "name",
                "button_text",
                "PRSPKT_Apps.Revit.");

            panelManagement.AddItem(pbAddinManager);

            // Check Settings file to dynamically create remaining Panels/Buttons as needed.

            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                "settings file name";
            var storedAddins = File.Exists(filePath)
                ? Revit.Management.AddinManager.Serialization.Deserialize(filePath)
                : null;
            if (storedAddins == null) return;
            foreach (var addin in storedAddins)
            {
                if (!addin.Install) continue;
                try
                {
                    var panel = app.GetRibbonPanels("tabname")
                        .FirstOrDefault(x => x.Name == addin.Panel) ??
                        app.CreateRibbonPanel("tabname", addin.Panel);

                    var pb = RibbonUtilities.CreatePushButton(
                        addin.Name,
                        addin.ButtonText,
                        addin.FullName,
                        addin.ImageName,
                        addin.Description);
                    panel.AddItem(pb);
                }
                catch
                {
                    // ignored
                    throw;
                }
            }
        }
    }
}
