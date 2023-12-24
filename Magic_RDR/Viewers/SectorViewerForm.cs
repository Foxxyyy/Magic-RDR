using Magic_RDR.Application;
using Magic_RDR.RPF;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Text;
using static Magic_RDR.RPF6.RPF6TOC;
using System;

namespace Magic_RDR.Models
{
    public partial class SectorViewerForm : Form
    {
        private IOReader Reader;
        private byte[] HeaderData;

        public SectorViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Text = string.Format("MagicRDR - Sector Data Viewer - [{0}]", entry.Entry.Name);

            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();
            byte[] compressedData = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            byte[] fileData = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(compressedData);

            HeaderData = new byte[0x18];
            for (int i = 0; i < HeaderData.Length; i++)
            {
                HeaderData[i] = compressedData[i];
            }

            Reader = new IOReader(new MemoryStream(fileData), IOReader.Endian.Big);
            Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

            SectorData data = new SectorData(Reader);

            //Recompress into a single block
            Reader.BaseStream.Position = 0x0;
            byte[] newData = Reader.ReadBytes(Reader.BaseStream.Length);
            newData = DataUtils.CompressLZX(newData);
            byte[] buffer = new byte[HeaderData.Length + newData.Length];

            byte[] dataLength = BitConverter.GetBytes(newData.Length.Swap());
            HeaderData[20] = dataLength[0];
            HeaderData[21] = dataLength[1];
            HeaderData[22] = dataLength[2];
            HeaderData[23] = dataLength[3];

            Buffer.BlockCopy(HeaderData, 0, buffer, 0, HeaderData.Length);
            Buffer.BlockCopy(newData, 0, buffer, HeaderData.Length, newData.Length);

            TreeNode sectorInfo = treeView.Nodes.Add("Sector Info");
            TreeNode globalSector = sectorInfo.Nodes.Add("World Sector");

            int count = 0;
            foreach (var child in SectorData.Data)
            {
                if (count == 0)
                {
                    globalSector.Nodes.Add(string.Format("Global Bounds Min : {0}, {1}, {2}", FormatFloat(child.BoundsMin.X), FormatFloat(child.BoundsMin.Y), FormatFloat(child.BoundsMin.Z)));
                    globalSector.Nodes.Add(string.Format("Global Bounds Max : {0}, {1}, {2}", FormatFloat(child.BoundsMax.X), FormatFloat(child.BoundsMax.Y), FormatFloat(child.BoundsMax.Z)));
                    count++;
                    continue;
                }

                TreeNode childSector = globalSector.Nodes.Add(string.Format("Child : {0}", child.SectorName));
                childSector.Nodes.Add(string.Format("Bounds Min : {0}, {1}, {2}", FormatFloat(child.BoundsMin.X), FormatFloat(child.BoundsMin.Y), FormatFloat(child.BoundsMin.Z)));
                childSector.Nodes.Add(string.Format("Bounds Max : {0}, {1}, {2}", FormatFloat(child.BoundsMax.X), FormatFloat(child.BoundsMax.Y), FormatFloat(child.BoundsMax.Z)));

                if (child.ChildProps != null)
                {
                    for (int i = 0; i < child.ChildProps.Count; i++)
                    {
                        TreeNode prop = childSector.Nodes.Add(string.Format("Prop {0} : {1}", i + 1, child.ChildProps[i]));
                        prop.Nodes.Add(string.Format("Position : {0}, {1}, {2}", FormatFloat(child.ChildPosition[i].X), FormatFloat(child.ChildPosition[i].Y), FormatFloat(child.ChildPosition[i].Z)));
                    }
                }
                count++;
            }
        }

        public string FormatFloat(float value)
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }
    }

    public class SectorData
    {
        private IOReader Reader;
        public static List<SectorInfoData> Data;
        public StringBuilder sb = new StringBuilder();

        public SectorData(object reader)
        {
            Reader = (IOReader)reader;
            Data = new List<SectorInfoData>();
            XSI_ReadMainStructure();
        }

        private void XSI_ReadMainStructure(int pointer = -1)
        {
            if (pointer != -1)
            {
                Reader.BaseStream.Seek(pointer, SeekOrigin.Begin);
            }

            uint _VMT = Reader.ReadUInt32();
            int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32()); //Unused
            int SectorNamePointer = Reader.ReadOffset(Reader.ReadInt32());
            string SectorName = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, SectorNamePointer, true);
            Vector4 Unknown_0xC = Reader.ReadVector4(); //Actually uint[4]

            uint SectorNameHash = Reader.ReadUInt32();
            sb.AppendLine(SectorName);

            uint[] Padding = new uint[14];
            for (int i = 0; i < Padding.Length; i++)
            {
                Padding[i] = Reader.ReadUInt32();
            }

            int SectorCurvesStructurePointer = Reader.ReadOffset(Reader.ReadInt32());

            uint[] Padding2 = new uint[21];
            for (int i = 0; i < Padding2.Length; i++)
            {
                Padding2[i] = Reader.ReadUInt32();
            }

            Vector4 GlobalBoundsMin = Reader.ReadVector4();
            Vector4 GlobalBoundsMax = Reader.ReadVector4();

            _ = Reader.ReadUInt32();

            RGArray Unknown_0xD4;
            Unknown_0xD4.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0xD4.Count = Reader.ReadUInt16();
            Unknown_0xD4.Size = Reader.ReadUInt16();

            int Unknown_0xDC = Reader.ReadOffset(Reader.ReadInt32());
            int Unknown_0xE0 = Reader.ReadOffset(Reader.ReadInt32());
            uint _pad = Reader.ReadUInt32();

            RGArray Unknown_0xE8;
            Unknown_0xE8.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0xE8.Count = Reader.ReadUInt16();
            Unknown_0xE8.Size = Reader.ReadUInt16();
            int ChildsPointer = Reader.ReadOffset(Reader.ReadInt32());

            RGArray Unknown_0xF4;
            Unknown_0xF4.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0xF4.Count = Reader.ReadUInt16();
            Unknown_0xF4.Size = Reader.ReadUInt16();

            uint[] Padding3 = new uint[40];
            for (int i = 0; i < Padding.Length; i++)
            {
                Padding3[i] = Reader.ReadUInt32();
            }

            RGArray ChildCollectionPointer;
            ChildCollectionPointer.Offset = Reader.ReadOffset(Reader.ReadInt32());
            ChildCollectionPointer.Count = Reader.ReadUInt16();
            ChildCollectionPointer.Size = Reader.ReadUInt16();

            _ = Reader.ReadUInt32();
            _ = Reader.ReadUInt32();
            RGArray UnkCollectionPointer;
            UnkCollectionPointer.Offset = Reader.ReadOffset(Reader.ReadInt32());
            UnkCollectionPointer.Count = Reader.ReadUInt16();
            UnkCollectionPointer.Size = Reader.ReadUInt16();

            SectorInfoData data = new SectorInfoData(SectorName, GlobalBoundsMin, GlobalBoundsMax, null, null);
            Data.Add(data);

            if (SectorCurvesStructurePointer != 0)
            {
                //Reader.BaseStream.Seek(SectorCurvesStructurePointer, SeekOrigin.Begin);
                //XSI_GetCurves();
            }

            if (Unknown_0xE8.Offset != 0 || Unknown_0xE8.Count != 0)
                XSI_GetAllSectorsBounds(Unknown_0xE8.Offset, Unknown_0xE8.Count);
            if (ChildsPointer != 0) //Used untill there's no more childs
                XSI_ReadChilds(ChildsPointer);

            else if (Unknown_0xF4.Offset != 0) //Used only for tiles ???
            {
                Reader.BaseStream.Seek(Unknown_0xF4.Offset, SeekOrigin.Begin);
                XSI_ReadUnknownStructure(Unknown_0xF4.Offset, Unknown_0xF4.Count);
            }

            if (Unknown_0xD4.Offset != 0) //Used by childs
            {
                Reader.BaseStream.Seek(Unknown_0xD4.Offset, SeekOrigin.Begin);
                (List<string>, List<Vector4>) childs = XSI_ReadFragmentsData(Unknown_0xD4.Count);
                Data[Data.Count - 1].ChildProps = childs.Item1;
                Data[Data.Count - 1].ChildPosition = childs.Item2;
            }
        }

        private void XSI_GetAllSectorsBounds(int pointer, int count) //Size : 0x70 - seems to occurs only for swAll.xsi
        {
            Reader.BaseStream.Seek(pointer, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                Vector4 BoundsMin = Reader.ReadVector4();
                if (BoundsMin.X == -2211.86182f && BoundsMin.Y == 6.040268f)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position - 16, SeekOrigin.Begin);
                    Reader.BaseStream.Write(GetBytes(new Vector3(-3000.0f, 10.0f, 2000.0f)), 0, 12);
                    Reader.BaseStream.Seek(Reader.BaseStream.Position + 4, SeekOrigin.Begin);
                }
                Vector4 BoundsMax = Reader.ReadVector4();
                if (BoundsMax.X == -2059.69775f && BoundsMax.Y == 26.6699753f)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position - 16, SeekOrigin.Begin);
                    Reader.BaseStream.Write(GetBytes(new Vector3(-2000.0f, 50.0f, 3000.0f)), 0, 12);
                    Reader.BaseStream.Seek(Reader.BaseStream.Position + 4, SeekOrigin.Begin);
                }
                uint Unknown_0x20 = Reader.ReadUInt32(); // Always zero
                string SectorName = Reader.ReadString(IOReader.StringType.ASCII_NULL_TERMINATED, 0);

                while (Reader.ReadByte() == 0xCD)
                {
                    continue;
                }
                Reader.BaseStream.Seek(Reader.BaseStream.Position - 1, SeekOrigin.Begin);

                int SectorNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                uint Unknown_0x68 = Reader.ReadUInt32();
                uint Unknown_0x6C = Reader.ReadUInt32();

                SectorInfoData data = new SectorInfoData(SectorName, BoundsMin, BoundsMax, null, null);
                Data.Add(data);
            }
        }

        private void XSI_GetCurves()
        {
            Vector4 BoundsMin = Reader.ReadVector4();
            Vector4 BoundsMax = Reader.ReadVector4();

            RGArray CurvesStructure;
            CurvesStructure.Offset = Reader.ReadOffset(Reader.ReadInt32());
            CurvesStructure.Count = Reader.ReadUInt16();
            CurvesStructure.Size = Reader.ReadUInt16();

            for (int i = 0; i < CurvesStructure.Count; i++)
            {
                Reader.BaseStream.Seek(CurvesStructure.Offset + 0x180, SeekOrigin.Begin);

                RGArray Curves;
                Curves.Offset = Reader.ReadOffset(Reader.ReadInt32());
                Curves.Count = Reader.ReadUInt16();
                Curves.Size = Reader.ReadUInt16();

                for (int c = 0; c < CurvesStructure.Count; c++)
                {
                    //0x50
                    Reader.BaseStream.Seek(Curves.Offset, SeekOrigin.Begin);
                    uint VMT = Reader.ReadUInt32();
                    int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
                    uint Unknown_0x08 = Reader.ReadUInt32();
                    int CurveDataPointer = Reader.ReadOffset(Reader.ReadInt32());
                    uint Unknown_0x10 = Reader.ReadUInt32();
                    float Unknown_0x14 = Reader.ReadFloat();
                    float Unknown_0x18 = Reader.ReadFloat();
                    uint Unknown_0x1C = Reader.ReadUInt32();
                    Vector4 CurveBoundsMin = Reader.ReadVector4();
                    Vector4 CurveBoundsMax = Reader.ReadVector4();
                    int Unknown_0x40 = Reader.ReadOffset(Reader.ReadInt32());
                    uint ChildNameHash = Reader.ReadUInt32();
                }
            }
        }

        private void XSI_ReadChilds(int pointer)
        {
            Reader.BaseStream.Seek(pointer, SeekOrigin.Begin);
            RGArray Unknown_0x0;
            Unknown_0x0.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0x0.Count = Reader.ReadUInt16();
            Unknown_0x0.Size = Reader.ReadUInt16();

            RGArray Unknown_0x08;
            Unknown_0x08.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0x08.Count = Reader.ReadUInt16();
            Unknown_0x08.Size = Reader.ReadUInt16();

            RGArray Unknown_0x10;
            Unknown_0x10.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0x10.Count = Reader.ReadUInt16();
            Unknown_0x10.Size = Reader.ReadUInt16();

            int ChildNamePointer = Reader.ReadOffset(Reader.ReadInt32());
            string SectorName = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, ChildNamePointer, true);
            uint Value = Reader.ReadUInt32();

            for (int i = 0; i < Unknown_0x0.Count; i++)
            {
                Reader.BaseStream.Seek(Unknown_0x0.Offset + (i * 4), SeekOrigin.Begin);
                XSI_ReadMainStructure(Reader.ReadOffset(Reader.ReadInt32()));
            }

            int[] PointerCollection1 = new int[Unknown_0x08.Count]; //Think it just links to the parent
            for (int i = 0; i < Unknown_0x08.Count; i++)
            {
                Reader.BaseStream.Seek(Unknown_0x08.Offset + (i * 4) + 0x20, SeekOrigin.Begin);
                PointerCollection1[i] = Reader.ReadOffset(Reader.ReadInt32());
            }
        }

        private (List<string>, List<Vector4>) XSI_ReadFragmentsData(int count)
        {
            List<string> ChildProps = new List<string>();
            List<Vector4> ChildPosition = new List<Vector4>();

            for (int i = 0; i < count; i++) //Size : 0x30
            {
                int ChildPropPointer = Reader.ReadOffset(Reader.ReadInt32());
                ChildProps.Add(Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, ChildPropPointer, true));
                uint Pad = Reader.ReadUInt32();
                float Unk1 = Reader.ReadFloat();
                float Unk2 = Reader.ReadFloat();
                ChildPosition.Add(Reader.ReadVector4());
                uint Pad2 = Reader.ReadUInt32();
                uint Flags = Reader.ReadUInt32();
                uint Pad3 = Reader.ReadUInt32();
                uint Pad4 = Reader.ReadUInt32();
            }
            return (ChildProps, ChildPosition);
        }

        private void XSI_ReadUnknownStructure(int offset, int count)
        {
            int[] PointerCollection = new int[count];
            for (int i = 0; i < count; i++)
            {
                Reader.BaseStream.Seek(offset + (i * 4), SeekOrigin.Begin);
                PointerCollection[i] = Reader.ReadOffset(Reader.ReadInt32());
                if (PointerCollection[i] != 0)
                    XSI_ReadMainStructure(PointerCollection[i]);
            }
        }

        private byte[] GetBytes(uint value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            Array.Reverse(temp);
            return temp;
        }

        private byte[] GetBytes(float value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            Array.Reverse(temp);
            return temp;
        }

        private byte[] GetBytes(Vector3 value)
        {
            byte[] buffer = new byte[12];
            Buffer.BlockCopy(GetBytes(value.X), 0, buffer, 0, 4);
            Buffer.BlockCopy(GetBytes(value.Y), 0, buffer, 4, 4);
            Buffer.BlockCopy(GetBytes(value.Z), 0, buffer, 8, 4);
            return buffer;
        }

        private byte[] GetBytes(Vector4 value)
        {
            byte[] buffer = new byte[16];
            Buffer.BlockCopy(GetBytes(value.X), 0, buffer, 0, 4);
            Buffer.BlockCopy(GetBytes(value.Y), 0, buffer, 4, 4);
            Buffer.BlockCopy(GetBytes(value.Z), 0, buffer, 8, 4);
            Buffer.BlockCopy(GetBytes(value.W), 0, buffer, 12, 4);
            return buffer;
        }
    }

    public class SectorInfoData
    {
        public string SectorName { get; set; }
        public Vector4 BoundsMin { get; set; }
        public Vector4 BoundsMax { get; set; }
        public List<string> ChildProps { get; set; }
        public List<Vector4> ChildPosition { get; set; }

        public SectorInfoData(string sectorName, Vector4 boundsMin, Vector4 boundsMax, List<string> childProps, List<Vector4> childPosition)
        {
            SectorName = sectorName;
            BoundsMin = boundsMin;
            BoundsMax = boundsMax;
            ChildProps = childProps;
            ChildPosition = childPosition;
        }
    }
}
