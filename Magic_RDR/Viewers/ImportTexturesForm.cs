using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using static Magic_RDR.Viewers.TextureViewerForm;
using static Magic_RDR.RPF.Texture;
using System.Linq;
using System.IO;
using System.Drawing;

namespace Magic_RDR.Viewers
{
    public partial class ImportTexturesForm : Form
    {
        public DialogResult DialogResultValue { get; private set; }
        private TextureInfo[] DefaultTextures;
        private List<string> CorrectTexturesPaths;
        private List<string> AddedTexturesPaths;
        private string EntryName;
        private int TotalCorrectTextures = 0;

        public ImportTexturesForm(string currentFileName)
        {
            this.InitializeComponent();

            this.CorrectTexturesPaths = new List<string>();
            this.AddedTexturesPaths = new List<string>();
            this.EntryName = currentFileName;
            this.DefaultTextures = XTD_TextureDictionary.TexInfos;
            this.validateButton.Enabled = false;

            this.UpdateListView();
        }

        private void addTextureButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open",
                Filter = "Direct Draw Surface File|*.dds"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!AddedTexturesPaths.Contains(dialog.FileName))
                {
                    this.AddedTexturesPaths.Add(dialog.FileName);
                    this.UpdateTextBox();
                }
            }
        }

        private void addDirectoryButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string[] files = Directory.GetFiles(dialog.SelectedPath, "*.dds");
                    foreach (string file in files)
                    {
                        try
                        {
                            if (!AddedTexturesPaths.Contains(file))
                            {
                                this.AddedTexturesPaths.Add(file);
                                this.UpdateTextBox();
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void validateButton_Click(object sender, EventArgs e)
        {
            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                string exportPath = Path.Combine(currentPath, "Temp");
 
                //Copy the selected valid DDS to the temp folder
                foreach (var file in this.CorrectTexturesPaths)
                {
                    File.Copy(file, exportPath + "\\" + this.EntryName.Replace(".wtd", "") + "\\" + Path.GetFileName(file), true);
                }
                this.DialogResultValue = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured :\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateListView()
        {
            List<ListViewItem> list = new List<ListViewItem>();
            foreach (var texture in this.DefaultTextures)
            {
                if (texture == null)
                {
                    continue;
                }

                string newTextureName = texture.TextureName;
                if (newTextureName.Contains("/"))
                {
                    newTextureName = newTextureName.Substring(newTextureName.LastIndexOf("/") + 1);
                }
                if (newTextureName.Contains(":"))
                {
                    newTextureName = newTextureName.Substring(newTextureName.LastIndexOf(":") + 1);
                }

                var item = new ListViewItem();
                item.Text = newTextureName;
                item.SubItems.Add(string.Format("{0}x{1}", texture.Width, texture.Height));
                item.SubItems.Add(texture.PixelFormat.ToString());
                list.Add(item);
            }
            this.columnTexture.Text = string.Format("Texture ({0})", this.DefaultTextures.Length);
            this.listView.BeginUpdate();
            this.listView.Items.Clear();
            this.listView.Items.AddRange(list.ToArray());
            this.listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.listView.EndUpdate();
        }

        private void UpdateTextBox()
        {
            var sb = new StringBuilder();
            foreach (var texture in this.AddedTexturesPaths)
            {
                if (!sb.ToString().Contains(texture))
                {
                    var matchingItems = this.listView.Items.Cast<ListViewItem>().
                        Where(item => item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                        .Any(subitem => subitem.Text.Equals(Path.GetFileName(texture))));

                    if (matchingItems.Any())
                    {
                        sb.AppendLine("FOUND : " + texture);
                        this.CorrectTexturesPaths.Add(texture);
                        this.validateButton.Enabled = true;
                        this.TotalCorrectTextures++;
                    }
                    else sb.AppendLine("NOT FOUND : " + texture);
                }
            }
            this.richTextBox1.Text = sb.ToString();
            this.correctTextureLabel.Text = "Correct Textures : " + TotalCorrectTextures.ToString();
            this.addedTextureLabel.Text = "Added Textures : " + AddedTexturesPaths.Count.ToString();

            // Color specific strings
            this.ColorText("FOUND", Color.Green);
            this.ColorText("NOT FOUND", Color.Red);
        }

        private void ColorText(string searchText, Color color)
        {
            int start = 0;
            while (start < this.richTextBox1.TextLength)
            {
                int foundStart = this.richTextBox1.Find(searchText, start, RichTextBoxFinds.None);
                if (foundStart == -1)
                {
                    break;
                }

                this.richTextBox1.SelectionStart = foundStart;
                this.richTextBox1.SelectionLength = searchText.Length;
                this.richTextBox1.SelectionColor = color;

                start = foundStart + searchText.Length;
            }
        }

        private void ImportTexturesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.TotalCorrectTextures > 0)
                this.DialogResultValue = DialogResult.OK;
            else
                this.DialogResultValue = DialogResult.Cancel;
        }
    }
}
