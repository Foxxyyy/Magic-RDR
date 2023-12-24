using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_RDR
{
    public partial class NameDirectoryForm : Form
    {
        public string NewDirectoryName { get; set; }

        public NameDirectoryForm()
        {
            InitializeComponent();
            NewDirectoryName = string.Empty;
        }

        private void createDirectoryButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.directoryNameBox.Text))
            {
                return;
            }
            NewDirectoryName = directoryNameBox.Text;
            Close();
        }

        private void directoryNameBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return && e.KeyCode != Keys.Return)
            {
                return;
            }
            createDirectoryButton_Click(this, null);
        }
    }
}
