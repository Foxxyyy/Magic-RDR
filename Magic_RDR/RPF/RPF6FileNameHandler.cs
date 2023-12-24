using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media;

namespace Magic_RDR.RPF
{
    public static class RPF6FileNameHandler
    {
        //File names
        public static Dictionary<uint, string> FileNames { get; set; }

        //Settings
        public static SortOrder Sorting { get; set; }
        public static string SortColumn { get; set; }
        public static PictureBoxSizeMode ImageSizeMode { get; set; }
        public static string LastRPFPath { get; set; }
        public static bool UseLastRPF { get; set; }
        public static bool ShowPlusMinus { get; set; }
        public static bool ShowLines { get; set; }
        public static bool UseCustomColor { get; set; }
        public static Color CustomColor1 { get; set; }
        public static Color CustomColor2 { get; set; }
        public static System.Drawing.Color TextureBackgroundColor { get; set; }
        public static string SAVFilePath { get; set; }

        public static void LoadSettings(Stream settingsFile = null)
        {
            if (settingsFile == null || settingsFile.Length == 0x0)
            {
                settingsFile.Dispose();
                SetDefaultSettings();
                return;
            }

            StreamReader streamReader = new StreamReader(settingsFile);
            while (!streamReader.EndOfStream)
            {
                string str = streamReader.ReadLine();
                if (!str.Contains(":"))
                {
                    continue;
                }

                int index = str.IndexOf(":") + 1;
                string settings = str.Substring(0, index - 1);
                string settingsValue = str.Substring(index, str.Length - index);

                switch (settings)
                {
                    case "RPFLoadLastRPF":
                        UseLastRPF = settingsValue != "False";
                        break;
                    case "RPFLastRPFPath":
                        LastRPFPath = settingsValue;
                        break;
                    case "ListViewSorting":
                        switch (settingsValue)
                        {
                            case "Descending":
                                Sorting = SortOrder.Descending;
                                break;
                            default:
                                Sorting = SortOrder.Ascending;
                                break;
                        }
                        break;
                    case "ListViewSortingColumn":
                        switch (settingsValue)
                        {
                            case "Type":
                                SortColumn = "Type";
                                break;
                            case "Size":
                                SortColumn = "Size";
                                break;
                            default:
                                SortColumn = "Name";
                                break;
                        }
                        break;
                    case "TreeViewShowPlusMinus":
                        ShowPlusMinus = settingsValue != "False";
                        break;
                    case "TreeViewShowLines":
                        ShowLines = settingsValue != "False";
                        break;
                    case "WPFUseCustomColor":
                        UseCustomColor = settingsValue != "False";
                        break;
                    case "WPFCustomColor1":
                        CustomColor1 = (Color)ColorConverter.ConvertFromString(settingsValue);
                        break;
                    case "WPFCustomColor2":
                        CustomColor2 = (Color)ColorConverter.ConvertFromString(settingsValue);
                        break;
                    case "TextureImageSizeMode":
                        switch (settingsValue)
                        {
                            case "Zoom":
                                ImageSizeMode = PictureBoxSizeMode.Zoom;
                                break;
                            case "CenterImage":
                                ImageSizeMode = PictureBoxSizeMode.CenterImage;
                                break;
                            case "StretchImage":
                                ImageSizeMode = PictureBoxSizeMode.StretchImage;
                                break;
                            default:
                                ImageSizeMode = PictureBoxSizeMode.AutoSize;
                                break;
                        }
                        break;
                    case "TextureBackgroundColor":
                        switch (settingsValue)
                        {
                            case "Color [Black]":
                                TextureBackgroundColor = System.Drawing.Color.Black;
                                break;
                            case "Color [White]":
                                TextureBackgroundColor = System.Drawing.Color.White;
                                break;
                            case "Color [Red]":
                                TextureBackgroundColor = System.Drawing.Color.Red;
                                break;
                            case "Color [Green]":
                                TextureBackgroundColor = System.Drawing.Color.Green;
                                break;
                            case "Color [Blue]":
                                TextureBackgroundColor = System.Drawing.Color.Blue;
                                break;
                            default:
                                TextureBackgroundColor = System.Drawing.Color.Transparent;
                                break;
                        }
                        break;
                    case "SAVFilePath":
                        SAVFilePath = settingsValue;
                        break;
                    default:
                        break;
                }
            }
        }

        public static void SetDefaultSettings()
        {
            UseLastRPF = false;
            LastRPFPath = "None";
            Sorting = SortOrder.None;
            SortColumn = "Name";
            ShowPlusMinus = true;
            ShowLines = true;
            UseCustomColor = false;
            CustomColor1 = Color.FromArgb(255, 104, 138, 213);
            CustomColor2 = Color.FromArgb(255, 66, 92, 148);
            TextureBackgroundColor = System.Drawing.Color.Black;
            ImageSizeMode = PictureBoxSizeMode.AutoSize;
            SAVFilePath = "None";
            SaveSettings();
        }

        public static void LoadNames(Stream textFile = null)
        {
            if (FileNames == null)
                FileNames = new Dictionary<uint, string>();

            if (textFile == null)
                return;

            StreamReader streamReader = new StreamReader(textFile);
            int num = 1;

            while (!streamReader.EndOfStream)
            {
                string str = streamReader.ReadLine();
                uint hash = DataUtils.GetHash(str);

                if (!FileNames.ContainsKey(hash))
                {
                    FileNames.Add(hash, str);
                }
                ++num;
            }
        }

        public static bool AddName(string name)
        {
            uint hash = DataUtils.GetHash(name);
            if (FileNames.ContainsKey(hash))
            {
                return false;
            }
            FileNames.Add(hash, name);
            return true;
        }

        public static void SaveNames()
        {
            StreamWriter streamWriter = new StreamWriter(AppUtils.OpenFile("/Settings/ImportedFileNames.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
            streamWriter.BaseStream.SetLength(0L);

            foreach (KeyValuePair<uint, string> fileName in FileNames)
            {
                streamWriter.WriteLine(fileName.Value);
            }
            streamWriter.Flush();
            streamWriter.Close();
        }

        public static void SaveSettings()
        {
            try
            {
                StreamWriter streamWriter = new StreamWriter(AppUtils.OpenFile("/Settings/Settings.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read));
                streamWriter.BaseStream.SetLength(0L);
                streamWriter.WriteLine("RPFLoadLastRPF:" + UseLastRPF.ToString());
                streamWriter.WriteLine("RPFLastRPFPath:" + LastRPFPath.ToString());
                streamWriter.WriteLine("ListViewSorting:" + Sorting.ToString());
                streamWriter.WriteLine("ListViewSortingColumn:" + SortColumn.ToString());
                streamWriter.WriteLine("TreeViewShowLines:" + ShowLines.ToString());
                streamWriter.WriteLine("TreeViewShowPlusMinus:" + ShowPlusMinus.ToString());
                streamWriter.WriteLine("WPFUseCustomColor:" + UseCustomColor.ToString());
                streamWriter.WriteLine("WPFCustomColor1:" + CustomColor1.ToString());
                streamWriter.WriteLine("WPFCustomColor2:" + CustomColor2.ToString());
                streamWriter.WriteLine("TextureBackgroundColor:" + TextureBackgroundColor.ToString());
                streamWriter.WriteLine("TextureImageSizeMode:" + ImageSizeMode.ToString());
                streamWriter.WriteLine("SAVFilePath:" + SAVFilePath.ToString());
                streamWriter.Flush();
                streamWriter.Close();
            }
            catch { }
        }
    }
}
