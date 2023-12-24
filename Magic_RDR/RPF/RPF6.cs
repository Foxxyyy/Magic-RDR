using Magic_RDR.RPF;
using Pik.IO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;

namespace Magic_RDR
{
    public class RPF6
    {
        public static EventHandler<RPF6Report> OnOperation;

        public static string Gen_PossibleSectorCalc(long size, long pos)
        {
            long num1 = pos >> 23;
            long num2 = size + pos;
            long num3 = num1;
            long num4 = num2 + -1L >> 23;
            return num3 <= num4 ? string.Format("{0}|{1}", (object)num4, (object)num3) : throw new Exception("wat");
        }

        public static byte[] CurrentEncryptionKey = AppGlobals.EncryptionKey;

        public RPF6Header Header { get; set; }

        public RPF6TOC TOC { get; set; }

        public PikIO RPFIO { get; set; }

        public RPF6(Stream xIn)
        {
            PikIO io = new PikIO(xIn, PikIO.Endianess.Big);
            RPFIO = io;

            OnOperation?.Invoke(this, new RPF6Report()
            {
                StatusOperationText = "Reading",
                StatusText = nameof(Header),
                TitleText = "Reading..."
            });
            Header = RPF6Header.ReadHeader(io);

            OnOperation?.Invoke(this, new RPF6Report()
            {
                StatusOperationText = "Reading",
                StatusText = nameof(TOC),
                TitleText = "Reading..."
            });
            TOC = RPF6TOC.ReadTOC(io, this);

            if (OnOperation == null)
                return;

            //RPF6TOC.ReadStringTable(io, Header, TOC);

            OnOperation(this, new RPF6Report()
            {
                StatusOperationText = "Finished",
                StatusText = nameof(TOC),
                TitleText = "Done Reading"
            });
        }

        public RPF6()
        {
            Header = new RPF6Header();
            TOC = new RPF6TOC(this);
        }

        public void CloseAllStreams()
        {
            foreach (RPF6TOC.TOCSuperEntry superEntry in this.TOC.SuperEntries)
            {
                if (superEntry.CustomDataStream != null)
                {
                    superEntry.CustomDataStream.Close();
                    superEntry.CustomDataStream = (Stream)null;
                }
            }
        }

        public void Write(Stream xOut)
        {
            PikIO io = new PikIO(xOut, PikIO.Endianess.Big);

            OnOperation?.Invoke(this, new RPF6Report()
            {
                StatusOperationText = "Rebuilding",
                StatusText = "TOC",
                TitleText = "Rebuilding.."
            });
            TOC.Rebuild();

            OnOperation?.Invoke(this, new RPF6Report()
            {
                StatusOperationText = "Writing",
                StatusText = "Header",
                TitleText = "Writing.."
            });

            RPF6Header.WriteHeader(io, this.Header);
            long position = io.Position;
            io.WriteBytes(new byte[Header.TOCSize]);
            io.WriteBytes(new byte[DataUtils.NumLeftTill((int)io.Length, 2048) == 2048 ? 0 : DataUtils.NumLeftTill((int)io.Length, 2048)]);

            while (io.Position < 671743L)
            {
                io.WriteByte(0);
            }
            List<RPF6TOC.TOCSuperEntry> tocSuperEntryList1 = new List<RPF6TOC.TOCSuperEntry>();
            List<RPF6TOC.TOCSuperEntry> tocSuperEntryList2 = new List<RPF6TOC.TOCSuperEntry>();

            foreach (RPF6TOC.TOCSuperEntry superEntry in TOC.SuperEntries)
            {
                if (!superEntry.IsDir)
                {
                    if (superEntry.Entry.AsFile.FlagInfo.IsResource)
                        tocSuperEntryList2.Add(superEntry);
                    else
                        tocSuperEntryList1.Add(superEntry);
                }
            }
            List<RPF6TOC.TOCSuperEntry> tocSuperEntryList3 = new List<RPF6TOC.TOCSuperEntry>();
            tocSuperEntryList3.AddRange(tocSuperEntryList2);
            tocSuperEntryList3.AddRange(tocSuperEntryList1);

            List<RPF6TOC.TOCSuperEntry> source = new List<RPF6TOC.TOCSuperEntry>();
            List<RPF6TOC.TOCSuperEntry> tocSuperEntryList4 = new List<RPF6TOC.TOCSuperEntry>();

            foreach (RPF6TOC.TOCSuperEntry tocSuperEntry in tocSuperEntryList3)
            {
                if (!tocSuperEntry.IsDir)
                {
                    if (tocSuperEntry.Entry.AsFile.KeepOffset.HasValue || tocSuperEntry.ReadBackFromRPF)
                        source.Add(tocSuperEntry);
                    else
                        tocSuperEntryList4.Add(tocSuperEntry);
                }
            }

            List<RPF6TOC.TOCSuperEntry> list = source.OrderBy(x => x.Entry.AsFile.GetOffset(true)).ToList();
            list.AddRange(tocSuperEntryList4);

            Action<RPF6TOC.TOCSuperEntry> WriteEntry = null;
            WriteEntry = (super =>
            {
                if (super.IsDir || !super.Write || super.DoesHaveParentMarkedNotToBeWritten || super.Written)
                    return;

                foreach (RPF6TOC.TOCSuperEntry writeBeforeChild in super.WriteBefore_Children)
                {
                    WriteEntry(writeBeforeChild);
                }

                RPF6TOC.FileEntry asFile = super.Entry.AsFile;
                byte[] b = null;

                if (asFile.FlagInfo.IsCompressed && !asFile.FlagInfo.IsResource && super.CustomDataStream != null)
                {
                    byte[] numArray = new byte[super.CustomDataStream.Length];
                    super.CustomDataStream.Read(numArray, 0, numArray.Length);
                    if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                        b = DataUtils.CompressZStandard(numArray);
                    else
                        b = DataUtils.Compress(numArray, 9);
                }
                int num = super.ReadBackFromRPF ? asFile.SizeInArchive : (b != null ? b.Length : (int)super.CustomDataStream.Length);
                asFile.SizeInArchive = num;

                if (!asFile.FlagInfo.IsResource)
                {
                    long length;
                    if (num >= 131072)
                    {
                        length = PikIO.Utils.RoundUp(io.Position, 2048L) - io.Position;
                        if (length == 0L)
                            length = 2048L;
                    }
                    else
                    {
                        length = PikIO.Utils.RoundUp(io.Position, 8L) - io.Position;
                        if (length == 0L)
                            length = 8L;
                    }
                    io.WriteBytes(new byte[length]);
                }
                else
                {
                    long length = PikIO.Utils.RoundUp(io.Position, 2048L) - io.Position;
                    if (length == 0L)
                        length = 2048L;
                    io.WriteBytes(new byte[length]);
                }

                if (super.CustomDataStream != null && !super.ReadBackFromRPF)
                {
                    asFile.SetOffset(io.Position);
                    if (b == null)
                        super.CustomDataStream.CopyTo(io.BaseStream);
                    else
                        io.WriteBytes(b);
                    super.CustomDataStream.Position = 0L;
                }
                else if (super.ReadBackFromRPF)
                {
                    long r = asFile.GetOffset();
                    RPFIO.Position = r;
                    asFile.SetOffset(io.Position);
                    RPFIO.BufferCopy(io.BaseStream, (uint)asFile.SizeInArchive, 262144U);
                }

                foreach (RPF6TOC.TOCSuperEntry writeAfterChild in super.WriteAfterChildren)
                {
                    WriteEntry(writeAfterChild);
                }
                super.Written = true;
            });

            int num1 = 0;
            foreach (RPF6TOC.TOCSuperEntry tocSuperEntry in list)
            {
                if (OnOperation != null)
                {
                    OnOperation(this, new RPF6Report()
                    {
                        StatusOperationText = "Writing",
                        StatusText = string.Format("{0}", tocSuperEntry.Entry.Name),
                        TitleText = string.Format("Writing Files ({0}/{1})", num1 + 1, list.Count),
                        CurrentOperation = num1,
                        TotalOperations = list.Count
                    });
                }
                WriteEntry(tocSuperEntry);
                ++num1;
            }
            for (int index = 0; index < tocSuperEntryList3.Count; ++index)
            {
                tocSuperEntryList3[index].Written = false;
            }
            
            var t = PikIO.Utils.RoundUp(io.Position, 2048L) - io.Position;
            io.WriteBytes(new byte[(int)(t)]);        
            io.Position = position;
            RPF6TOC.WriteTOC(io, TOC);
            io.Flush();
        }

        public class RPF6Header
        {
            public const int Ident = 1380992566;

            public int EntryCount { get; set; }

            public int TOCSize { get; set; }

            public int DebugDataOffset { get; set; }

            public int EncFlag { get; set; }

            public bool Encrypted
            {
                get => (uint)EncFlag > 0U;
                set
                {
                    if (value)
                        EncFlag = -3;
                    else
                        EncFlag = 0;
                }
            }

            public int FileCount { get; set; }

            public int DirectoryCount { get; set; }

            public static bool HasIdentifier(PikIO io)
            {
                long position = io.Position;
                int num = io.ReadInt32();
                io.Position = position;
                return num == 1380992566;
            }

            public static RPF6Header ReadHeader(PikIO io)
            {
                RPF6Header rpF6Header = new RPF6Header();
                io.ReadInt32();
                rpF6Header.EntryCount = io.ReadInt32();
                rpF6Header.TOCSize = (int)((((rpF6Header.EntryCount << 2) + rpF6Header.EntryCount << 2) + 15) & 4294967280L);
                rpF6Header.DebugDataOffset = io.ReadInt32();
                rpF6Header.EncFlag = io.ReadInt32();
                return rpF6Header;
            }

            public static void WriteHeader(PikIO io, RPF6Header header)
            {
                io.Write(1380992566);
                io.Write(header.EntryCount);
                io.Write(header.DebugDataOffset);
                io.Write(header.EncFlag);
            }
        }

        public class RPF6TOC
        {
            public List<TOCEntry> Entries { get; set; }

            public List<TOCSuperEntry> SuperEntries { get; set; }

            public static RPF6 RPFFile { get; set; }

            private static int GetOffset(int entryCount, int startIndex)
            {
                int num1 = entryCount + startIndex >> 1;
                int num2 = num1 << 2;
                return (num1 + num2) * 4;
            }

            public RPF6TOC(RPF6 rpfFile)
            {
                RPFFile = rpfFile;
                Entries = new List<TOCEntry>();
                SuperEntries = new List<TOCSuperEntry>();
            }

            public class RPF6EntryDebug
            {
                public uint NameOffset { get; set; }
                public uint LastModified { get; set; }

                public ulong GetLastModified()
                {
                    if (LastModified == 0)
                        return 0;

                    return (ulong)LastModified + 12591158400UL - 11644473600UL;
                }
            }

            public static RPF6TOC ReadTOC(PikIO io, RPF6 rpfFile)
            {
                RPF6TOC rpF6Toc = new RPF6TOC(rpfFile);
                MemoryStream memoryStream = new MemoryStream();
                byte[] numArray;

                if (rpfFile.Header.Encrypted)
                {
                    long tocSize = rpfFile.Header.TOCSize;
                    numArray = DataUtils.Decrypt(io.ReadBytes((int)tocSize), CurrentEncryptionKey);
                }
                else
                {
                    numArray = io.ReadBytes(rpfFile.Header.TOCSize);
                    if (AppGlobals.Settings.WriteDecryptedTOC)
                        File.WriteAllBytes("toc_decrypted", numArray);
                }

                PikIO io1 = new PikIO(new MemoryStream(numArray), PikIO.Endianess.Big);
                int num1 = 0;
                int num2 = 0;
                long num3 = 0;
                SortedList<long, TOCSuperEntry> source1 = new SortedList<long, TOCSuperEntry>();
                StreamWriter streamWriter1 = null;
                StreamWriter streamWriter2 = null;

                if (AppGlobals.Settings.WriteTOCOrder)
                    streamWriter1 = new StreamWriter(AppUtils.CreateFile("toc_order.txt"));
                if (AppGlobals.Settings.WriteRSCInfo)
                    streamWriter2 = new StreamWriter(AppUtils.CreateFile("rsc_info.txt"));

                TreeView tree = new TreeView();
                for (int index = 0; index < rpfFile.Header.EntryCount; ++index)
                {
                    TOCEntry tocEntry = TOCEntry.ReadEntry(io1);
                    if (AppGlobals.Settings.WriteTOCOrder)
                    {
                        int num4 = 0;
                        if (tocEntry.IsFile && tocEntry.AsFile.FlagInfo.IsResource)
                            num4 = tocEntry.AsFile.ResourceType;
                        streamWriter1.WriteLine(string.Format("[{0}]\t[{1}] {2} {3} [{4}]", index, tocEntry.IsFile ? "F" : "D", tocEntry.Name, tocEntry.IsDirectory ? string.Format("[-> {0}, {1}]", tocEntry.AsDirectory.ContentEntryIndex, (GetOffset(tocEntry.AsDirectory.ContentEntryCount, tocEntry.AsDirectory.ContentEntryIndex) / 20)) : "", num4));
                    }

                    if (AppGlobals.Settings.WriteRSCInfo && tocEntry.IsFile && tocEntry.AsFile.FlagInfo.IsResource)
                    {
                        ResourceUtils.FlagInfo flagInfo = new ResourceUtils.FlagInfo(tocEntry.AsFile.FlagInfo.Flag1, tocEntry.AsFile.FlagInfo.Flag2);
                        streamWriter2.WriteLine(string.Format("---- {0} [RSC {1}]", tocEntry.Name, tocEntry.AsFile.ResourceType));
                        streamWriter2.WriteLine(flagInfo.ToString());
                        streamWriter2.WriteLine("##################################################################");
                    }

                    TOCSuperEntry tocSuperEntry = new TOCSuperEntry();
                    tocEntry.SuperOwner = tocSuperEntry;
                    tocSuperEntry.ReadBackFromRPF = true;
                    tocSuperEntry.IsFileFromRPF = true;
                    tocSuperEntry.Entry = tocEntry;
                    rpF6Toc.Entries.Add(tocEntry);
                    rpF6Toc.SuperEntries.Add(tocSuperEntry);

                    if (tocEntry.IsFile && AppGlobals.Settings.MakeFileInfoData)
                    {
                        source1.Add(tocEntry.AsFile.GetOffset(), tocSuperEntry);
                    }

                    if (tocEntry.IsFile)
                    {
                        ++num1;
                        num3 += tocEntry.AsFile.SizeInArchive;
                    }
                    else
                        ++num2;
                }
                if (AppGlobals.Settings.WriteRSCInfo)
                {
                    streamWriter2.Flush();
                    streamWriter2.Close();
                }
                if (AppGlobals.Settings.WriteTOCOrder)
                {
                    streamWriter1.Flush();
                    streamWriter1.Close();
                }

                KeyValuePair<long, TOCSuperEntry> keyValuePair1;
                if (AppGlobals.Settings.MakeFileInfoData)
                {
                    StreamWriter streamWriter3 = new StreamWriter(AppUtils.CreateFile("file_data.txt"));
                    for (int index = 0; index < source1.Count; ++index)
                    {
                        keyValuePair1 = source1.ElementAt(index);
                        long key = keyValuePair1.Key;
                        keyValuePair1 = source1.ElementAt(index);
                        FileEntry asFile = keyValuePair1.Value.Entry.AsFile;
                        string str1 = asFile.FlagInfo.IsResource ? "R" : "F";
                        string str2 = Gen_PossibleSectorCalc(asFile.SizeInArchive, asFile.Offset);
                        bool flag1 = asFile.GetOffset() % 8L == 0L;
                        bool flag2 = asFile.GetOffset() % 2048L == 0L;
                        streamWriter3.WriteLine(string.Format("{0}     {1}\t[{2}, {3}]\t[{4} | {5}] [SIZE: {6}] [C:{7}, {8}] [SEC: {9}]", str1, asFile.GetOffset(), flag1.ToString(), flag2.ToString(), asFile.Name, asFile.NameOffset.ToString("X8"), asFile.SizeInArchive, asFile.FlagInfo.IsCompressed ? "Y" : "N", asFile.FlagInfo.GetTotalSize(), str2));
                    }
                    streamWriter3.Flush();
                    streamWriter3.Close();
                }

                if (AppGlobals.Settings.MakeDiffDataFile)
                {
                    StreamWriter streamWriter3 = new StreamWriter(AppUtils.CreateFile("diff_data.txt"));
                    SortedList<long, FileEntry> source2 = new SortedList<long, FileEntry>();

                    for (int index = 0; index < rpF6Toc.Entries.Count; ++index)
                    {
                        if (rpF6Toc.Entries[index].IsFile)
                        {
                            FileEntry asFile = rpF6Toc.Entries[index].AsFile;
                            source2.Add(asFile.GetOffset(), asFile);
                        }
                    }

                    long num4 = 671744;
                    FileEntry fileEntry = null;

                    for (int index = 0; index < source2.Count; ++index)
                    {
                        KeyValuePair<long, FileEntry> keyValuePair2 = source2.ElementAt(index);
                        long num5 = keyValuePair2.Key - num4;
                        StreamWriter streamWriter4 = streamWriter3;
                        keyValuePair2 = source2.ElementAt(index);
                        string str = string.Format("{0} was {1} bytes away from {2}", keyValuePair2.Value.Name, num5, (fileEntry == null ? "FILESTART" : fileEntry.Name));
                        streamWriter4.WriteLine(str);
                        keyValuePair2 = source2.ElementAt(index);
                        long offset = keyValuePair2.Value.GetOffset();
                        keyValuePair2 = source2.ElementAt(index);
                        long sizeInArchive = keyValuePair2.Value.SizeInArchive;
                        num4 = offset + sizeInArchive;
                        keyValuePair2 = source2.ElementAt(index);
                        fileEntry = keyValuePair2.Value;
                    }
                    streamWriter3.Flush();
                    streamWriter3.Close();
                }

                rpfFile.Header.FileCount = num1;
                rpfFile.Header.DirectoryCount = num2;

                StringBuilder sb = new StringBuilder();
                for (int index = 0; index < rpF6Toc.Entries.Count; ++index)
                {
                    TOCEntry entry = rpF6Toc.Entries[index];
                    if (entry.Name.StartsWith("0x"))
                        sb.AppendLine(entry.Name);

                    if (entry.IsDirectory)
                    {
                        DirectoryEntry asDirectory = entry.AsDirectory;
                        for (int contentEntryIndex = asDirectory.ContentEntryIndex; contentEntryIndex < asDirectory.ContentEntryIndex + asDirectory.ContentEntryCount; ++contentEntryIndex)
                        {
                            rpF6Toc.Entries[contentEntryIndex].Parent = entry.AsDirectory;
                            rpF6Toc.SuperEntries[contentEntryIndex].SuperParent = rpF6Toc.SuperEntries[index];
                            rpF6Toc.SuperEntries[index].Children.Add(rpF6Toc.SuperEntries[contentEntryIndex]);
                        }
                    }
                }

                //File.WriteAllText(@"C:\Users\fumol\OneDrive\Bureau\sorted.txt", sb.ToString()); //Export remaining unknown hashs
                if (AppGlobals.Settings.MakeFileDirectoryListing)
                {
                    StreamWriter streamWriter3 = new StreamWriter(AppUtils.CreateFile("directory_listing.txt"));
                    for (int index = 0; index < source1.Count; ++index)
                    {
                        keyValuePair1 = source1.ElementAt(index);
                        long key = keyValuePair1.Key;
                        keyValuePair1 = source1.ElementAt(index);
                        TOCSuperEntry tocSuperEntry = keyValuePair1.Value;
                        streamWriter3.WriteLine(string.Format("{0}", tocSuperEntry.Entry.GetPath()));
                    }
                    streamWriter3.Flush();
                    streamWriter3.Close();
                }
                return rpF6Toc;
            }

            public static void WriteTOC(PikIO io, RPF6TOC toc)
            {
                MemoryStream memoryStream = new MemoryStream();
                PikIO io1 = new PikIO(memoryStream, PikIO.Endianess.Big);

                for (int index = 0; index < toc.Entries.Count; ++index)
                {
                    TOCEntry.WriteEntry(io1, toc.Entries[index]);
                }

                byte[] b = new byte[PikIO.Utils.RoundUp(memoryStream.Position, 16L) - memoryStream.Position];
                io1.WriteBytes(b);
                byte[] numArray = memoryStream.ToArray();

                if (RPFFile.Header.Encrypted)
                {
                    numArray = DataUtils.Encrypt(numArray, CurrentEncryptionKey);
                }
                io.WriteBytes(numArray);
            }

            public static void ReadStringTable(PikIO io, RPF6Header header, RPF6TOC toc)
            {
                if (header.DebugDataOffset <= 0) //Xbox 360 & PS3
                {
                    return;
                }

                io.BaseStream.Position = header.DebugDataOffset * 8;
                try
                {
                    byte[] buffer = io.ReadBytes((int)(io.BaseStream.Length - io.Position));
                    buffer = DataUtils.Decrypt(buffer, CurrentEncryptionKey);

                    List<RPF6EntryDebug> debugEntries = new List<RPF6EntryDebug>();
                    for (int i = 0; i < header.EntryCount; i++)
                    {
                        debugEntries.Add(new RPF6EntryDebug());
                    }

                    var br = new PikIO(new MemoryStream(buffer));
                    for (int i = 0; i < debugEntries.Count; i++)
                    {
                        debugEntries[i].NameOffset = DataUtils.SwapEndian(br.ReadUInt32());
                        debugEntries[i].LastModified = DataUtils.SwapEndian(br.ReadUInt32());
                    }

                    char[] debugNames = new char[buffer.Length - (debugEntries.Count * 8)];
                    Array.Copy(buffer, debugEntries.Count * 8, debugNames, 0, debugNames.Length);

                    string[] names = new string(debugNames).Split('\0');
                    for (int i = 0; i < names.Length; i++)
                    {
                        string hash = DataUtils.FormatHexHash(DataUtils.GetHash(names[i]));
                        var entry = toc.Entries.FirstOrDefault(e => e.Name == hash);

                        if (entry == null)
                            continue;
                        else
                        {
                            int index = toc.Entries.IndexOf(entry);
                            toc.Entries[index].DebugName = names[i];
                        }
                    }
                }
                catch
                {

                }
            }

            public void Rebuild()
            {
                List<TOCSuperEntry> newSupers = new List<TOCSuperEntry>();
                Action<TOCSuperEntry> sortSupers = null;

                sortSupers = (entry =>
                {
                    if (!entry.Write || entry.DoesHaveParentMarkedNotToBeWritten)
                        return;

                    if (!newSupers.Contains(entry))
                        newSupers.Add(entry);

                    if (entry.IsDir && entry.Children != null && entry.Children.Count > 0)
                    {
                        entry.StartIndex = newSupers.Count;
                        newSupers.AddRange(entry.Children.OrderBy(o => o.Entry.NameOffset));

                        foreach (TOCSuperEntry child in entry.Children)
                        {
                            sortSupers(child);
                        }
                    }
                });

                sortSupers(SuperEntries[0]);
                SuperEntries = newSupers;
                List<TOCEntry> tocEntryList = new List<TOCEntry>();

                foreach (TOCSuperEntry superEntry in SuperEntries)
                {
                    if (superEntry.Write && !superEntry.DoesHaveParentMarkedNotToBeWritten)
                    {
                        if (superEntry.Entry.IsDirectory)
                        {
                            DirectoryEntry asDirectory = superEntry.Entry.AsDirectory;
                            asDirectory.ContentEntryIndex = superEntry.StartIndex;
                            asDirectory.ContentEntryCount = superEntry.Children.Count;
                        }
                        tocEntryList.Add(superEntry.Entry);
                    }
                }
                Entries = tocEntryList;
                RPFFile.Header.FileCount = Entries.Count(x => x.IsFile);
                RPFFile.Header.DirectoryCount = Entries.Count(x => x.IsDirectory);
                RPFFile.Header.EntryCount = Entries.Count;
            }

            public void ExtractFile(TOCSuperEntry entry, Stream xOut)
            {
                long position = xOut.Position;
                if (entry.CustomDataStream != null && !entry.ReadBackFromRPF)
                {
                    entry.CustomDataStream.Position = 0L;
                    entry.CustomDataStream.CopyTo(xOut);
                }
                else
                {
                    FileEntry asFile = entry.Entry.AsFile;
                    if (asFile.FlagInfo.IsCompressed && !asFile.FlagInfo.IsResource)
                    {
                        RPFFile.RPFIO.Position = asFile.GetOffset();
                        byte[] buffer;
                        if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                            buffer = DataUtils.DecompressZStandard(RPFFile.RPFIO.ReadBytes(asFile.SizeInArchive));
                        else
                            buffer = DataUtils.DecompressDeflate(RPFFile.RPFIO.ReadBytes(asFile.SizeInArchive), asFile.FlagInfo.GetTotalSize());
                        xOut.Write(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        RPFFile.RPFIO.Position = asFile.GetOffset();
                        RPFFile.RPFIO.BufferCopy(xOut, (uint)entry.Entry.AsFile.SizeInArchive, 32768U);
                    }
                }
                xOut.Position = position;
            }

            public static void ReplaceEntry(TOCSuperEntry entryToReplace, TOCSuperEntry replaceWith)
            {
                TOCSuperEntry superParent = entryToReplace.SuperParent;
                int index = superParent.Children.IndexOf(entryToReplace);
                superParent.Children.Remove(entryToReplace);
                superParent.Children.Insert(index, replaceWith);
            }

            public static TOCSuperEntry GetEntryByPath(string fullPath, TOCSuperEntry superEntry, char seperator = '/')
            {
                if (fullPath.EndsWith(string.Format("{0}", seperator)))
                    fullPath = fullPath.Remove(fullPath.Length - 1, 1);

                string[] strArray = fullPath.Split(seperator);

                if (strArray[0] != "root")
                    return null;

                TOCSuperEntry tocSuperEntry = superEntry;
                for (int index = 1; index < strArray.Length; ++index)
                {
                    uint hash = !strArray[index].StartsWith("0x") ? DataUtils.GetHash(strArray[index]) : uint.Parse(strArray[index].Substring(2), NumberStyles.HexNumber);
                    TOCSuperEntry child = tocSuperEntry.GetChild(hash);

                    if (child == null)
                        return null;
                    tocSuperEntry = child;
                }
                return tocSuperEntry;
            }

            public class TOCSuperEntry
            {
                private bool _Write;

                public TOCSuperEntry()
                {
                    Children = new List<TOCSuperEntry>();
                    RemovedChildren = new List<TOCSuperEntry>();
                    WriteBefore_Children = new List<TOCSuperEntry>();
                    WriteAfterChildren = new List<TOCSuperEntry>();
                    _Write = true;
                }

                public bool ReadBackFromRPF { get; set; }

                public bool IsFileFromRPF { get; set; }

                public bool Written { get; set; }

                public bool Write => this._Write;

                public bool DoesHaveParentMarkedNotToBeWritten
                {
                    get
                    {
                        TOCSuperEntry superParent = SuperParent;

                        if (superParent == null)
                            return false;

                        for (; superParent != null; superParent = superParent.SuperParent)
                        {
                            if (!superParent.Write)
                                return true;
                        }
                        return false;
                    }
                }

                public void MarkAsNotToBeWritten(bool write, List<TOCSuperEntry> allSupers)
                {
                    if (!write & _Write)
                    {
                        _Write = write;

                        if (SuperParent == null)
                            return;

                        SuperParent.Children.Remove(this);
                        SuperParent.RemovedChildren.Add(this);
                        allSupers.Remove(this);
                    }
                    else
                    {
                        _Write = write;
                        if (SuperParent != null)
                        {
                            SuperParent.Children.Add(this);
                            SuperParent.RemovedChildren.Remove(this);
                            allSupers.Add(this);
                        }
                    }
                }

                public void AddChild(TOCSuperEntry child)
                {
                    child.SuperParent = this;
                    Children.Add(child);
                }

                public bool DoesHaveEntry(string name, bool checkALLCHILDREN = false) => DoesHaveEntry(DataUtils.GetHash(name), checkALLCHILDREN);

                public bool DoesHaveEntry(uint hash, bool checkALLCHILDREN = false)
                {
                    foreach (TOCSuperEntry tocSuperEntry in checkALLCHILDREN ? AllChildren : Children)
                    {
                        if (((int)tocSuperEntry.Entry.NameOffset == (int)hash))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                public TOCSuperEntry GetChild(string name) => GetChild(DataUtils.GetHash(name));

                public TOCSuperEntry GetChild(uint hash)
                {
                    foreach (TOCSuperEntry child in Children)
                    {
                        if ((int)child.Entry.NameOffset == (int)hash)
                        {
                            return child;
                        }
                    }
                    return null;
                }

                public TOCEntry Entry { get; set; }

                public TOCEntry OldEntry { get; set; }

                public Stream CustomDataStream { get; set; }

                public void SetCustomStream(Stream custom, int virt, int phys)
                {
                    CustomDataStream = custom;
                    Entry.AsFile.FlagInfo.SetTotalSize(virt, phys);
                }

                public bool IsDir => this.Entry.IsDirectory;

                public int StartIndex { get; set; }

                public TOCSuperEntry SuperParent { get; set; }

                public TOCSuperEntry WriteOrderParent { get; set; }

                public List<TOCSuperEntry> Children { get; set; }

                public List<TOCSuperEntry> RemovedChildren { get; set; }

                public List<TOCSuperEntry> WriteBefore_Children { get; set; }

                public List<TOCSuperEntry> WriteAfterChildren { get; set; }

                public List<TOCSuperEntry> AllChildren
                {
                    get
                    {
                        List<TOCSuperEntry> tocSuperEntryList = new List<TOCSuperEntry>();
                        tocSuperEntryList.AddRange(Children);
                        tocSuperEntryList.AddRange(RemovedChildren);
                        return tocSuperEntryList;
                    }
                }

                public TOCSuperEntry[] GetPathArray()
                {
                    List<TOCSuperEntry> source = new List<TOCSuperEntry>();
                    source.Add(this);

                    for (TOCSuperEntry superParent = SuperParent; superParent != null; superParent = superParent.SuperParent)
                    {
                        source.Add(superParent);
                    }
                    return source.Reverse<TOCSuperEntry>().ToArray();
                }

                public string GetPath(char seperator = '/')
                {
                    string str = string.Empty;
                    TOCSuperEntry[] pathArray = GetPathArray();

                    for (int index = 0; index < pathArray.Length; ++index)
                    {
                        str = index != pathArray.Length - 1 ? str + string.Format("{0}{1}", pathArray[index].Entry.Name, seperator) : str + string.Format("{0}", pathArray[index].Entry.Name);
                    }
                    return str;
                }
            }

            public abstract class TOCEntry
            {
                public DirectoryEntry Parent { get; set; }

                public TOCSuperEntry SuperOwner { get; set; }

                public DirectoryEntry AsDirectory => this as DirectoryEntry;

                public FileEntry AsFile => this as FileEntry;

                public bool IsFile => this is FileEntry;

                public bool IsDirectory => this is DirectoryEntry;

                public uint NameOffset { get; set; }

                public string DebugName { get; set; }

                public static bool ReadAsDir(PikIO io)
                {
                    io.Seek(8L, SeekOrigin.Current);
                    byte num = io.ReadByte();
                    io.Seek(-9L, SeekOrigin.Current);
                    return num == 128;
                }

                public static TOCEntry ReadEntry(PikIO io)
                {
                    if (ReadAsDir(io))
                    {
                        DirectoryEntry directoryEntry = new DirectoryEntry();
                        directoryEntry.NameOffset = io.ReadUInt32();
                        directoryEntry.Flags = io.ReadInt32();
                        directoryEntry.ContentEntryIndex = io.ReadInt32() & int.MaxValue;
                        directoryEntry.ContentEntryCount = io.ReadInt32() & 268435455;
                        directoryEntry.UNK = io.ReadInt32();

                        if ((uint)directoryEntry.UNK > 0U)
                            throw new Exception("What?");

                        return directoryEntry;
                    }

                    FileEntry fileEntryRDR = new FileEntry();
                    fileEntryRDR.NameOffset = io.ReadUInt32();
                    fileEntryRDR.SizeInArchive = io.ReadInt32() & 268435455;
                    fileEntryRDR.Offset = io.ReadInt32();
                    fileEntryRDR.FlagInfo = new ResourceUtils.FlagInfo()
                    {
                        Flag1 = io.ReadInt32(),
                        Flag2 = io.ReadInt32()
                    };
                    return fileEntryRDR;
                }

                public static void WriteEntry(PikIO io, TOCEntry entry)
                {
                    io.Write(entry.NameOffset);
                    if (entry.IsDirectory)
                    {
                        DirectoryEntry asDirectory = entry.AsDirectory;
                        io.Write(asDirectory.Flags);
                        io.Write((int)(2147483648L | (asDirectory.ContentEntryIndex & int.MaxValue)));
                        io.Write(asDirectory.ContentEntryCount & 268435455);
                        io.Write(asDirectory.UNK);
                    }
                    else
                    {
                        FileEntry asFile = entry.AsFile;
                        io.Write(asFile.SizeInArchive);
                        io.Write((uint)asFile.Offset);
                        io.Write(asFile.FlagInfo.Flag1);
                        io.Write(asFile.FlagInfo.Flag2);
                    }
                }

                public string Name
                {
                    get
                    {
                        if (IsDirectory && NameOffset == 0U)
                            return "root";

                        bool external = RPF6FileNameHandler.FileNames.ContainsKey(NameOffset);
                        if (external)
                            return RPF6FileNameHandler.FileNames[NameOffset];

                        if (DebugName != null && DebugName != "")
                            return DebugName;

                        return string.Format("0x{0}", NameOffset.ToString("X8"));
                    }
                    set => NameOffset = value.ToLower() == "root" ? 0U : DataUtils.GetHash(value);
                }

                public string GetPath(string seperator = "/")
                {
                    string str = this.Name;
                    for (DirectoryEntry parent = Parent; parent != null; parent = parent.Parent)
                    {
                        str = str.Insert(0, string.Format("{0}{1}", parent.Name, seperator));
                    }
                    return str;
                }

                public string GetPathTill(DirectoryEntry tillDirectory, string seperator = "/")
                {
                    string str = Name;
                    for (DirectoryEntry parent = Parent; parent != null && parent != tillDirectory; parent = parent.Parent)
                    {
                        str = str.Insert(0, string.Format("{0}{1}", parent.Name, seperator));
                    }
                    return str;
                }

                public int GetLevel()
                {
                    int num = 0;
                    for (DirectoryEntry parent = Parent; parent != null; parent = parent.Parent)
                    {
                        ++num;
                    }
                    return num;
                }
            }

            public class DirectoryEntry : TOCEntry
            {
                public int ContentEntryIndex { get; set; }

                public int ContentEntryCount { get; set; }

                public int Flags { get; set; }

                public int UNK { get; set; }
            }

            public class FileEntry : TOCEntry
            {
                public ResourceUtils.FlagInfo FlagInfo;

                public int SizeInArchive { get; set; }

                public long Offset { get; set; }

                public long? KeepOffset { get; set; }

                public byte ResourceType
                {
                    get => (byte)((ulong)Offset & byte.MaxValue);
                    set => Offset = Offset & -256L | value;
                }

                public void SetOffset(long offset)
                {
                    if ((ulong)offset % 8UL > 0UL)
                    {
                        Offset = -1L;
                        throw new Exception("INVALID_SET_OFFSET");
                    }

                    if (FlagInfo.IsResource)
                        Offset = offset / 8L | ResourceType;
                    else
                        Offset = offset / 8L;
                }

                public long GetOffset(bool getKeepOffset = false)
                {
                    if (getKeepOffset && KeepOffset.HasValue)
                        return KeepOffset.Value;

                    return FlagInfo.IsResource ? (Offset & 2147483392L) * 8L : (Offset & int.MaxValue) * 8L;
                }
            }
        }
    }
}

