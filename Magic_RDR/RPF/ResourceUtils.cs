using Pik.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace Magic_RDR.RPF
{
    public static class ResourceUtils
    {
        public class FlagInfo
        {
            public const uint RSC05Magic = 88298322;
            public const uint RSC06Magic = 105075538;
            public const uint RSC85Magic = 2235781970;
            public const uint RSC86Magic = 2252559186;
            public const int MaxPageSize = 524288;

            public int Flag1 { get; set; }

            public int Flag2 { get; set; }

            public FlagInfo()
            {
            }

            public FlagInfo(int flag) => Flag1 = flag;

            public int RSC05_VPage4
            {
                get => Flag1 & 1;
                set => Flag1 = Flag1 & -2 | value & 1;
            }

            public int RSC05_VPage3
            {
                get => Flag1 >> 1 & 1;
                set => Flag1 = Flag1 & -3 | (value & 1) << 1;
            }

            public int RSC05_VPage2
            {
                get => Flag1 >> 2 & 1;
                set => Flag1 = Flag1 & -5 | (value & 1) << 2;
            }

            public int RSC05_VPage1
            {
                get => Flag1 >> 3 & 1;
                set => Flag1 = Flag1 & -9 | (value & 1) << 3;
            }

            public int RSC05_VPage0
            {
                get => Flag1 >> 4 & (int)sbyte.MaxValue;
                set => Flag1 = Flag1 & -2033 | (value & (int)sbyte.MaxValue) << 4;
            }

            public int RSC05_VSize
            {
                get => Flag1 >> 11 & 15;
                set => Flag1 = Flag1 & -30721 | (value & 15) << 11;
            }

            public int RSC05_PPage4
            {
                get => Flag1 >> 15 & 1;
                set => Flag1 = Flag1 & -32769 | (value & 1) << 15;
            }

            public int RSC05_PPage3
            {
                get => Flag1 >> 16 & 1;
                set => Flag1 = Flag1 & -65537 | (value & 1) << 16;
            }

            public int RSC05_PPage2
            {
                get => Flag1 >> 17 & 1;
                set => Flag1 = Flag1 & -131073 | (value & 1) << 17;
            }

            public int RSC05_PPage1
            {
                get => Flag1 >> 18 & 1;
                set => Flag1 = Flag1 & -262145 | (value & 1) << 18;
            }

            public int RSC05_PPage0
            {
                get => Flag1 >> 19 & sbyte.MaxValue;
                set => Flag1 = Flag1 & -66584577 | (value & sbyte.MaxValue) << 19;
            }

            public int RSC05_PSize
            {
                get => Flag1 >> 26 & 15;
                set => Flag1 = Flag1 & -1006632961 | (value & 15) << 26;
            }

            public bool RSC05_Compressed
            {
                get => (Flag1 >> 30 & 1) == 1;
                set => Flag1 = Flag1 & -1073741825 | (value ? 1 : 0) << 30;
            }

            public bool RSC05_Resource
            {
                get => (Flag1 >> 31 & 1) == 1;
                set => Flag1 = Flag1 & int.MaxValue | (value ? 1 : 0) << 31;
            }

            public int RSC05_VPageCount => RSC05_VPage4 + RSC05_VPage3 + RSC05_VPage2 + RSC05_VPage1 + RSC05_VPage0;

            public int RSC05_PPageCount => RSC05_PPage4 + RSC05_PPage3 + RSC05_PPage2 + RSC05_PPage1 + RSC05_PPage0;

            public int RSC05_GetTotalVSize => (Flag1 & 2047) << RSC05_VSize + 8;

            public int RSC05_GetTotalPSize => (Flag1 >> 15 & 2047) << RSC05_PSize + 8;

            public int RSC05_GetSizeVPage0 => 4096 << RSC05_VSize;

            public int RSC05_GetSizePPage0 => 4096 << RSC05_PSize;

            public void RSC05_SetMemSizes(int vSize, int pSize) => Flag1 = RSC05_GenerateMemSizes(vSize, pSize);

            public static int RSC05_GenerateMemSizes(int vSize, int pSize)
            {
                int num1 = vSize >> 8;
                int num2 = 0;

                while (num1 > 63)
                {
                    if ((uint)(num1 & 1) > 0U)
                        num1 += 2;
                    num1 >>= 1;
                    ++num2;
                }

                int num3 = pSize >> 8;
                int num4 = 0;

                while (num3 > 63)
                {
                    if ((uint)(num3 & 1) > 0U)
                        num3 += 2;
                    num3 >>= 1;
                    ++num4;
                }
                int flag = num1 | num2 << 11 | num3 << 15 | num4 << 26;
                return num1;
            }

            public static byte[] RSC05_PackResource(byte[] _allData, int vSize, int pSize, int resType, AppGlobals.PlatformEnum platform, bool wtd = false)
            {
                MemoryStream memoryStream = new MemoryStream();
                PikIO pikIo = new PikIO(memoryStream, (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? PikIO.Endianess.Little : PikIO.Endianess.Big);

                uint val1 = platform == AppGlobals.PlatformEnum.PS3 ? 105075538U : 88298322U;
                pikIo.Write(val1);
                pikIo.Write(resType);

                int val2 = int.MinValue;
                if (wtd)
                    val2 = GetFlags(vSize, pSize);
                else
                    val2 = -1073741824 | RSC05_GenerateMemSizes(vSize, pSize);
                pikIo.Write(val2);
                byte[] b = null;

                switch (platform)
                {
                    case AppGlobals.PlatformEnum.PS3:
                        b = DataUtils.Compress(_allData, 9, false);
                        break;
                    case AppGlobals.PlatformEnum.Xbox:
                        b = DataUtils.CompressLZX(_allData);
                        pikIo.Write(267719409);
                        pikIo.Write(b.Length);
                        break;
                    case AppGlobals.PlatformEnum.Switch:
                        b = DataUtils.CompressZStandard(_allData);
                        break;
                }
                pikIo.WriteBytes(b);
                return memoryStream.ToArray();
            }

            public static int GetCompactSize(int size)
            {
                int s = size >> 8;
                int i = 0;

                while ((s % 2 == 0) && (s >= 32) && (i < 15))
                {
                    i++;
                    s = s >> 1;
                }
                return ((i & 0xF) << 11) | (s & 0x7FF);
            }

            public static int GetFlags(int sysSegSize, int gpuSegSize)
            {
                return (GetCompactSize(sysSegSize) & 0x7FFF) | (GetCompactSize(gpuSegSize) & 0x7FFF) << 15 | 3 << 30;
            }

            public FlagInfo(int flag1, int flag2)
            {
                Flag1 = flag1;
                Flag2 = flag2;
            }

            public bool RSC85_bResource
            {
                get => (Flag1 & 2147483648L) == 2147483648L;
                set => Flag1 = DataUtils.SetBit(Flag1, 31, value);
            }

            public int RSC85_VPage0
            {
                get => Flag1 >> 14 & 3;
                set => Flag1 = Flag1 & -3145729 | (value & 3) << 14;
            }

            public int RSC85_VPage1
            {
                get => Flag1 >> 8 & 63;
                set => Flag1 = Flag1 & -16129 | (value & 63) << 8;
            }

            public int RSC85_VPage2
            {
                get => Flag1 & byte.MaxValue;
                set => Flag1 = Flag1 & -256 | value & byte.MaxValue;
            }

            public int RSC85_PPage0
            {
                get => Flag1 >> 28 & 7;
                set => Flag1 = Flag1 & -1879048193 | (value & 7) << 28;
            }

            public int RSC85_PPage1
            {
                get => Flag1 >> 24 & 15;
                set => Flag1 = Flag1 & -251658241 | (value & 15) << 24;
            }

            public int RSC85_PPage2
            {
                get => Flag1 >> 16 & byte.MaxValue;
                set => Flag1 = Flag1 & -16711681 | (value & byte.MaxValue) << 16;
            }

            public bool RSC85_bUseExtendedSize
            {
                get => (Flag2 & 2147483648L) == 2147483648L;
                set => Flag2 = Flag2 & int.MaxValue | (value ? 1 : 0) << 31;
            }

            public int RSC85_ObjectStartPage
            {
                get => Flag2 >> 28 & 7;
                set => Flag2 = Flag2 & -1879048193 | (value & 7) << 28;
            }

            public int RSC85_ObjectStartPageSize
            {
                get => 4096 << RSC85_ObjectStartPage;
                set => RSC85_ObjectStartPage = DataUtils.TrailingZeroes(value) - 12;
            }

            public int RSC85_TotalVSize
            {
                get => (Flag2 & 16383) << 12;
                set => Flag2 = Flag2 & -16384 | value >> 12 & 16383;
            }

            public int RSC85_TotalPSize
            {
                get => (Flag2 >> 14 & 16383) << 12;
                set => Flag2 = Flag2 & -268419073 | (value >> 12 & 16383) << 14;
            }

            public int[] RSC85_PageSizesVirtual
            {
                get
                {
                    List<int> intList = new List<int>();
                    int num1 = -1;
                    int num2 = 524288;
                    int num3 = 0;
                    int rsC85TotalVsize = RSC85_TotalVSize;
                    int c85ObjectStartPage = RSC85_ObjectStartPage;

                    int[] numArray = new int[4]
                    {
                        RSC85_VPage0,
                        RSC85_VPage1,
                        RSC85_VPage2,
                        int.MaxValue
                    };

                    for (int index1 = 0; index1 < 4; ++index1)
                    {
                        for (int index2 = 0; index2 < numArray[index1] && (uint)rsC85TotalVsize > 0U; ++index2)
                        {
                            while (num2 > rsC85TotalVsize)
                                num2 >>= 1;
                            if (num2 == c85ObjectStartPage && num1 == -1)
                                num1 = num3;
                            num3 += num2;
                            intList.Add(num2);
                            rsC85TotalVsize -= num2;
                        }
                        num2 >>= 1;
                    }
                    return intList.ToArray();
                }
            }

            public int[] RSC85_PageSizesPhysical
            {
                get
                {
                    List<int> intList = new List<int>();
                    int num1 = -1;
                    int num2 = 524288;
                    int num3 = 0;
                    int rsC85TotalPsize = RSC85_TotalPSize;
                    int c85ObjectStartPage = RSC85_ObjectStartPage;

                    int[] numArray = new int[4]
                    {
                        RSC85_PPage0,
                        RSC85_PPage1,
                        RSC85_PPage2,
                        int.MaxValue
                    };

                    for (int index1 = 0; index1 < 4; ++index1)
                    {
                        for (int index2 = 0; index2 < numArray[index1] && (uint)rsC85TotalPsize > 0U; ++index2)
                        {
                            while (num2 > rsC85TotalPsize)
                                num2 >>= 1;

                            if (num2 == c85ObjectStartPage && num1 == -1)
                                num1 = num3;

                            num3 += num2;
                            intList.Add(num2);
                            rsC85TotalPsize -= num2;
                        }
                        num2 >>= 1;
                    }
                    return intList.ToArray();
                }
            }

            public int RSC85_ObjectStart
            {
                get
                {
                    int[] pageSizesVitrual = RSC85_PageSizesVirtual;
                    int num = 0;
                    for (int index = 0; index < pageSizesVitrual.Length; ++index)
                    {
                        if (pageSizesVitrual[index] == RSC85_ObjectStartPageSize)
                            return num;
                        num += pageSizesVitrual[index];
                    }
                    return 0;
                }
            }

            public static (int, int, int, int, int, int) RSC85_GenerateMemorySizes(int virt, int phys, int pageBaseSize = 4096)
            {
                int[] numArray = new int[6];
                int num1 = virt;
                int num2 = pageBaseSize << 7;
                int num3 = num2;

                for (int index = 0; index < 3; ++index)
                {
                    int num4 = num1 / (num3 >> index);
                    num1 -= num4 * (num3 >> index);
                    numArray[index] = num4;
                }

                int num5 = num2;
                int num6 = phys;

                for (int index = 0; index < 3; ++index)
                {
                    int num4 = num6 / (num5 >> index);
                    num6 -= num4 * (num5 >> index);
                    numArray[index + 3] = num4;
                }
                return (numArray[0], numArray[1], numArray[2], numArray[3], numArray[4], numArray[5]);
            }

            public void RSC85_SetMemSizes(int totalVirt, int totalPhys, int pageBaseSize = 4096)
            {
                (RSC85_VPage0, RSC85_VPage1, RSC85_VPage2, RSC85_PPage0, RSC85_PPage1, RSC85_PPage2) = ResourceUtils.FlagInfo.RSC85_GenerateMemorySizes(totalVirt, totalPhys, pageBaseSize);
                RSC85_TotalVSize = totalVirt;
                RSC85_TotalPSize = totalPhys;
            }

            public int[] RSC85_GetAvaliableObjectStartPage(int pageBaseSize = 4096)
            {
                List<int> intList = new List<int>();
                foreach (int num in RSC85_PageSizesVirtual)
                {
                    for (int index = 0; index < 7; ++index)
                    {
                        if (pageBaseSize << index == num && !intList.Contains(index))
                            intList.Add(index);
                    }
                }
                return intList.ToArray();
            }

            public bool IsResource
            {
                get => RSC05_Resource;
                set => RSC05_Resource = value;
            }

            public bool IsExtendedFlags
            {
                get => RSC85_bUseExtendedSize;
                set => RSC85_bUseExtendedSize = value;
            }

            public bool IsCompressed
            {
                get => !IsExtendedFlags && RSC05_Compressed;
                set
                {
                    if (IsExtendedFlags)
                        return;
                    RSC05_Compressed = value;
                }
            }

            public int BaseResourceSizeP => IsExtendedFlags ? RSC85_TotalPSize : RSC05_GetTotalPSize;

            public int BaseResourceSizeV => IsExtendedFlags ? RSC85_TotalVSize : RSC05_GetTotalVSize;

            public int ResourceStart => !IsResource || !IsExtendedFlags ? 0 : RSC85_ObjectStart;

            public int GetTotalSize() => IsResource ? BaseResourceSizeP + BaseResourceSizeV : Flag1 & DataUtils.IntFromUInt(3221225471U);

            public void SetTotalSize(int virtOrSize, int phys)
            {
                if (IsResource)
                {
                    if (IsExtendedFlags)
                    {
                        RSC85_TotalVSize = virtOrSize;
                        RSC85_TotalPSize = phys;
                    }
                    else
                        RSC85_SetMemSizes(virtOrSize, phys);
                }
                else
                    Flag1 = (int)((long)Flag1 & 1073741824L | (long)virtOrSize & 3221225471L);
            }

            public bool IsRSC85 => IsExtendedFlags;

            public bool IsRSC05 => !IsExtendedFlags;

            public override string ToString()
            {
                if (!IsResource)
                    return "Regular File";
                if (!IsExtendedFlags)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("IVRSC:");
                    stringBuilder.AppendLine(string.Format("RSC: {0}", (object)RSC05_Resource));
                    stringBuilder.AppendLine(string.Format("Compressed: {0}", (object)RSC05_Compressed));
                    stringBuilder.AppendLine(string.Format("VPage0: {0}", (object)RSC05_VPage0));
                    stringBuilder.AppendLine(string.Format("VPage1: {0}", (object)RSC05_VPage1));
                    stringBuilder.AppendLine(string.Format("VPage2: {0}", (object)RSC05_VPage2));
                    stringBuilder.AppendLine(string.Format("VPage3: {0}", (object)RSC05_VPage3));
                    stringBuilder.AppendLine(string.Format("VPage4: {0}", (object)RSC05_VPage4));
                    stringBuilder.AppendLine(string.Format("VSize: {0}", (object)RSC05_VSize));
                    stringBuilder.AppendLine(string.Format("PPage0: {0}", (object)RSC05_PPage0));
                    stringBuilder.AppendLine(string.Format("PPage1: {0}", (object)RSC05_PPage1));
                    stringBuilder.AppendLine(string.Format("PPage2: {0}", (object)RSC05_PPage2));
                    stringBuilder.AppendLine(string.Format("PPage3: {0}", (object)RSC05_PPage3));
                    stringBuilder.AppendLine(string.Format("PPage4: {0}", (object)RSC05_PPage4));
                    stringBuilder.AppendLine(string.Format("PSize: {0}", (object)RSC05_PSize));
                    stringBuilder.AppendLine(string.Format("TotalVSize: {0}", (object)RSC05_GetTotalVSize));
                    stringBuilder.AppendLine(string.Format("TotalPSize: {0}", (object)RSC05_GetTotalPSize));
                    stringBuilder.AppendLine(string.Format("VPage0Size: {0}", (object)RSC05_GetSizeVPage0));
                    stringBuilder.AppendLine(string.Format("PPage0Size: {0}", (object)RSC05_GetSizePPage0));
                    return stringBuilder.ToString();
                }
                StringBuilder stringBuilder1 = new StringBuilder();
                stringBuilder1.AppendLine("RSC85:");
                stringBuilder1.AppendLine(string.Format("RSC: {0}", (object)RSC85_bResource));
                stringBuilder1.AppendLine(string.Format("VPage0: {0}", (object)RSC85_VPage0));
                stringBuilder1.AppendLine(string.Format("VPage1: {0}", (object)RSC85_VPage1));
                stringBuilder1.AppendLine(string.Format("VPage2: {0}", (object)RSC85_VPage2));
                stringBuilder1.AppendLine(string.Format("PPage0: {0}", (object)RSC85_PPage0));
                stringBuilder1.AppendLine(string.Format("PPage1: {0}", (object)RSC85_PPage1));
                stringBuilder1.AppendLine(string.Format("PPage2: {0}", (object)RSC85_PPage2));
                stringBuilder1.AppendLine("-----------");
                stringBuilder1.AppendLine("Flag2:");
                stringBuilder1.AppendLine(string.Format("UseExt: {0}", (object)RSC85_bUseExtendedSize));
                stringBuilder1.AppendLine(string.Format("StartPage: {0}", (object)RSC85_ObjectStartPage));
                stringBuilder1.AppendLine(string.Format("StartPageSize: {0}", (object)RSC85_ObjectStartPageSize));
                stringBuilder1.AppendLine(string.Format("TotalVSize: {0}", (object)RSC85_TotalVSize));
                stringBuilder1.AppendLine(string.Format("TotalPSize: {0}", (object)RSC85_TotalPSize));
                stringBuilder1.AppendLine("-----------");
                int[] pageSizesVitrual = RSC85_PageSizesVirtual;
                stringBuilder1.AppendLine(string.Format("Virtual Page Sizes: [{0}]", (object)pageSizesVitrual.Length));
                for (int index = 0; index < pageSizesVitrual.Length; ++index)
                    stringBuilder1.AppendLine(string.Format("{0}", (object)pageSizesVitrual[index]));
                int[] pageSizesPhysical = RSC85_PageSizesPhysical;
                stringBuilder1.AppendLine(string.Format("Physical Page Sizes: [{0}]", (object)pageSizesPhysical.Length));
                for (int index = 0; index < pageSizesPhysical.Length; ++index)
                    stringBuilder1.AppendLine(string.Format("{0}", (object)pageSizesPhysical[index]));
                return stringBuilder1.ToString();
            }
        }

        public class ResourceInfo
        {
            public static string GetResourceVersionStringFromIdent(uint ident)
            {
                switch (ident)
                {
                    case 1381188357:
                        return "05CSR";
                    case 1381188358:
                        return "06CSR";

                    case 1381188485:
                        return "85CSR";
                    case 1381188486:
                        return "86CSR";

                    case 88298322:
                        return "RSC05";
                    case 105075538:
                        return "RSC06";

                    case 2235781970:
                        return "RSC85";
                    case 2252559186:
                        return "RSC86";

                    default:
                        return "INVALID_RSC_IDENT";
                }
            }

            public static (bool, string, int, int, int) GetResourceInformation(Stream fileStream)
            {
                bool flag = false;
                PikIO pikIo = new PikIO(fileStream, (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? PikIO.Endianess.Little : PikIO.Endianess.Big);
                uint ident = pikIo.ReadUInt32();
                int num1;

                switch (ident)
                {
                    case 88298322:
                    case 2235781970:
                    case 2252559186:
                        num1 = 1;
                        break;
                    default:
                        num1 = ident == 105075538U ? 1 : 0;
                        break;
                }
                if (num1 != 0) flag = true;

                int num2 = pikIo.ReadInt32();
                int num3 = 0;
                int num4 = 0;
                string versionStringFromIdent = GetResourceVersionStringFromIdent(ident);

                if (ident == 2235781970U || ident == 2252559186U)
                {
                    num3 = pikIo.ReadInt32();
                    num4 = pikIo.ReadInt32();
                }
                else if (ident == 88298322U || ident == 105075538U)
                {
                    num3 = pikIo.ReadInt32();
                    num4 = 0;
                }
                else Console.WriteLine("UNK_RES");

                fileStream.Position = 0L;
                return (flag, versionStringFromIdent, num2, num3, num4);
            }

            public static bool IsResourceStream(Stream xIn)
            {
                long position = xIn.Position;
                uint num1 = new PikIO(xIn, (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? PikIO.Endianess.Little : PikIO.Endianess.Big).ReadUInt32();
                int num2;

                switch (num1)
                {
                    case 88298322:
                    case 2235781970:
                    case 2252559186:
                        num2 = 1;
                        break;
                    default:
                        num2 = num1 == 105075538U ? 1 : 0;
                        break;
                }
                bool flag = num2 != 0;
                xIn.Position = position;
                return flag;
            }

            public static byte[] GetDataFromResourceBytes(byte[] data) => GetDataFromStream(new MemoryStream(data));

            public static byte[] GetDataFromStream(Stream resourceStream)
            {
                PikIO pikIo1 = new PikIO(resourceStream, (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch) ? PikIO.Endianess.Little : PikIO.Endianess.Big);
                uint num1 = pikIo1.ReadUInt32();
                int num2 = pikIo1.ReadInt32();
                FlagInfo flagInfo;

                if (num1 == 2235781970U || num1 == 2252559186U)
                {
                    flagInfo = new FlagInfo(pikIo1.ReadInt32(), pikIo1.ReadInt32());
                }
                else
                {
                    if (num1 != 88298322U && num1 != 105075538U)
                    {
                        return null;
                    }
                    flagInfo = new FlagInfo(pikIo1.ReadInt32());
                }

                byte[] numArray = pikIo1.ReadBytes((int)(pikIo1.Length - pikIo1.Position));
                if (num2 == 2 && flagInfo.IsRSC85)
                {
                    numArray = DataUtils.Decrypt(numArray, AppGlobals.EncryptionKey);
                }

                PikIO pikIo2 = new PikIO(new MemoryStream(numArray), PikIO.Endianess.Big);
                int dLen = flagInfo.BaseResourceSizeP + flagInfo.BaseResourceSizeV;
                byte[] decompressedData = new byte[dLen];

                switch (AppGlobals.Platform)
                {
                    case AppGlobals.PlatformEnum.PS3:
                    {
                        int length = (int)pikIo2.Length;
                        byte[] data = pikIo2.ReadBytes(length);
                        try
                        {
                            decompressedData = DataUtils.DecompressDeflate(data, dLen, false);
                            break;
                        }
                        catch
                        {
                            break;
                        }
                    }
                    case AppGlobals.PlatformEnum.Xbox:
                    {
                        pikIo2.ReadUInt32();
                        int len = pikIo2.ReadInt32();
                        DataUtils.DecompressLZX(pikIo2.ReadBytes(len), out decompressedData, ref dLen);
                        break;
                    }
                    case AppGlobals.PlatformEnum.Switch:
                    {
                        int length = (int)pikIo2.Length;
                        byte[] data = pikIo2.ReadBytes(length);
                        try
                        {
                            decompressedData = DataUtils.DecompressZStandard(data);
                            break;
                        }
                        catch //Some resources still use zlib (.wtx)
                        {
                            try
                            {
                                decompressedData = DataUtils.DecompressDeflate(data, dLen, false);
                                break;
                            }
                            catch { break; }
                        }
                    }
                }
                return decompressedData;
            }
        }

        public static bool IsTextFile(string entryName)
        {
            if (!entryName.Contains("."))
                return false;

            string extension = entryName.Substring(entryName.LastIndexOf("."));
            switch (extension)
            {
                case ".xml":
                case ".tr":
                case ".txt":
                case ".csv":
                case ".sc.xml":
                case ".refgroup":
                case ".fxlist":
                case ".modellist":
                case ".emitlist":
                case ".fullfxlist":
                case ".shaderlist":
                case ".texlist":
                case ".ptxlist":
                case ".raw":
                case ".expl":
                case ".textune":
                case ".hud":
                case ".envclothmanager":
                case ".charclothmanager":
                case ".traffic":
                case ".arm":
                case ".mtl":
                case ".weap":
                case ".cfg":
                case ".fx":
                case ".todlight":
                case ".ppp":
                case ".dat":
                case ".tune":
                    return true;
                default:
                    return false;
            }
        }
    }
}
