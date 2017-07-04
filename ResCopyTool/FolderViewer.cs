using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace ResCopyTool
{
    
    [Export(typeof(IInitializable))]
    [Export(typeof(FolderViewer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FolderViewer : IInitializable, IControlHostClient
    {
        [ImportingConstructor]
        public FolderViewer(IControlHostService controlHostService/*ResCopyTool.MainForm mainForm*/)
        {
            //m_mainForm = mainForm;
            m_controlHostService = controlHostService;
            m_treeControl = new TreeControl();
            m_treeControl.Text = "依赖文件树";
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.SelectionMode = SelectionMode.One;
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.Width = 256;
            m_path = new ResFileInfo();

            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            m_fileTreeView = new FileTreeView(m_treeControlAdapter);
            ControlInfo controlInfo = new ControlInfo("资源文件依赖树", "所要拷贝的资源文件列表", StandardControlGroup.Center);
            m_controlHostService.RegisterControl(m_treeControl, controlInfo, this);
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
            m_treeControlAdapter.TreeView = m_fileTreeView;
            if (m_treeControl != null)
            {
                ContextMenu menu = new ContextMenu();
                menu.MenuItems.Add(new MenuItem("打开文件所在文件夹", contextMenu_OpenFolder));
                menu.Popup += contextMenu_Popup;
                m_treeControl.ContextMenu = menu;
            }
        }

        #endregion
        private ResFileInfo m_path;

        public ResFileInfo Path
        {
            //get { return m_fileTreeView.Path; }
            set
            {
                m_path = value;
                m_fileTreeView.Path = value;
                
            }
        }
        public bool IsOnlyShowName
        {
            set
            {
                m_fileTreeView.IsOnlyShowName = value;
            }
            get
            {
                return m_fileTreeView.IsOnlyShowName;
            }
        }

        private void contextMenu_OpenFolder(object sender, EventArgs e)
        {
            var obj = m_treeControlAdapter.LastHit;
            if (obj != null)
            {
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
        private void contextMenu_Popup(object sender, EventArgs e)
        {
            var obj = m_treeControlAdapter.LastHit;
            if (obj != null)
            {
                ResFileInfo info = (ResFileInfo)obj;
                bool enable = info != null;
                m_treeControl.ContextMenu.MenuItems[0].Enabled = enable;
            }
        }


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

        private TreeControl m_treeControl;
        private TreeControlAdapter m_treeControlAdapter;
        private FileTreeView m_fileTreeView;
        private IControlHostService m_controlHostService;
        //private ResCopyTool.MainForm m_mainForm;
    }

}
