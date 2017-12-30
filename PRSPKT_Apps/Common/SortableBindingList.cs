// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace PRSPKT_Apps.Common
{
    public class SortableBindingListCollection<T> : System.ComponentModel.BindingList<T>
    {
        private bool _sorted;
        private System.ComponentModel.ListSortDirection _sortDirectionm;
        private System.ComponentModel.PropertyDescriptor _sortPropertym;

        protected override System.ComponentModel.ListSortDirection SortDirectionCore
        {
            get { return this._sortDirectionm; }
        }

        protected override System.ComponentModel.PropertyDescriptor SortPropertyCore
        {
            get { return this._sortPropertym; }
        }

        protected override bool IsSortedCore => this._sorted;
        protected override bool SupportsSortingCore => true;
        protected override void RemoveSortCore()
        {
            this._sorted = false;
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (prop.PropertyType.GetInterface("IComparable") == null)
            {
                return;
            }

            var list = this.Items as List<T>;
            if (list == null)
            {
                this._sorted = false;
            }
            else
            {
                var comparer = new PropertyComparer(prop.Name, direction);
                list.Sort(comparer);
                this._sorted = true;
                this._sortDirectionm = direction;
                this._sortPropertym = prop;
            }
        }

        private class PropertyComparer : IComparer<T>
        {
            public PropertyComparer(string name, ListSortDirection direction)
            {
                this.PropInfo = typeof(T).GetProperty(name);
                this.Direction = direction;
            }

            public PropertyInfo PropInfo { get; set; }
            public ListSortDirection Direction { get; set; }

            public int Compare(T x, T y)
            {
                var xc = this.PropInfo.GetValue(x, null);
                var yc = this.PropInfo.GetValue(y, null);
                if (this.Direction == ListSortDirection.Ascending)
                {
                    return Comparer.Default.Compare(xc, yc);
                }
                else
                {
                    return System.Collections.Comparer.Default.Compare(yc, xc);
                }
            }
        }
    }
}
