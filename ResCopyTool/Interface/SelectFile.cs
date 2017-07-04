using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.FolderSelection;

namespace ResCopyTool.Interface
{
    public partial class SelectFile : Form
    {
        public enum SelectMode
        {
            File,
            Folder,
            Save
        }

        public SelectFile()
        {
            InitializeComponent();
            textBox1.ReadOnly = true;
        }

        public string SelectedPath
        {
            get
            {
                return (textBox1.Text);
            }
        }

        public bool IsTiledMode
        {
            get 
            { 
                return checkBoxCopyMode.Checked; 
            }
        }

        public string InitPath
        {
            get
            {
                return (m_initPath);
            }

            set
            {
                m_initPath = value;
                textBox1.Text = m_initPath;
            }
        }

        public string Filter
        {
            get;
            set;
        }

        public SelectMode Mode
        {
            get;
            set;
        }

        public string DialogInfo
        {
            get
            {
                return (Text);
            }

            set
            {
                Text = value;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case SelectMode.File:
                    {
                        CustomOpenFileDialog dialog = new CustomOpenFileDialog();
                        dialog.ForcedInitialDirectory = InitPath;
                        dialog.Multiselect = false;
                        dialog.Filter = Filter;
                        if (dialog.ShowDialog() == DialogResult.OK)
                            textBox1.Text = dialog.FileName;
                    }
                    break;
                case SelectMode.Folder:
                    {
                        FolderSelectDialog dialog = new FolderSelectDialog();
                        dialog.InitialDirectory = InitPath;
                        if (dialog.ShowDialog() == DialogResult.OK)
                            textBox1.Text = dialog.SelectedPath;
                    }
                    break;
                case SelectMode.Save:
                    {
                        SaveFileDialog dialog = new SaveFileDialog();
                        dialog.InitialDirectory = InitPath;
                        dialog.Filter = Filter;
                        dialog.FilterIndex = 0;
                        dialog.RestoreDirectory = true;
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            this.textBox1.Text = dialog.FileName;
                        }
                    }
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private string m_initPath = string.Empty;
    }
}
