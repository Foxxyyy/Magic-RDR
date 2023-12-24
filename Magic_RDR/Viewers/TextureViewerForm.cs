using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Magic_RDR.Application;
using Magic_RDR.RPF;
using Pik.IO;
using static Magic_RDR.RPF.Texture;
using static Magic_RDR.RPF6.RPF6TOC;

namespace Magic_RDR.Viewers
{
    public partial class TextureViewerForm : Form
    {
        private TOCSuperEntry Entry;
        private Image Background;
        private IOReader Reader;
        private IOWriter Writer;
        private FileEntry file;
        private Dictionary<BitmapSource, string> FixedTextureList;
        private byte[] CompressedData, HeaderData;
        public int FileFlags;
        private List<(DDS, TextureInfo)> LargerTextures;
        private XTD_TextureDictionary textureDictionary;

        public TextureViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Text = string.Format("MagicRDR - Texture Viewer [{0}]", entry.Entry.Name);
            Entry = entry;
            Background = splitContainer1.Panel2.BackgroundImage;
            importButton.Enabled = !entry.Entry.Name.EndsWith(".dds");

            //Load settings
            splitContainer1.Panel2.BackgroundImage = null;
            pictureBox.SizeMode = RPF6FileNameHandler.ImageSizeMode;
            splitContainer1.Panel2.BackColor = RPF6FileNameHandler.TextureBackgroundColor;
            if (splitContainer1.Panel2.BackColor == Color.Transparent)
            {
                splitContainer1.Panel2.BackgroundImage = Background;
            }

            FileFlags = -1;
            LargerTextures = new List<(DDS, TextureInfo)>();
            file = entry.Entry.AsFile;

            RPFFile.RPFIO.Position = file.GetOffset();
            CompressedData = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            byte[] data = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(CompressedData);

            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                HeaderData = new byte[0xC];
            else if (entry.Entry.Name.EndsWith(".xsf") || entry.Entry.Name.EndsWith(".wsf") || file.Name.Contains("swall.xtd") || file.Name.Contains("swall.wtd"))
                HeaderData = new byte[0x14];
            else
                HeaderData = new byte[0x18];

            for (int i = 0; i < HeaderData.Length; i++)
            {
                HeaderData[i] = CompressedData[i];
            }

            #region Execution

            if (entry.Entry.Name.EndsWith(".dds"))
            {
                FileFlags = -1;
                this.multiImportButton.Enabled = false;
                if (data == null)
                {
                    RPFFile.RPFIO.Position = 0x0;
                    data = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
                    try
                    {
                        if (file.FlagInfo.IsCompressed && !file.FlagInfo.IsResource)
                        {
                            RPFFile.RPFIO.Position = file.GetOffset();
                            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                                data = DataUtils.DecompressZStandard(RPFFile.RPFIO.ReadBytes(file.SizeInArchive));
                            else
                                data = DataUtils.DecompressDeflate(RPFFile.RPFIO.ReadBytes(file.SizeInArchive), file.FlagInfo.GetTotalSize());
                        }
                    }
                    catch { }
                }

                if (data == null)
                {
                    MessageBox.Show("An error occured while reading file\n\nViewer will now close itself...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }

                var image = Pfim.Pfim.FromStream(new MemoryStream(data));
                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    var rawData = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, rawData);
                    pictureBox.Image = bitmap;
                }
                finally
                {
                    handle.Free();
                }

                List<ListViewItem> list = new List<ListViewItem>();
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = entry.Entry.Name;
                listViewItem.SubItems.Add(string.Format("{0}x{1}", image.Height, image.Width));
                listViewItem.SubItems.Add(image.Format.ToString().ToUpper());
                listViewItem.SubItems.Add("1");
                listViewItem.SubItems.Add(image.DataLen.ToString());
                list.Add(listViewItem);

                listView.BeginUpdate();
                listView.Items.Clear();
                listView.Items.AddRange(list.ToArray());
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listView.EndUpdate();
            }
            else if (entry.Entry.Name.EndsWith(".xtx") || entry.Entry.Name.EndsWith(".wtx"))
            {
                FileFlags = 1;
                if (data == null)
                {
                    return;
                }
                if (entry.Entry.Name.EndsWith(".wtx"))
                {
                    this.saveButton.Enabled = false;
                    this.importButton.Enabled = false;
                }
                this.multiImportButton.Enabled = false;
                Reader = new IOReader(new MemoryStream(data), AppGlobals.Platform == AppGlobals.PlatformEnum.Switch ? IOReader.Endian.Little : IOReader.Endian.Big);
                Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

                XTX_TextureDictionary textures = new XTX_TextureDictionary(Reader, file);
                BitmapSource[] Textures = new BitmapSource[] { XTX_TextureDictionary.Textures };

                if (Textures != null)
                {
                    int valid = 0;
                    while (Textures[valid] == null)
                    {
                        valid++;
                    }
                    pictureBox.Image = Image.FromStream(new MemoryStream(BufferFromImageSource(Textures[valid])));
                    List<ListViewItem> list = new List<ListViewItem>();
                    FixedTextureList = new Dictionary<BitmapSource, string>();

                    for (int i = 0; i < Textures.Length; i++)
                    {
                        if (Textures[i] == null)
                            continue;

                        var tex = XTX_TextureDictionary.TexInfos;
                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = tex.TextureName;
                        listViewItem.SubItems.Add(string.Format("{0}x{1}", Textures[i].Height, Textures[i].Width));
                        listViewItem.SubItems.Add(tex.PixelFormat.ToString());
                        listViewItem.SubItems.Add(tex.MipMaps.ToString());
                        listViewItem.SubItems.Add(tex.TextureSize.ToString());

                        list.Add(listViewItem);
                        FixedTextureList.Add(Textures[i], tex.TextureName);
                    }
                    columnTexture.Text = string.Format("Texture ({0})", Textures.Length);
                    listView.BeginUpdate();
                    listView.Items.Clear();
                    listView.Items.AddRange(list.ToArray());
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    listView.EndUpdate();
                }
            }
            else if (entry.Entry.Name.EndsWith(".xtd") || file.ResourceType == 10)
            {
                FileFlags = 0;
                if (data == null)
                {
                    return;
                }

                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                Reader = new IOReader(new MemoryStream(data), isSwitchVersion ? IOReader.Endian.Little : IOReader.Endian.Big);
                Writer = new IOWriter(new MemoryStream(), isSwitchVersion ? IOWriter.Endian.Little : IOWriter.Endian.Big);
                Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

                if (!isSwitchVersion)
                {
                    this.multiImportButton.Enabled = false;
                }
                textureDictionary = new XTD_TextureDictionary(Reader, file);
                BitmapSource[] Textures = XTD_TextureDictionary.Textures;

                if (Textures != null && Textures.Length > 0)
                {
                    int valid = 0;
                    while (Textures[valid] == null)
                    {
                        valid++;
                        if (valid >= Textures.Length)
                            return;
                    }
                    pictureBox.Image = Image.FromStream(new MemoryStream(BufferFromImageSource(Textures[valid])));
                    List<ListViewItem> list = new List<ListViewItem>();
                    FixedTextureList = new Dictionary<BitmapSource, string>();

                    for (int i = 0; i < Textures.Length; i++)
                    {
                        if (Textures[i] == null)
                            continue;

                        var t = XTD_TextureDictionary.TexInfos[i];
                        var listViewItem = new ListViewItem
                        {
                            Text = t.TextureName
                        };
                        listViewItem.SubItems.Add(string.Format("{0}x{1}", Textures[i].Width, Textures[i].Height));
                        listViewItem.SubItems.Add(t.PixelFormat.ToString());
                        listViewItem.SubItems.Add(t.MipMaps.ToString());
                        listViewItem.SubItems.Add(t.TextureSize.ToString());
                        list.Add(listViewItem);
                        FixedTextureList.Add(Textures[i], t.TextureName);
                    }
                    columnTexture.Text = string.Format("Texture ({0})", Textures.Length);
                    listView.BeginUpdate();
                    listView.Items.Clear();
                    listView.Items.AddRange(list.ToArray());
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    listView.EndUpdate();
                }
            }
            else if (entry.Entry.Name.EndsWith(".xsf") || entry.Entry.Name.EndsWith(".wsf"))
            {
                FileFlags = 2;
                if (data == null)
                {
                    return;
                }
                if (entry.Entry.Name.EndsWith(".wsf"))
                {
                    this.saveButton.Enabled = false;
                    this.importButton.Enabled = false;
                }
                this.multiImportButton.Enabled = false;
                Reader = new IOReader(new MemoryStream(data), AppGlobals.Platform == AppGlobals.PlatformEnum.Switch ? IOReader.Endian.Little : IOReader.Endian.Big);
                Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

                XSF_TextureResource textures = new XSF_TextureResource(Reader, file);
                BitmapSource[] Textures = XSF_TextureResource.ImagesSource;

                if (Textures != null)
                {
                    int valid = 0;
                    while (Textures[valid] == null)
                    {
                        valid++;
                    }
                    pictureBox.Image = Image.FromStream(new MemoryStream(BufferFromImageSource(Textures[valid])));
                    List<ListViewItem> list = new List<ListViewItem>();
                    FixedTextureList = new Dictionary<BitmapSource, string>();

                    for (int i = 0; i < Textures.Length; i++)
                    {
                        if (Textures[i] == null)
                            continue;

                        var t = XSF_TextureResource.TexInfos[i];
                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = t.TextureName;
                        listViewItem.SubItems.Add(string.Format("{0}x{1}", Textures[i].Height, Textures[i].Width));
                        listViewItem.SubItems.Add(t.PixelFormat.ToString());
                        listViewItem.SubItems.Add(t.MipMaps.ToString());
                        listViewItem.SubItems.Add(t.TextureSize.ToString());
                        list.Add(listViewItem);
                        FixedTextureList.Add(Textures[i], t.TextureName);
                    }
                    columnTexture.Text = string.Format("Texture ({0})", Textures.Length);
                    listView.BeginUpdate();
                    listView.Items.Clear();
                    listView.Items.AddRange(list.ToArray());
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    listView.EndUpdate();
                }
            }

            #endregion
        }

        #region Controls

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!MainForm.HasJustEditedTexture)
            {
                MessageBox.Show("You didn't import any texture, no need to save !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] newData = null;
                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                BitmapSource[] textures = XTD_TextureDictionary.Textures;

                /*if (textures != null && textures.Length > 0 && isSwitchVersion)
                {
                    if (textures.Length > 1)
                    {
                        Writer.BaseStream.Position = 0;
                        textureDictionary.XTD_WriteTextureDictionary(LargerTextures, ref Writer);

                        byte[] b = Enumerable.Repeat((byte)0xCD, (int)(PikIO.Utils.RoundUp(Writer.BaseStream.Position, 2048L) - Writer.BaseStream.Position)).ToArray();
                        Writer.WriteBytes(b);

                        Writer.BaseStream.Position = 0;
                        newData = Writer.ReadBytes(Writer.BaseStream.Length);
                    }
                    else if (LargerTextures.Count > 0)
                    {
                        Writer.BaseStream.Position = 0;
                        textureDictionary.XTD_WriteTextureDictionary(LargerTextures, ref Writer);

                        byte[] b = Enumerable.Repeat((byte)0xCD, (int)(PikIO.Utils.RoundUp(Writer.BaseStream.Position, 2048L) - Writer.BaseStream.Position)).ToArray();
                        Writer.WriteBytes(b);

                        Writer.BaseStream.Position = 0;
                        newData = Writer.ReadBytes(Writer.BaseStream.Length);
                    }
                }

                int vSize = textures.Length * 256;
                File.WriteAllBytes(@"C:\Users\fumol\OneDrive\Bureau\test1.data", newData);
                byte[] packedData = ResourceUtils.FlagInfo.RSC05_PackResource(newData, vSize, newData.Length - vSize, 10, AppGlobals.Platform, true);
                File.WriteAllBytes(@"C:\Users\fumol\OneDrive\Bureau\test1.wtd", packedData);*/

                //Recompress into a single block
                if (LargerTextures.Count <= 0)
                {
                    Reader.BaseStream.Position = 0;
                    newData = Reader.ReadBytes(Reader.BaseStream.Length);
                }

                if (isSwitchVersion)
                    newData = DataUtils.CompressZStandard(newData);
                else
                    newData = DataUtils.CompressLZX(newData);

                if (!isSwitchVersion)
                {
                    byte[] dataLength = BitConverter.GetBytes(newData.Length.Swap());
                    if (FileFlags == 2 || file.Name.Contains("swall.xtd"))
                    {
                        HeaderData[16] = dataLength[0];
                        HeaderData[17] = dataLength[1];
                        HeaderData[18] = dataLength[2];
                        HeaderData[19] = dataLength[3];
                    }
                    else
                    {
                        HeaderData[20] = dataLength[0];
                        HeaderData[21] = dataLength[1];
                        HeaderData[22] = dataLength[2];
                        HeaderData[23] = dataLength[3];
                    }
                }

                byte[] buffer = new byte[HeaderData.Length + newData.Length];
                Buffer.BlockCopy(HeaderData, 0, buffer, 0, HeaderData.Length);
                Buffer.BlockCopy(newData, 0, buffer, HeaderData.Length, newData.Length);

                TOCSuperEntry selectedReplaceFile = Entry;
                TOCSuperEntry selectedFile = Entry;
                NewImportReplaceForm importReplaceForm = new NewImportReplaceForm(true, Entry.SuperParent, selectedFile, selectedReplaceFile, null, Entry.Entry.Name, false, new MemoryStream(buffer));

                if (!importReplaceForm.IsDisposed)
                    importReplaceForm.ShowDialog();
                if (importReplaceForm.TOCResult == null)
                    return;

                selectedFile.CustomDataStream?.Close();
                selectedFile.CustomDataStream = importReplaceForm.TOCResult.CustomDataStream;
                selectedFile.Entry = importReplaceForm.TOCResult.Entry;
                selectedFile.OldEntry = importReplaceForm.TOCResult.OldEntry;
                selectedFile.ReadBackFromRPF = false;

                MessageBox.Show("Succesfully rebuilt the file !\nMake sure to save the .RPF", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MainForm.HasJustEditedTexture = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while saving the file :\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void importButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("Current image is invalid", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Open",
                Filter = "Direct Draw Surface File|*.dds"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int index = listView.FocusedItem.Index;
                var texInfos = FileFlags == 0 ? XTD_TextureDictionary.TexInfos : FileFlags == 1 ? new TextureInfo[] { XTX_TextureDictionary.TexInfos } : XSF_TextureResource.TexInfos;

                try
                {
                    var largerTexture = InjectDDS(Reader, dialog.FileName, texInfos[index], Writer);
                    //LargerTextures.Add((largerTexture.Item1, largerTexture.Item2));

                    MainForm.HasJustEditedTexture = true;
                    this.multiImportButton.Enabled = false;
                    MessageBox.Show("Succesfully imported texture.\nMake sure to rebuild the file!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while importing texture :\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void multiImportButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("Current image is invalid", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                string exportFolderName = "Temp";
                string wtdName = Entry.Entry.Name;

                string exportPath = Path.Combine(currentPath, exportFolderName);
                string wtdPath = Path.Combine(exportPath, wtdName);
                string executablePath = Path.Combine(exportPath, "convert.exe");

                //Make sure we have a export directory for our application
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }

                //Export the current .wtd to the temp folder
                File.WriteAllBytes(wtdPath, CompressedData);

                //Unpack the file and prepare data using the batch
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = executablePath;
                    process.StartInfo.Arguments = $"\"{wtdPath}\"";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();

                    Thread.Sleep(500);
                    try //Rather catch a exception than keep an unused process in memory
                    {
                        process.CloseMainWindow();
                        KillProcessAndChildren(process.Id);
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill();
                        }
                        process.Dispose();
                    }
                    catch
                    {

                    }
                }

                using (ImportTexturesForm form = new ImportTexturesForm(Entry.Entry.Name))
                {
                    form.ShowDialog();
                    if (form.DialogResultValue == DialogResult.Cancel)
                    {
                        return;
                    }
                    MessageBox.Show("Importing textures...\n\nIn the upcoming dialog, click 'Replace' to save and do not select any options", "Importing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //Unpack the file and prepare data using the batch
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = executablePath;
                    process.StartInfo.Arguments = $"\"{wtdPath.Replace(".wtd", ".otd")}\"";
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.UseShellExecute = false;
                    process.Start();

                    Thread.Sleep(1500);
                    try //Rather catch a exception than keep an unused process in memory
                    {
                        process.CloseMainWindow();
                        KillProcessAndChildren(process.Id);
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill();
                        }
                        process.Dispose();
                    }
                    catch
                    {

                    }
                }

                //Read the new .wtd created
                byte[] buffer = File.ReadAllBytes(wtdPath);

                //Remove all the useless files
                string[] files = Directory.GetFiles(exportPath);
                foreach (var file in files)
                {
                    string filename = Path.GetFileName(file);
                    if (File.Exists(file) && !filename.EndsWith(".exe") && !filename.EndsWith(".ini"))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }

                //Remove all the useless directories
                string[] subDirectories = Directory.GetDirectories(exportPath);
                foreach (var subDirectory in subDirectories)
                {
                    Directory.Delete(subDirectory, true);
                }

                //Import and replace the .wtd
                TOCSuperEntry selectedReplaceFile = Entry;
                TOCSuperEntry selectedFile = Entry;
                NewImportReplaceForm importReplaceForm = new NewImportReplaceForm(true, Entry.SuperParent, selectedFile, selectedReplaceFile, null, Entry.Entry.Name, false, new MemoryStream(buffer));

                if (!importReplaceForm.IsDisposed)
                    importReplaceForm.ShowDialog();
                if (importReplaceForm.TOCResult == null)
                    return;

                selectedFile.CustomDataStream?.Close();
                selectedFile.CustomDataStream = importReplaceForm.TOCResult.CustomDataStream;
                selectedFile.Entry = importReplaceForm.TOCResult.Entry;
                selectedFile.OldEntry = importReplaceForm.TOCResult.OldEntry;
                selectedFile.ReadBackFromRPF = false;

                MessageBox.Show("Succesfully rebuilt the file !\nMake sure to save the .RPF", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MainForm.HasJustEditedTexture = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured :\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void KillProcessAndChildren(int pid)
        {
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
            {
                var moc = searcher.Get();
                foreach (ManagementObject mo in moc)
                {
                    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                }
                try
                {
                    var proc = Process.GetProcessById(pid);
                    proc.Kill();
                }
                catch (Exception e)
                {
                    //Process already exited.
                }
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FixedTextureList == null)
                return;

            int index = listView.FocusedItem.Index;
            BitmapSource image = FixedTextureList.ElementAt(index).Key;
            if (image == null)
            {
                return;
            }
            pictureBox.Image = Image.FromStream(new MemoryStream(BufferFromImageSource(image)));
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("There's nothing to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int index = listView.FocusedItem.Index;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = FixedTextureList.ElementAt(index).Value.Replace(".dds", "").Replace(".xtd", "");
            dialog.Filter = "Portable Network Graphics (*.png)|*.png|DDS Image (*.dds)|*.dds";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FileName.EndsWith(".dds"))
                {
                    var texInfos = FileFlags == 0 ? XTD_TextureDictionary.TexInfos : FileFlags == 1 ? new TextureInfo[] { XTX_TextureDictionary.TexInfos } : XSF_TextureResource.TexInfos;
                    try
                    {
                        SaveDDS(Reader, dialog.FileName, texInfos[index]);
                        MessageBox.Show("Successfully exported texture", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occured while saving texture :\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    using (var fileStream = new FileStream(dialog.FileName, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(FixedTextureList.ElementAt(index).Key));
                        encoder.Save(fileStream);
                        MessageBox.Show("Successfully exported texture", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void exportAllButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("There's nothing to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string extension = ".png";
            if (MessageBox.Show("Would you like to export all textures in .png format ?\n\nOtherwise it will be .dds", "Choose a format", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.No)
            {
                extension = ".dds";
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.Description = "Select a destination to export textures";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int count = 0;
                for (int i = 0; i < FixedTextureList.Count; i++)
                {
                    if (FixedTextureList.ElementAt(i).Key == null) //Shouldn't be possible
                        continue;

                    string texture = FixedTextureList.ElementAt(i).Value.Replace(".dds", extension).Replace(".xtd", extension).Replace("/", "-");
                    texture = texture.Substring(texture.LastIndexOf(":") + 1); //Remove invalid characters

                    if (texture.EndsWith(".dds"))
                    {
                        var texInfos = FileFlags == 0 ? XTD_TextureDictionary.TexInfos : FileFlags == 1 ? new TextureInfo[] { XTX_TextureDictionary.TexInfos } : XSF_TextureResource.TexInfos;
                        try
                        {
                            SaveDDS(Reader, dialog.SelectedPath + "\\" + texture, texInfos[i]);
                            count++;
                        }
                        catch { } //Just in case
                    }
                    else
                    {
                        using (var fileStream = new FileStream(dialog.SelectedPath + "\\" + texture, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(FixedTextureList.ElementAt(i).Key));
                            encoder.Save(fileStream);
                            count++;
                        }
                    }
                }
                MessageBox.Show(string.Format("Successfully exported {0} texture(s)", count), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        #endregion

        #region XTD

        public class XTD_TextureDictionary
        {
            private IOReader Reader;
            private FileEntry Entry;
            public static BitmapSource[] Textures;
            public static TextureInfo[] TexInfos;

            private TextureType FormatType;
            private int TextureDataPointer, MipDataPointer;

            //Texture structure
            public uint VTable { get; set; }
            public uint BlockMapPointer { get; set; }
            public uint ParentDictionary { get; set; }
            public uint UsageCount { get; set; }
            public uint HashTablePointer { get; set; }
            public ushort TextureCount { get; set; }
            public ushort TextureCount2 { get; set; }
            public uint TextureListPointer { get; set; }
            public ushort TextureCount3 { get; set; }
            public ushort TextureCount4 { get; set; }
            public uint[] TextureHashes { get; set; }
            public uint[] TexturesPointers { get; set; }
            public TextureItem[] TextureItems { get; set; }

            public XTD_TextureDictionary(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;
                Textures = null;

                XTD_ReadTextureDictionary();

                //Remove null textures
                var temp = new List<TextureInfo>();
                for (int i = 0; i < TexInfos.Length; i++)
                {
                    if (TexInfos[i] == null)
                        continue;
                    temp.Add(TexInfos[i]);
                }
                TexInfos = temp.ToArray();
                
                //Remove null textures
                var temp2 = new List<BitmapSource>();
                for (int i = 0; i < Textures.Length; i++)
                {
                    if (Textures[i] == null)
                        continue;
                    temp2.Add(Textures[i]);
                }
                Textures = temp2.ToArray();
            }

            private void XTD_ReadTextureDictionary()
            {
                VTable = Reader.ReadUInt32();
                BlockMapPointer = (uint)Reader.ReadOffset(Reader.ReadInt32());
                ParentDictionary = Reader.ReadUInt32();
                UsageCount = Reader.ReadUInt32();
                HashTablePointer = (uint)Reader.ReadOffset(Reader.ReadInt32());
                TextureCount = Reader.ReadUInt16();
                TextureCount2 = Reader.ReadUInt16();
                TextureListPointer = (uint)Reader.ReadOffset(Reader.ReadInt32());
                TextureCount3 = Reader.ReadUInt16();
                TextureCount4 = Reader.ReadUInt16();

                TextureHashes = new uint[TextureCount];
                Reader.BaseStream.Seek(HashTablePointer, SeekOrigin.Begin);
                for (int index = 0; index < TextureCount; ++index)
                {
                    TextureHashes[index] = Reader.ReadUInt32();
                }

                TexInfos = new TextureInfo[TextureCount];
                Textures = new BitmapSource[TextureCount];

                TexturesPointers = new uint[TextureCount];
                TextureItems = new TextureItem[TextureCount];

                for (int i = 0; i < TexturesPointers.Length; i++) //grcTextureXenonOffset
                {
                    Reader.BaseStream.Seek(TextureListPointer + (i * 4), SeekOrigin.Begin);
                    TexturesPointers[i] = (uint)Reader.ReadOffset(Reader.ReadInt32());
                    XTD_ReadTextureData(TexturesPointers[i], i);
                }
            }

            public void XTD_WriteTextureDictionary(List<(DDS, TextureInfo)> liste, ref IOWriter writer) //Actually only used for WTD (Nintendo Switch)
            {
                writer.WriteUInt32(VTable);
                if (BlockMapPointer != 0)
                    writer.WritePointer(BlockMapPointer);
                else
                    writer.WriteUInt32(0);
                writer.WriteUInt32(ParentDictionary);
                writer.WriteUInt32(UsageCount);
                writer.WritePointer(HashTablePointer);
                writer.WriteUInt16(TextureCount);
                writer.WriteUInt16(TextureCount2);
                writer.WritePointer(TextureListPointer);
                writer.WriteUInt16(TextureCount3);
                writer.WriteUInt16(TextureCount4);

                //Sort all textures by their data pointer
                Array.Sort(TextureItems, (i1, i2) => i1.TextureDataPointer.CompareTo(i2.TextureDataPointer));
                
                //Update the actual data for all the textures with modified dimensions
                for (int i = 0; i < liste.Count; i++)
                {
                    var newTex = liste[i].Item1;
                    var oldTex = liste[i].Item2;
                    var modifiedTexDim = TexInfos.First(it => it.TextureName == oldTex.TextureName);

                    //No dimensions were modified, no need to go further, this shouldn't happen!
                    if (modifiedTexDim == null)
                    {
                        return;
                    }

                    //Get the new texture data
                    int totalSize = 0;
                    for (int i1 = 0; i1 < newTex.dwMipMapCount; i1++)
                    {
                        int t = GetTextureDataSize(i1, newTex.dwHeight, newTex.dwWidth, true, newTex.dwMipMapCount, modifiedTexDim.PixelFormat);
                        totalSize += t;
                    }

                    //Update data
                    writer.BaseStream.Seek(oldTex.TextureDataPointer, SeekOrigin.Begin);
                    var updateItem = TextureItems.First(it => it.TextureName == oldTex.TextureName);
                    if (updateItem != null)
                    {
                        updateItem.TextureData = writer.ReadBytes(totalSize);
                        updateItem.TextureSize = (uint)totalSize;
                    }
                }

                //Move the buffer into a new IOWriter
                writer.BaseStream.Seek(0x0, SeekOrigin.Begin);
                byte[] header = writer.ReadBytes(0x20);

                IOWriter newWriter = new IOWriter(new MemoryStream(), AppGlobals.Platform == AppGlobals.PlatformEnum.Switch ? IOWriter.Endian.Little : IOWriter.Endian.Big);
                newWriter.Write(header);

                //If there's more than one texture, they must be written before anything else
                if (TextureCount > 0)
                {
                    //Write each structure data and store the offsets
                    for (int i = 0; i < TexturesPointers.Length; i++) //grcTextureXenonOffset
                    {
                        TexturesPointers[i] = (uint)newWriter.BaseStream.Position;
                        TextureItems[i].Position = (uint)newWriter.BaseStream.Position;
                        XTD_WriteTextureData(i, liste, false, ref newWriter);

                        //Add padding
                        newWriter.WriteUInt32(1);
                        byte[] b = Enumerable.Repeat((byte)0xCD, (int)(PikIO.Utils.RoundUp(newWriter.BaseStream.Position, 16L) - newWriter.BaseStream.Position)).ToArray();
                        newWriter.WriteBytes(b);
                    }

                    //Write all textures names and then write their pointer to the texture stucture
                    for (int i = 0; i < TexturesPointers.Length; i++)
                    {
                        TextureItems[i].TextureNamePointer = (uint)newWriter.BaseStream.Position;
                        newWriter.WriteString(TextureItems[i].TextureName, IOWriter.StringType.ASCII_NULL_TERMINATED);

                        //Add padding
                        if (i == TexturesPointers.Length - 1)
                        {
                            byte[] b = Enumerable.Repeat((byte)0xCD, (int)(PikIO.Utils.RoundUp(newWriter.BaseStream.Position, 16L) - newWriter.BaseStream.Position)).ToArray();
                            newWriter.WriteBytes(b);
                            HashTablePointer = (uint)newWriter.BaseStream.Position;
                        }
                    }

                    //Update all textures names offsets
                    for (int i = 0; i < TexturesPointers.Length; i++) //grcTextureXenonOffset
                    {
                        newWriter.BaseStream.Seek(TexturesPointers[i] + 0x18, SeekOrigin.Begin);
                        newWriter.WritePointer(TextureItems[i].TextureNamePointer);
                    }

                    //Write texture hashs
                    newWriter.BaseStream.Seek(HashTablePointer, SeekOrigin.Begin);
                    for (int index = 0; index < TextureCount; ++index)
                    {
                        newWriter.WriteUInt32(TextureItems[index].Hash);
                        //Add padding
                        if (index == TexturesPointers.Length - 1)
                        {
                            byte[] b = Enumerable.Repeat((byte)0xCD, (int)(PikIO.Utils.RoundUp(newWriter.BaseStream.Position, 16L) - newWriter.BaseStream.Position)).ToArray();
                            newWriter.WriteBytes(b);
                            TextureListPointer = (uint)newWriter.BaseStream.Position;
                        }
                    }

                    //Write structures offsets
                    newWriter.BaseStream.Seek(TextureListPointer, SeekOrigin.Begin);
                    for (int index = 0; index < TextureCount; ++index)
                    {
                        newWriter.WritePointer(TexturesPointers[index]);
                    }

                    //Add padding till the texture data
                    long round = PikIO.Utils.RoundUp(newWriter.BaseStream.Position, 256 * TextureCount);
                    byte[] padding = Enumerable.Repeat((byte)0xCD, (int)(round - newWriter.BaseStream.Position)).ToArray();
                    newWriter.WriteBytes(padding);

                    //Write the actual texture data
                    long position = newWriter.BaseStream.Position;
                    for (int i = 0; i < TextureItems.Length; i++)
                    {
                        TextureItems[i].TextureDataPointer = (uint)position;
                        newWriter.Write(TextureItems[i].TextureData);

                        //Add padding
                        //byte[] b = Enumerable.Repeat((byte)0xCD, (int)(PikIO.Utils.RoundUp(newWriter.BaseStream.Position, 16L) - newWriter.BaseStream.Position)).ToArray();
                        //newWriter.WriteBytes(b);

                        //Store end offset
                        position = newWriter.BaseStream.Position;
                    }

                    for (int i = 0; i < TextureItems.Length; i++)
                    {
                        newWriter.BaseStream.Seek(TextureItems[i].Position + 0x4C, SeekOrigin.Begin);
                        newWriter.WritePointer((uint)(TextureItems[i].TextureDataPointer - (256 * TextureCount)), true);
                    }

                    //Write HashTablePointer
                    newWriter.BaseStream.Seek(0x10, SeekOrigin.Begin);
                    newWriter.WritePointer(HashTablePointer);

                    //Write TextureListPointer
                    newWriter.BaseStream.Seek(0x18, SeekOrigin.Begin);
                    newWriter.WritePointer(TextureListPointer);

                    //Write BlockMapPointer
                    if (BlockMapPointer != 0)
                    {
                        //Write BlockMapPointer
                        newWriter.BaseStream.Seek(BlockMapPointer, SeekOrigin.Begin);
                        newWriter.Write(0);

                        //Get back at the first texture data position and prepare to write it
                        newWriter.BaseStream.Seek(position, SeekOrigin.Begin);
                    }
                    writer = newWriter;
                }
                else
                {
                    //Write all the pointers from the array
                    while (writer.BaseStream.Position < TextureListPointer)
                    {
                        writer.WriteUInt32(0xCDCDCDCD);
                    }

                    for (int i = 0; i < TexturesPointers.Length; i++) //grcTextureXenonOffset
                    {
                        writer.BaseStream.Seek(TextureListPointer + (i * 4), SeekOrigin.Begin);
                        writer.BaseStream.Seek(TexturesPointers[i], SeekOrigin.Begin);
                        XTD_WriteTextureData(i, liste, true, ref writer);

                        //Write texture name
                        writer.BaseStream.Seek(TextureItems[i].TextureNamePointer, SeekOrigin.Begin);
                        writer.WriteString(TextureItems[i].TextureName, IOWriter.StringType.ASCII_NULL_TERMINATED);
                    }

                    //Write texture hashs
                    writer.BaseStream.Seek(HashTablePointer, SeekOrigin.Begin);
                    for (int index = 0; index < TextureCount; ++index)
                    {
                        writer.WriteUInt32(TextureHashes[index]);
                    }

                    //Add padding till the textures pointers array position
                    while (writer.BaseStream.Position < TextureListPointer)
                    {
                        writer.WriteUInt32(0xCDCDCDCD);
                    }
                    for (int index = 0; index < TextureCount; ++index)
                    {
                        writer.WritePointer(TexturesPointers[index]);
                    }

                    //Add padding till the texture data position
                    writer.BaseStream.Seek(TextureListPointer + TexturesPointers.Length * 4, SeekOrigin.Begin);
                    while (writer.BaseStream.Position < TextureItems[0].TextureDataPointer)
                    {
                        writer.WriteUInt32(0xCDCDCDCD);
                    }

                    if (BlockMapPointer != 0)
                    {
                        long position = writer.BaseStream.Position;

                        //Write BlockMapPointer
                        writer.BaseStream.Seek(BlockMapPointer, SeekOrigin.Begin);
                        writer.Write(0);

                        //Get back at the first texture data position and prepare to write it
                        writer.BaseStream.Seek(position, SeekOrigin.Begin);
                    }
                }
            }

            public void XTD_ReadTextureData(uint pointer, int index)
            {
                Reader.BaseStream.Seek(pointer + 0x14, SeekOrigin.Begin);

                TextureItem item = new TextureItem();
                item.TextureSize = Reader.ReadUInt32();
                item.TextureNamePointer = (uint)Reader.ReadOffset(Reader.ReadInt32());
                int D3DBaseTexturePointer = Reader.ReadOffset(Reader.ReadInt32());
                item.Width = Reader.ReadUInt16();
                item.Height = Reader.ReadUInt16();
                item.MipMapCount = Reader.ReadInt32();

                bool isSwitch = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                if (isSwitch)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position - 4, SeekOrigin.Begin);

                    string sFormat = Reader.ReadString(IOReader.StringType.ASCII, 4);
                    switch (sFormat)
                    {
                        case "DXT1":
                            FormatType = TextureType.DXT1;
                            break;
                        case "DXT3":
                            FormatType = TextureType.DXT3;
                            break;
                        case "DXT5":
                            FormatType = TextureType.DXT5;
                            break;
                        case "2\0\0\0":
                            FormatType = TextureType.L8;
                            break;
                        default:
                            FormatType = TextureType.A8R8G8B8; //Likely
                            break;
                    }
                    byte pad = Reader.ReadByte();
                    ushort unkValue = Reader.ReadUInt16();
                    item.MipMapCount = (int)Reader.ReadByte();
                    float unknownFloat1 = Reader.ReadSingle(); //1.0f
                    float unknownFloat2 = Reader.ReadSingle(); //1.0f
                    float unknownFloat3 = Reader.ReadSingle(); //1.0f
                    float unknownFloat4 = Reader.ReadSingle(); //0.0f
                    float unknownFloat5 = Reader.ReadSingle(); //0.0f
                    float unknownFloat6 = Reader.ReadSingle(); //0.0f
                    int prevTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                    int nextTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                    TextureDataPointer = Reader.ReadOffset(Reader.ReadInt32());

                    if (TextureDataPointer >> 28 == 6)
                        TextureDataPointer = Entry.FlagInfo.BaseResourceSizeV + Reader.GetDataOffset(TextureDataPointer);
                    if (item.TextureSize == 0)
                        item.TextureSize = (uint)((FormatType == TextureType.DXT1) ? (item.Width * item.Height / 2) : (item.Width * item.Height));
                }

                item.TextureName = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, (int)item.TextureNamePointer);             
                if (!isSwitch)
                {
                    Reader.BaseStream.Seek(D3DBaseTexturePointer + 0x1C, SeekOrigin.Begin);
                    XTD_ReadTextureStartOffset();
                }

                try
                {
                    TexInfos[index] = new TextureInfo()
                    {
                        TextureName = item.TextureName,
                        Width = item.Width,
                        Height = item.Height,
                        MipMaps = item.MipMapCount,
                        TextureSize = item.TextureSize,
                        TextureDataPointer = TextureDataPointer,
                        MipDataPointer = MipDataPointer,
                        PixelFormat = (Texture.TextureType)FormatType
                    };

                    byte[] data = ReadTextureInfo(Reader, 0, TexInfos[index]);
                    Textures[index] = LoadImage(data);

                    //Save the structure data in memory
                    Reader.BaseStream.Seek(pointer, SeekOrigin.Begin);
                    item.StructureData = Reader.ReadBytes(0x50);

                    //Save the texture data in memory
                    Reader.BaseStream.Seek(TextureDataPointer, SeekOrigin.Begin);
                    int totalSize = 0;
                    for (int i = 0; i < TexInfos[index].MipMaps; i++)
                    {
                        totalSize += GetTextureDataSize(i, TexInfos[index].Height, TexInfos[index].Width, true, TexInfos[index].MipMaps, TexInfos[index].PixelFormat);
                    }
                    item.TextureData = Reader.ReadBytes(totalSize);
                    item.Position = pointer;
                    item.Hash = TextureHashes[index];
                    item.TextureDataPointer = (uint)TextureDataPointer;
                    TextureItems[index] = item;

                }
                catch { TexInfos[index] = null; Textures[index] = null; TextureItems[index] = null; }
            }

            public void XTD_WriteTextureData(int index, List<(DDS, TextureInfo)> liste, bool seekToSavedOffset, ref IOWriter writer)
            {
                var it = TextureItems[index];

                //Write the original data
                if (seekToSavedOffset)
                    writer.BaseStream.Seek(it.Position, SeekOrigin.Begin);
                writer.Write(it.StructureData);

                try
                {
                    var s = liste.First(t => t.Item2.TextureName == it.TextureName);
                    if (s.Item1.dwHeight > 0)
                    {
                        //Write the new data size
                        writer.BaseStream.Seek(TextureItems[index].Position + 0x14, SeekOrigin.Begin);
                        writer.Write(it.TextureSize);

                        //Write the new height, width
                        writer.BaseStream.Seek(writer.BaseStream.Position + 0x08, SeekOrigin.Begin);
                        writer.Write((ushort)s.Item1.dwWidth);
                        writer.Write((ushort)s.Item1.dwHeight);

                        //Write the new mips value
                        writer.BaseStream.Seek(writer.BaseStream.Position + 0x07, SeekOrigin.Begin);
                        writer.Write((byte)s.Item1.dwMipMapCount);

                        //Write the new texture pointer
                        writer.BaseStream.Seek(writer.BaseStream.Position + 0x20, SeekOrigin.Begin);
                        writer.WritePointer(TextureItems[index].TextureDataPointer);
                    }
                }
                catch
                {
                    if (!seekToSavedOffset)
                    {
                        //Write the new texture pointer
                        writer.BaseStream.Seek(it.Position + 0x4C, SeekOrigin.Begin);
                        writer.WritePointer(TextureItems[index].TextureDataPointer);
                    }
                }
            }

            public void XTD_ReadTextureStartOffset()
            {
                int Format = Reader.ReadInt32(); //-2130706430
                int Value = Reader.ReadInt32(); //1610612820
                int GPUTextureDataPointer = Reader.ReadOffset(Value); //1610612820

                int Type = (Value << 26) >> 26; //20
                int Endian = (Value << 24) >> 30; //1
                FormatType = (TextureType)(Value & byte.MaxValue);

                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                int MaxMipValue = Reader.ReadInt32(); //0
                int Flags = Reader.ReadInt32(); //512

                int DataFormat = (Format << 26) >> 26; //D3DBaseStructure //2
                int Shift1 = (int)((MaxMipValue & 0xC0000000) >> 6); //0
                int Shift2 = (MaxMipValue & 0x00030000) << 10; //0
                int MaxMipLevel = (Shift1 | Shift2) + 1; //1

                uint BaseAdress = (uint)(Value >> 0xC);
                TextureDataPointer = ((int)(BaseAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                uint MipMapAdress = (uint)(Flags >> 0xC);
                MipDataPointer = ((int)(MipMapAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;
            }

            public enum TextureType
            {
                L8 = 2,
                DXT1 = 82, // 0x00000052
                DXT3 = 83, // 0x00000053
                DXT5 = 84, // 0x00000054
                A8R8G8B8 = 134, // 0x00000086
            }
        }

        public class TextureItem
        {
            public uint Position { get; set; } //0x00
            public uint TextureSize { get; set; } //0x14
            public uint TextureNamePointer { get; set; } //0x18
            public ushort Width { get; set; } //0x20
            public ushort Height { get; set; } //0x22
            public int MipMapCount { get; set; } //0x24
            public uint Hash { get; set; }
            public Texture.TextureType Format { get; set; }
            public byte[] StructureData { get; set; }
            public byte[] TextureData { get; set; }
            public string TextureName { get; set; }
            public uint TextureDataPointer { get; set; }
        }

        #endregion

        #region XTX

        public class XTX_TextureDictionary
        {
            private IOReader Reader;
            private FileEntry Entry;
            public static BitmapSource Textures;
            public static TextureInfo TexInfos;
            private TextureType FormatType;
            private int TextureDataPointer, MipDataPointer;

            public XTX_TextureDictionary(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;
                Textures = null;
                XTX_ReadTextureDictionary();
            }

            private void XTX_ReadTextureDictionary()
            {
                uint VTable = Reader.ReadUInt32();
                int BlockMapPointer = Reader.ReadOffset(Reader.ReadInt32());
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                uint TextureSize = Reader.ReadUInt32();
                int TextureNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                int TextureInfoPointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort Width = Reader.ReadUInt16();
                ushort Height = Reader.ReadUInt16();
                int MipMapCount = Reader.ReadInt32();

                bool isSwitch = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                if (isSwitch)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position - 4, SeekOrigin.Begin);

                    string sFormat = Reader.ReadString(IOReader.StringType.ASCII, 4);
                    switch (sFormat)
                    {
                        case "DXT1":
                            FormatType = TextureType.DXT1;
                            break;
                        case "DXT3":
                            FormatType = TextureType.DXT3;
                            break;
                        case "DXT5":
                            FormatType = TextureType.DXT5;
                            break;
                        case "2\0\0\0":
                            FormatType = TextureType.L8;
                            break;
                        default:
                            FormatType = TextureType.A8R8G8B8; //Likely
                            break;
                    }
                    byte pad = Reader.ReadByte();
                    ushort unkValue = Reader.ReadUInt16();
                    MipMapCount = (int)Reader.ReadByte();
                    float unknownFloat1 = Reader.ReadSingle(); //1.0f
                    float unknownFloat2 = Reader.ReadSingle(); //1.0f
                    float unknownFloat3 = Reader.ReadSingle(); //1.0f
                    float unknownFloat4 = Reader.ReadSingle(); //0.0f
                    float unknownFloat5 = Reader.ReadSingle(); //0.0f
                    float unknownFloat6 = Reader.ReadSingle(); //0.0f
                    int prevTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                    int nextTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                    TextureDataPointer = Reader.ReadOffset(Reader.ReadInt32());

                    if (TextureDataPointer >> 28 == 6)
                        TextureDataPointer = Entry.FlagInfo.BaseResourceSizeV + Reader.GetDataOffset(TextureDataPointer);
                    if (TextureSize == 0)
                        TextureSize = (uint)((FormatType == TextureType.DXT1) ? (Width * Height / 2) : (Width * Height));
                }

                string Name = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer);
                if (!isSwitch)
                {
                    Reader.BaseStream.Seek(TextureInfoPointer, SeekOrigin.Begin);
                    XTX_ReadTextureInfo((int)Reader.BaseStream.Position);
                }

                try
                {
                    TexInfos = new TextureInfo()
                    {
                        TextureName = Name,
                        Width = Width,
                        Height = Height,
                        MipMaps = MipMapCount,
                        TextureSize = TextureSize,
                        TextureDataPointer = TextureDataPointer,
                        MipDataPointer = 0,
                        PixelFormat = (Texture.TextureType)FormatType
                    };

                    byte[] data = ReadTextureInfo(Reader, 0, TexInfos);
                    Textures = LoadImage(data);
                }
                catch { Textures = null; TexInfos = null; }
            }

            private void XTX_ReadTextureInfo(int pointer)
            {
                Reader.BaseStream.Seek(pointer + 0x20, SeekOrigin.Begin);
                int Value = Reader.ReadInt32();
                int GPUTextureDataPointer = Reader.ReadOffset(Value);

                int Type = (Value << 26) >> 26;
                int Endian = (Value << 24) >> 30;
                FormatType = (TextureType)(Value & byte.MaxValue);

                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                int MaxMipValue = Reader.ReadInt32();
                int Flags = Reader.ReadInt32();

                int Shift1 = (int)((MaxMipValue & 0xC0000000) >> 6);
                int Shift2 = (MaxMipValue & 0x00030000) << 10;
                int MaxMipLevel = (Shift1 | Shift2) + 1;

                uint BaseAdress = (uint)(Value >> 0xC);
                TextureDataPointer = ((int)(BaseAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                uint MipMapAdress = (uint)(Flags >> 0xC);
                MipDataPointer = ((int)(MipMapAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;
            }

            public enum TextureType
            {
                L8 = 2,
                DXT1 = 82, // 0x00000052
                DXT3 = 83, // 0x00000053
                DXT5 = 84, // 0x00000054
                A8R8G8B8 = 134, // 0x00000086
            }
        }

        #endregion

        #region XSF

        public class XSF_TextureResource
        {
            private IOReader Reader;
            private FileEntry Entry;
            public static BitmapSource[] ImagesSource;
            public static TextureInfo[] TexInfos;
            private List<int> TextureListPointer = new List<int>();
            private int ItemArrayPointer;
            private ushort ItemCount;
            private bool isSwitch = false;

            public XSF_TextureResource(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;
                TextureListPointer.Clear();
                ImagesSource = null;
                isSwitch = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;

                XSF_ReadMainStructure();
                XSF_ReadItemStructure();

                if (TextureListPointer.Count <= 0)
                {
                    return;
                }

                TexInfos = new TextureInfo[TextureListPointer.Count];
                ImagesSource = new BitmapSource[TextureListPointer.Count];

                for (int i = 0; i < TextureListPointer.Count; i++)
                {
                    Reader.BaseStream.Seek(TextureListPointer[i] + 20, SeekOrigin.Begin);
                    uint TextureSize = Reader.ReadUInt32();
                    string Name = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, Reader.ReadOffset(Reader.ReadInt32()), true);
                    int D3DBaseTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                    ushort Width = Reader.ReadUInt16();
                    ushort Height = Reader.ReadUInt16();
                    int MipMapCount = Reader.ReadInt32(); //Fonts are always single-mipmapped

                    Texture.TextureType format;
                    int currentTextureDataPointer;
                    if (isSwitch)
                    {
                        Reader.BaseStream.Seek(Reader.BaseStream.Position - 4, SeekOrigin.Begin);

                        string sFormat = Reader.ReadString(IOReader.StringType.ASCII, 4);
                        switch (sFormat)
                        {
                            case "CRND":
                            case "DXT1":
                                format = Texture.TextureType.DXT1;
                                break;
                            case "DXT3":
                                format = Texture.TextureType.DXT3;
                                break;
                            case "DXT5":
                                format = Texture.TextureType.DXT5;
                                break;
                            default:
                                //string test = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer);
                                //throw new Exception("Unsupported format");
                                format = Texture.TextureType.A8R8G8B8; //Likely
                                break;
                        }
                        byte pad = Reader.ReadByte();
                        ushort unkValue = Reader.ReadUInt16();
                        MipMapCount = (int)Reader.ReadByte();
                        float unknownFloat1 = Reader.ReadSingle(); //1.0f
                        float unknownFloat2 = Reader.ReadSingle(); //1.0f
                        float unknownFloat3 = Reader.ReadSingle(); //1.0f
                        float unknownFloat4 = Reader.ReadSingle(); //0.0f
                        float unknownFloat5 = Reader.ReadSingle(); //0.0f
                        float unknownFloat6 = Reader.ReadSingle(); //0.0f
                        int prevTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                        int nextTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                        currentTextureDataPointer = Reader.ReadOffset(Reader.ReadInt32());

                        if (currentTextureDataPointer >> 28 == 6)
                            currentTextureDataPointer = Entry.FlagInfo.BaseResourceSizeV + Reader.GetDataOffset(currentTextureDataPointer);
                        if (TextureSize == 0)
                            TextureSize = (uint)Width * Height / 2;
                    }
                    else
                    {
                        //BaseAdress
                        Reader.BaseStream.Seek(D3DBaseTextureOffset + 0x20, SeekOrigin.Begin);
                        uint Value = Reader.ReadUInt32();
                        uint BaseAdress = Value >> 0xC;
                        currentTextureDataPointer = ((int)(BaseAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;
                        format = (Texture.TextureType)(Value & byte.MaxValue);
                    }

                    try
                    {
                        TexInfos[i] = new TextureInfo()
                        {
                            TextureName = Name,
                            Width = Width,
                            Height = Height,
                            MipMaps = MipMapCount,
                            TextureSize = TextureSize,
                            TextureDataPointer = currentTextureDataPointer,
                            MipDataPointer = 0, //No mips for #sf
                            PixelFormat = format
                        };

                        byte[] buffer = ReadTextureInfo(Reader, 0, TexInfos[i]);
                        ImagesSource[i] = LoadImage(buffer);
                    }
                    catch { TexInfos[i] = null; ImagesSource[i] = null; }
                }
            }

            private void XSF_ReadMainStructure()
            {
                uint Magic = Reader.ReadUInt32();

                Reader.BaseStream.Seek(44, SeekOrigin.Begin);
                int swfObjectPointer = Reader.ReadOffset(Reader.ReadInt32());

                Reader.BaseStream.Seek(swfObjectPointer + 24, SeekOrigin.Begin);
                ItemArrayPointer = Reader.ReadOffset(Reader.ReadInt32());

                Reader.BaseStream.Seek(swfObjectPointer + 50, SeekOrigin.Begin);
                ItemCount = Reader.ReadUInt16();
            }

            private void XSF_ReadItemStructure()
            {
                int[] ItemPointers = new int[ItemCount];
                for (int i = 0; i < ItemPointers.Length; i++)
                {
                    Reader.BaseStream.Seek(i == 0 ? ItemArrayPointer + 0x4 : ItemArrayPointer + (i * 4), SeekOrigin.Begin);
                    ItemPointers[i] = Reader.ReadOffset(Reader.ReadInt32());
                    XSF_GetItemType(ItemPointers[i], i);
                }
            }

            private void XSF_GetItemType(int pointer, int index)
            {
                Reader.BaseStream.Seek(pointer + 0x8, SeekOrigin.Begin);
                ItemType itemType = (ItemType)Reader.ReadByte();

                if (itemType == ItemType.FONT)
                {
                    long offset = Reader.BaseStream.Position + 155;
                    for (int i = 0; i < 3; i++)
                    {
                        Reader.BaseStream.Seek(offset + (i * 8), SeekOrigin.Begin);
                        int dwObjectOffset = Reader.ReadOffset(Reader.ReadInt32());
                        XSF_GetTextureOffset(dwObjectOffset);
                    }
                }
                else if (itemType == ItemType.BITMAP)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position + 0x3, SeekOrigin.Begin);
                    TextureListPointer.Add(Reader.ReadOffset(Reader.ReadInt32()));
                }
            }

            private void XSF_GetTextureOffset(int pointer)
            {
                if (pointer <= 0)
                    return;

                Reader.BaseStream.Seek(pointer, SeekOrigin.Begin);
                int grcTextureStructureOffset = Reader.ReadOffset(Reader.ReadInt32());

                if (grcTextureStructureOffset <= 0)
                    return;

                Reader.BaseStream.Seek(pointer + 20, SeekOrigin.Begin);
                ushort Count = Reader.ReadUInt16();

                for (int i = 0; i < Count; i++)
                {
                    Reader.BaseStream.Seek(grcTextureStructureOffset + (i * 4), SeekOrigin.Begin);
                    TextureListPointer.Add(Reader.ReadOffset(Reader.ReadInt32()));
                }
            }

            public enum ItemType
            {
                BITMAP = 4,
                FONT = 5
            }

            public enum TextureType
            {
                L8 = 2,
                DXT1 = 82, // 0x00000052
                DXT3 = 83, // 0x00000053
                DXT5 = 84, // 0x00000054
                A8R8G8B8 = 134 // 0x00000086
            }
        }
        #endregion
    }
}