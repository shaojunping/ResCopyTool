using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sce.Atf;
using Sce.Atf.Applications;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Keys = Sce.Atf.Input.Keys;

namespace ResCopyTool
{
    [Export(typeof(Editor))]
    [Export(typeof(IInitializable))]
    //[Export(typeof(StatusService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class Editor : IControlHostClient, IInitializable, ICommandClient
    {
        [ImportingConstructor]
        public Editor(
            IControlHostService controlHostService,
            IContextRegistry contextRegistry,
            IStatusService statusService,
            ICommandService commandService,
            FileViewer fileViewer
            )
        {
            m_controlHostService = controlHostService;
            m_statusService = statusService;
            m_commandService = commandService;
            m_fileViewer = fileViewer;
        }

        private IControlHostService m_controlHostService;
        private IStatusService m_statusService;
        protected ICommandService m_commandService;
        private FileViewer m_fileViewer;
        private ContextMenu m_menu;
        protected enum CommandType
        {
            CopyFile
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up scripting service</summary>
        void IInitializable.Initialize()
        {
            m_commandService.UnregisterCommand(StandardCommand.EditCopy, this);
            //m_commandService.UnregisterCommand(CommandId.EditKeyboard,this);
            var commandInfo = new CommandInfo(CommandType.CopyFile,
                StandardMenu.File,
                StandardCommandGroup.FileNew,
                "资源拷贝",
                "资源拷贝",
                Keys.None,
                Sce.Atf.Resources.CopyImage,
                CommandVisibility.All);
            m_commandService.RegisterCommand(commandInfo, this);

            ContextMenu menu = new ContextMenu();
            m_fileViewer.Menu = menu;
            MenuItem item = new MenuItem("删除");
            item.Click += new System.EventHandler(this.MenuItemDelClick);
            menu.MenuItems.Add(item);

            MenuItem itemCopy = new MenuItem("拷贝");
            itemCopy.Click += new System.EventHandler(this.MenuItemCopyClick);
            menu.MenuItems.Add(itemCopy);

            MenuItem itemOnlyShowName = new MenuItem("只显示名称");
            itemOnlyShowName.Checked = true;
            itemOnlyShowName.Click += new System.EventHandler(this.MenuItemOnlyShowNameClick);
            menu.MenuItems.Add(itemOnlyShowName);

            MenuItem itemShowFullPath = new MenuItem("显示文件完整路径");
            itemShowFullPath.Checked = false;
            itemShowFullPath.Click += new System.EventHandler(this.MenuItemShowFullPathClick);
            menu.MenuItems.Add(itemShowFullPath);

            menu.MenuItems.Add(new MenuItem("打开文件所在文件夹", m_fileViewer.contextMenu_OpenFolder));
            menu.Popup += m_fileViewer.contextMenu_Popup;
            m_menu = menu;
        }

        #endregion

        #region ICommandClient

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool canDo = false;
            if (commandTag is CommandType)
            {
                var tag = (CommandType)commandTag;
                switch (tag)
                {
                    case CommandType.CopyFile:
                        canDo = true;
                        break;
                    //case CommandType.OnlyShowName:
                    //    canDo = !m_fileViewer.IsOnlyShowName;
                    //    break;
                    //case CommandType.ShowFullPath:
                    //    canDo = m_fileViewer.IsOnlyShowName;
                    //    break;
                }
            }
            return (canDo);
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            if (commandTag is CommandType)
            {
                switch ((CommandType)commandTag)
                {
                    case CommandType.CopyFile:
                        CopyFile(true);
                        break;
                    //case CommandType.OnlyShowName:
                    //    m_fileViewer.IsOnlyShowName = true;
                    //    break;
                    //case CommandType.ShowFullPath:
                    //    m_fileViewer.IsOnlyShowName = false;
                    //    break;
                }
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState state)
        {
        }
        #endregion
        
        #region copy

        public void CopyFile(bool isAllList = true)
        {
            string defaultPath = Properties.Settings.Default.dstPath;
            Interface.SelectFile dialog = new Interface.SelectFile();
            dialog.InitPath = defaultPath;
            dialog.Mode = Interface.SelectFile.SelectMode.Folder;
            dialog.Filter = "所有文件 (*.*)|*.*";
            dialog.DialogInfo = "请选择要输出资源文件的目录：";
            if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedPath.Length > 0)
            {
                if (!Directory.Exists(dialog.SelectedPath))
                {
                    DialogResult dialogResult = MessageBox.Show(dialog.SelectedPath + "不存在，是否创建？", "提示", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Directory.CreateDirectory(dialog.SelectedPath);
                    }
                    else
                    {
                        return;
                    }
                }

                DateTime dt1 = DateTime.Now;
                string dstDir = Path.GetFullPath(dialog.SelectedPath);
                bool isTiledMode = dialog.IsTiledMode;
                double totalFileNum = 0;
                if (isTiledMode)
                {
                    CopyByTiledMode(dstDir, out totalFileNum, isAllList);
                }
                else
                {
                    CopyNotByTiledMode(dstDir, out totalFileNum, isAllList);
                }
                DateTime dt2 = DateTime.Now;
                TimeSpan ts = dt2.Subtract(dt1);
                double secInterval = ts.TotalSeconds;
                double hour = ts.TotalHours;
                double minute = ts.TotalMinutes;
                double second = ts.TotalSeconds;
                string result = "拷贝成功！花费时间" + Math.Floor(hour) + "小时，" + Math.Floor(minute) + "分钟，"+Math.Floor(second)+"秒。共拷贝了"+totalFileNum+"个文件";
                MessageBox.Show(result);
                Outputs.WriteLine(OutputMessageType.Info, result);
                Properties.Settings.Default.dstPath = dstDir;
                Properties.Settings.Default.Save();
            }
        }

        private void CopyByTiledMode(string dstDir, out double totalFileNum, bool isAllList = true)
        {
            totalFileNum = 0;
            Dictionary<string, ResFileInfo> listSrcFile = new Dictionary<string, ResFileInfo>();
            if (isAllList)
            {
                listSrcFile = m_fileViewer.PathList;
            }
            else
            {
                foreach (object o in m_fileViewer.SelectList)
                {
                    ResFileInfo info = (ResFileInfo)o;
                    listSrcFile.Add(info.FullName, info);
                }
            }
            m_statusService.BeginProgress("拷贝中...", false);
            int count = listSrcFile.Count();
            Dictionary<string, ResFileInfo> listFileNeedCopy = new Dictionary<string, ResFileInfo>();
            int i = 0;
            foreach (KeyValuePair<string, ResFileInfo> kv in listSrcFile)
            {
                ResFileInfo info = kv.Value;
                Dictionary<string, ResFileInfo> subFileList = info.GetAllSubFiles();
                i++;
                m_statusService.ShowProgress((float)i / count);
                foreach (KeyValuePair<string, ResFileInfo> subInfo in subFileList)
                {
                    string srcPath = subInfo.Key;
                    if (!listFileNeedCopy.ContainsKey(srcPath))
                    {
                        if (Utility.CopyFileByTiledMode(srcPath, dstDir))
                        {
                            listFileNeedCopy.Add(srcPath, subInfo.Value);
                        }
                    }
                }
                string myPath = info.FullName;
                if (!listFileNeedCopy.ContainsKey(myPath))
                {
                    if (Utility.CopyFileByTiledMode(myPath, dstDir))
                    {
                        listFileNeedCopy.Add(myPath, info);
                    }
                }
                Outputs.WriteLine(OutputMessageType.Info, "拷贝" + myPath + "所依赖的文件完成！");
            }
            totalFileNum = listFileNeedCopy.Count;
            m_statusService.EndProgress();
        }

        

        private void CopyNotByTiledMode(string dstDir1, out double totalFileNum, bool isAllList = true)
        {
            totalFileNum = 0;
            Dictionary<string, ResFileInfo> listSrcFile = new Dictionary<string, ResFileInfo>();
            if (isAllList)
            {
                listSrcFile = m_fileViewer.PathList;
            }
            else
            {
                foreach (object o in m_fileViewer.SelectList)
                {
                    ResFileInfo info = (ResFileInfo)o;
                    listSrcFile.Add(info.FullName, info);
                }
            }
                   
            string srcDstDir = dstDir1;
            int count = listSrcFile.Count;
            m_statusService.BeginProgress("拷贝中...", false);
            int i = 0;
            Dictionary<string, ResFileInfo> listFileNeedCopy = new Dictionary<string, ResFileInfo>();
            foreach (KeyValuePair<string, ResFileInfo> kv in listSrcFile)
            {
                i++;
                m_statusService.ShowProgress((float)i / count);
                ResFileInfo info = kv.Value;
                string dstDir = srcDstDir + "\\" + Utility.GetOnlyNameByFullName(info.FullName);
                if (Directory.Exists(dstDir))
                {
                    int index = 1;
                    for (; ; )
                    {
                        if (Directory.Exists(dstDir + "_" + index))
                        {
                            index++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    dstDir += "_" + index;
                }
                if (!Directory.Exists(dstDir))
                {
                    Directory.CreateDirectory(dstDir);
                }
                    
                Dictionary<string, ResFileInfo> subFileList = info.GetAllSubFiles();
                foreach (KeyValuePair<string, ResFileInfo> subInfo in subFileList)
                {
                    string srcPath = subInfo.Key;
                    if (srcPath.Length == 0)
                    {
                        continue;
                    }
                    if (!listFileNeedCopy.ContainsKey(srcPath))
                    {
                        if (Utility.CopyFileNotByTiledMode(srcPath, dstDir))
                        {
                            listFileNeedCopy.Add(srcPath, subInfo.Value);
                        }
                    }
                }
                string myPath = info.FullName;
                if (!listFileNeedCopy.ContainsKey(myPath))
                {
                    if (Utility.CopyFileNotByTiledMode(myPath, dstDir))
                    {
                        listFileNeedCopy.Add(myPath, info);
                    }
                }
                Outputs.WriteLine(OutputMessageType.Info, "拷贝" + myPath + "所依赖的文件完成！");
            }
            totalFileNum = listFileNeedCopy.Count;
            m_statusService.EndProgress();
        }
        
        #endregion

        

        #region IControlHostClient Members

        /// <summary>
        void IControlHostClient.Activate(System.Windows.Forms.Control control)
        {
        }

        /// <summary>
        void IControlHostClient.Deactivate(System.Windows.Forms.Control control)
        {
        }

        /// <summary>
        bool IControlHostClient.Close(System.Windows.Forms.Control control)
        {
            bool closed = true;
            if (closed)
            {
            }

            return closed;
        }

        #endregion

        #region contextMenu
        private void MenuItemDelClick(Object sender, System.EventArgs e)
        {
            m_fileViewer.MenuItemDelClick();
        }

        private void MenuItemCopyClick(Object sender, System.EventArgs e)
        {
            CopyFile(false);
        }

        private void MenuItemOnlyShowNameClick(Object sender, System.EventArgs e)
        {
            m_menu.MenuItems[3].Checked = false;
            m_menu.MenuItems[2].Checked = true;
            m_fileViewer.IsOnlyShowName = true;
        }

        private void MenuItemShowFullPathClick(Object sender, System.EventArgs e)
        {
            m_menu.MenuItems[3].Checked = true;
            m_menu.MenuItems[2].Checked = false;
            m_fileViewer.IsOnlyShowName = false;
        }
        #endregion
    }
}
