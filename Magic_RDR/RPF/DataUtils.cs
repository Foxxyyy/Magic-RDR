using ICSharpCode.SharpZipLib.Zip.Compression;
using Magic_RDR.Application;
using Magic_RDR.RPF;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Zstandard.Net;

namespace Magic_RDR
{
    public static class DataUtils
    {
        public static byte[] Decrypt(byte[] dataIn, byte[] key)
        {
            byte[] numArray = new byte[dataIn.Length];
            dataIn.CopyTo(numArray, 0);

            if (numArray == null)
            {
                return null;
            }

            int inputCount = numArray.Length & -16;
            if (inputCount > 0)
            {
                Rijndael rijndael = Rijndael.Create();
                rijndael.BlockSize = 128;
                rijndael.KeySize = 256;
                rijndael.Mode = CipherMode.ECB;
                rijndael.Key = key;
                rijndael.IV = new byte[16];
                rijndael.Padding = PaddingMode.None;
                ICryptoTransform decryptor = rijndael.CreateDecryptor();

                for (int index = 0; index < 16; ++index)
                {
                    decryptor.TransformBlock(numArray, 0, inputCount, numArray, 0);
                }
            }
            return numArray;
        }

        public static byte[] Encrypt(byte[] dataIn, byte[] key)
        {
            byte[] numArray = new byte[dataIn.Length];
            dataIn.CopyTo(numArray, 0);

            if (numArray == null)
            {
                return null;
            }

            int inputCount = numArray.Length & -16;
            if (inputCount > 0)
            {
                Rijndael rijndael = Rijndael.Create();
                rijndael.BlockSize = 128;
                rijndael.KeySize = 256;
                rijndael.Mode = CipherMode.ECB;
                rijndael.Key = key;
                rijndael.IV = new byte[16];
                rijndael.Padding = PaddingMode.None;
                ICryptoTransform encryptor = rijndael.CreateEncryptor();

                for (int index = 0; index < 16; ++index)
                {
                    encryptor.TransformBlock(numArray, 0, inputCount, numArray, 0);
                }
            }
            return numArray;
        }

        public static string FormatHexHash(uint hash)
        {
            return $"0x{hash:X8}";
        }

        public static uint GetHash(string str)
        {
            char[] charArray = str.ToLower().ToCharArray();
            uint num1, num2;

            for (num2 = num1 = 0U; num1 < charArray.Length; ++num1)
            {
                uint num3 = num2 + charArray[(int)num1];
                uint num4 = num3 + (num3 << 10);
                num2 = num4 ^ num4 >> 6;
            }
            uint num5 = num2 + (num2 << 3);
            uint num6 = num5 ^ num5 >> 11;
            return num6 + (num6 << 15);
        }

        public static byte[] DecompressDeflate(byte[] data, int decompSize, bool noHeader = true)
        {
            byte[] buffer = new byte[decompSize];
            Inflater inflater = new Inflater(noHeader);
            inflater.SetInput(data);
            inflater.Inflate(buffer);
            return buffer;
        }

        public static byte[] Compress(byte[] input, int level, bool noHeader = true)
        {
            byte[] output = new byte[input.Length];
            Deflater deflater = new Deflater(level, noHeader);
            byte[] numArray;

            try
            {
                deflater.SetInput(input, 0, input.Length);
                deflater.Finish();
                numArray = new byte[deflater.Deflate(output)];
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Array.Copy(output, 0, numArray, 0, numArray.Length);
            return numArray;
        }

        public static byte[] DecompressZStandard(byte[] compressedData)
        {
            byte[] decompressedData = null;

            using (var memoryStream = new MemoryStream(compressedData))
            using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Decompress))
            using (var temp = new MemoryStream())
            {
                compressionStream.CopyTo(temp);
                decompressedData = temp.ToArray();
            }
            return decompressedData;
        }

        public static byte[] CompressZStandard(byte[] decompressedData)
        {
            byte[] compressedData = null;

            using (var memoryStream = new MemoryStream())
            using (var compressionStream = new ZstandardStream(memoryStream, CompressionMode.Compress))
            {
                compressionStream.Write(decompressedData, 0, decompressedData.Length);
                compressionStream.Close();
                compressedData = memoryStream.ToArray();
            }
            return compressedData;
        }

        [HandleProcessCorruptedStateExceptions]
        public static int DecompressLZX(byte[] compressedData, out byte[] decompressedData, ref int dLen, uint windowSize = 131072)
        {
            int pContext = 0;
            decompressedData = null;

            if (xCompress.XMemCreateDecompressionContext(xCompress.XMEMCODEC_TYPE.XMEMCODEC_LZX, 0, 0, ref pContext) != 0)
                throw new Exception("LZX Error:  CREATE_DECOMPRESSION_CONTEXT");

            xCompress.XMemResetDecompressionContext(pContext);
            byte[] pDestination = new byte[dLen];
            int length = compressedData.Length;
            int num = xCompress.XMemDecompressStream(pContext, pDestination, ref dLen, compressedData, ref length);
            decompressedData = pDestination;
            xCompress.XMemDestroyDecompressionContext(pContext);
            return num;
        }

        public static byte[] CompressLZX(byte[] decompressedData, uint windowSize = 131072)
        {
            int pContext = 0;
            xCompress.XMEMCODEC_PARAMETERS_LZX prams;

            prams.Flags = 0U;
            prams.CompressionPartitionSize = 0U;
            prams.WindowSize = windowSize;

            if (xCompress.XMemCreateCompressionContext(xCompress.XMEMCODEC_TYPE.XMEMCODEC_LZX, ref prams, 0, ref pContext) == 0)
                xCompress.XMemResetCompressionContext(pContext);
            else
                throw new Exception("LZX Error: CREATE_COMPRESSION_CONTEXT");

            byte[] array = new byte[decompressedData.Length * 2];
            int length1 = array.Length;
            int length2 = decompressedData.Length;

            xCompress.XMemCompress(pContext, array, ref length1, decompressedData, length2);
            Array.Resize<byte>(ref array, length1);
            xCompress.XMemDestroyCompressionContext(pContext);
            return array;
        }

        public static int SetBit(int val, int bit, bool trueORfalse)
        {
            bool flag = (uint)(val & 1 << bit) > 0U;
            if (trueORfalse)
            {
                if (!flag)
                    return val |= 1 << bit;
            }
            else if (flag)
            {
                return val ^ 1 << bit;
            }
            return val;
        }

        public static int IntFromUInt(uint x) => (int)x;

        public static uint UIntFromInt(int x) => (uint)x;

        public static int NumLeftTill(int current, int roundTo)
        {
            int num = Math.Abs(roundTo - current % roundTo);
            return num == roundTo ? 0 : num;
        }

        public static long RoundUp(long num, long multiple)
        {
            if (multiple == 0L)
            {
                return 0;
            }
            long num1 = multiple / Math.Abs(multiple);
            long num2 = (num + multiple - num1) / multiple * multiple;
            return (num + multiple - num1) / multiple * multiple;
        }

        public static int TrailingZeroes(int n)
        {
            int num1 = 1;
            int num2 = 0;

            while (num2 < 32)
            {
                if ((uint)(n & num1) > 0U)
                {
                    return num2;
                }
                ++num2;
                num1 <<= 1;
            }
            return 32;
        }

        public static int LeadingZeros(int x)
        {
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x -= x >> 1 & 1431655765;
            x = (x >> 2 & 858993459) + (x & 858993459);
            x = (x >> 4) + x & 252645135;
            x += x >> 8;
            x += x >> 16;
            return 32 - (x & 63);
        }

        public static int GenerateMask32(int maskBegin, int maskEnd)
        {
            int num = 0;
            for (int index = maskBegin; index <= maskEnd; ++index)
            {
                num |= 1 << index;
            }
            return num;
        }

        private static uint GetDec3NFromVector3(Vector3 vector)
        {
            bool negX = (vector.X < 0.0f);
            bool negY = (vector.Y < 0.0f);
            bool negZ = (vector.Z < 0.0f);

            uint x = Math.Min((uint)(Math.Abs(vector.X) * 511.0f), 511);
            uint y = Math.Min((uint)(Math.Abs(vector.Y) * 511.0f), 511);
            uint z = Math.Min((uint)(Math.Abs(vector.Z) * 511.0f), 511);

            long ux = ((negX ? ~x : x) & 0x1FF) + (negX ? 0x200 : 0);
            long uy = ((negY ? ~y : y) & 0x1FF) + (negY ? 0x200 : 0);
            long uz = ((negZ ? ~z : z) & 0x1FF) + (negZ ? 0x200 : 0);

            return (uint)(ux + (uy << 10) + (uz << 20) + (0u << 30));
        }

        public static Vector3 GetVector3FromDec3N(uint value)
        {
            uint ux = (value >> 0) & 0x3FF;
            uint uy = (value >> 10) & 0x3FF;
            uint uz = (value >> 20) & 0x3FF;
            uint uw = (value >> 30) & 0x3; //Used for tangents

            bool posX = (ux & 0x200) > 0;
            bool posY = (uy & 0x200) > 0;
            bool posZ = (uz & 0x200) > 0;

            var x = ((posX ? ~ux : ux) & 0x1FF) / (posX ? -511.0f : 511.0f);
            var y = ((posY ? ~uy : uy) & 0x1FF) / (posY ? -511.0f : 511.0f);
            var z = ((posZ ? ~uz : uz) & 0x1FF) / (posZ ? -511.0f : 511.0f);
            return new Vector3(x, y, z);
        }

        public static float GetHalfFloat(byte firstByte, byte secondByte)
        {
            var intVal = BitConverter.ToInt32(new byte[] { secondByte, firstByte, 0, 0 }, 0);

            int mant = intVal & 0x03FF;
            int exp = intVal & 0x7C00;
            if (exp == 0x7C00) exp = 0x3FC00;
            else if (exp != 0)
            {
                exp += 0x1C000;
                if (mant == 0 && exp > 0x1C400)
                    return BitConverter.ToSingle(BitConverter.GetBytes((intVal & 0x8000) << 16 | exp << 13 | 0x3FF), 0);
            }
            else if (mant != 0)
            {
                exp = 0x1c400;
                do
                {
                    mant <<= 1;
                    exp -= 0x400;
                } while ((mant & 0x400) == 0);
                mant &= 0x3FF;
            }
            return BitConverter.ToSingle(BitConverter.GetBytes((intVal & 0x8000) << 16 | (exp | mant) << 13), 0);
        }

        public static string ConvertToSingle(byte[] array)
        {
            float value = BitConverter.ToSingle(array, 0);
            return value.ToString() + (Math.Round(value) == value ? ".0f" : "f");
        }

        public static string Represent(long value, Stack.DataType type)
        {
            switch (type)
            {
                case Stack.DataType.Float:
                    return ConvertToSingle(BitConverter.GetBytes(value));
                case Stack.DataType.Bool:
                    return value == 0 ? "false" : "true"; //Still need to fix bools
                case Stack.DataType.FloatPtr:
                case Stack.DataType.IntPtr:
                case Stack.DataType.StringPtr:
                case Stack.DataType.UnkPtr:
                    return "NULL";
            }

            if (value > int.MaxValue && value <= int.MaxValue)
                return ((int)(uint)value).ToString();
            return value.ToString();
        }

        public static bool IntParse(string temp, out int value)
        {
            if (temp.Contains("/*") && temp.Contains("*/"))
            {
                int index = temp.IndexOf("/*");
                int index2 = temp.IndexOf("*/", index + 1);
                if (index2 == -1)
                {
                    value = -1;
                    return false;
                }
                temp = temp.Substring(0, index) + temp.Substring(index2 + 2);
            }

            if (temp.StartsWith("joaat(\""))
            {
                temp = temp.Remove(temp.Length - 2).Substring(7);
                uint val = GetHash(temp);
                value = unchecked((int)val);
                return true;
            }
            return int.TryParse(temp, out value);
        }

        public static float SwapEndian(float num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToSingle(data, 0);
        }

        public static Vector3 SwapEndian(Vector3 num)
        {
            return new Vector3(SwapEndian(num.X), SwapEndian(num.Y), SwapEndian(num.Z));
        }

        public static Vector4 SwapEndian(Vector4 num)
        {
            return new Vector4(SwapEndian(num.X), SwapEndian(num.Y), SwapEndian(num.Z), SwapEndian(num.W));
        }

        public static uint SwapEndian(uint num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public static int SwapEndian(int num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public static ulong SwapEndian(ulong num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public static long SwapEndian(long num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public static ushort SwapEndian(ushort num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public static short SwapEndian(short num)
        {
            byte[] data = BitConverter.GetBytes(num);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public static int[] VertexSizeMapping = { 2, 4, 6, 8, 4, 8, 12, 16, 4, 4, 4, 0, 0, 0, 4, 8 };

        public static uint ReverseBytes(uint value) => (uint)(((int)value & byte.MaxValue) << 24 | ((int)value & 65280) << 8) | (value & 16711680U) >> 8 | (value & 4278190080U) >> 24;
    }
}

