using System;
using System.Collections.ObjectModel;
using System.Text;

namespace PRSPKT_Apps.ExportManager
{
    public class ExportLog
    {
        const string ErrorPrefix = "[EE]";
        const string WarningPrefix = "[WW]";
        private StringBuilder fulllog;
        private int errors;
        private int warnings;
        private DateTime startTime;
        private DateTime endTime;
        private Collection<ExportLogItem> errorLog;
        private Collection<ExportLogItem> warningLog;
        private Collection<ExportLogItem> errorsLog;

        public StringBuilder Fulllog { get => fulllog; set => fulllog = value; }
        public int TotalExports { get; private set; }
        public int Errors { get => errors; set => errors = value; }
        public int Warnings { get => warnings; set => warnings = value; }
        public DateTime StartTime { get => startTime; set => startTime = value; }
        public DateTime EndTime { get => endTime; set => endTime = value; }
        internal Collection<ExportLogItem> WarningLog { get => warningLog; set => warningLog = value; }
        public Collection<ExportLogItem> ErrorsLog { get => errorsLog; set => errorsLog = value; }

        public ExportLog()
        {
            Errors = 0;
            Warnings = 0;
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            TotalExports = 0;
            errorLog = new Collection<ExportLogItem>();
            WarningLog = new Collection<ExportLogItem>();
            Fulllog = new StringBuilder();
        }


        public void AddMessage(string message)
        {
            AddLogItem(message);
        }

        public void AddLogItem(string message)
        {
            Fulllog.Append(message).AppendLine();
        }

        public void AddError(string filename, string message)
        {
            AddLogItem(ErrorPrefix + message);
            errors++;
            ErrorsLog.Add(new ExportLogItem(message, filename));
        }

    }
}