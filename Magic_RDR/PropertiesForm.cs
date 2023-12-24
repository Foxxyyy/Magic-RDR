using Magic_RDR.RPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_RDR
{
    public partial class PropertiesForm : Form
    {
        private RPF6.RPF6TOC.TOCSuperEntry PropertyOfEntry;
        private RPF6.RPF6TOC.TOCEntry TempEntry;

        public bool ModifiedProperties { get; set; }

        public PropertiesForm(RPF6.RPF6TOC.TOCSuperEntry entry)
        {
            InitializeComponent();
            PropertyOfEntry = entry;

            if (PropertyOfEntry.IsDir)
            {
                RPF6.RPF6TOC.DirectoryEntry asDirectory = PropertyOfEntry.Entry.AsDirectory;
                RPF6.RPF6TOC.DirectoryEntry directoryEntry = new RPF6.RPF6TOC.DirectoryEntry();
                directoryEntry.Flags = asDirectory.Flags;
                directoryEntry.ContentEntryCount = asDirectory.ContentEntryCount;
                directoryEntry.ContentEntryIndex = asDirectory.ContentEntryIndex;
                directoryEntry.NameOffset = asDirectory.NameOffset;
                directoryEntry.Parent = asDirectory.Parent;
                directoryEntry.UNK = asDirectory.UNK;
                TempEntry = directoryEntry;
            }
            else
            {
                RPF6.RPF6TOC.FileEntry asFile = PropertyOfEntry.Entry.AsFile;
                RPF6.RPF6TOC.FileEntry fileEntry = new RPF6.RPF6TOC.FileEntry();
                fileEntry.NameOffset = asFile.NameOffset;
                fileEntry.Parent = asFile.Parent;
                fileEntry.SizeInArchive = asFile.SizeInArchive;
                fileEntry.Offset = asFile.Offset;
                fileEntry.FlagInfo = new ResourceUtils.FlagInfo()
                {
                    Flag1 = asFile.FlagInfo.Flag1,
                    Flag2 = asFile.FlagInfo.Flag2
                };
                TempEntry = fileEntry;
            }
            if (entry.Entry.AsFile.FlagInfo.IsResource)
            {
                MessageBox.Show(entry.Entry.AsFile.FlagInfo.ToString());
            }
            isFileResource.CheckedChanged -= new EventHandler(isFileResource_CheckedChanged);
            fileCompressed.CheckedChanged -= new EventHandler(fileCompressed_CheckedChanged);
            isFileResource.Checked = !entry.IsDir && entry.Entry.AsFile.FlagInfo.IsResource;
            fileCompressed.Checked = !entry.IsDir && entry.Entry.AsFile.FlagInfo.IsCompressed;
            isFileResource.CheckedChanged += new EventHandler(isFileResource_CheckedChanged);
            fileCompressed.CheckedChanged += new EventHandler(fileCompressed_CheckedChanged);
            propertiesOflabel.Text = string.Format("Properties Of {0}", PropertyOfEntry.IsDir ? "Directory" : "File");
            LoadDataFile();
        }

        private void applyChangesButton_Click(object sender, EventArgs e)
        {
            if (!PropertyOfEntry.IsDir)
            {
                RPF6.RPF6TOC.FileEntry asFile = PropertyOfEntry.Entry.AsFile;
                SaveDataFile();
            }
            ModifiedProperties = true;
            Close();
        }

        private void FlagBoxes_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;

            if (int.TryParse(text, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out int result) || !(text != string.Empty))
            {
                return;
            }
            textBox.Text = text.Remove(text.Length - 1, 1);
            textBox.SelectionStart = textBox.Text.Length;
        }

        private void isFileResource_CheckedChanged(object sender, EventArgs e)
        {
            fileResourceType.Enabled = isFileResource.Checked;
            TempEntry.AsFile.FlagInfo.IsResource = isFileResource.Checked;
            TempEntry.AsFile.ResourceType = isFileResource.Checked ? PropertyOfEntry.Entry.AsFile.ResourceType : (byte)0;
            LoadDataFile();
        }

        private void fileCompressed_CheckedChanged(object sender, EventArgs e)
        {
            isFileResource.CheckedChanged -= new EventHandler(isFileResource_CheckedChanged);
            isFileResource.Checked = false;
            int virtOrSize = PropertyOfEntry.CustomDataStream == null || PropertyOfEntry.ReadBackFromRPF ? TempEntry.AsFile.FlagInfo.GetTotalSize() : (int)PropertyOfEntry.CustomDataStream.Length;
            TempEntry.AsFile.FlagInfo.IsCompressed = fileCompressed.Checked;
            TempEntry.AsFile.FlagInfo.SetTotalSize(virtOrSize, 0);
            LoadDataFile();
            isFileResource.CheckedChanged += new EventHandler(isFileResource_CheckedChanged);
        }

        private void LoadDataFile()
        {
            isFileResource.CheckedChanged -= new EventHandler(isFileResource_CheckedChanged);
            propertiesOfNameLabel.Text = TempEntry.Name;
            fileInfoPanel.Visible = isFileResource.Visible = fileCompressed.Visible = !PropertyOfEntry.IsDir;

            if (!PropertyOfEntry.IsDir)
            {
                RPF6.RPF6TOC.FileEntry asFile = TempEntry.AsFile;
                fileResourceType.Value = (isFileResource.Checked ? asFile.ResourceType : 0);
                FileResourceFlag1.Text = asFile.FlagInfo.Flag1.ToString("X8");
                FileResourceFlag2.Text = asFile.FlagInfo.Flag2.ToString("X8");
                resourceFlag1Label.Text = string.Format("{0}", isFileResource.Checked ? "Resource Flag1:" : "Flag1:");
                resourceFlag2Label.Text = string.Format("{0}", isFileResource.Checked ? "Resource Flag2:" : "Flag2:");
                fileResourceStart.Text = asFile.FlagInfo.ResourceStart.ToString();
                fileInfoSize.Text = string.Format("{0}", asFile.SizeInArchive);
                fileCompressed.Visible = !isFileResource.Checked && !asFile.FlagInfo.IsExtendedFlags;
            }
            isFileResource.CheckedChanged += new EventHandler(isFileResource_CheckedChanged);
        }

        private void SaveDataFile()
        {
            if (PropertyOfEntry.IsDir)
            {
                return;
            }
            if (isFileResource.Checked)
            {
                TempEntry.AsFile.FlagInfo.IsResource = isFileResource.Checked;
                TempEntry.AsFile.ResourceType = (byte)fileResourceType.Value;
            }
            TempEntry.AsFile.FlagInfo.Flag1 = int.Parse(FileResourceFlag1.Text, NumberStyles.HexNumber);
            TempEntry.AsFile.FlagInfo.Flag2 = int.Parse(FileResourceFlag2.Text, NumberStyles.HexNumber);
            PropertyOfEntry.Entry = TempEntry;
        }
    }
}
