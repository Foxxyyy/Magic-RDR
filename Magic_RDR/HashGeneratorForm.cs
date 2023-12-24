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
    public partial class HashGeneratorForm : Form
    {
        public HashGeneratorForm()
        {
            InitializeComponent();
        }

        private void generateHashButton_Click(object sender, EventArgs e)
        {
            outputBox.Text = "";
            if (string.IsNullOrEmpty(inputBox.Text) || inputBox.Text == "\n" || inputBox.Text == "Enter text...")
            {
                return;
            }

            string[] lines = inputBox.Text.Split('\n');
            foreach (string line in lines)
            {
                uint hashValue = DataUtils.GetHash(line);
                string hash = "";

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        hash = "0x" + hashValue.ToString("X");
                        break;
                    case 1:
                        hash = hashValue.ToString();
                        break;
                }
                outputBox.Text += hash + "\n";
            }
        }

        private void inputBox_Enter(object sender, EventArgs e)
        {
            if (inputBox.Text == "Enter text...")
            {
                inputBox.Text = "";
                inputBox.ForeColor = Color.Black;
            }
        }

        private void inputBox_Leave(object sender, EventArgs e)
        {
            if (inputBox.Text == "")
            {
                inputBox.Text = "Enter text...";
                inputBox.ForeColor = Color.Silver;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            generateHashButton_Click(null, null);
            inputBox.Focus();
        }

        private void HashGeneratorForm_Load(object sender, EventArgs e)
        {
            ActiveControl = inputBox;
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked) return;
            generateHashButton_Click(null, null);
        }
    }
}
