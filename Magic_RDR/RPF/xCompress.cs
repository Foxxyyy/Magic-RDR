using System.Runtime.InteropServices;

namespace Magic_RDR.RPF
{
    public static class xCompress
    {
        public const int XMEMCOMPRESS_STREAM = 1;

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemCreateDecompressionContext(XMEMCODEC_TYPE CodecType, int pCodecParams, int Flags, ref int pContext);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern void XMemDestroyDecompressionContext(int Context);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemResetDecompressionContext(int Context);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemDecompress(int Context, byte[] pDestination, ref int pDestSize, byte[] pSource, int pSrcSize);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemDecompressStream(int Context, byte[] pDestination, ref int pDestSize, byte[] pSource, ref int pSrcSize);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemCreateCompressionContext(XMEMCODEC_TYPE CodecType, ref XMEMCODEC_PARAMETERS_LZX prams, int Flags, ref int pContext);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern void XMemDestroyCompressionContext(int Context);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemResetCompressionContext(int Context);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemCompress(int Context, byte[] pDestination, ref int pDestSize, byte[] pSource, int pSrcSize);

        [DllImport("Assemblies/xcompress32.dll")]
        public static extern int XMemCompressStream(int Context, byte[] pDestination, ref int pDestSize, byte[] pSource, ref int pSrcSize);

        public enum XMEMCODEC_TYPE
        {
            XMEMCODEC_DEFAULT,
            XMEMCODEC_LZX,
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct XMEMCODEC_PARAMETERS_LZX
        {
            [FieldOffset(0)]
            public uint Flags;
            [FieldOffset(4)]
            public uint WindowSize;
            [FieldOffset(8)]
            public uint CompressionPartitionSize;
        }
    }
}