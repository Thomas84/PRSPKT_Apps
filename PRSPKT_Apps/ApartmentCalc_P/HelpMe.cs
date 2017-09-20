using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.ApartmentCalc_P;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    class HelpMe
    {
        Document _doc;
        UIDocument _UIDoc;
        //PRSPKT_Apps.ApartmentCalc_P.LevelsControl userLevelsControl = new LevelsControl(_UIDoc);


        public HelpMe(Document doc)
        {
            _doc = doc;

            public string RoomType()
            {
                return userLevelsControl.txtBoxType.Text;

            }
        }

        public void Initialize()
        {
            if (userLevelsControl.DialogResult == true)
            {

            }
            else
            {
                TaskDialog.Show("Error", "Error");
            }

        }



    }
}
