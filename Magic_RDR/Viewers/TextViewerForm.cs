using Magic_RDR.RPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Magic_RDR
{
    public partial class TextViewerForm : Form
    {
        public RPF6.RPF6TOC.TOCSuperEntry Entry;
        public byte[] FileData;
        private string OriginalFileContent;

        public TextViewerForm(RPF6.RPF6TOC.TOCSuperEntry entry, byte[] data)
        {
            InitializeComponent();
            Entry = entry;
            FileData = data;
            Text = string.Format("MagicRDR - TextViewer [{0}]", entry.Entry.Name);

            textBox.Text = Encoding.UTF8.GetString(data);
            OriginalFileContent = textBox.Text;
            saveButton.Enabled = !entry.Entry.Name.EndsWith(".dat");

            charCountLabel.Text = string.Format("{0} characters, {1} lines", textBox.Text.Length, textBox.LinesCount);
            zoomLabel.Text = string.Format("Zoom {0}%", textBox.Zoom);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox.Text) || Entry == null)
            {
                MessageBox.Show("There's nothing to export...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.FileName = Entry.Entry.Name;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, textBox.Text);
                MessageBox.Show("Successfully exported !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (Entry == null)
                return;

            if (textBox.Text == OriginalFileContent)
            {
                MessageBox.Show("No need to save, you didn't change anything...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("This will overwrite the current file\n\nContinue ?", "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            RPF6.RPF6TOC.TOCSuperEntry NewEntry = new RPF6.RPF6TOC.TOCSuperEntry();
            byte[] data = Encoding.UTF8.GetBytes(textBox.Text);

            NewEntry.CustomDataStream = new MemoryStream(data);
            NewEntry.OldEntry = Entry.Entry;
            NewEntry.ReadBackFromRPF = false;

            RPF6.RPF6TOC.FileEntry fileEntry = new RPF6.RPF6TOC.FileEntry()
            {
                FlagInfo = new ResourceUtils.FlagInfo()
            };
            fileEntry.FlagInfo.Flag1 = Entry.Entry.AsFile.FlagInfo.Flag1;
            fileEntry.FlagInfo.Flag2 = Entry.Entry.AsFile.FlagInfo.Flag2;
            if (Entry.Entry.AsFile.FlagInfo.IsCompressed)
            {
                fileEntry.FlagInfo.IsCompressed = true;
            }

            byte[] temp;
            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                temp = DataUtils.CompressZStandard(data);
            else
                temp = DataUtils.Compress(data, 9);

            fileEntry.FlagInfo.SetTotalSize(data.Length, 0);
            fileEntry.SizeInArchive = temp.Length;
            fileEntry.NameOffset = Entry.Entry.NameOffset;
            NewEntry.Entry = fileEntry;
            NewEntry.Entry.AsFile.KeepOffset = new long?(NewEntry.OldEntry.AsFile.GetOffset());

            RPF6.RPF6TOC.ReplaceEntry(Entry, NewEntry);
            MessageBox.Show("Successfully saved file !\nMake sure to also save the RPF to see changes", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MainForm.HasJustEditedRegularFile = true;
            this.Close();
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Ctrl + C to copy");
            sb.AppendLine("Ctrl + V to paste");
            sb.AppendLine("Ctrl + Z to undo");
            sb.AppendLine("Ctrl + Y to redo");
            sb.AppendLine("Ctrl + F to search");
            sb.AppendLine("Alt + Mouse Wheel to zoom");
            MessageBox.Show(sb.ToString(), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = toolStripComboBox1.SelectedIndex;
            FastColoredTextBoxNS.Language language;

            switch (index)
            {
                case 1:
                    language = FastColoredTextBoxNS.Language.CSharp;
                    break;
                case 2:
                    language = FastColoredTextBoxNS.Language.VB;
                    break;
                case 3:
                    language = FastColoredTextBoxNS.Language.HTML;
                    break;
                case 4:
                    language = FastColoredTextBoxNS.Language.SQL;
                    break;
                case 5:
                    language = FastColoredTextBoxNS.Language.PHP;
                    break;
                case 6:
                    language = FastColoredTextBoxNS.Language.JS;
                    break;
                case 7:
                    language = FastColoredTextBoxNS.Language.Lua;
                    break;
                default:
                    language = FastColoredTextBoxNS.Language.XML;
                    break;
            }
            textBox.Language = language;
            string backup = textBox.Text;
            textBox.Text = "";
            textBox.Text = backup;
            textBox.Refresh();
        }

        private void textBox_ZoomChanged(object sender, EventArgs e)
        {
            zoomLabel.Text = string.Format("Zoom {0}%", textBox.Zoom);
        }

        private void textBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            charCountLabel.Text = string.Format("{0} characters, {1} lines", textBox.Text.Length, textBox.LinesCount);
        }

        private void zoomLabel_Click(object sender, EventArgs e)
        {
            textBox.Zoom = 100;
        }
    }
}
