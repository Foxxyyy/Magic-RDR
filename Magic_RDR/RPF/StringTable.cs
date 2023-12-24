using FastColoredTextBoxNS;
using Pik.IO;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text;

namespace Magic_RDR.RPF
{
    public class StringTable
    {
        private uint _VMT;
        public uint VMT { get { return _VMT; } }
        public uint TableModValue { get; set; }
        public uint AllCount { get { return (uint)Entries.Count; } }
        public uint SegmentCount { get; set; }
        public uint[] SegmentOffset { get; set; }
        public Segment[] Segments { get; set; }
        public uint StringCount { get; set; }
        public byte[] StringTableBlock { get; set; }

        public List<StringTableEntry> Entries { get; set; }

        public StringTable()
        {
            this.Entries = new List<StringTableEntry>();
        }

        public StringTable(Stream xIn, bool resource)
        {
            this.Entries = new List<StringTableEntry>();
            if (resource)
                this.Read(new MemoryStream(ResourceUtils.ResourceInfo.GetDataFromStream(xIn)));
            else
                this.ReadN(xIn);
        }

        public static bool IsValidSTResource(Stream xIn)
        {
            (bool, string, int, int, int) resourceInformation = ResourceUtils.ResourceInfo.GetResourceInformation(xIn);
            return resourceInformation.Item1 && resourceInformation.Item3 == 1;
        }

        private void ReadN(Stream xIn)
        {
            PikIO reader = new PikIO(xIn, PikIO.Endianess.Little);
            this.SegmentCount = reader.ReadUInt32();
            this.SegmentOffset = new uint[this.SegmentCount];
            this.Segments = new Segment[this.SegmentCount];

            for (int i = 0; i < this.SegmentCount; i++)
            {
                this.SegmentOffset[i] = reader.ReadUInt32(); //89 201 313 429 531 641 697 749 807 881 997 1109 1205
            }

            int len = (int)(this.SegmentOffset[0] - reader.Position);
            StringTableBlock = reader.ReadBytes(len); //String Names IDs

            for (int i = 0; i < this.SegmentOffset.Length; i++)
            {
                xIn.Position = this.SegmentOffset[i];
                this.Segments[i] = new Segment()
                {
                    StringCount = reader.ReadUInt32(),
                    UnkValue = reader.ReadUInt32(),
                    Name = this.GetLanguageFromIndex(i)
                };

                xIn.Position += 0x6;
                for (int index = 0; index < this.Segments[i].StringCount; index++)
                {
                    uint nextStringLength = reader.ReadUInt32();
                    string str = ReadString(reader, i + 1);

                    this.Segments[i].SegmentEntryBlock.Add(reader.ReadBytes(0x14));
                    this.Segments[i].Strings.Add(str);
                    this.StringCount++;
                }
            }
        }

        private void Read(Stream xIn)
        {
            PikIO reader = new PikIO(xIn, (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? PikIO.Endianess.Little : PikIO.Endianess.Big);
            this._VMT = reader.ReadUInt32();
            reader.Seek(4L, SeekOrigin.Current);

            uint num1 = reader.ReadUInt32() & 268435455U;
            reader.Position = num1;
            uint num2 = reader.ReadUInt32();
            uint num3 = reader.ReadUInt32() & 268435455U;

            reader.Seek(4L, SeekOrigin.Current);
            reader.ReadUInt32();
            reader.Position = num3;
            TableModValue = num2;

            for (int index = 0; index < num2; ++index)
            {
                uint num4 = reader.ReadUInt32() & 268435455U;
                long position = reader.Position;

                if (num4 > 0U)
                {
                    reader.Position = num4;
                    reader.ReadUInt32();
                    uint num5 = reader.ReadUInt32() & 268435455U; //23232
                    uint num6 = reader.ReadUInt32() & 268435455U; //20144
                    int num7 = 0;

                    while (num5 > 0U)
                    {
                        reader.Position = num5;
                        uint num8 = reader.ReadUInt32(); //3505858470
                        uint num9 = reader.ReadUInt32() & 268435455U;
                        reader.Position = num9;

                        List<byte> byteList = new List<byte>();
                        while (true)
                        {
                            byte num10 = reader.ReadByte();
                            if (num10 > 0)
                                byteList.Add(num10);
                            else
                                break;
                        }

                        Entries.Add(new StringTableEntry()
                        {
                            Key = num8,
                            Level = num7,
                            StrValue = Encoding.UTF8.GetString(byteList.ToArray())
                        });

                        if (num6 != 0U && num8 > 0U)
                        {
                            reader.Position = num6;
                            reader.ReadUInt32();
                            num5 = reader.ReadUInt32() & 268435455U;
                            num6 = reader.ReadUInt32() & 268435455U;
                        }
                        else
                            num5 = 0U;
                        ++num7;
                    }
                }
                reader.Position = position;
            }
        }

        public void WriteTable(Stream xOut, bool resource)
        {
            if (resource)
                this.Write(xOut);
            else
                this.WriteN(xOut);
        }

        public void WriteN(Stream xOut)
        {
            xOut.Position = 0x0;
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);

            writer.Write(this.SegmentCount);
            for (int i = 0; i < this.SegmentCount; i++)
            {
                writer.Write((uint)0); //Skipping segments offsets for now
            }
            writer.Write(this.StringTableBlock);

            for (int i = 0; i < this.Segments.Length; i++)
            {
                var seg = this.Segments[i];
                this.SegmentOffset[i] = (uint)writer.BaseStream.Position;

                writer.Write(seg.StringCount);
                writer.Write(seg.UnkValue);
                writer.Write(1U);
                writer.Write((ushort)0);

                var encoding = GetEncodingForLanguage(i + 1);
                for (int s = 0; s < seg.StringCount; s++)
                {
                    writer.Write(seg.Strings[s].Length + 1);
                    this.WriteString(writer, seg.Strings[s], i + 1);

                    if (s < seg.StringCount - 1)
                    {
                        writer.Write(seg.SegmentEntryBlock[s]);
                    }
                }
                writer.Write(1.0f);
                writer.Write(1.0f);
                writer.Write((ushort)0);
            }

            writer.BaseStream.Position = 0x4;
            for (int i = 0; i < this.SegmentOffset.Length; i++)
            {
                writer.Write(this.SegmentOffset[i]);
            }

            writer.BaseStream.Position = 0x0;
            byte[] buffer = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(buffer, 0, buffer.Length);
            xOut.Write(buffer, 0, buffer.Length);
        }

        public void Write(Stream xOut)
        {
            Dictionary<string, long> dictionary1 = new Dictionary<string, long>();
            Dictionary<uint, long> dictionary2 = new Dictionary<uint, long>();
            Dictionary<uint, long> dictionary3 = new Dictionary<uint, long>();

            bool[] flagArray = new bool[(int)TableModValue];
            dictionary1.Add("PositionOf_OffsetToStringTableOffset", 0L);
            dictionary1.Add("TempOffset", 0L);

            MemoryStream memoryStream = new MemoryStream();
            PikIO reader = new PikIO(memoryStream, (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? PikIO.Endianess.Little : PikIO.Endianess.Big);
            reader.Write(_VMT);
            reader.Write(1342177312);
            reader.Write(1342177840);
            reader.Write(0);
            reader.Write(0);

            for (int index = 0; index < 3; ++index)
                reader.Write(3452816845U);

            reader.Write(0);
            for (int index = 0; index < 131; ++index)
                reader.Write(3452816845U);

            reader.Write(TableModValue);
            dictionary1["PositionOf_OffsetToStringTableOffset"] = reader.Position;
            reader.Write(0);
            reader.Write((ushort)TableModValue);
            reader.Write((ushort)TableModValue);
            reader.Write(AllCount);
            Func<int, uint, int> func1 = (index, modVal) => Entries.Count(x => (x.Key % modVal) == index);

            StringTableEntry[] func2(int index, uint modVal)
            {
                List<StringTableEntry> stEntryList = new List<StringTableEntry>();
                stEntryList.AddRange(Entries.Where(x => (x.Key % modVal) == index).ToList().OrderBy(x => x.Key % modVal));
                return stEntryList.ToArray();
            }

            dictionary1["TempOffset"] = reader.Position;
            reader.Position = dictionary1["PositionOf_OffsetToStringTableOffset"];
            reader.Write((uint)(1342177280 | (int)(uint)dictionary1["TempOffset"] & 268435455));
            reader.Position = dictionary1["TempOffset"];
            reader.WriteBytes(new byte[4 * (int)TableModValue]);

            for (int index = 0; index < TableModValue; ++index)
            {
                int num1 = func1(index, TableModValue);

                if (!flagArray[index])
                {
                    dictionary1["TempOffset"] = reader.Position;
                    reader.Position = dictionary1["PositionOf_OffsetToStringTableOffset"] + 12L + (index * 4);

                    if (num1 == 0)
                        reader.Write(0);
                    else
                        reader.Write((uint)(1342177280 | (int)(uint)dictionary1["TempOffset"] & 268435455));

                    reader.Position = dictionary1["TempOffset"];
                    flagArray[index] = true;
                }

                int num2 = 0;
                StringTableEntry[] stEntryArray = func2(index, TableModValue);

                foreach (StringTableEntry stEntry in stEntryArray)
                {
                    reader.Write(stEntry.Key);
                    dictionary2.Add(stEntry.Key, reader.Position);
                    reader.Write(0);

                    if (stEntryArray.Length > num2 + 1)
                        reader.Write(1342177280U | (uint)((ulong)(reader.Position + 4L) & 268435455UL));
                    else
                        reader.Write(0);
                    ++num2;
                }
            }

            for (int index = 0; index < TableModValue; ++index)
            {
                foreach (StringTableEntry stEntry in func2(index, TableModValue))
                {
                    dictionary1["TempOffset"] = reader.Position;
                    reader.Position = dictionary2[stEntry.Key];
                    reader.Write(1342177280U | (uint)((ulong)dictionary1["TempOffset"] & 268435455UL));
                    reader.Position = dictionary1["TempOffset"];
                    reader.Write(stEntry.Key);
                    dictionary3.Add(stEntry.Key, reader.Position);
                    reader.Write(0);
                }
            }

            foreach (StringTableEntry entry in Entries)
            {
                dictionary1["TempOffset"] = reader.Position;
                reader.Position = dictionary3[entry.Key];
                reader.Write(1342177280U | (uint)((ulong)dictionary1["TempOffset"] & 268435455UL));
                reader.Position = dictionary1["TempOffset"];
                reader.WriteBytes(Encoding.ASCII.GetBytes(entry.StrValue + "\0"));
            }

            byte[] b = new byte[DataUtils.NumLeftTill((int)reader.Length, (int)AppGlobals.Platform)];

            for (int index = 0; index < b.Length; ++index)
                b[index] = 205;

            reader.WriteBytes(b);
            byte[] buffer = ResourceUtils.FlagInfo.RSC05_PackResource(memoryStream.ToArray(), (int)memoryStream.Length, 0, 1, AppGlobals.Platform, true);
            xOut.Write(buffer, 0, buffer.Length);
        }

        public void SetVMT(uint vmt_val)
        {
            _VMT = vmt_val;
        }

        public void AddEntry(uint key, string val)
        {
            Entries.Add(new StringTableEntry()
            {
                Key = key,
                StrValue = val
            });
        }

        public void AddEntry(string key, string val)
        {
            AddEntry(DataUtils.GetHash(key), val);
        }

        public void RemoveEntry(uint key)
        {
            for (int index = 0; index < Entries.Count; ++index)
            {
                if ((int)Entries[index].Key == (int)key)
                {
                    Entries.RemoveAt(index);
                    break;
                }
            }
        }

        public void RemoveEntry(string key)
        {
            RemoveEntry(DataUtils.GetHash(key));
        }

        public string ReadString(PikIO reader, int language)
        {
            Encoding encoding = GetEncodingForLanguage(language);
            List<byte> byteList = new List<byte>();

            byte test;
            bool correct = false;
            int temp = (int)reader.Position;

            while (!correct)
            {
                test = reader.ReadByte();
                if (test == 0x00)
                {
                    reader.Position -= 0x1;
                    if (reader.ReadUInt32() == 1065353216)
                    {
                        reader.Position -= 0x4;
                        correct = true;
                    }
                    else reader.Position -= 0x3;
                }
            }

            int endString = (int)reader.Position;
            reader.Position = temp;
            byteList.AddRange(reader.ReadBytes(endString - temp).ToArray());

            if (encoding != Encoding.Unicode)
            {
                int i = 0;
                while (i < byteList.Count - 1)
                {
                    if (byteList[i] != 0 && byteList[i + 1] != 0)
                    {
                        i++;
                    }
                    else
                    {
                        byteList.RemoveAt(i + 1);
                    }
                }
            }
            string str = encoding.GetString(byteList.ToArray());
            return str;
        }

        public void WriteString(BinaryWriter writer, string value, int language)
        {
            Encoding encoding = GetEncodingForLanguage(language);
            List<byte> byteList = new List<byte>();

            if (encoding != Encoding.Unicode)
            {
                // Remove padding zeroes if the encoding is not Unicode
                for (int i = 0; i < value.Length; i++)
                {
                    byteList.Add((byte)value[i]);
                    byteList.Add(0); // Add a padding zero
                }
                byteList.Add(0);
                byteList.Add(0);
            }
            else
            {
                // If the encoding is Unicode, write each character as a ushort (2 bytes)
                foreach (char c in value)
                {
                    byteList.AddRange(BitConverter.GetBytes(c));
                }
            }

            // Write the bytes to the binary writer
            writer.Write(byteList.ToArray());
        }

        public string GetLanguageFromIndex(int index)
        {
            string name = string.Empty;
            switch (index)
            {
                case 0:
                    name = "English";
                    break;
                case 1:
                    name = "Spanish";
                    break;
                case 2:
                    name = "French";
                    break;
                case 9:
                    name = "Spanish (Spain)";
                    break;
                case 10:
                    name = "Spanish (Mexico)";
                    break;
                case 11:
                    name = "Portuguese";
                    break;
                case 3:
                    name = "German";
                    break;
                case 4:
                    name = "Italian";
                    break;
                case 5:
                    name = "Japanese";
                    break;
                case 6:
                    name = "Chinese (Traditional)";
                    break;
                case 7:
                    name = "Chinese (Simplified)";
                    break;
                case 8:
                    name = "Korean";
                    break;
                case 12:
                    name = "Polish";
                    break;
                case 13:
                    name = "Russian";
                    break;
            }
            return name;
        }

        private Encoding GetEncodingForLanguage(int language)
        {
            switch (language)
            {
                case 1: // English
                    return Encoding.UTF8;
                case 2: // Spanish
                case 3: // French
                case 10: // Spanish (Spain)
                case 11: // Spanish (Mexico)
                    return Encoding.GetEncoding("ISO-8859-1");
                case 4: // German
                case 5: // Italian
                case 6: // Japanese
                case 7: // Chinese (Traditional)
                case 8: // Chinese (Simplified)
                case 9: // Korean
                case 12: // Portuguese
                case 13: // Polish
                case 14: // Russian
                    return Encoding.Unicode;
                default:
                    throw new NotSupportedException($"Language {language} is not supported.");
            }
        }

        public class StringTableEntry
        {
            public string StrValue { get; set; }
            public uint Key { get; set; }
            public int Level { get; set; }
            public string KeyString
            {
                set
                {
                    Key = DataUtils.GetHash(value);
                }
            }
        }

        public class Segment
        {
            public string Name { get; set; }
            public uint StringCount { get; set; }
            public uint UnkValue { get; set; }

            public List<string> Strings = new List<string>();
            public List<byte[]> SegmentEntryBlock = new List<byte[]>();
        }
    }
}
