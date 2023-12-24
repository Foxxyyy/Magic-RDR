using Magic_RDR.RPF;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_RDR
{
    static class Program
    {
        public static string SaveRPFPath = string.Empty;

        [STAThread]
        static void Main(string[] args)
        {
            if (!AppUtils.FileExists("/Assemblies/xcompress32.dll"))
                throw new Exception("Could not find \"xcompress32.dll\"");
            if (!AppUtils.FileExists("/Assemblies/PikIO.dll"))
                throw new Exception("Could not find \"PikIO.dll\"");

            string fileName = "/Settings/ImportedFileNames.txt";
            string settingsName = "/Settings/Settings.txt";

            if (AppUtils.FileExists(fileName))
            {
                Stream textFile1 = AppUtils.OpenFile(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                RPF6FileNameHandler.LoadNames(textFile1);
                textFile1.Close();
            }

            if (args.Length == 0)
            {
                if (Environment.OSVersion.Version.Major >= 6)
                    SetProcessDPIAware();

                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

                if (AppUtils.FileExists(settingsName))
                {
                    Stream settingsFile = AppUtils.OpenFile(settingsName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    RPF6FileNameHandler.LoadSettings(settingsFile);
                    settingsFile.Close();
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("RPFLoadLastRPF:False");
                    sb.AppendLine("RPFLastRPFPath:None");
                    sb.AppendLine("ListViewSorting:None");
                    sb.AppendLine("TreeViewShowLines:True");
                    sb.AppendLine("TreeViewShowPlusMinus:True");
                    sb.AppendLine("WPFUseCustomColor:False");
                    sb.AppendLine("WPFCustomColor1:#FFFF8000");
                    sb.AppendLine("WPFCustomColor2:#00000000");
                    File.AppendAllText(string.Format("{0}\\{1}", AppUtils.GetAppPath(), settingsName), sb.ToString());
                }
                System.Windows.Forms.Application.Run(new MainForm(string.Empty));
            }
            else
            {
                Console.WriteLine("\n---------------------");
                Console.WriteLine("\nMagicRDR by Im Foxxyyy");
                string actionType = args[0];
                string rpfPath = args[1];
                string filesDirectoryPath = args[2];
                string filesImportPath = args[3];
                string saveType = args[4];

                if (args.Length >= 6)
                {
                    SaveRPFPath = args[5];
                }

                if (!File.Exists(rpfPath))
                    throw new Exception("Invalid arguments! (incorrect RPF input)");

                if (!Directory.Exists(filesImportPath) && !File.Exists(filesImportPath))
                    throw new Exception("Invalid arguments! (incorrect import path)");

                if (actionType != "-import" && actionType != "-replace")
                    throw new Exception("Invalid arguments! (incorrect import type)");

                if (SaveRPFPath != string.Empty && (!Directory.Exists(Path.GetDirectoryName(SaveRPFPath)) || Path.GetExtension(SaveRPFPath) != ".rpf"))
                    throw new Exception("Invalid arguments! (incorrect RPF save path)");

                if (saveType == "-new" && SaveRPFPath == string.Empty)
                    throw new Exception("Invalid arguments! (no output path specified)");

                //Load the .RPF
                var form = new MainForm(rpfPath);
                var rpf = form.CurrentRPF;

                //Verify if the user wants to import one or more files
                string singleFile = string.Empty;
                if (Path.HasExtension(filesImportPath))
                {
                    singleFile = Path.GetFileName(filesImportPath);
                }

                //Get to the .RPF directory
                filesDirectoryPath = filesDirectoryPath.Replace("\\", "/");
                foreach (var entry in rpf.TOC.Entries)
                {
                    if (entry is RPF6.RPF6TOC.DirectoryEntry directory)
                    {
                        if (directory.SuperOwner.GetPath() == filesDirectoryPath)
                        {
                            form.CurrentDirectory = entry.SuperOwner;
                            break;
                        }
                    }
                }

                if (form.CurrentDirectory == null)
                {
                    throw new Exception("Invalid RPF directory!");
                }

                Console.WriteLine(string.Format("Selected RPF directory : {0}", form.CurrentDirectory.GetPath()));

                //Get the file(s) to import
                string[] files;
                if (singleFile == string.Empty)
                    files = Directory.GetFiles(filesImportPath);
                else
                    files = new string[] { filesImportPath };
                
                if (files.Length <= 0)
                {
                    return;
                }

                Console.WriteLine(string.Format("Found {0} file(s) to {1}", files.Length, (actionType == "-replace") ? "replace" : "import"));

                //Import-replace the files
                foreach (var file in files)
                {
                    if (form.CurrentDirectory.DoesHaveEntry(Path.GetFileName(file)) && actionType == "-replace")
                    {
                        form.DoFileImportReplace(replaceFileLoc: file, skipChecks: true);
                    }
                    else if (actionType == "-import")
                    {
                        form.DoFileImportReplace(importFileLoc: file, skipChecks: true);
                    }
                }
                
                //Save the .RPF
                try
                {
                    if (saveType == "-new")
                        form.asNewToolStripMenuItem_Click(null, null);
                    else
                        form.saveToolStripMenuItem1_Click(null, null);

                    Console.WriteLine(string.Format("Done {0}", (actionType == "-replace") ? "replacing" : "importing"));
                    Console.WriteLine("Closing in 3...");
                    Thread.Sleep(1000);
                    Console.Write(" 2...");
                    Thread.Sleep(1000);
                    Console.Write(" 1...\n");
                    Thread.Sleep(1000);
                    Console.WriteLine("---------------------\n");
                    Environment.Exit(0);
                }
                catch
                {
                    throw new Exception("Couldn't save the .RPF!");
                }
                Console.ReadLine();
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
