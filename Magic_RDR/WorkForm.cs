using Magic_RDR.RPF;
using Pik.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_RDR
{
    public partial class WorkForm : Form
    {
        private Stream openRPFStream;
        private Stream saveRPFStream;
        private Stream saveRPFStreamCopyTo;
        private RPF6 rpfToSave;
        private RPF6 rpfToExportFilesFrom;
        private RPF6 rpfToLoadDirFrom;
        private bool ExportResources;
        private bool UnpackResources;
        private string ExportFilesLocation;
        private string locationToExportTo;
        private RPF6.RPF6TOC.TOCSuperEntry directoryToExport;
        private RPF6.RPF6TOC.TOCSuperEntry directoryToLoad;
        public RPF6 OpenRPFReturn;
        public Exception OpenRPFException;
        public Exception SaveRPFException;
        public Exception ExportFilesException;
        public Exception LoadDirectoryException;
        public List<ListViewItem> ItemsToAddToListView;
        public List<TreeNode> DirectoryList;
        public bool Done;

        private void WorkForm_Load(object sender, EventArgs e)
        {
        }

        public WorkForm(RPF6 currentRPF, RPF6.RPF6TOC.TOCSuperEntry dir, string outLoc, string text = "Exporting...")
        {
            this.InitializeComponent();
            if (MainForm.CommandLine)
            {
                foreach (Control control in this.Controls)
                {
                    control.Visible = false;
                }
            }
            this.rpfToExportFilesFrom = currentRPF;
            this.directoryToExport = dir;
            this.locationToExportTo = outLoc;
            this.DoExportDirectoryWork();
        }

        public WorkForm(RPF6 currentRPF, RPF6.RPF6TOC.TOCSuperEntry dirToLoad)
        {
            this.InitializeComponent();
            if (MainForm.CommandLine)
            {
                foreach (Control control in this.Controls)
                {
                    control.Visible = false;
                }
            }
            this.rpfToLoadDirFrom = currentRPF;
            this.directoryToLoad = dirToLoad;
            this.DoLoadDirectoryWork();
        }

        private async void DoSaveRPFWork()
        {
            try
            {
                await Task.Factory.StartNew((Action)(() =>
                {
                    this.rpfToSave.Write(this.saveRPFStream);
                    if (this.saveRPFStreamCopyTo == null)
                    {
                        return;
                    }
                    this.saveRPFStreamCopyTo.Position = 0L;
                    this.saveRPFStreamCopyTo.SetLength(0L);
                    this.saveRPFStream.Position = 0L;
                    this.saveRPFStream.CopyTo(this.saveRPFStreamCopyTo);
                }));
                this.Done = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                this.SaveRPFException = ex;
                this.Hide();
            }
        }

        private async void DoOpenRPFWork()
        {
            try
            {
                await Task.Factory.StartNew((() =>
                {
                    if (!RPF6.RPF6Header.HasIdentifier(new PikIO(openRPFStream, PikIO.Endianess.Big)))
                    {
                        throw new Exception("RPF File does not have valid header magic");
                    }
                    OpenRPFReturn = new RPF6(openRPFStream);
                    OpenRPFException = null;
                }));
                Done = true;
                Hide();
            }
            catch (Exception ex)
            {
                OpenRPFReturn = null;
                OpenRPFException = ex;
                Hide();
            }
        }

        private async void DoExportFilesWork()
        {
            try
            {
                await Task.Factory.StartNew((() =>
                {
                    RPF6.RPF6TOC.TOCSuperEntry superEntry1 = rpfToExportFilesFrom.TOC.SuperEntries[0];
                    foreach (RPF6.RPF6TOC.TOCSuperEntry superEntry2 in rpfToExportFilesFrom.TOC.SuperEntries)
                    {
                        if (!superEntry2.IsDir && superEntry2.Entry.AsFile.FlagInfo.IsResource == ExportResources)
                        {
                            if (superEntry2.Entry.AsFile.FlagInfo.IsResource && UnpackResources)
                            {
                                MemoryStream memoryStream = new MemoryStream();
                                rpfToExportFilesFrom.TOC.ExtractFile(superEntry2, memoryStream);
                                Console.WriteLine(string.Format("Exporting Resource: {0}", superEntry2.Entry.GetPath()));

                                if (ResourceUtils.ResourceInfo.IsResourceStream(memoryStream))
                                {
                                    byte[] dataFromStream = ResourceUtils.ResourceInfo.GetDataFromStream(memoryStream);
                                    string path = string.Format("{0}{1}{2}", ExportFilesLocation, Path.DirectorySeparatorChar, (object)superEntry2.Entry.GetPathTill(superEntry1.Entry.AsDirectory, new string(new char[1]
                                    {
                                       Path.DirectorySeparatorChar
                                    })));
                                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                                    {
                                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                                    }
                                    Stream stream = File.Create(path);
                                    stream.Write(dataFromStream, 0, dataFromStream.Length);
                                    stream.Flush();
                                    stream.Close();
                                }
                                else
                                {
                                    int num = (int)MessageBox.Show(string.Format("File \"{0}\" was not a valid resource\r\n\r\nContinuing extraction..", superEntry2.Entry.Name), "Invalid Resource", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                }
                            }
                            else
                            {
                                string path = string.Format("{0}{1}{2}", ExportFilesLocation, Path.DirectorySeparatorChar, superEntry2.Entry.GetPathTill(superEntry1.Entry.AsDirectory, new string(new char[1]
                                {
                                    Path.DirectorySeparatorChar
                                })));

                                if (!Directory.Exists(Path.GetDirectoryName(path)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                                }
                                Stream xOut = File.Create(path);
                                this.rpfToExportFilesFrom.TOC.ExtractFile(superEntry2, xOut);
                                xOut.Flush();
                                xOut.Close();
                            }
                        }
                    }
                }));
                Done = true;
                Hide();
            }
            catch (Exception ex)
            {
                ExportFilesException = ex;
                Hide();
            }
        }

        private async void DoExportDirectoryWork()
        {
            try
            {
                await Task.Factory.StartNew((Action)(() =>
                {
                    Action<RPF6.RPF6TOC.TOCSuperEntry, string> extractItem = null;
                    extractItem = ((item, origPath) =>
                    {
                        string path = string.Format("{0}{1}{2}", origPath, Path.DirectorySeparatorChar, item.Entry.GetPathTill(directoryToExport.Entry.Parent, Path.DirectorySeparatorChar.ToString()));
                        if (item.IsDir)
                        {
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            foreach (RPF6.RPF6TOC.TOCSuperEntry child in item.Children)
                            {
                                extractItem(child, origPath);
                            }
                        }
                        else
                        {
                            Stream xOut = File.Create(path);
                            rpfToExportFilesFrom.TOC.ExtractFile(item, xOut);
                            xOut.Flush();
                            xOut.Close();
                        }
                    });
                    extractItem(directoryToExport, locationToExportTo);
                }));
                Done = true;
                Hide();
            }
            catch (Exception ex)
            {
                ExportFilesException = ex;
                Hide();
            }
        }

        public static string GetFileEntryInfo(RPF6.RPF6TOC.FileEntry asFile)
        {
            string str = "[";

            if ((asFile.FlagInfo.IsCompressed || asFile.FlagInfo.IsResource) && !asFile.Name.StartsWith("0x"))
            {
                if (asFile.FlagInfo.IsResource)
                {
                    switch (asFile.ResourceType)
                    {
                        case 1:
                            if (asFile.Name.EndsWith(".xst") || asFile.Name.EndsWith(".cst") || asFile.Name.EndsWith(".sst"))
                                str += "String Table"; //.xst
                            else if (asFile.Name.EndsWith(".xcs") || asFile.Name.EndsWith(".ccs") || asFile.Name.EndsWith(".wcs"))
                                str += "Battle Set"; //.xcs
                            else if (asFile.Name.EndsWith(".xfd") || asFile.Name.EndsWith(".cfd") || asFile.Name.EndsWith(".wfd"))
                                str += "Fragment Drawable"; //.xfd
                            break;
                        case 2:
                            str += "Script Resource"; //.xsc
                            break;
                        case 6:
                            str += "Animation Set"; //.xas
                            break;
                        case 10:
                            if (asFile.Name.EndsWith(".xtd") || asFile.Name.EndsWith(".ctd") || asFile.Name.EndsWith(".wtd"))
                                str += "Texture Dictionary"; //.xtd
                            else
                                str += "Texture Data"; //.xtx
                            break;
                        case 11:
                            str += "Texture Data"; //.xedt
                            break;
                        case 18:
                            if (asFile.Name.EndsWith(".xgd") || asFile.Name.EndsWith(".cgd") || asFile.Name.EndsWith(".wgd"))
                                str += "Gringo Dictionary"; //.xgd
                            else if (asFile.Name.EndsWith(".xsg") || asFile.Name.EndsWith(".csg") || asFile.Name.EndsWith(".wsg"))
                                str += "Grass Material"; //.xsg
                            break;
                        case 30:
                            str += "Action Tree"; //.xat
                            break;
                        case 31:
                            str += "Bounds Dictionary"; //.xbd
                            break;
                        case 33:
                            if (asFile.Name.EndsWith(".xnm") || asFile.Name.EndsWith(".cnm") || asFile.Name.EndsWith(".wnm"))
                                str += "Nav Mesh"; //.xnm
                            else if (asFile.Name.EndsWith(".xsf") || asFile.Name.EndsWith(".csf") || asFile.Name.EndsWith(".wsf"))
                                str += "Flash Textures"; //.xsf
                            break;
                        case 36:
                            str += "Terrain Collision"; //.xtb
                            break;
                        case 116:
                            str += "Speed Tree"; //.xsp
                            break;
                        case 133:
                            str += "Volume Data"; //.xvd
                            break;
                        case 134:
                            str += "Sector Info"; //.xsi
                            break;
                        case 138:
                            str += "Fragment Type"; //.xft
                            break;
                        default:
                            str += string.Format("Resource {0}", asFile.ResourceType);
                            break;
                    }
                }
                if (!asFile.FlagInfo.IsResource && asFile.FlagInfo.IsCompressed)
                {
                    int index = asFile.Name.LastIndexOf(".") + 1;
                    string extension = asFile.Name.Substring(index, asFile.Name.Length - index);

                    if (asFile.Name.EndsWith(".fxc") || asFile.Name.EndsWith(".nvn"))
                        str += "Compiled Shader";
                    else if (asFile.Name.Contains(".") && extension.Length <= 6)
                        str += string.Format("{0} File", extension.ToUpper());
                    else
                        str += string.Format("{0}Compressed", asFile.FlagInfo.IsResource ? ", " : "");
                }
            }
            else if (asFile.Name.Contains("."))
            {
                if (asFile.Name.EndsWith(".awc"))
                    str += "Audio Wave Container";
                else if (asFile.Name.EndsWith(".sco"))
                    str += "Script Resource";
                else
                {
                    int index = asFile.Name.LastIndexOf(".") + 1;
                    string extension = asFile.Name.Substring(index, asFile.Name.Length - index);

                    if (extension.Length > 6)
                        str += "Unknown";
                    else
                        str += string.Format("{0} File", extension.ToUpper());
                }
            }
            else
            {
                str += "Unknown";
            }
            return str + "]";
        }

        private void DoLoadDirectoryWork()
        {
            try
            {
                ItemsToAddToListView = new List<ListViewItem>();

                foreach (RPF6.RPF6TOC.TOCSuperEntry tocSuperEntry in directoryToLoad.AllChildren.OrderBy(x => !x.IsDir).ToList())
                {
                    if (tocSuperEntry.IsDir) continue;

                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Text = tocSuperEntry.Entry.Name;
                    listViewItem.Tag = tocSuperEntry;

                    if (!tocSuperEntry.Write || tocSuperEntry.DoesHaveParentMarkedNotToBeWritten)
                        listViewItem.BackColor = Color.FromArgb(150, byte.MaxValue, 50, 50);
                    else if (tocSuperEntry.CustomDataStream != null)
                        listViewItem.BackColor = Color.FromArgb(byte.MaxValue, 50, byte.MaxValue, 50);

                    RPF6.RPF6TOC.FileEntry asFile = tocSuperEntry.Entry.AsFile;
                    string text = GetFileEntryInfo(asFile);
                    listViewItem.SubItems.Add(text);
                    listViewItem.SubItems.Add(tocSuperEntry.Entry.AsFile.SizeInArchive.ToString());
                    listViewItem.ImageIndex = 1;
                    ItemsToAddToListView.Add(listViewItem);
                }
                Done = true;
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                LoadDirectoryException = ex;
                Hide();
            }
        }
    }
}
