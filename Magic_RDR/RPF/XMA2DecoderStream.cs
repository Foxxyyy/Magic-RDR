using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Magic_RDR.RPF
{
    class MP3DecoderStream : Stream
    {
        public Stream _stream;
        private IntPtr _state;

        delegate int ReadDelegate(IntPtr destPtr, int offset, int count);

        ReadDelegate readFunc;

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr mp3_dec_init(ReadDelegate readFunc, int bits);

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int mp3_dec_read(IntPtr ctx, byte[] output, int output_offset, int output_size);

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mp3_dec_free(IntPtr State);


        private int UnsafeRead(IntPtr destPtr, int offset, int count)
        {
            byte[] dest = new byte[count];
            int res = mp3_dec_read(_state, dest, offset, count);
            Marshal.Copy(dest, 0, destPtr, res);
            return res;
        }

        public MP3DecoderStream(Stream stream)
        {
            _stream = stream;
            readFunc = UnsafeRead;
            _state = mp3_dec_init(readFunc, 16);

            if (_state == IntPtr.Zero)
            {
                throw new Exception("XMemCreateDecompressionContext failed");
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    class XMA2DecoderStream : Stream
    {
        public Stream _stream;
        private IntPtr _ctx;

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr xma2_dec_init(int sample_rate, int channels, int bits);

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int xma2_dec_prepare_packet(IntPtr ctx, byte[] input, int input_size);

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int xma2_dec_read(IntPtr ctx, byte[] output, int output_offset, int output_size);

        [DllImport(@"Assemblies/libav_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void xma2_dec_free(IntPtr State);

        public XMA2DecoderStream(Stream stream)
        {
            _stream = stream;
            if (!_stream.CanRead)
                throw new ArgumentException("Stream not readable", "stream");

            // need seeking for eof checking
            if (!_stream.CanSeek)
                throw new ArgumentException("Stream not seekable", "stream");

            _ctx = xma2_dec_init(32000, 1, 16);
            if (_ctx == IntPtr.Zero)
            {
                throw new Exception("XMemCreateDecompressionContext failed");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset", "Need non-negitive number");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Need non-negitive number");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Invalid offset and length");

            int originalOffset = offset;

            while (count > 0)
            {
                int read = xma2_dec_read(_ctx, buffer, offset, count);

                if (read < 0)
                {
                    throw new Exception("xma2_dec_read failed");
                }
                offset += read;
                count -= read;

                if (read == 0)
                {
                    // Read one packet
                    byte[] packet = new byte[0x800];

                    if (_stream.Read(packet, 0, packet.Length) != packet.Length)
                    {
                        // EOF, failed to read whole packet
                        break;
                    }

                    if (xma2_dec_prepare_packet(_ctx, packet, packet.Length) < 0)
                    {
                        throw new Exception("xma2_dec_prepare_packet failed");
                    }
                }
            }

            return offset - originalOffset;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Close();
            }
            if (_ctx != IntPtr.Zero)
            {
                xma2_dec_free(_ctx);
                _ctx = IntPtr.Zero;
            }
            _stream = null;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position { get => _stream.Position; set => _stream.Position = value; }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}
