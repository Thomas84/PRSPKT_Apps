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
            int[] numArray = CountElementsPerWorkset();
            int ind = -1;
            foreach (Workset workset in _allWorkSetArray)
            {
                ++ind;
                var rootNode = new TreeViewItem()
                {
                    Header = GenNodeName(workset.Name, numArray[ind]),
                    Tag = _allElsEls[ind]
                };
                treeWorkset.Items.Add(rootNode);
                foreach (TreeViewItem categoryNode in GenCategoryNodes(ind))
                {
                    rootNode.Items.Add(categoryNode);
                    foreach (var item in _allElsEls[ind])
                    {
                        categoryNode.Items.Add(item);
                    }

                }
                

            }

            //treeWorkset.ItemsSource = _allElsEls;
            //treeWorkset.DisplayMemberPath = "Name";
        }


        private TreeViewItem[] GenCategoryNodes(int ind)
        {
            var treeViewList = new List<TreeViewItem>();
            var sList = new List<string>();
            var elementListList = new List<List<Element>>();


            foreach (Element item in _allElsEls[ind])
            {
                int index = sList.IndexOf(item.Category.Name);
                if (index != -1)
                {
                    elementListList[index].Add(item);
                }
                else
                {
                    sList.Add(item.Category.Name);
                    elementListList.Add(new List<Element>());
                    elementListList[elementListList.Count - 1].Add(item);
                }
            }

            int index1 = -1;
            foreach (string str in sList)
            {
                ++index1;
                treeViewList.Add(new TreeViewItem() { Header = GenNodeName(sList[index1], elementListList[index1].Count), Tag = (object)elementListList[index1] });
            }

            return treeViewList.ToArray();

        }

        private TreeViewItem[] GenFamilyNodes(List<Element> elsOfCat)
        {
            var treeNodeList = new List<TreeViewItem>();
            var sList = new List<string>();
            List<List<Element>> elementListList = new List<List<Element>>();

            foreach (Element element in elsOfCat)
            {
                if (_doc.GetElement(element.GetTypeId()) is FamilySymbol)
                {
                    string name = ((FamilySymbol)_doc.GetElement(element.GetTypeId())).Family.Name;
                    int index = sList.IndexOf(name);
                    if (index != -1)
                    {
                        elementListList[index].Add(element);
                    }
                    else
                    {
                        sList.Add(name);
                        elementListList.Add(new List<Element>());
                        elementListList[elementListList.Count - 1].Add(element);
                    }
                }
                else if (_doc.GetElement(element.GetTypeId()) is WallType)
                {
                    string str = ((WallType)_doc.GetElement(element.GetTypeId())).Kind.ToString();
                    int index = sList.IndexOf(str);
                    if (index != -1)
                    {
                        elementListList[index].Add(element);
                    }
                    else
                    {
                        sList.Add(str);
                        elementListList.Add(new List<Element>());
                        elementListList[elementListList.Count - 1].Add(element);
                    }
                }
                else if (element is ModelCurve)
                {
                    string name = element.Name;
                    int index = sList.IndexOf(name);
                    if (index != -1)
                    {
                        elementListList[index].Add(element);
                    }
                    else
                    {
                        sList.Add(name);
                        elementListList.Add(new List<Element>());
                        elementListList[elementListList.Count - 1].Add(element);
                    }
                }
                else
                {
                    string name = element.Category.Name;
                    int index = sList.IndexOf(name);
                    if (index != -1)
                    {
                        elementListList[index].Add(element);
                    }
                    else
                    {
                        sList.Add(name);
                        elementListList.Add(new List<Element>());
                        elementListList[elementListList.Count - 1].Add(element);
                    }
                }
            }

            int index1 = -1;
            foreach (string str in sList)
            {
                ++index1;
                treeNodeList.Add(new TreeViewItem() { Header = GenNodeName(sList[index1], elementListList[index1].Count), Tag = (object)elementListList[index1] });
            }
            return treeNodeList.ToArray();
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
