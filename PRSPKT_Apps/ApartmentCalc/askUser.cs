using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PRSPKT_Apps.ApartmentCalc
{
    public partial class askUser : Form
    {
        public askUser()
        {
            InitializeComponent();
            //
            // TODO: Добавить кнопку Advanced для настройки округления (roundCount)
            // TODO: Добавить возможность выбора для двухэтажных помещений (выбор двух этажей)
            //
        }

        void Cb_LevelsSelectedIndexChanged(object sender, EventArgs e)
        {

            if (cb_Levels.SelectedIndex > -1)
            {
                btn_OK.Enabled = true;
            }
            else btn_OK.Enabled = false;
            //			DialogResult result;
            //						if (DialogResult == DialogResult.OK) {
            //				index = cb_Levels.SelectedIndex;
            //			}
            //		else
            //		{
            //			cb_Levels.SelectedIndex = index;
            //		}

        }
    }
}
