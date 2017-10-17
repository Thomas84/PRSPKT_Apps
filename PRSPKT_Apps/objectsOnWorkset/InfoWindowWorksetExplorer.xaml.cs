using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PRSPKT_Apps.objectsOnWorkset
{
    /// <summary>
    /// Логика взаимодействия для InfoWindowWorksetExplorer.xaml
    /// </summary>
    public partial class InfoWindowWorksetExplorer : Window
    {
        private List<List<Element>> _allElsEls;
        private Document _doc;
        private Workset[] _allWorkSetArray;
        private List<Element> _selectedElementList;
        private TreeView treeView1;

        public InfoWindowWorksetExplorer(List<List<Element>> allElsEls, Document doc, Workset[] allWorksetsArray)
        {
            InitializeComponent();
            this._allElsEls = allElsEls;
            this._doc = doc;
            this._allWorkSetArray = allWorksetsArray;
            this.genWorksetNodes();
        }

        public List<Element> GetSelectedElements()
        {
            return _selectedElementList == null ? (List<Element>)null : this._selectedElementList;
        }

        private string GenNodeName(string name, int ct)
        {
            return name + " (" + ct.ToString() + ")";
        }

        private int[] CountElementsPerWorkset()
        {
            int[] numArray = new int[this._allWorkSetArray.Length];
            int index = -1;
            foreach (Workset workset in this._allWorkSetArray)
            {
                ++index;
                numArray.SetValue((object)this._allElsEls[index].Count, index);
            }
            return numArray;
        }

        private void GenWorksetNodes()
        {
            int[] numArray = this.CountElementsPerWorkset();
            int ind = -1;
            foreach (Workset workset in this._allWorkSetArray)
            {
                ++ind;
                this.treeView1.Items.Add(workset.Name, numArray[ind]), this.GenCategoryNodes(ind))
                {
                    Tag = (object)this._allElsEls[ind];
                }
            }
        }
    }
}
