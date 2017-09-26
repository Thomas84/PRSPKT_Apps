using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.ExportManager
{
    public class PostExportHookCommand
    {
        private string _cmd;
        private string _args;
        private string _name;
        private Collection<string> _supportedFilenameExtensions;

        public PostExportHookCommand()
        {
            this._cmd = string.Empty;
            this._args = string.Empty;
            this.Name = string.Empty;
            this._supportedFilenameExtensions = new Collection<string>();
        }

        public string Name { get => _name; set => _name = value; }

        public static string FormatConfigurationString(ExportSheet sheet, string value, string extension)
        {
            string result = value;
            result = result.Replace(@"$height", sheet.Height.ToString(CultureInfo.InvariantCulture));
            result = result.Replace(@"$width", sheet.Width.ToString(CultureInfo.InvariantCulture));
            result = result.Replace(@"$fullExportName", sheet.FullExportName);
            result = result.Replace(@"$fullExportPath", sheet.FullExportPath(extension));
            result = result.Replace(@"$exportDir", sheet.ExportDirectory);
            result = result.Replace(@"$pageSize", sheet.PageSize);
            result = result.Replace(@"$projectNumber", sheet.ProjectNumber);
            result = result.Replace(@"$sheetDescription", sheet.SheetDescription);
            result = result.Replace(@"$sheetNumber", sheet.SheetNumber);
            result = result.Replace(@"$sheetRevision", sheet.SheetRevision);
            result = result.Replace(@"$sheetRevisionDate", sheet.SheetRevisionDate);
            result = result.Replace(@"$sheetRevisionDescription", sheet.SheetRevisionDescription);
            result = result.Replace(@"$fileExtension", extension);
            return result;
        }

        public void SetCommand(string command) { this._cmd = command; }
        public void SetArguments(string arguments) { this._args = arguments; }
        public void SetName(string newName) { this._name = newName; }
        public void AddSupportedFilenameExtension(string extension) { this._supportedFilenameExtensions.Add(extension); }
        public bool HasExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }
            if (this._supportedFilenameExtensions == null || this._supportedFilenameExtensions.Count < 1)
            {
                return false;
            }
            return this._supportedFilenameExtensions.Contains(extension);
        }

        public string ListExtensions()
        {
            string s = string.Empty;
            foreach (string fne in this._supportedFilenameExtensions)
            {
                s += fne + System.Environment.NewLine;
            }
            return s;
        }

        internal void Run(ExportSheet sheet, string extension)
        {
            string a = FormatConfigurationString(sheet, this._args, extension);
            if (!string.IsNullOrEmpty(a))
            {
                Common.ConsoleUtilities.StartHiddenConsoleProg(this._cmd, a);
            }
        }
    }
}
