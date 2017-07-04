
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
#pragma warning disable 0649
namespace ResCopyTool
{
    [Export(typeof(Form))]
    [Export(typeof(MainForm))]
    [Export(typeof(Sce.Atf.Applications.IMainWindow))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MainForm : Sce.Atf.Applications.MainForm
    {
        public MainForm()
        {
            Text = "资源拷贝工具";

            m_splitContainer = new SplitContainer();
            m_splitContainer.Orientation = Orientation.Vertical;
            m_splitContainer.Dock = DockStyle.Fill;
            Controls.Add(m_splitContainer);
        }

        public MainForm(ToolStripContainer toolStripContainer)
        {
            InitializeComponent();

            StartPosition = FormStartPosition.Manual; // so we can persist bounds
            //m_mainFormBounds = Bounds;

            if (toolStripContainer != null)
            {
                m_toolStripContainer = toolStripContainer;
                m_toolStripContainer.Dock = DockStyle.Fill;
                Controls.Add(m_toolStripContainer);
            }

            Text = "资源拷贝工具";

            m_splitContainer = new SplitContainer();
            m_splitContainer.Orientation = Orientation.Vertical;
            m_splitContainer.Dock = DockStyle.Fill;
            Controls.Add(m_splitContainer);
        }

        public SplitContainer SplitContainer
        {
            get { return m_splitContainer; }
        }

        private SplitContainer m_splitContainer;
        private ToolStripContainer m_toolStripContainer;
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.ClientSize = new System.Drawing.Size(1403, 763);
            this.MainFormBounds = new System.Drawing.Rectangle(15, 15, 1419, 801);
            this.Name = "MainForm";
            //this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.ResumeLayout(false);

        }

        //private void MainForm_DragDrop(object sender, DragEventArgs e)
        //{
        //    string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        //    MessageBox.Show(path);
        //}

        //private void MainForm_DragEnter(object sender, DragEventArgs e)
        //{
        //    string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        //    MessageBox.Show(path);
        //}
    }
}
