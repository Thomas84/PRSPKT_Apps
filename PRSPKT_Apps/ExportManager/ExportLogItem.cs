namespace PRSPKT_Apps.ExportManager
{
    public class ExportLogItem
    {
        public string Filename { get; private set; }
        public string Description { get; private set; }

        public ExportLogItem(string description, string filename)
        {
            Filename = filename;
            Description = description;
        }

    }
}