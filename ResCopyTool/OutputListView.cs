using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using System.Drawing;

namespace ResCopyTool
{
    [Export(typeof(IInitializable))]
    [Export(typeof(OutputListView))]
    [Export(typeof(IListView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class OutputListView : IListView, IItemView, IObservableContext, ISelectionContext, IInitializable
    {
        [ImportingConstructor]
        public OutputListView(ListViewAdapter adapter)
        {
            m_listViewAdapter = adapter;

        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
        }

        #endregion

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when an item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// Event that is raised when an item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// Event that is raised when the collection has been reloaded</summary>
        public event EventHandler Reloaded;

        public event EventHandler<ItemSelectedEventArgs<object>> ItemSelected;
        //public event EventHandler SelectionChanged;
        #endregion

        #region ISelectionContext Members

        public IEnumerable<object> Selection
        {
            get { return m_selection; }
            set { m_selection.SetRange(value); }
        }

        public IEnumerable<T> GetSelection<T>()
                    where T : class
        {
            return m_selection.AsIEnumerable<T>();
        }

        public object LastSelected
        {
            get { return m_selection.LastSelected; }
        }

        public T GetLastSelected<T>()
                    where T : class
        {
            return m_selection.GetLastSelected<T>();
        }

        public bool SelectionContains(object item)
        {
            return m_selection.Contains(item);
        }

        public int SelectionCount
        {
            get { return m_selection.Count; }
        }

        public event EventHandler SelectionChanging;

        public event EventHandler SelectionChanged;

        #endregion

        #region IListView Members

        /// <summary>
        /// Gets names for file list view columns</summary>
        public string[] ColumnNames
        {
            get
            {
                string[] result = new string[1];
                result[0] = "输出列表";
                m_listViewAdapter.SetColumnWidth("输出列表", 300);
                return result;
            }
        }

        /// <summary>
        /// Gets the items in the list</summary>
        public IEnumerable<object> Items
        {
            get
            {
                //if (m_pathList == null || m_pathList.Count == 0)
                //{
                    return EmptyEnumerable<object>.Instance;
                //}
                //List<object> children = new List<object>(m_pathList.Count);
                //children.AddRange(m_pathList.Values);
                //return children;
            }
            //set
        }
        #endregion

        private void Insert(Color color, string messageText)
        {
            //ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(Items.Count() + 1, srcFile));
        }

        #region IItemView Members
        public void GetInfo(object item, ItemInfo info)
        {
            if (item == null)
            {
                info = null;
            }
            //m_listViewAdapter.
            ResFileInfo fileInfo = item as ResFileInfo;
            Dictionary<string, ResFileInfo> dicNotExists = fileInfo.GetNotExistSubFiles();
            if (dicNotExists.Count > 0)
            {
                info.Label = fileInfo.Name + "(有的依赖文件不存在)";
            }
            else
            {
                info.Label = fileInfo.Name /*+ file.GetNotExistsFiles()*/;
            }
            foreach (KeyValuePair<string, ResFileInfo> kv in dicNotExists)
            {
                Outputs.WriteLine(OutputMessageType.Error, fileInfo.FullName + "所依赖的资源文件" + kv.Key + "不存在，请查看！");
            }
            if (fileInfo.IsAtom)
            {
                info.IsLeaf = true;
            }

            info.ImageIndex = info.GetImageList().Images.IndexOfKey(Utility.m_imgType[(int)fileInfo.DataType]);
        }

        #endregion
        private ListViewAdapter m_listViewAdapter;
        private Selection<object> m_selection;
        //private List<> m_outputList;
    }
}
