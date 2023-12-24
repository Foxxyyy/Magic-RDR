using Magic_RDR.RPF;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Magic_RDR
{
    public partial class NewImportReplaceForm : Form
    {
        public RPF6.RPF6TOC.TOCSuperEntry TOCResult;
        private Stream ImportFileStream;
        private Stream ReplaceFileStream;
        public FileMode Mode;
        private RPF6.RPF6TOC.TOCSuperEntry SelectedFile;
        private RPF6.RPF6TOC.TOCSuperEntry SelectedReplaceFile;
        private string NewImportFileName;
        private string NewReplaceFileName;

        public NewImportReplaceForm(bool replaceTab, RPF6.RPF6TOC.TOCSuperEntry fileParentDir, RPF6.RPF6TOC.TOCSuperEntry selectedFile = null, RPF6.RPF6TOC.TOCSuperEntry selectedReplaceFile = null, string importLoc = null, string replaceLoc = null, bool skipChecks = false, Stream fileStream = null)
        {
            InitializeComponent();
            SelectedFile = selectedFile;
            NewImportFileName = importLoc;
            NewReplaceFileName = replaceLoc;
            SelectedReplaceFile = selectedReplaceFile;

            if (MainForm.CommandLine)
            {
                foreach (Control control in this.Controls)
                {
                    control.Visible = false;
                }
            }

            if (fileStream != null)
                ReplaceFileStream = fileStream;

            if (selectedFile == null || !replaceTab)
                Mode = FileMode.Import;
            if (SelectedFile != null && !SelectedFile.IsDir)
                Mode |= FileMode.Replace;

            ConfigureTabs(replaceTab);
            if (skipChecks)
            {
                if (selectedFile == null || !replaceTab)
                    importFileButton_Click(null, null);
                else
                    replaceButton_Click(null, null);
            }
        }

        private void LoadImportTabData()
        {
            ImportFileStream = File.Open(NewImportFileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
            (bool, string, int, int, int) resourceInformation = ResourceUtils.ResourceInfo.GetResourceInformation(ImportFileStream);

            if (resourceInformation.Item1)
            {
                importResourceVersionLabel.Text = string.Format("Resource Version: {0}", resourceInformation.Item2);
                importResourceType.Value = resourceInformation.Item3;
                importResourceFlag1.Text = resourceInformation.Item4.ToString("X8");
                importResourceFlag2.Text = resourceInformation.Item5.ToString("X8");
                importFileResourcePanel.Enabled = true;
            }
            else
            {
                importFileResourcePanel.Enabled = false;
            }
        }

        private void LoadReplaceTabData()
        {
            if (ReplaceFileStream == null)
            {
                ReplaceFileStream = File.Open(NewReplaceFileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            (bool, string, int, int, int) resourceInformation = ResourceUtils.ResourceInfo.GetResourceInformation(ReplaceFileStream);

            if (resourceInformation.Item1)
            {
                replaceResourceVersionLabel.Text = string.Format("Resource Version: {0}", resourceInformation.Item2);
                replaceResourceType.Value = resourceInformation.Item3;
                replaceResourceFlag1.Text = resourceInformation.Item4.ToString("X8");
                replaceResourceFlag2.Text = resourceInformation.Item5.ToString("X8");
                replaceFileResourcePanel.Enabled = true;
            }
            else
            {
                replaceFileResourcePanel.Enabled = false;
            }
        }

        private void LoadImportTab()
        {
            import_Append.Enabled = true;
            if (SelectedFile != null && !SelectedFile.IsDir)
            {
                import_Before.Enabled = true;
                import_After.Enabled = true;
                import_After.Text = string.Format("After {0}", SelectedFile.Entry.Name);
                import_Before.Text = string.Format("Before {0}", SelectedFile.Entry.Name);
            }

            bool flag = false;
            if (NewImportFileName != null)
            {
                flag = true;
                importFileName.Text = NewImportFileName;
            }
            if (!flag)
            {
                return;
            }
            LoadImportTabData();
        }

        private void LoadReplaceTab()
        {
            bool flag = false;
            replacingFileName.Text = SelectedReplaceFile == null ? SelectedFile.Entry.Name : SelectedReplaceFile.Entry.Name;

            if (NewReplaceFileName != null)
            {
                flag = true;
                replacingWithFileName.Text = NewReplaceFileName;
            }
            if (!flag)
            {
                return;
            }
            LoadReplaceTabData();
        }

        private void ConfigureTabs(bool repTab)
        {
            if (repTab)
                mainTabControl.SelectedTab = replaceTab;

            if (Mode == FileMode.Import)
                mainTabControl.TabPages.Remove(replaceTab);
            LoadImportTab();

            if ((Mode & FileMode.Replace) != FileMode.Replace)
                return;
            LoadReplaceTab();
        }

        private void importFileButton_Click(object sender, EventArgs e)
        {
            RPF6.RPF6TOC.TOCSuperEntry tocSuperEntry = new RPF6.RPF6TOC.TOCSuperEntry();
            tocSuperEntry.CustomDataStream = ImportFileStream;

            RPF6.RPF6TOC.FileEntry fileEntry = new RPF6.RPF6TOC.FileEntry()
            {
                FlagInfo = new ResourceUtils.FlagInfo()
            };
            fileEntry.FlagInfo.Flag1 = DataUtils.IntFromUInt(string.IsNullOrEmpty(importResourceFlag1.Text) ? 0U : uint.Parse(importResourceFlag1.Text, NumberStyles.HexNumber));
            fileEntry.FlagInfo.Flag2 = DataUtils.IntFromUInt(string.IsNullOrEmpty(importResourceFlag2.Text) ? 0U : uint.Parse(importResourceFlag2.Text, NumberStyles.HexNumber));

            if (fileEntry.FlagInfo.IsResource)
                fileEntry.ResourceType = (byte)importResourceType.Value;
            else if (fileEntry.FlagInfo.IsCompressed || ResourceUtils.IsTextFile(Path.GetFileName(importFileName.Text)))
            {
                fileEntry.FlagInfo.IsCompressed = true;
                fileEntry.FlagInfo.SetTotalSize((int)ImportFileStream.Length, 0);
            }
            else
                fileEntry.FlagInfo.SetTotalSize((int)ImportFileStream.Length, 0);

            fileEntry.SizeInArchive = (int)ImportFileStream.Length;
            tocSuperEntry.Entry = fileEntry;
            fileEntry.NameOffset = DataUtils.GetHash(Path.GetFileName(importFileName.Text));

            if (SelectedFile != null)
            {
                switch (import_Before.Checked ? ImportOrder.Before : (import_After.Checked ? ImportOrder.After : ImportOrder.Append))
                {
                    case ImportOrder.Before:
                        SelectedFile.WriteBefore_Children.Add(tocSuperEntry);
                        break;
                    case ImportOrder.After:
                        SelectedFile.WriteAfterChildren.Add(tocSuperEntry);
                        break;
                }
            }
            TOCResult = tocSuperEntry;
            Mode = FileMode.Import;
            Close();
        }

        private void replaceButton_Click(object sender, EventArgs e)
        {
            RPF6.RPF6TOC.TOCSuperEntry tocSuperEntry1 = new RPF6.RPF6TOC.TOCSuperEntry();
            RPF6.RPF6TOC.TOCSuperEntry tocSuperEntry2 = SelectedReplaceFile == null ? SelectedFile : SelectedReplaceFile;
            tocSuperEntry1.CustomDataStream = ReplaceFileStream;
            tocSuperEntry1.OldEntry = tocSuperEntry2.Entry;
            tocSuperEntry1.ReadBackFromRPF = false;

            RPF6.RPF6TOC.FileEntry fileEntry = new RPF6.RPF6TOC.FileEntry()
            {
                FlagInfo = new ResourceUtils.FlagInfo()
            };
            fileEntry.FlagInfo.Flag1 = DataUtils.IntFromUInt(string.IsNullOrEmpty(replaceResourceFlag1.Text) ? 0U : uint.Parse(replaceResourceFlag1.Text, NumberStyles.HexNumber));
            fileEntry.FlagInfo.Flag2 = DataUtils.IntFromUInt(string.IsNullOrEmpty(replaceResourceFlag2.Text) ? 0U : uint.Parse(replaceResourceFlag2.Text, NumberStyles.HexNumber));

            if (fileEntry.FlagInfo.IsResource)
                fileEntry.ResourceType = (byte)replaceResourceType.Value;
            else if (fileEntry.FlagInfo.IsCompressed || ResourceUtils.IsTextFile(Path.GetFileName(replacingWithFileName.Text)))
            {
                fileEntry.FlagInfo.IsCompressed = true;
                fileEntry.FlagInfo.SetTotalSize((int)ReplaceFileStream.Length, 0);
            }
            else
                fileEntry.FlagInfo.SetTotalSize((int)ReplaceFileStream.Length, 0);

            fileEntry.SizeInArchive = (int)ReplaceFileStream.Length;
            if (replaceReplaceNameCheck.Checked)
            {
                fileEntry.NameOffset = DataUtils.GetHash(replacingWithFileName.Text);
                RPF6FileNameHandler.AddName(Path.GetFileName(replacingWithFileName.Text));
                RPF6FileNameHandler.SaveNames();
            }
            else fileEntry.NameOffset = tocSuperEntry2.Entry.NameOffset;

            tocSuperEntry1.Entry = fileEntry;
            tocSuperEntry1.Entry.AsFile.KeepOffset = new long?(tocSuperEntry1.OldEntry.AsFile.GetOffset());
            TOCResult = tocSuperEntry1;
            Mode = FileMode.Replace;
            Close();
        }

        private void importOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Import File...";
            openFileDialog.Filter = "|*.*";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            if (ImportFileStream != null)
                ImportFileStream.Close();

            ImportFileStream = null;
            NewImportFileName = openFileDialog.FileName;
            LoadImportTab();
        }

        private void replaceFileOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = string.Format("Replace {0} with...", SelectedFile.Entry.Name);
            openFileDialog.Filter = "|*.*";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            if (ReplaceFileStream != null)
                ReplaceFileStream.Close();

            ReplaceFileStream = null;
            NewReplaceFileName = openFileDialog.FileName;
            LoadReplaceTab();
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

        public enum FileMode
        {
            Import,
            Replace,
        }

        public enum ImportOrder
        {
            Append,
            Before,
            After,
        }
    }
}
