using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using Magic_RDR.Application;
using Magic_RDR.RPF;
using static Magic_RDR.RPF6.RPF6TOC;

namespace Magic_RDR
{
    public partial class ScriptViewerForm : Form
    {
        private IOReader Reader;
        private string FileName = "";
        private string TempFileName;

        public static object ThreadLock;
        public static string DecompiledCode;
        public static int ExceptionsHandled = 0;

        public ScriptViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            ThreadLock = new object();
            Text = string.Format("MagicRDR - Script Viewer [{0}]", entry.Entry.Name);
            FileName = entry.Entry.Name;
            ExceptionsHandled = 0;

            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();

            byte[] fileData = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(RPFFile.RPFIO.ReadBytes(file.SizeInArchive));
            Reader = new IOReader(new MemoryStream(fileData), (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? IOReader.Endian.Little : IOReader.Endian.Big);
            Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

            ScriptFile script = new ScriptFile(Reader, file);

            try
            {
                textBox.Text = DecompiledCode;
            }
            catch
            {
                MessageBox.Show("This script is too big. It'll now be shown as plain text...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox.Text = "";
                TempFileName = Path.GetTempFileName();
                File.WriteAllText(TempFileName, DecompiledCode);
                textBox.OpenBindingFile(TempFileName, Encoding.UTF8);
            }
            charCountLabel.Text = string.Format("{0} characters, {1} lines", textBox.Text.Length, textBox.LinesCount);
            zoomLabel.Text = string.Format("Zoom {0}%", textBox.Zoom);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                MessageBox.Show("There's nothing to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.Filter = "C Source Code|*.c";
            dialog.FileName = FileName.Replace(".xsc", ".c").Replace(".wsc", ".c");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dialog.FileName, textBox.Text);
                MessageBox.Show("Successfully exported !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            charCountLabel.Text = string.Format("{0} characters, {1} lines", textBox.Text.Length, textBox.LinesCount);
        }

        private void zoomLabel_Click(object sender, EventArgs e)
        {
            textBox.Zoom = 100;
        }

        private void textBox_ZoomChanged(object sender, EventArgs e)
        {
            zoomLabel.Text = string.Format("Zoom {0}%", textBox.Zoom);
        }

        private void ScriptViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (TempFileName == string.Empty)
                return;

            try { textBox.CloseBindingFile(); }
            catch { }
        }
    }

    public class ScriptFile
    {
        private IOReader Reader;
        private FileEntry Entry;
        private int ParameterCount, StaticCount, NativeCount;
        private int StaticPointer;
        private int[] CodeTablePointers;
        private int PageCount, Offset;

        public static int ReturnType, Attempts;
        public static bool OpcodeReturn;

        private List<byte> CodeTable = new List<byte>();
        public List<Function> Functions;
        public Dictionary<int, FunctionName> FunctionLoc;
        public NativeTable NativeTable;
        internal Variables Statics;
        internal static NativeParamInfo NativeInfo = new NativeParamInfo();
        public Dictionary<string, Tuple<int, int>> Function_loc = new Dictionary<string, Tuple<int, int>>();

        public ScriptFile(object reader, FileEntry entry)
        {
            Reader = (IOReader)reader;
            Entry = entry;
            XSC_ReadMainStructure();
        }

        private void XSC_ReadMainStructure()
        {
            Reader.BaseStream.Position = Entry.FlagInfo.RSC85_ObjectStart;
            uint magic = Reader.ReadUInt32();
            int SubHeaderPointer = Reader.ReadOffset(Reader.ReadInt32());
            int CodePointer = Reader.ReadOffset(Reader.ReadInt32());
            int CodeLength = Reader.ReadInt32();
            ParameterCount = Reader.ReadInt32();
            StaticCount = Reader.ReadInt32();
            StaticPointer = Reader.ReadOffset(Reader.ReadInt32());
            int GlobalVersion = Reader.ReadInt32();
            NativeCount = Reader.ReadOffset(Reader.ReadInt32());
            int NativePointer = Reader.ReadOffset(Reader.ReadInt32());

            Reader.BaseStream.Seek(CodePointer, SeekOrigin.Begin);
            PageCount = Reader.GetPageCount(CodeLength);
            CodeTablePointers = new int[PageCount];

            for (int i = 0; i < PageCount; i++)
            {
                CodeTablePointers[i] = Reader.ReadOffset(Reader.ReadInt32());
            }

            for (int i = 0; i < PageCount; i++)
            {
                Reader.BaseStream.Seek(CodeTablePointers[i], SeekOrigin.Begin);
                long Tablesize = Reader.BaseStream.Position + Reader.GetPageLengthAtPage(CodeTablePointers, CodeLength, i);
                
                byte[] working = new byte[Tablesize];
                Reader.BaseStream.Read(working, 0, (int)Tablesize);
                CodeTable.AddRange(working);
            }

            NativeTable = new NativeTable(Reader, NativePointer, NativeCount);
            GetStaticInfo();
            Functions = new List<Function>();
            FunctionLoc = new Dictionary<int, FunctionName>();
            GetFunctions();

            for (int i = 0; i < Functions.Count - 1; i++)
            {
                try
                {
                    Functions[i].PreDecode();
                }
                catch
                {
                   ScriptViewerForm.ExceptionsHandled++;
                }
            }

            Statics.CheckVariables();
            for (int i = 0; i < Functions.Count - 1; i++)
            {
                try
                {
                    Functions[i].Decode();
                }
                catch
                {
                    ScriptViewerForm.ExceptionsHandled++;
                }
            }

            int index = 1;
            StringBuilder fullCode = new StringBuilder();
            fullCode.AppendLine("//Decompiled with MagicRDR v1.0");
            fullCode.AppendLine(string.Format("//Function Count : {0}", Functions.Count));
            fullCode.AppendLine(string.Format("//Statics Count : {0}", StaticCount));
            fullCode.AppendLine(string.Format("//Natives Count : {0}", NativeCount));
            fullCode.AppendLine(string.Format("//Parameters Count : {0}\n", ParameterCount));
            fullCode.AppendLine("#region Local Var");
            index++;

            try
            {
                foreach (string statics in Statics.GetDeclaration(false))
                {
                    fullCode.AppendLine("\t" + statics);
                    index++;
                }
            }
            catch { }
            fullCode.AppendLine("#endregion\n");
            index += 2;

            foreach (Function f in Functions)
            {
                try
                {
                    string s = f.ToString();
                    fullCode.AppendLine(s);
                    Function_loc.Add(f.Name, new Tuple<int, int>(index, f.Location));
                    index += f.LineCount;
                }
                catch { }
            }
            ScriptViewerForm.DecompiledCode = fullCode.ToString();
        }

        private void GetStaticInfo()
        {
            Statics = new Variables(Variables.ListType.Statics);
            Statics.SetScriptParamCount(ParameterCount);

            Reader.BaseStream.Seek(StaticPointer, SeekOrigin.Begin);
            for (int count = 0; count < StaticCount; count++)
            {
                Statics.AddVar(Reader.ReadInt32());
            }
        }

        void GetFunctions()
        {
            int returnpos = -3;
            while (Offset < CodeTable.Count)
            {
                switch (CodeTable[Offset])
                {
                    case 37: AdvancePosition(1); break;
                    case 38: AdvancePosition(2); break;
                    case 39: AdvancePosition(3); break;
                    case 40: AdvancePosition(4); break;
                    case 41: AdvancePosition(4); break;
                    case 44: AdvancePosition(2); break;
                    case 45: try { AddFunction(Offset, returnpos + 3); Attempts++; AdvancePosition(CodeTable[Offset + 4] + 4); } catch { } break;
                    case 46: returnpos = Offset; AdvancePosition(2); break;
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                    case 58:
                    case 59:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64: AdvancePosition(1); break;
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 72:
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                    case 86:
                    case 87:
                    case 88:
                    case 89:
                    case 90:
                    case 91:
                    case 92:
                    case 93:
                    case 94:
                    case 95:
                    case 96:
                    case 97:
                    case 98:
                    case 99:
                    case 100:
                    case 101:
                    case 102:
                    case 103:
                    case 104:
                    case 105: AdvancePosition(2); break;
                    case 106:
                    case 107:
                    case 108:
                    case 109: AdvancePosition(3); break;
                    case 110: AdvancePosition(1 + CodeTable[Offset + 1] * 6); break;
                    case 111: AdvancePosition(1 + CodeTable[Offset + 1]); break;
                    case 112: AdvancePosition(5 + CodeTable[Offset + 1]); break;
                    case 114:
                    case 115:
                    case 116:
                    case 117: AdvancePosition(1); break;
                    case 122:
                    case 123:
                    case 124:
                    case 125:
                    case 126:
                    case 127:
                    case 128:
                    case 129:
                    case 130:
                    case 131:
                    case 132:
                    case 133:
                    case 134:
                    case 135:
                    case 136:
                    case 137: returnpos = Offset; break;
                }
                AdvancePosition(1);
            }
            GetFunctionCode();
        }

        void AddFunction(int start1, int start2)
        {
            byte namelen = CodeTable[start1 + 4];
            string name = "";

            if (namelen > 0)
            {
                for (int i = 0; i < namelen; i++)
                {
                    try
                    {
                        name += (char)CodeTable[start1 + 5 + i];
                    }
                    catch
                    {
                        name = "Function_" + Functions.Count.ToString();
                    }
                }
            }
            else if (start1 == 0)
                name = "main";
            else
                name = "Function_" + Functions.Count.ToString();

            if (name.StartsWith("0~"))
                name = name.Replace("0~", "static ");

            int pcount = CodeTable[Offset + 1];
            int tmp1 = CodeTable[Offset + 2], tmp2 = CodeTable[Offset + 3];
            int vcount = (tmp1 << 0x8) | tmp2;

            if (vcount < 0)
            {
                throw new Exception("Well this shouldn't have happened");
            }
            int temp = start1 + 5 + namelen;

            while (!IsReturnInstruction(temp))
            {
                //Console.WriteLine(name + " " + CodeTable[temp]);
                switch (CodeTable[temp])
                {
                    case 37: temp += 1; break;
                    case 38: temp += 2; break;
                    case 39: temp += 3; break;
                    case 40: temp += 4; break;
                    case 41: temp += 4; break;
                    case 44: temp += 2; break;
                    case 45: throw new Exception("OpCode 'enter' was unexpected");
                    case 46: throw new Exception("OpCode 'return' was unexpected");
                    case 52: temp += 1; break;
                    case 53: temp += 1; break;
                    case 54: temp += 1; break;
                    case 55: temp += 1; break;
                    case 56: temp += 1; break;
                    case 57: temp += 1; break;
                    case 58: temp += 1; break;
                    case 59: temp += 1; break;
                    case 60: temp += 1; break;
                    case 61: temp += 1; break;
                    case 62: temp += 1; break;
                    case 63: temp += 1; break;
                    case 64: temp += 1; break;
                    case 65: temp += 2; break;
                    case 66: temp += 2; break;
                    case 67: temp += 2; break;
                    case 68: temp += 2; break;
                    case 69: temp += 2; break;
                    case 70: temp += 2; break;
                    case 71: temp += 2; break;
                    case 72: temp += 2; break;
                    case 73: temp += 2; break;
                    case 74: temp += 2; break;
                    case 75: temp += 2; break;
                    case 76: temp += 2; break;
                    case 77: temp += 2; break;
                    case 78: temp += 2; break;
                    case 79: temp += 2; break;
                    case 80: temp += 2; break;
                    case 81: temp += 2; break;
                    case 82: temp += 2; break;
                    case 83: temp += 2; break;
                    case 84: temp += 2; break;
                    case 85: temp += 2; break;
                    case 86: temp += 2; break;
                    case 87: temp += 2; break;
                    case 88: temp += 2; break;
                    case 89: temp += 2; break;
                    case 90: temp += 2; break;
                    case 91: temp += 2; break;
                    case 92: temp += 2; break;
                    case 93: temp += 2; break;
                    case 94: temp += 2; break;
                    case 95: temp += 2; break;
                    case 96: temp += 2; break;
                    case 97: temp += 2; break;
                    case 98: temp += 2; break;
                    case 99: temp += 2; break;
                    case 100: temp += 2; break;
                    case 101: temp += 2; break;
                    case 102: temp += 2; break;
                    case 103: temp += 2; break;
                    case 104: temp += 2; break;
                    case 105: temp += 2; break;
                    case 106: temp += 3; break;
                    case 107: temp += 3; break;
                    case 108: temp += 3; break;
                    case 109: temp += 3; break;
                    case 110: temp += 1 + CodeTable[temp + 1] * 6; break;
                    case 111: temp += 1 + CodeTable[temp + 1]; break;
                    case 112: temp += 5 + CodeTable[temp + 1]; break;
                    case 114: temp += 1; break;
                    case 115: temp += 1; break;
                    case 116: temp += 1; break;
                    case 117: temp += 1; break;
                }
                temp += 1;
            }
            if (OpcodeReturn)
                ReturnType = CodeTable[temp + 2];

            int Location = start2;
            if (start1 == start2)
                Functions.Add(new Function(this, name, pcount, vcount, ReturnType, Location, -1));
            else
                Functions.Add(new Function(this, name, pcount, vcount, ReturnType, Location, start1));
        }

        public void GetFunctionCode()
        {
            for (int i = 0; i < Functions.Count - 1; i++)
            {
                int start = Functions[i].MaxLocation;
                int end = Functions[i + 1].Location;

                if (end > start)
                    Functions[i].CodeBlock = CodeTable.GetRange(start, end - start);
                else
                    Functions[i].CodeBlock = CodeTable.GetRange(start, start - end);
            }
            int startLast = Functions[Functions.Count - 1].MaxLocation;
            int endLast = CodeTable.Count - Functions[Functions.Count - 1].MaxLocation;
            Functions[Functions.Count - 1].CodeBlock = CodeTable.GetRange(startLast, endLast);
        }

        public bool IsReturnInstruction(int temp)
        {
            switch (CodeTable[temp])
            {
                case 123:
                case 127:
                case 131:
                case 135:
                    ReturnType = 1;
                    OpcodeReturn = false;
                    break;
                case 124:
                case 128:
                case 132:
                case 136:
                    ReturnType = 2;
                    OpcodeReturn = false;
                    break;
                case 125:
                case 129:
                case 133:
                case 137:
                    ReturnType = 3;
                    OpcodeReturn = false;
                    break;
                case 46:
                    OpcodeReturn = true;
                    break;
                default:
                    ReturnType = 0;
                    OpcodeReturn = false;
                    break;
            }
            return CodeTable[temp] == 46 || (CodeTable[temp] > 121 && CodeTable[temp] < 138);
        }

        void AdvancePosition(int position)
        {
            Offset += position;
        }
    }
}
