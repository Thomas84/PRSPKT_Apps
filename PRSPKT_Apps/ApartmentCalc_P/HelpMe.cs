using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    public class HelpMe
    {
        Document _doc;
        UIDocument _UIDoc;

        //PRSPKT_Apps.ApartmentCalc_P.LevelsControl userLevelsControl = new LevelsControl(_UIDoc);
        private string _roomType;

        public string RoomType { get => _roomType; }

        public HelpMe(UIDocument uidoc)
        {
            _UIDoc = uidoc;
            var userLevels = new LevelsControl(_UIDoc);

        }
    }
}
