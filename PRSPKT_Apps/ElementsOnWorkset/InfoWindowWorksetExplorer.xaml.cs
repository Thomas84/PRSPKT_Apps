#region Namespaces
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
#endregion

namespace PRSPKT_Apps.ElementsOnWorkset
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
        private Dictionary<int, TreeViewItem> flattenedTree;

        public InfoWindowWorksetExplorer(List<List<Element>> allElsEls, Document doc, Workset[] allWorksetsArray)
        {
            InitializeComponent();
            _allElsEls = allElsEls;
            _doc = doc;
            _allWorkSetArray = allWorksetsArray;
            flattenedTree = new Dictionary<int, TreeViewItem>();
            GenWorksetNodes();


        }

        public List<Element> GetSelectedElements()
        {
            return _selectedElementList ?? null;
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
                var treeNode = new TreeViewItem()
                {
                    Header = GenNodeName(workset.Name, numArray[ind]),
                    Tag = workset
                };

                var childNode = new TreeViewItem()
                {

                    Header = GenCategoryNodes(ind)
                };
                treeWorkset.Items.Add(treeNode);
                treeWorkset.Items.Add(childNode);

            }

            //treeWorkset.ItemsSource = _allElsEls;
            //treeWorkset.DisplayMemberPath = "Name";
        }

    

    private List<string> GenCategoryNodes(int ind)
    {
        var treeViewList = new List<string>();
        var sList = new List<string>();
        var elementListList = new List<List<Element>>();
        using (List<Element>.Enumerator enumerator = _allElsEls[ind].GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                Element current = enumerator.Current;
                int index = sList.IndexOf(current.Category.Name);
                if (index != -1)
                {
                    elementListList[index].Add(current);
                }
                else
                {
                    sList.Add(current.Category.Name);
                    elementListList.Add(new List<Element>());
                    elementListList[elementListList.Count - 1].Add(current);
                }
            }
        }
        int index1 = -1;
        foreach (string str in sList)
        {
            ++index1;
            treeViewList.Add(GenNodeName(sList[index1], elementListList[index1].Count));

        }
        return treeViewList;

    }

    /// <summary>
    /// Cancel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Cancel_Button_Click(object sender, RoutedEventArgs e)
    {

        this.DialogResult = false;
        this.Close();
    }


    /// <summary>
    /// OK
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OK_Button_Click(object sender, RoutedEventArgs e)
    {
        if (!(treeWorkset.SelectedItem is List<Element>))
            return;

        _selectedElementList = treeWorkset.SelectedItem as List<Element>;
        this.DialogResult = true;
        this.Close();

    }
}
}
