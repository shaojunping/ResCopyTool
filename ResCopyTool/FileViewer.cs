using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace ResCopyTool
{
    [Export(typeof(IInitializable))]
    [Export(typeof(FileViewer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FileViewer:IInitializable, IControlHostClient
    {
        [ImportingConstructor]
        public FileViewer(IControlHostService controlHostService,
            IStatusService statusService,
            //ICommandService commandService,
            //IContextRegistry contextRegistry,
            FolderViewer folderView /*ResCopyTool.MainForm mainForm,*/)
        {
            //m_mainForm = mainForm;
            m_resFolderTreeView = folderView;
            m_controlHostService = controlHostService;
            m_listView = new ListView();
            m_listView.Dock = DockStyle.Fill;
            m_listView.AllowDrop = true;
            m_listView.DragEnter += fileListView_DragEnter;
            m_listView.DragDrop += fileListView_Added;
            m_listView.Text = "File Viewer";
            m_listView.BackColor = SystemColors.Window;
            m_listView.SmallImageList = ResourceUtil.GetImageList16();
            m_listView.AllowColumnReorder = true;
            m_listView.SelectedIndexChanged += selectedIndexChanged;
            
            m_listViewAdapter = new ListViewAdapter(m_listView);
            m_resListView = new ResListView(statusService, m_listViewAdapter);
            m_resListView.SelectionChanged += fileListView_SelectionChanged;

            ControlInfo controlInfo = new ControlInfo("文件列表", "所要查询的文件列表", StandardControlGroup.Left);
            m_controlHostService.RegisterControl(m_listView, controlInfo, this);

            //m_commandService = commandService;
            
        }

        public ContextMenu Menu
        {
            set
            {
                m_listView.ContextMenu = value;
            }
            get
            {
                return m_listView.ContextMenu;
            }
        }

        public void MenuItemDelClick()
        {
            foreach (object o in m_resListView.Selection)
            {
                ResFileInfo info = (ResFileInfo)o;
                m_resListView.PathList.Remove(info.FullName);
            }
            foreach (ListViewItem item in m_listView.SelectedItems)
            {
                m_listView.Items.Remove(item);
            }
            m_resFolderTreeView.Path = new ResFileInfo();
        }

        public void SetSelect(string str)
        {
            m_resListView.SetSelect(str);
        }

        public IEnumerable<Object> SelectList
        {
            get
            {
                return m_resListView.Selection;
            }
        }
        
        //private ICommandService m_commandService;
        //private IContextRegistry m_contextRegistry;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
            m_listViewAdapter.ListView = m_resListView;
            //m_mainForm.SplitContainer.Panel1.Controls.Add(m_listView);
        }

        #endregion
        private void selectedIndexChanged(object sender, EventArgs e)
        {
            m_listView.FocusedItem = m_listView.SelectedItems[0];
            m_listView.SelectedItems[0].EnsureVisible();
        }

        private void fileListView_SelectionChanged(object sender, EventArgs e)
        {
            ResFileInfo srcFile = m_resListView.LastSelected as ResFileInfo;
            m_resFolderTreeView.Path = srcFile;
        }

        private void fileListView_Added(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            Outputs.WriteLine(OutputMessageType.Info, "拖动" + path + "到工具中。");
            m_resListView.Insert(path);
        }

        private void fileListView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        public void contextMenu_OpenFolder(object sender, EventArgs e)
        {
            var obj = m_listViewAdapter.LastHit;
            if (obj != null)
            {
                //var doc = obj.As<ResFileInfo>();
                ResFileInfo info = (ResFileInfo)obj;

                if (File.Exists(info.FullName))
                {
                    string path = info.FullName;
                    System.Diagnostics.Process.Start("explorer.exe", "/select, " + path);
                }
                else
                    MessageBox.Show("无法找到原文件: " + info.FullName, "警告", MessageBoxButtons.OK);
            }
        }

        public void contextMenu_Popup(object sender, EventArgs e)
        {
            var obj = m_listViewAdapter.LastHit;
            if (obj != null)
            {
                ResFileInfo info = (ResFileInfo)obj;
                bool enable = info != null;
                Menu.MenuItems[2].Enabled = enable;
            }
        }

        private ListView m_listView;
        private ListViewAdapter m_listViewAdapter;
        private IControlHostService m_controlHostService;
        private ResListView m_resListView;
        private FolderViewer m_resFolderTreeView;

        public bool IsOnlyShowName
        {
            get { return m_resFolderTreeView.IsOnlyShowName; }
            set
            {
                m_resFolderTreeView.IsOnlyShowName = value;
            }
        }

        public Dictionary<string, ResFileInfo> PathList
        {
            get
            {
                return m_resListView.PathList;
            }
        }
        //private ResCopyTool.MainForm m_mainForm;

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(System.Windows.Forms.Control control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(System.Windows.Forms.Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        bool IControlHostClient.Close(System.Windows.Forms.Control control)
        {
            return true;
        }
        #endregion


        
    }
}
