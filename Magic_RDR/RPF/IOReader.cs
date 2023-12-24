using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static Magic_RDR.Viewers.ShaderViewerForm;

namespace Magic_RDR.Application
{
    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y) : this()
        {
            X = x;
            Y = y;
        }
    }

    public struct Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Multiply(Vector3 vector, Vector3 scale)
        {
            return new Vector3(vector.X * scale.X, vector.Y * scale.Y, vector.Z * scale.Z);
        }

        public static Vector3 Substract(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.X - vector2.X, vector1.Y - vector2.Y, vector1.Z - vector2.Z);
        }
    }

    public struct Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector4(float x, float y, float z, float w) : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }

    public static partial class NumberHelpers
    {
        public static short Swap(this short value)
        {
            return (short)((0x00FF & (value >> 8)) | (0xFF00 & (value << 8)));
        }

        public static ushort Swap(this ushort value)
        {
            return (ushort)((0x00FF & (value >> 8)) | (0xFF00 & (value << 8)));
        }

        public static int Swap(this int value)
        {
            uint uvalue = (uint)value;
            return (int)((0x000000FF & (uvalue >> 24)) | (0x0000FF00 & (uvalue >> 8)) | (0x00FF0000 & (uvalue << 8)) | (0xFF000000 & (uvalue << 24)));
        }

        public static uint Swap(this uint value)
        {
            return (0x000000FF & (value >> 24)) | (0x0000FF00 & (value >> 8)) | (0x00FF0000 & (value << 8)) | (0xFF000000 & (value << 24));
        }

        public static ulong Swap(this ulong value)
        {
            return (0x00000000000000FF & (value >> 56)) | (0x000000000000FF00 & (value >> 40)) | (0x0000000000FF0000 & (value >> 24)) | (0x00000000FF000000 & (value >> 8)) | (0x000000FF00000000 & (value << 8)) | (0x0000FF0000000000 & (value << 24)) | (0x00FF000000000000 & (value << 40)) | (0xFF00000000000000 & (value << 56));
        }

        public static float Swap(this float value)
        {
            byte[] data = BitConverter.GetBytes(value);
            int rawValue = BitConverter.ToInt32(data, 0).Swap();
            return BitConverter.ToSingle(BitConverter.GetBytes(rawValue), 0);
        }
    }

    public class IOReader : BinaryReader
    {
        public enum Endian
        {
            Big,
            Little
        }

        public enum StringType
        {
            ASCII,
            ASCII_NULL_TERMINATED,
            UNICODE,
            UNICODE_NULL_TERMINATED
        }

        private Endian BYTE_ORDER;

        #region Constructors

        public IOReader(Stream input, Endian byteOrder) : this(input, byteOrder, 0) { }

        public IOReader(Stream input, Endian byteOrder, long startingOffset) : base(input)
        {
            if (startingOffset > base.BaseStream.Length)
                throw new EndOfStreamException();
            else base.BaseStream.Position = startingOffset;
            BYTE_ORDER = byteOrder;
        }

        #endregion

        #region Reading Methods

        public void Seek(object position, SeekOrigin origin = SeekOrigin.Current)
        {
            base.BaseStream.Seek(Convert.ToInt64(position), origin);
        }

        public override byte ReadByte()
        {
            return base.ReadByte();
        }

        public ushort ReadUShort()
        {
            var value = base.ReadUInt16();
            return BYTE_ORDER == Endian.Big ? value.Swap() : value;
        }

        public short ReadShort()
        {
            var value = base.ReadInt16();
            return BYTE_ORDER == Endian.Big ? value.Swap() : value;
        }

        public int ReadInt32(bool LittleEndian = false)
        {
            byte[] byteBuffer = base.ReadBytes(4);
            if (LittleEndian || BYTE_ORDER == Endian.Little)
                return BitConverter.ToInt32(byteBuffer, 0);
            else
                return (byteBuffer[0] << 24) | (byteBuffer[1] << 16) | (byteBuffer[2] << 8) | byteBuffer[3];
        }

        public int ReadOffset(int offset)
        {
            if (offset >> 28 != 5)
                return offset;
            return offset & 0x0fffffff;
        }

        public int GetPageCount(int size)
        {
            return (size + (1 << 14) - 1) >> 14;
        }

        public int GetPageLengthAtPage(int[] pages, int pageLength, int page)
        {
            return (page == pages.Length - 1) ? (pageLength % 16384) : 16384;
        }

        public int GetDataOffset(int offset)
        {
            return offset & 0x0fffffff;
        }

        public uint ReadUInt()
        {
            var value = base.ReadUInt32();
            return BYTE_ORDER == Endian.Big ? value.Swap() : value;
        }

        public ulong ReadULong()
        {
            var value = base.ReadUInt64();
            return BYTE_ORDER == Endian.Big ? value.Swap() : value;
        }

        public float ReadFloat()
        {
            var value = base.ReadSingle();
            return BYTE_ORDER == Endian.Big ? value.Swap() : value;
        }

        public Vector3 ReadVector3()
        {
            var x = ReadFloat();
            var y = ReadFloat();
            var z = ReadFloat();
            return BYTE_ORDER == Endian.Little ? new Vector3(x.Swap(), y.Swap(), z.Swap()) : new Vector3(x, y, z);
        }

        public Vector4 ReadVector4()
        {
            var x = ReadFloat();
            var y = ReadFloat();
            var z = ReadFloat();
            var h = ReadFloat();
            return BYTE_ORDER == Endian.Little ? new Vector4(x.Swap(), y.Swap(), z.Swap(), h.Swap()) : new Vector4(x, y, z, h);
        }

        public byte[] ReadBytes(object count)
        {
            byte[] buffer = new byte[Convert.ToInt32(count)];
            Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public string ReadCustomString(StringType type, int length, int stringPosition, bool seekBack = false)
        {
            long position = base.BaseStream.Position;
            base.BaseStream.Seek(stringPosition, SeekOrigin.Begin);
            string str = ReadString(type, length);

            if (seekBack)
                base.BaseStream.Seek(position, SeekOrigin.Begin);
            return str;
        }

        public string ReadString(StringType type, int length)
        {
            switch (type)
            {
                case StringType.ASCII:
                    return Encoding.ASCII.GetString(base.ReadBytes(length));
                case StringType.ASCII_NULL_TERMINATED:
                    string result = "";
                    int lastChar = base.ReadByte();
                    while (lastChar != 0)
                    {
                        result += (char)lastChar;
                        lastChar = base.ReadByte();
                    }
                    return result;
                case StringType.UNICODE:
                    return Encoding.BigEndianUnicode.GetString(base.ReadBytes(length * 2)).Replace("\0", string.Empty);
                case StringType.UNICODE_NULL_TERMINATED:
                    string str = string.Empty;
                    while (true)
                    {
                        ushort num = ReadUShort();
                        if (num == 0)
                        {
                            return str;
                        }
                        str = str + ((char)num);
                    }
                default:
                    return string.Empty;
            }
        }

        public override ushort ReadUInt16()
        {
            return ReadUShort();
        }

        public override uint ReadUInt32()
        {
            return ReadUInt();
        }

        public override ulong ReadUInt64()
        {
            return ReadULong();
        }

        #endregion
    }

    public class IOWriter : BinaryWriter
    {
        public enum Endian
        {
            Big,
            Little
        }

        public enum StringType
        {
            ASCII,
            ASCII_NULL_TERMINATED,
            UNICODE,
            UNICODE_NULL_TERMINATED
        }

        private Endian BYTE_ORDER;

        #region Constructors

        public IOWriter(Stream input, Endian byteOrder) : this(input, byteOrder, 0) { }

        public IOWriter(Stream input, Endian byteOrder, long startingOffset) : base(input)
        {
            if (startingOffset > base.BaseStream.Length)
                throw new EndOfStreamException();
            else base.BaseStream.Position = startingOffset;
            BYTE_ORDER = byteOrder;
        }

        #endregion

        #region Writing Methods

        public void Seek(object position, SeekOrigin origin = SeekOrigin.Current)
        {
            base.BaseStream.Seek(Convert.ToInt64(position), origin);
        }

        public byte[] ReadBytes(object count)
        {
            byte[] buffer = new byte[Convert.ToInt32(count)];
            base.BaseStream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public void WriteByte(byte value)
        {
            base.Write(value);
        }

        public void WriteUInt16(ushort value)
        {
            base.Write(value);
        }

        public void WriteInt16(short value)
        {
            base.Write(value);
        }

        public void WriteInt32(int value)
        {
            base.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            base.Write(value);
        }

        public void WritePointer(uint value, bool virtualSize = false)
        {
            if (virtualSize)
                base.Write(1610612736 | (uint)(value & 268435455UL));
            else
                base.Write(1342177280U | (uint)(value & 268435455UL));
        }

        public void WriteULong(ulong value)
        {
            base.Write(value);
        }

        public void WriteFloat(float value)
        {
            base.Write(value);
        }
        public void WriteVector3(Vector3 value)
        {
            base.Write(value.X);
            base.Write(value.Y);
            base.Write(value.Z);
        }

        public void WriteVector4(Vector4 value)
        {
            base.Write(value.X);
            base.Write(value.Y);
            base.Write(value.Z);
            base.Write(value.W);
        }

        public void WriteBytes(byte[] array)
        {
            base.Write(array);
        }

        public void WriteCustomString(string value, StringType type, int stringPosition, bool seekBack = false)
        {
            long position = base.BaseStream.Position;
            base.BaseStream.Seek(stringPosition, SeekOrigin.Begin);
            this.WriteString(value, type);

            if (seekBack)
            {
                base.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }

        public void WriteString(string value, StringType type)
        {
            switch (type)
            {
                case StringType.ASCII:
                    byte[] asciiBytes = Encoding.ASCII.GetBytes(value);
                    base.Write(asciiBytes);
                    break;

                case StringType.ASCII_NULL_TERMINATED:
                    byte[] asciiNullBytes = Encoding.ASCII.GetBytes(value);
                    base.Write(asciiNullBytes);
                    base.Write((byte)0); // Null-terminate the string
                    break;

                case StringType.UNICODE:
                    byte[] unicodeBytes = Encoding.BigEndianUnicode.GetBytes(value);
                    base.Write(unicodeBytes);
                    break;

                case StringType.UNICODE_NULL_TERMINATED:
                    byte[] unicodeNullBytes = Encoding.BigEndianUnicode.GetBytes(value);
                    base.Write(unicodeNullBytes);
                    base.Write((ushort)0); // Null-terminate the string
                    break;
            }
        }

        #endregion
    }

    #region Resource Structures

    [StructLayout(LayoutKind.Explicit)]
    public struct RGArray
    {
        [FieldOffset(0)] public int Offset;
        [FieldOffset(4)] public ushort Count;
        [FieldOffset(6)] public ushort Size;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RGModel
    {
        [FieldOffset(0)] public uint Unknown0x00;
        [FieldOffset(4)] public RGArray Geometry;
        [FieldOffset(12)] public int Unknown0x0C;
        [FieldOffset(16)] public int Unknown0x10;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RGTextureResource
    {
        [FieldOffset(0)] public uint Unknown_0x00;
        [FieldOffset(4)] public uint DataSize;
        [FieldOffset(8)] public int NameTexturePointer;
        [FieldOffset(12)] public int BaseTexturePointer;
        [FieldOffset(16)] public ushort Width;
        [FieldOffset(18)] public ushort Height;
        [FieldOffset(20)] public int MipMapCount;
        [FieldOffset(24)] public Vector3 Unknown_0x18;
        [FieldOffset(36)] public Vector3 Unknown_0x24;
    }

    public struct RGTexture
    {
        public RGTextureResource Resource;
        public uint DataSize;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RGVertexBuffer
    {
        [FieldOffset(0)] public uint Unknown0x00;
        [FieldOffset(4)] public ushort VertexCount;
        [FieldOffset(6)] public byte IsLocked;
        [FieldOffset(7)] public byte Unknown0x06;
        [FieldOffset(8)] public int LockedData;
        [FieldOffset(12)] public ushort Unknown0x08;
        [FieldOffset(14)] public ushort VertexStride;
        [FieldOffset(16)] public int VertexData;
        [FieldOffset(20)] public uint LockThreadID;
        [FieldOffset(24)] public int VertexFormat;
        [FieldOffset(28)] public int VertexBuffer;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RGVertexFormat
    {
        [FieldOffset(0)] public uint Mask; // Bit mask of the used FVFIndices
        [FieldOffset(4)] public byte VertexSize;
        [FieldOffset(5)] public byte Unknown0x05;
        [FieldOffset(6)] public byte Unknnown0x06;
        [FieldOffset(7)] public byte DeclarationCount; // Number of 1 in nbMask
        [FieldOffset(8)] public ulong FVFIndices; // 16 fields, 4 bits each - index in the FVF table
    }

    #endregion

    #region Shader Names
    public enum ShaderNameTerrains : uint
    {
        rdr2_wire = 0xA3805DAD,
        rdr2_injury = 0xB43FA777,
        rdr2_bump = 0x2DA9ADFA,
        rdr2_alpha = 0x32A4918E,
        rdr2_alpha_cloth = 0x63FF371E,
        rdr2_diffuse = 0xA042C1CE,
        rdr2_diffuse_cloth = 0xD8554A3F,
        rdr2_alpha_bspec_ao = 0x248159BA,
        rdr2_alpha_bspec_ao_cpv = 0xD42FD651,
        rdr2_alpha_foliage = 0xC714B86E,
        rdr2_alpha_foliage_no_fade = 0x592D7DC2,
        rdr2_beacon = 0xBDBE7C77,
        rdr2_bump_ambocc = 0x2E9C4C9E,
        rdr2_bump_ambocc_alphaao = 0xCF9AB40,
        rdr2_bump_ambocc_cpv = 0x1600534F,
        rdr2_bump_spec = 0x7028E7F1,
        rdr2_bump_spec_alpha = 0xE6C3E8D4,
        rdr2_bump_spec_ambocc_cpv = 0x47CC2100,
        rdr2_bump_spec_ambocc_shared = 0x2FE0F698,
        rdr2_bump_spec_ao_cloth = 0xF8A043A1,
        rdr2_layer_2_nospec_ambocc = 0x5A170205,
        rdr2_layer_2_nospec_ambocc_bridge = 0xF35BF0CC,
        rdr2_layer_2_nospec_ambocc_decal = 0xB34AF114,
        rdr2_layer_3_nospec_normal_ambocc = 0x24982D70,
        rdr2_treerock_prototype = 0x18C56B10,
        rdr2_cliffwall = 0xE3915961,
        rdr2_cliffwall_ao = 0x249BB297,
        rdr2_cliffwall_ao_low_lod = 0x3103407E,
        rdr2_cliffwall_alpha = 0x227C5611,
        rdr2_cliffwall_alpha2 = 0xEDB8BB72,
        rdr2_clouds_Anim = 0xE3136EFD,
        rdr2_clouds_AnimSoft = 0x60F5992A,
        rdr2_clouds_Fast = 0x5218CB1D,
        rdr2_clouds_Soft = 0xF6C04CCD,
        rdr2_clouds_Fog = 0xFE72D6A5,
        rage_CurvedModel = 0x4A9FDAA1,
        rdr2_atmoscatt = 0x565D658E,
        rdr2_glass_notint = 0x4BC61B93,
        rdr2_glass_notint_shared = 0x18E2B6,
        rdr2_low_lod = 0x2E1239A8,
        rdr2_low_lod_singlesided = 0xD70C66E0,
        rdr2_low_lod_nodirt = 0x387E0FDE,
        rdr2_low_lod_nodirt_singlesided = 0xED7CD8D7,
        rdr2_low_lod_decal = 0x24C91669,
        rdr2_billboard = 0xBF17403F,
        rdr2_glass_nodistortion_bump_spec_ao = 0x72A21FFE,
        rdr2_glass_nodistortion_bump_spec_ao_shared = 0x7668B157,
        rdr2_flattenTerrain = 0xB71272EA,
        rdr2_flattenTerrain_blend = 0x707EF967,
        rdr2_glass_decal = 0x644D8CE5,
        rdr2_alpha_bspec_ao_cloth = 0xC47E1378,
        rdr2_river_water = 0xA1100B4E,
        rdr2_river_water_joint = 0x372E2B02,
        rdr2_window_glow = 0x6C25115D,
        rdr2_door_glow = 0x25A07A25,
        rage_lightprepass = 0x350D3225,
        rdr2_taa = 0x9674A250,
        rdr2_glass_glow = 0x171C9E47,
        rdr2_dome_clouds = 0x46A1C02D,
        rdr2_breakableglass = 0xD847F1C8,
        rdr2_wheeltrack = 0xFA6CDDD9,
        rdr2_grass = 0x173D5F9D,
        rdr2_lightcone = 0x2B90B746,
        rdr2_lightglow = 0xBB89090,
        rdr2_bump_spec_ao_dirt_cloth = 0x2EE5E6BB,
        rdr2_waterdecal = 0x17BE7116,
        rdr2_waterjointdecal = 0x1CF3EB10,
        rdr2_scope_barrel = 0xCA9C8A10,
        rdr2_scope_lens = 0x40734423,
        rdr2_scope_lens_distortion = 0x3AAE170A,
        rdr2_binoculars_barrel = 0x7C122805,
        rdr2_binoculars_lens = 0xA8EE77F,
        rdr2_weapon = 0xE9DDA9A0,
        rdr2_alpha_bspec_ao_shared = 0x949EC19C,
        rdr2_shadowonly = 0x4C03B90B,
        rdr2_poster = 0xAA95CD3F,
        rdr2_terrain = 0xF98973D1,
        rdr2_terrain4 = 0x6D27D953,
        rdr2_terrain_blend = 0xC242DAA7,
        rdr2_terrain_shoreline = 0x70BFB810,
        rdr2_terrain_low_lod = 0xF9DD37D9,
        rdr2_debris = 0xC1B762B,
        rdr2_map = 0x9667690,
        rdr2_postfx = 0x4C6F0D97,
        rdr2_footprint = 0x3C9D6D6D,
        rdr2_mirror = 0xF551D60F,
        rdr2_glow = 0xE310BD52,
        rdr2_cati = 0x27F82C88,
        rdr2_bump_spec_ambocc_smooth_shared = 0x34454DEE,
        rdr2_bump_spec_ambocc_reflection_shared = 0x23DFA9FB,
        rdr2_pond_water = 0x6B8805B0,
        rdr2_glass_notint_nodistortion = 0xB3B832DC,
        rdr2_traintrack = 0xB5FDEE16,
        rdr2_traintrack_low_lod = 0x31EF2DCB,
        rdr2_lod4_water = 0x1D66C5F4
    }
    #endregion
}
