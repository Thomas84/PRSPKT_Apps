using System.Collections.ObjectModel;
using System.Globalization;

namespace PRSPKT_Apps.ExportManager
{
    public class SegmentedSheetName
    {
        private Collection<string> _hooks;

        public SegmentedSheetName()
        {
            this.Hooks = new Collection<string>();
            this.Name = "YYYYMMDD-AD-NNN[R]";
        }

        public string Name { get; set; }
        public string NameFormat { get; set; }
        public Collection<string> Hooks { get => _hooks; set => _hooks = value; }

        public void AddHook(string hookName)
        {
            this._hooks.Add(hookName);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[SegmentedSheetName Hooks={0}, Name={1}, NameFormat={2}", this._hooks, this.Name, this.NameFormat);
        }
    }


}