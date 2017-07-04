using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using System.IO;

namespace ResCopyTool
{
    [Export(typeof(IInitializable))]
    [Export(typeof(FileTreeView))]
    [Export(typeof(ITreeView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FileTreeView : ITreeView, IItemView, ISelectionContext, IObservableContext, IInitializable
    {
        [ImportingConstructor]
        public FileTreeView(TreeControlAdapter treeControlAdapter)
        {
            m_selection = new Selection<object>();
            m_path = new ResFileInfo();
            m_selection.Changed += selection_Changed;
            m_treeControlAdapter = treeControlAdapter;

            // suppress compiler warning
            if (SelectionChanging == null) return;
            if (ItemInserted == null) return;
            if (ItemRemoved == null) return;
            if (ItemChanged == null) return;
        }
        public ResFileInfo Path
        {
            set
            {
                m_path = value;
                Reloaded.Raise(this, EventArgs.Empty);
            }
        }

        private ResFileInfo m_path;
        private bool m_isOnlyShowName = true;
        private TreeControlAdapter m_treeControlAdapter;

        public bool IsOnlyShowName
        {
            set
            {
                m_isOnlyShowName = value;
                Reloaded.Raise(this, EventArgs.Empty);
            }
            get
            {
                return m_isOnlyShowName;
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
        }

        #endregion

        #region ITreeView Members

        public object Root
        {
            get
            {
                return m_path;
            }
            set 
            {
                Root = value;
            }
        }

        public IEnumerable<object> GetChildren(object parent)
        {
            IEnumerable<object> result = null;
            ResFileInfo info = parent as ResFileInfo;
            if (info != null && !info.IsAtom)
            {
                result = info.SubFiles;
            }
            if (result == null)
                return EmptyEnumerable<object>.Instance;
            return result;
        }

        #endregion

        #region IItemView Members

        public void GetInfo(object item, Sce.Atf.Applications.ItemInfo info)
        {
            ResFileInfo fileInfo = item as ResFileInfo;
            if (fileInfo != null && fileInfo.FullName.Length > 0)
            {
                Dictionary<string, ResFileInfo> dicNotExists = fileInfo.GetNotExistSubFiles();
                string label = "";
                if (m_isOnlyShowName)
                {
                    label = fileInfo.Name;
                }
                else
                {
                    label = fileInfo.FullName;
                }
                if (dicNotExists.Count > 0)
                {
                    info.Label = label + "(有的依赖文件不存在)";
                }
                else if (!File.Exists(fileInfo.FullName))
                {
                    info.Label = label + "(文件不存在)";
                }
                else
                {
                    info.Label = label;
                }

                if (fileInfo.IsAtom)
                {
                    info.IsLeaf = true;
                }

                info.ImageIndex = info.GetImageList().Images.IndexOfKey(Utility.m_imgType[(int)fileInfo.DataType]);
                m_treeControlAdapter.Expand(item);
            }
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

        private void selection_Changed(object sender, EventArgs e)
        {
            m_path = LastSelected as ResFileInfo;
            if (m_path != null && !m_path.IsAtom)
            {
                Root = m_path;
                SelectionChanged.Raise(this, EventArgs.Empty);
            }
        }

        private Selection<object> m_selection;
    }
}
