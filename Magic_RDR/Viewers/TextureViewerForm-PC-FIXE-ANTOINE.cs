using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Magic_RDR.Application;
using Magic_RDR.RPF;
using static Magic_RDR.RPF.Texture;
using static Magic_RDR.RPF6.RPF6TOC;

namespace Magic_RDR.Viewers
{
    public partial class TextureViewerForm : Form
    {
        private Image Background;
        private IOReader Reader;
        private FileEntry file;
        private Dictionary<BitmapSource, string> FixedTextureList;
        private byte[] HeaderData;
        public int FileFlags;

        public TextureViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Text = string.Format("MagicRDR - Texture Viewer [{0}]", entry.Entry.Name);
            Background = splitContainer1.Panel2.BackgroundImage;
            importButton.Enabled = !entry.Entry.Name.EndsWith(".dds");
            splitContainer1.Panel2.BackgroundImage = null;
            splitContainer1.Panel2.BackColor = Color.Black;

            FileFlags = -1;
            file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();
            byte[] compressedData = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            byte[] data = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(compressedData);

            if (entry.Entry.Name.EndsWith(".xsf"))
                HeaderData = new byte[0x14];
            else
                HeaderData = new byte[0x18];
            for (int i = 0; i < HeaderData.Length; i++)
            {
                HeaderData[i] = compressedData[i];
            }

            #region Execution

            if (entry.Entry.Name.EndsWith(".dds"))
            {
                FileFlags = -1;
                if (data == null)
                {
                    RPFFile.RPFIO.Position = 0x0;
                    data = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
                    try
                    {
                        if (file.FlagInfo.IsCompressed && !file.FlagInfo.IsResource)
                        {
                            RPFFile.RPFIO.Position = file.GetOffset();
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
                pictureBox.SizeMode = RPF6FileNameHandler.ImageSizeMode;

                List<ListViewItem> list = new List<ListViewItem>();
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = entry.Entry.Name;
                listViewItem.SubItems.Add(string.Format("{0}x{1}", image.Height, image.Width));
                listViewItem.SubItems.Add(image.Format.ToString().ToUpper());
                listViewItem.SubItems.Add("1");
                listViewItem.SubItems.Add("0x" + image.DataLen.ToString("X"));
                list.Add(listViewItem);

                listView.BeginUpdate();
                listView.Items.Clear();
                listView.Items.AddRange(list.ToArray());
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listView.EndUpdate();
            }
            else if (entry.Entry.Name.EndsWith(".xtx"))
            {
                FileFlags = 1;
                if (data == null)
                {
                    return;
                }
                Reader = new IOReader(new MemoryStream(data), IOReader.Endian.Big);
                Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

                XTX_TextureDictionary textures = new XTX_TextureDictionary(Reader, file);
                BitmapSource[] Textures = XTX_TextureDictionary.Textures;

                if (Textures != null)
                {
                    int valid = 0;
                    while (Textures[valid] == null)
                    {
                        valid++;
                    }
                    pictureBox.Image = Image.FromStream(new MemoryStream(Texture.BufferFromImageSource(Textures[valid])));
                    List<ListViewItem> list = new List<ListViewItem>();
                    FixedTextureList = new Dictionary<BitmapSource, string>();

                    for (int i = 0; i < Textures.Length; i++)
                    {
                        if (Textures[i] == null)
                            continue;

                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = XTX_TextureDictionary.TextureNames[i];
                        listViewItem.SubItems.Add(string.Format("{0}x{1}", Textures[i].Height, Textures[i].Width));
                        listViewItem.SubItems.Add(XTX_TextureDictionary.PixelFormat[i].ToString());
                        listViewItem.SubItems.Add(XTX_TextureDictionary.MipMaps[i].ToString());
                        listViewItem.SubItems.Add("0x" + XTX_TextureDictionary.TextureSize[i].ToString("X"));
                        list.Add(listViewItem);
                        FixedTextureList.Add(Textures[i], XTX_TextureDictionary.TextureNames[i]);
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
                Reader = new IOReader(new MemoryStream(data), IOReader.Endian.Big);
                Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

                XTD_TextureDictionary textures = new XTD_TextureDictionary(Reader, file);
                BitmapSource[] Textures = XTD_TextureDictionary.Textures;

                if (Textures != null)
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

                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = XTD_TextureDictionary.TextureNames[i];
                        listViewItem.SubItems.Add(string.Format("{0}x{1}", Textures[i].Height, Textures[i].Width));
                        listViewItem.SubItems.Add(XTD_TextureDictionary.PixelFormat[i].ToString());
                        listViewItem.SubItems.Add(XTD_TextureDictionary.MipMaps[i].ToString());
                        listViewItem.SubItems.Add("0x" + XTD_TextureDictionary.TextureSize[i].ToString("X"));
                        list.Add(listViewItem);
                        FixedTextureList.Add(Textures[i], XTD_TextureDictionary.TextureNames[i]);
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
            else if (entry.Entry.Name.EndsWith(".xsf"))
            {
                FileFlags = 2;
                if (data == null)
                {
                    return;
                }
                Reader = new IOReader(new MemoryStream(data), IOReader.Endian.Big);
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

                        ListViewItem listViewItem = new ListViewItem();
                        listViewItem.Text = XSF_TextureResource.TextureNames[i];
                        listViewItem.SubItems.Add(string.Format("{0}x{1}", Textures[i].Height, Textures[i].Width));
                        listViewItem.SubItems.Add(XSF_TextureResource.PixelFormat[i].ToString());
                        listViewItem.SubItems.Add(XSF_TextureResource.MipMaps[i].ToString());
                        listViewItem.SubItems.Add("0x" + XSF_TextureResource.TextureSize[i].ToString("X"));
                        list.Add(listViewItem);
                        FixedTextureList.Add(Textures[i], XSF_TextureResource.TextureNames[i]);
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

        private void importButton_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Not fully implemented yet", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //return;

            if (pictureBox.Image == null)
            {
                MessageBox.Show("Current image is invalid", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open";
            dialog.Filter = "Direct Draw Surface File|*.dds";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int index = listView.FocusedItem.Index;
                BitmapSource currentTexture = FixedTextureList.ElementAt(index).Key;
                int textureOffset = FileFlags == 0 ? XTD_TextureDictionary.TexturePointers[index] : FileFlags == 1 ? XTX_TextureDictionary.TexturePointer : XSF_TextureResource.TexturePointers[index];
                TextureType formats = FileFlags == 0 ? (TextureType)XTD_TextureDictionary.PixelFormat[index] : FileFlags == 1 ? (TextureType)XTX_TextureDictionary.PixelFormat[index] : (TextureType)XSF_TextureResource.PixelFormat[index]; ;
                int levels = FileFlags == 0 ? XTD_TextureDictionary.MipMaps[index] : FileFlags == 1 ? XTX_TextureDictionary.MipMaps[index] : XSF_TextureResource.MipMaps[index];

                InjectDDS(Reader, dialog.FileName, textureOffset, (int)currentTexture.Height, (int)currentTexture.Width, levels, formats);

                //Recompress into a single block
                Reader.BaseStream.Position = 0x0;
                byte[] newData = Reader.ReadBytes(Reader.BaseStream.Length);
                newData = DataUtils.CompressLZX(newData);
                byte[] buffer = new byte[HeaderData.Length + newData.Length];

                byte[] dataLength = BitConverter.GetBytes(newData.Length.Swap());
                if (FileFlags == 2)
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

                Buffer.BlockCopy(HeaderData, 0, buffer, 0, HeaderData.Length);
                Buffer.BlockCopy(newData, 0, buffer, HeaderData.Length, newData.Length);

                SaveFileDialog saveFile = new SaveFileDialog();
                saveFile.FileName = file.Name;
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(saveFile.FileName, buffer);
                    MessageBox.Show("Texture injected & file exported !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private void backgroundComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            int index = backgroundComboBox.SelectedIndex;
            splitContainer1.Panel2.BackgroundImage = null;

            switch (index)
            {
                case 1:
                    splitContainer1.Panel2.BackColor = Color.White;
                    break;
                case 2:
                    splitContainer1.Panel2.BackColor = Color.Red;
                    break;
                case 3:
                    splitContainer1.Panel2.BackColor = Color.Green;
                    break;
                case 4:
                    splitContainer1.Panel2.BackColor = Color.Blue;
                    break;
                case 5:
                    splitContainer1.Panel2.BackColor = Color.Transparent;
                    splitContainer1.Panel2.BackgroundImage = Background;
                    break;
                default:
                    splitContainer1.Panel2.BackColor = Color.Black;
                    break;
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listView.FocusedItem.Index;
            BitmapSource image = FixedTextureList.ElementAt(index).Key;
            if (image == null)
            {
                return;
            }
            pictureBox.Image = Image.FromStream(new MemoryStream(Texture.BufferFromImageSource(image)));
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
            dialog.FileName = FixedTextureList.ElementAt(index).Value.Replace(".dds", ".png").Replace(".xtd", ".png");
            dialog.Filter = "Portable Network Graphics (*.png)|*.png";

            if (dialog.ShowDialog() == DialogResult.OK)
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

        private void exportAllButton_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                MessageBox.Show("There's nothing to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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

                    string texture = FixedTextureList.ElementAt(i).Value.Replace(".dds", ".png").Replace(".xtd", ".png").Replace("/", "-");
                    using (var fileStream = new FileStream(dialog.SelectedPath + "\\\\" + texture, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(FixedTextureList.ElementAt(i).Key));
                        encoder.Save(fileStream);
                        count++;
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
            public static string[] TextureNames;
            public static int[] MipMaps, TextureSize, TexturePointers;
            public static TextureType[] PixelFormat;
            private TextureType FormatType;
            private int TextureDataPointer;

            public XTD_TextureDictionary(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;
                Textures = null;
                TextureNames = null;
                MipMaps = null;
                PixelFormat = null;
                TextureSize = null;
                TexturePointers = null;
                XTD_ReadTextureDictionary();
            }

            private void XTD_ReadTextureDictionary()
            {
                uint VTable = Reader.ReadUInt32();
                int BlockMapPointer = Reader.ReadOffset(Reader.ReadInt32());
                uint ParentDictionary = Reader.ReadUInt32();
                uint UsageCount = Reader.ReadUInt32();
                int HashTablePointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort TextureCount = Reader.ReadUInt16();
                ushort TextureCount2 = Reader.ReadUInt16();
                int TextureListPointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort TextureCount3 = Reader.ReadUInt16();
                ushort TextureCount4 = Reader.ReadUInt16();

                uint[] TextureHashes = new uint[TextureCount];
                Reader.BaseStream.Seek(HashTablePointer, SeekOrigin.Begin);
                for (int index = 0; index < TextureCount; ++index)
                {
                    TextureHashes[index] = Reader.ReadUInt32();
                }

                Textures = new BitmapSource[TextureCount];
                TextureNames = new string[TextureCount];
                MipMaps = new int[TextureCount];
                PixelFormat = new TextureType[TextureCount];
                TextureSize = new int[TextureCount];
                TexturePointers = new int[TextureCount];

                int[] TexturesPointers = new int[TextureCount]; //grcTextureXenonOffset
                for (int i = 0; i < TexturesPointers.Length; i++)
                {
                    Reader.BaseStream.Seek(TextureListPointer + (i * 4), SeekOrigin.Begin);
                    TexturesPointers[i] = Reader.ReadOffset(Reader.ReadInt32());
                    XTD_ReadTextureData(TexturesPointers[i], i);
                }
            }

            private void XTD_ReadTextureData(int pointer, int index)
            {
                Reader.BaseStream.Seek(pointer + 0x18, SeekOrigin.Begin);
                int TextureNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                int D3DBaseTexturePointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort Width = Reader.ReadUInt16();
                ushort Height = Reader.ReadUInt16();
                int MipMapCount = Reader.ReadInt32();

                TextureNames[index] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer);
                Reader.BaseStream.Seek(D3DBaseTexturePointer + 0x1C, SeekOrigin.Begin);
                XTD_ReadTextureStartOffset();

                try
                {
                    byte[] data = ReadTextureInfo(Reader, 0, TextureDataPointer + Entry.FlagInfo.BaseResourceSizeV, Height, Width, MipMapCount, (Texture.TextureType)FormatType, 0);
                    Textures[index] = LoadImage(data);
                    TexturePointers[index] = TextureDataPointer + Entry.FlagInfo.BaseResourceSizeV;
                    TextureSize[index] = data.Length;
                }
                catch { TextureSize[index] = 0; }
                MipMaps[index] = MipMapCount;
                PixelFormat[index] = FormatType;
            }

            private void XTD_ReadTextureStartOffset()
            {
                int Format = Reader.ReadInt32();
                int Value = Reader.ReadInt32();
                int GPUTextureDataPointer = Reader.ReadOffset(Value);

                int Type = (Value << 26) >> 26;
                int Endian = (Value << 24) >> 30;
                FormatType = (TextureType)(Value & byte.MaxValue);

                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                int MaxMipValue = Reader.ReadInt32();
                int Flags = Reader.ReadInt32();

                int DataFormat = (Format << 26) >> 26; //D3DBaseStructure
                int Shift1 = (int)((MaxMipValue & 0xC0000000) >> 6);
                int Shift2 = (MaxMipValue & 0x00030000) << 10;
                int MaxMipLevel = (Shift1 | Shift2) + 1;

                uint BaseAdress = (uint)(Value >> 0xC);
                TextureDataPointer = (int)(BaseAdress << 12) & 0xFFFFFFF;
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

        #region XTX

        public class XTX_TextureDictionary
        {
            private IOReader Reader;
            private FileEntry Entry;
            public static BitmapSource[] Textures;
            public static string[] TextureNames;
            public static int[] MipMaps, TextureSize;
            public static TextureType[] PixelFormat;
            public static int TexturePointer;
            private TextureType FormatType;
            private int TextureDataPointer;

            public XTX_TextureDictionary(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;
                Textures = null;
                TextureNames = null;
                MipMaps = null;
                PixelFormat = null;
                TextureSize = null;
                TexturePointer = 0;
                XTX_ReadTextureDictionary();
            }

            private void XTX_ReadTextureDictionary()
            {
                uint VTable = Reader.ReadUInt32();
                int BlockMapPointer = Reader.ReadOffset(Reader.ReadInt32());
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                _ = Reader.ReadInt32();
                int TextureNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                int TextureInfoPointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort Width = Reader.ReadUInt16();
                ushort Height = Reader.ReadUInt16();
                int Levels = Reader.ReadInt32();

                //Leaving these in case we have actually more than one texture per .xtx
                Textures = new BitmapSource[1];
                TextureNames = new string[1];
                MipMaps = new int[1];
                PixelFormat = new TextureType[1];
                TextureSize = new int[1];

                int[] TexturesPointers = new int[1];
                for (int i = 0; i < TexturesPointers.Length; i++)
                {
                    Reader.BaseStream.Seek(TextureInfoPointer + (i * 4), SeekOrigin.Begin);
                    XTX_ReadTextureInfo((int)Reader.BaseStream.Position, i);
                }

                try
                {
                    byte[] data = ReadTextureInfo(Reader, 0, TextureDataPointer + Entry.FlagInfo.BaseResourceSizeV, Height, Width, Levels, (Texture.TextureType)FormatType, 0);
                    Textures[0] = LoadImage(data);
                    TextureSize[0] = data.Length;
                }
                catch { TextureSize[0] = 0; }

                TexturePointer = TextureDataPointer + Entry.FlagInfo.BaseResourceSizeV;
                TextureNames[0] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer);
                MipMaps[0] = Levels;
                PixelFormat[0] = FormatType;
            }

            private void XTX_ReadTextureInfo(int pointer, int index)
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
                TextureDataPointer = (int)(BaseAdress << 12) & 0xFFFFFFF;
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
            private ImageData[] Images;

            private List<int> TextureListPointer = new List<int>();
            private int ItemArrayPointer;
            private ushort ItemCount;

            public static string[] TextureNames;
            public static int[] MipMaps, TextureSize, TexturePointers;
            public static TextureType[] PixelFormat;

            public XSF_TextureResource(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;
                TextureListPointer.Clear();
                ImagesSource = null;
                TextureNames = null;
                MipMaps = null;
                PixelFormat = null;
                TextureSize = null;

                XSF_ReadMainStructure();
                XSF_ReadItemStructure();

                if (TextureListPointer.Count <= 0)
                {
                    return;
                }

                ImagesSource = new BitmapSource[TextureListPointer.Count];
                Images = new ImageData[TextureListPointer.Count];
                TextureNames = new string[TextureListPointer.Count];
                MipMaps = new int[TextureListPointer.Count];
                PixelFormat = new TextureType[TextureListPointer.Count];
                TextureSize = new int[TextureListPointer.Count];
                TexturePointers = new int[TextureListPointer.Count];

                for (int i = 0; i < TextureListPointer.Count; i++)
                {
                    Reader.BaseStream.Seek(TextureListPointer[i] + 20, SeekOrigin.Begin);
                    uint Size = Reader.ReadUInt32();
                    string TextureName = TextureNames[i] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, Reader.ReadOffset(Reader.ReadInt32()), true);
                    int D3DBaseTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                    ushort Width = Reader.ReadUInt16();
                    ushort Height = Reader.ReadUInt16();
                    int Levels = MipMaps[i] = Reader.ReadInt32();

                    //BaseAdress
                    Reader.BaseStream.Seek(D3DBaseTextureOffset + 0x20, SeekOrigin.Begin);
                    uint Value = Reader.ReadUInt32();
                    uint BaseAdress = Value >> 0xC;
                    int CurrentTextureDataPointer = (int)(BaseAdress << 12) & 0xFFFFFFF;

                    //MipAdress
                    Reader.BaseStream.Seek(D3DBaseTextureOffset + 0x30, SeekOrigin.Begin);
                    uint Value_ = Reader.ReadUInt32();
                    uint MipMapAdress = Value_ >> 0xC;
                    int MipDataPointer = (int)(MipMapAdress << 12) & 0xFFFFFFF;

                    //Read texture data
                    Reader.BaseStream.Seek(D3DBaseTextureOffset + 0x23, SeekOrigin.Begin);
                    Texture.TextureType Format = (Texture.TextureType)(PixelFormat[i] = (TextureType)(Reader.ReadByte() == 0x52 ? Texture.TextureType.DXT1 : Texture.TextureType.DXT5));
                    byte[] buffer = ReadTextureInfo(Reader, 0, CurrentTextureDataPointer + Entry.FlagInfo.BaseResourceSizeV, Height, Width, Levels, Format, Size);

                    try
                    {
                        ImagesSource[i] = LoadImage(buffer);
                        Images[i] = new ImageData((int)Size, CurrentTextureDataPointer + Entry.FlagInfo.BaseResourceSizeV, Height, Width, Levels, Format, TextureName);
                        TextureSize[i] = buffer.Length;
                        TexturePointers[i] = CurrentTextureDataPointer + Entry.FlagInfo.BaseResourceSizeV;
                    }
                    catch { ImagesSource[i] = null; TextureSize[i] = buffer.Length; }
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