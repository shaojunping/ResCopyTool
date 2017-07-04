using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace ResCopyTool
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ResListView))]
    [Export(typeof(IListView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ResListView : IListView, IItemView, IObservableContext, ISelectionContext, IInitializable
    {
        [ImportingConstructor]
        public ResListView(IStatusService statusService, ListViewAdapter adapter)
        {
            m_listViewAdapter = adapter;
            m_selection = new Selection<object>();
            m_pathList = new Dictionary<string, ResFileInfo>();
            m_selection.Changed += selection_Changed;
            m_statusService = statusService;
            ItemInserted += addItem;
            // suppress compiler warning
            if (ItemSelected == null) return;

            // inhibit compiler warnings; we never raise these events, though it would be
            //  possible, using the file watcher support in .Net
            if (ItemInserted == null) return;
            if (ItemRemoved == null) return;
            if (ItemChanged == null) return;
            
        }
        private void addItem(object sender, ItemInsertedEventArgs<object> args)
        {
            ;
        }
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
        }

        #endregion

        public void SetSelect(string str)
        {
            List<ResFileInfo> list = new List<ResFileInfo>();
            //foreach (KeyValuePair<string, ResFileInfo> kv in m_pathList)
            //{
            //    if (kv.Key.Equals(str))
            //    {
            //        MessageBox.Show("Yeah!");
            //    }
            //    MessageBox.Show("这是字符串" + kv.Key + "," + str);
            //}
            ResFileInfo info = m_pathList[str];
            //MessageBox.Show(info.FullName);
            List<object> sel = new List<object>(1);
            list.Add(info);
            sel.AddRange(list);
            Selection = sel;
        }

        public void Insert(string path)
        {
            if (path.IndexOf("resources\\") < 0)
            {
                MessageBox.Show("所添加的文件不是resources目录下的，请重新拖入", "提示", MessageBoxButtons.OK);
                return;
            }
            if (File.Exists(path))
            {
                ResFileInfo srcFile = new ResFileInfo();
                srcFile.FullName = path;
                if (srcFile.DataType != Utility.DataType.DT_NULL && srcFile.DataType != Utility.DataType.DT_END
                    && !m_pathList.ContainsKey(path))
                {
                    m_pathList.Add(path, srcFile);
                    ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(Items.Count() + 1, srcFile));
                }
            }
            else if (Directory.Exists(path))
            {
                List<string> subFileList = GetFilesFromDirectory(path);
                int count = subFileList.Count;
                m_statusService.BeginProgress("文件载入中...", false);
                int i = 0;
                foreach(string file in subFileList)
                {
                    i++;
                    m_statusService.ShowProgress((float)i / count);
                    ResFileInfo srcFile = new ResFileInfo();
                    srcFile.FullName = file;
                    if (srcFile.DataType != Utility.DataType.DT_NULL && srcFile.DataType != Utility.DataType.DT_END
                        && !m_pathList.ContainsKey(file))
                    {
                        m_pathList.Add(file, srcFile);
                        ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(Items.Count() + 1, srcFile));
                    }
                }
                m_statusService.EndProgress();
            }
        }
        public static List<string> GetFilesFromDirectory(string path)
        {
            List<string> listFile = new List<string>();
            DirectoryInfo info = new DirectoryInfo(path);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                if (fsi is FileInfo)
                {
                    listFile.Add(fsi.FullName);
                }
                else
                {
                    List<string> listSubDirectoryFile = GetFilesFromDirectory(fsi.FullName);
                    foreach(string str in listSubDirectoryFile)
                    {
                        listFile.Add(str);
                    }
                }
            }
            return listFile;
        }
        
        private Dictionary<string, ResFileInfo> m_pathList;
        private Selection<object> m_selection;
        static private IStatusService m_statusService;
        private ListViewAdapter m_listViewAdapter;

        public Dictionary<string, ResFileInfo> PathList
        {
            get
            {
                return m_pathList;
            }
        }
        #region IListView Members

        /// <summary>
        /// Gets names for file list view columns</summary>
        public string[] ColumnNames
        {
            get
            {
                string[] result = new string[1];
                result[0] = "文件列表";
                m_listViewAdapter.SetColumnWidth("文件列表", 300);
                return result;
            }
        }

        /// <summary>
        /// Gets the items in the list</summary>
        public IEnumerable<object> Items
        {
            get
            {
                if (m_pathList== null || m_pathList.Count == 0)
                {
                    return EmptyEnumerable<object>.Instance;
                }
                List<object> children = new List<object>(m_pathList.Count);
                children.AddRange(m_pathList.Values);
                return children;
            }
            //set
        }
        #endregion

        #region IItemView Members
        public void GetInfo(object item, ItemInfo info)
        {
            if (item == null)
            {
                info = null;
            }
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
            set 
            { 
                m_selection.SetRange(value);
                SelectionChanged.Raise(this, EventArgs.Empty);
            }
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

        static ResListView()
        {
#pragma warning disable 0219
            string dummy = Resources.XMLImage; // force initialization of image resources
        }
        private void selection_Changed(object sender, EventArgs e)
        {
            SelectionChanged.Raise(this, EventArgs.Empty);
        }
    }
}
