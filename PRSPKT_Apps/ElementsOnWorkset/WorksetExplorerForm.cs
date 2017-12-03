using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PRSPKT_Apps.ElementsOnWorkset
{
    public partial class WorksetExplorerForm : System.Windows.Forms.Form
    {
        private List<List<Element>> _allElsEls;
        private Document _doc;
        private Workset[] _allWorkSetArray;
        private List<Element> _selectedElementList;


        public WorksetExplorerForm(List<List<Element>> allElsEls, Document doc, Workset[] allWorksetsArray)
        {
            InitializeComponent();
            _allElsEls = allElsEls;
            _doc = doc;
            _allWorkSetArray = allWorksetsArray;
            TreeNode treeNode = new TreeNode(_doc.Title);
            GenWorksetNodes();
            treeView1.Sort();
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
                treeView1.Nodes.Add(new TreeNode(GenNodeName(workset.Name, numArray[ind]), GenCategoryNodes(ind))
                {
                    Tag = _allElsEls[ind]
                });
            }
        }

        private TreeNode[] GenCategoryNodes(int ind)
        {
            var treeViewList = new List<TreeNode>();
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
                treeViewList.Add(new TreeNode(GenNodeName(sList[index1], elementListList[index1].Count), GenFamilyNodes(elementListList[index1]))
                {
                    ForeColor = System.Drawing.Color.Red,
                    Tag = elementListList[index1]
                });
            }
            return treeViewList.ToArray();
        }

        private TreeNode[] GenFamilyNodes(List<Element> elsOfCat)
        {
            var treeNodeList = new List<TreeNode>();
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
                treeNodeList.Add(new TreeNode(GenNodeName(sList[index1], elementListList[index1].Count), GenTypeNodes(elementListList[index1]))
                {
                    ForeColor = System.Drawing.Color.Blue,
                    Tag = (object)elementListList[index1]
                });
        }
            return treeNodeList.ToArray();
        }

    private TreeNode[] GenTypeNodes(List<Element> elsOfFam)
    {
        var treeViewList = new List<TreeNode>();
        var sList = new List<string>();
        var elementListList = new List<List<Element>>();

        foreach (Element current in elsOfFam)
        {
            var element = _doc.GetElement(current.GetTypeId());
            if (current is ModelCurve)
            {
                string name = ((ModelCurve)current).LineStyle.Name;
                int index = sList.IndexOf(current.Category.Name);
                if (index != -1)
                {
                    elementListList[index].Add(current);
                }
                else
                {
                    sList.Add(name);
                    elementListList.Add(new List<Element>());
                    elementListList[elementListList.Count - 1].Add(current);
                }
            }
            else if (null != element)
            {
                int index = sList.IndexOf(current.Category.Name);
                if (index != -1)
                {
                    elementListList[index].Add(current);
                }
                else
                {
                    sList.Add(element.Name);
                    elementListList.Add(new List<Element>());
                    elementListList[elementListList.Count - 1].Add(current);
                }
            }

        }
        int index1 = -1;
        foreach (string str in sList)
        {
            ++index1;
            treeViewList.Add(new TreeNode(GenNodeName(sList[index1], elementListList[index1].Count))
            {
                ForeColor = System.Drawing.Color.Brown,
                Tag = elementListList[index1]
            });
        }
        return treeViewList.ToArray();
    }



    private void WorksetExplorerForm_Load(object sender, EventArgs e)
    {

    }

        private void btn_Select_Click(object sender, EventArgs e)
        {
            if (!(treeView1.SelectedNode.Tag is List<Element>))
                return;

            _selectedElementList = treeView1.SelectedNode.Tag as List<Element>;
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {

        }
    }
}
