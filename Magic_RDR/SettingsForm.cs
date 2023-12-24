using Magic_RDR.RPF;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Magic_RDR
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            checkBoxUseLastRPF.Checked = RPF6FileNameHandler.UseLastRPF;
            checkBoxShowLines.Checked = RPF6FileNameHandler.ShowLines;
            checkBoxShowPlusMinus.Checked = RPF6FileNameHandler.ShowPlusMinus;
            sortOrderComboBox.Text = RPF6FileNameHandler.Sorting.ToString();
            sortColumnComboBox.Text = RPF6FileNameHandler.SortColumn.ToString();
            checkBoxUseCustomColor.Checked = RPF6FileNameHandler.UseCustomColor;
            sizeModeComboBox.Text = RPF6FileNameHandler.ImageSizeMode.ToString();
            backgroundTextureComboBox.Text = RPF6FileNameHandler.TextureBackgroundColor.ToString().Replace("Color [", "").Replace("]", "");
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            RPF6FileNameHandler.SaveSettings();
            if (MessageBox.Show("Successfully saved settings !\n\nDo you want to restart ?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                System.Windows.Forms.Application.Restart();
            else
                Close();
        }

        private void sortOrderComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int order = sortOrderComboBox.SelectedIndex;
            SortOrder sortOrder;

            switch (order)
            {
                case 1:
                    sortOrder = SortOrder.Descending;
                    break;
                default:
                    sortOrder = SortOrder.Ascending;
                    break;
            }
            RPF6FileNameHandler.Sorting = sortOrder;
        }

        private void sortColumnComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int order = sortColumnComboBox.SelectedIndex;
            string sortOrder;

            switch (order)
            {
                case 1:
                    sortOrder = "Type";
                    break;
                case 2:
                    sortOrder = "Size";
                    break;
                default:
                    sortOrder = "Name";
                    break;
            }
            RPF6FileNameHandler.SortColumn = sortOrder;
        }

        private void checkBoxUseCustomColor_CheckedChanged(object sender, EventArgs e)
        {
            RPF6FileNameHandler.UseCustomColor = checkBoxUseCustomColor.Checked;
        }

        private void checkBoxUseLastRPF_CheckedChanged(object sender, EventArgs e)
        {
            RPF6FileNameHandler.UseLastRPF = checkBoxUseLastRPF.Checked;
            RPF6FileNameHandler.LastRPFPath = MainForm.CurrentRPFFileName ?? "None";
        }

        private void checkBoxShowPlusMinus_CheckedChanged(object sender, EventArgs e)
        {
            RPF6FileNameHandler.ShowPlusMinus = checkBoxShowPlusMinus.Checked;
        }

        private void checkBoxShowLines_CheckedChanged(object sender, EventArgs e)
        {
            RPF6FileNameHandler.ShowLines = checkBoxShowLines.Checked;
        }

        private void sizeModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int order = sizeModeComboBox.SelectedIndex;
            PictureBoxSizeMode mode;

            switch (order)
            {
                case 1:
                    mode = PictureBoxSizeMode.Zoom;
                    break;
                case 2:
                    mode = PictureBoxSizeMode.CenterImage;
                    break;
                case 3:
                    mode = PictureBoxSizeMode.StretchImage;
                    break;
                default:
                    mode = PictureBoxSizeMode.AutoSize;
                    break;
            }
            RPF6FileNameHandler.ImageSizeMode = mode;
        }

        private void backgroundTextureComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = backgroundTextureComboBox.SelectedIndex;
            Color background;

            switch (index)
            {
                case 0:
                    background = Color.Black;
                    break;
                case 1:
                    background = Color.White;
                    break;
                case 2:
                    background = Color.Red;
                    break;
                case 3:
                    background = Color.Green;
                    break;
                case 4:
                    background = Color.Blue;
                    break;
                default:
                    background = Color.Transparent;
                    break;
            }
            RPF6FileNameHandler.TextureBackgroundColor = background;
        }
    }
}
