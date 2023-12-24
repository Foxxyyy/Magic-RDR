using System;
using System.IO;
using System.Reflection;

public static class AppGlobals
{
    private static DateTime buildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
    public static string BuildDate = string.Format("{0}{1}{2}", buildDate.Month, buildDate.Day, buildDate.Year % 100);
    public static string Version = "0.0.0";
    public static PlatformEnum Platform = PlatformEnum.Xbox;
    public static char PlatformChar = (char)120;
    public static string PlatformString = "Xbox 360";

    public static byte[] EncryptionKey = new byte[32]
    {
        0xB7, 0x62, 0xDF, 0xB6, 0xE2, 0xB2, 0xC6, 0xDE, 0xAF, 0x72, 0x2A, 0x32, 0xD2, 0xFB, 0x6F, 0x0C, 0x98, 0xA3, 0x21, 0x74, 0x62, 0xC9, 0xC4, 0xED, 0xAD, 0xAA, 0x2E, 0xD0, 0xDD, 0xF9, 0x2F, 0x10
    };

    public static void SetPlatform(PlatformEnum platform)
    {
        Platform = platform;
        switch (Platform)
        {
            case PlatformEnum.Xbox:
                PlatformChar = (char)120;
                PlatformString = "Xbox 360";
                break;
            case PlatformEnum.PS3:
                PlatformChar = (char)99;
                PlatformString = "PlayStation 3";
                break;
            case PlatformEnum.Switch:
                PlatformChar = (char)119;
                PlatformString = "Nintendo Switch";
                break;
        }
    }

    public static class Settings
    {
        public const int SettingsVersion = 2;
        public static bool MakeFileDirectoryListing;
        public static bool MakeDiffDataFile;
        public static bool MakeFileInfoData;
        public static bool WriteDecryptedTOC;
        public static bool WriteTOCOrder;
        public static bool WriteRSCInfo;

        public static void Save(Stream xOut)
        {
            BinaryWriter binaryWriter = new BinaryWriter(xOut);
            binaryWriter.Write(2);
            binaryWriter.Write(MakeFileDirectoryListing);
            binaryWriter.Write(MakeDiffDataFile);
            binaryWriter.Write(MakeFileInfoData);
            binaryWriter.Write(WriteDecryptedTOC);
            binaryWriter.Write(WriteTOCOrder);
            binaryWriter.Write(WriteRSCInfo);
            binaryWriter.Close();
        }

        public static void Load(Stream xIn)
        {
            BinaryReader binaryReader = new BinaryReader(xIn);
            if (binaryReader.ReadInt32() == 2)
            {
                MakeFileDirectoryListing = binaryReader.ReadBoolean();
                MakeDiffDataFile = binaryReader.ReadBoolean();
                MakeFileInfoData = binaryReader.ReadBoolean();
                WriteDecryptedTOC = binaryReader.ReadBoolean();
                WriteTOCOrder = binaryReader.ReadBoolean();
                WriteRSCInfo = binaryReader.ReadBoolean();
            }
            binaryReader.Close();
        }
    }

    public enum PlatformEnum
    {
        None = 0,
        PS3 = 4096, // 0x00001000
        Xbox = 8192, // 0x00002000
        Switch = 16384, // 0x00004000
    }
}
