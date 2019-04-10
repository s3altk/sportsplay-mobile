using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace Mobile
{
    public class UniversalList<T>
    {
        private readonly List<T> _itemsList;

        public RecyclerView.Adapter Adapter;

        public UniversalList()
        {
            _itemsList = new List<T>();
        }

        public int Count
        {
            get { return _itemsList.Count; }
        }

        public void Add(T item, int index)
        {
            _itemsList.Add(item);

            if (Adapter != null)
            {
                Adapter.NotifyItemInserted(index);
            }
        }

        public void AddRange(IEnumerable<T> list)
        {
            _itemsList.AddRange(list);
        }

        public void RemoveAt(int index)
        {
            _itemsList.RemoveAt(index);

            if (Adapter != null)
            {
                Adapter.NotifyItemRemoved(index);
            }
        }

        public void Edit(T item, int index)
        {
            _itemsList[index] = item;

            if (Adapter != null)
            {
                Adapter.NotifyItemChanged(index);
            }
        }

        public void Clear()
        {
            _itemsList.Clear();
        }

        public T this[int index]
        {
            get { return _itemsList[index]; }
        }
    }
}