using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Sce.Atf;
using Sce.Atf.Applications;

namespace ResCopyTool
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
            ChineseStringLocalizer.Register();
            Localizer.SetStringLocalizer(new ChineseStringLocalizer());

            var catalog = new TypeCatalog(
                typeof(SettingsService),
                typeof(CommandService),
                typeof(StatusService),
                typeof(ControlHostService),
                typeof(StandardFileExitCommand),
                typeof(DocumentRegistry),
                typeof(ContextRegistry),
                typeof(DefaultTabCommands),
                typeof(Outputs),                     // passes messages to all log writers
                typeof(ResFileInfo),
                typeof(FileTreeView),
                typeof(FolderViewer),
                typeof(ResListView),
                //typeof(ShortcutKey),
                typeof(FileViewer),
                typeof(MyOutputService),
                 typeof(Editor)
                //,typeof(StandardEditCommands)
                );

            var container = new CompositionContainer(catalog);
            var toolStripContainer = new ToolStripContainer();
            var mainForm = new ResCopyTool.MainForm(toolStripContainer)
            {
                Text = "美术资源移动工具".Localize(),
                Icon = GdiUtil.CreateIcon(ResourceUtil.GetImage(Sce.Atf.Resources.AtfIconImage))
            };

            var batch = new CompositionBatch();
            batch.AddPart(mainForm);
            //batch.AddPart(new WebHelpCommands("http://192.168.2.121:8090/pages/viewpage.action?pageId=14745613".Localize()));
            container.Compose(batch);

            container.InitializeAll();
            Application.Run(mainForm);
            container.Dispose();
        }
    }
}
