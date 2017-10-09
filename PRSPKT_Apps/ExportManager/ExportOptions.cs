namespace PRSPKT_Apps.ExportManager
{
    public enum ExportOptions
    {
        /// <summary>
        /// Export Nothing.
        /// </summary>
        None = 0,
        /// <summary>
        /// Export files using Adobe Acrobat.
        /// </summary>
        PDF = 1,
        /// <summary>
        /// Export an AutoCAD file.
        /// </summary>
        DWG = 2,
        /// <summary>
        /// Export a Microstation file.
        /// </summary>
        DGN = 4,
        /// <summary>
        /// Export a Autodesk DWF file.
        /// </summary>
        DWF = 8,
        /// <summary>
        /// Export files using Ghostscript to create pdf's.
        /// </summary>
        GhostscriptPDF = 16,
        /// <summary>
        /// Remove titleblock from sheet before exporting.
        /// </summary>
        NoTitle = 32,
    }

    public enum LogType
    {
        Error,
        Warning,
        Normal
    }
}