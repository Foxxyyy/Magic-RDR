using Magic_RDR.Application;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Magic_RDR.RPF.Texture.DDS;

namespace Magic_RDR.RPF
{
    public class Texture
    {
        public static int Height { get; set; }
        public static int Width { get; set; }
        public static int VirtualDimension1 { get; set; }
        public static int VirtualDimension2 { get; set; }
        public static int CurrentLevel { get; set; }
        public static TextureType CurrentFormat { get; set; }

        public enum TextureType
        {
            L8 = 2,
            DXT1 = 82, // 0x00000052
            DXT3 = 83, // 0x00000053
            DXT5 = 84, // 0x00000054
            A8R8G8B8 = 134, // 0x00000086
        }

        public static int GetDimension(int size, int level)
        {
            return size / (int)Math.Pow(2.0, level);
        }

        public static int GetVirtualSize(int size)
        {
            if (size % 128 != 0)
            {
                size += 128 - size % 128;
            }
            return size;
        }

        public static int GetVirtualDimension(int size, int level)
        {
            return GetVirtualSize(GetDimension(size, level));
        }

        public static int GetTextureDataSize(int levelUsed, int heigth, int width, bool UseVirtualSizes, int mipmaps, TextureType format)
        {
            int num1 = GetDimension(width, levelUsed);
            int num2 = GetDimension(heigth, levelUsed);
            int num3;

            if (UseVirtualSizes && AppGlobals.Platform == AppGlobals.PlatformEnum.Xbox)
            {
                num1 = GetVirtualDimension(width, levelUsed);
                num2 = GetVirtualDimension(heigth, levelUsed);
            }

            switch (format)
            {
                case TextureType.DXT1:
                    num3 = num1 * num2 / 2;
                    break;
                case TextureType.L8:
                case TextureType.A8R8G8B8:
                case TextureType.DXT3:
                case TextureType.DXT5:
                    num3 = num1 * num2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return num3;
        }

        public static byte[] GetTextureData(IOReader reader, int level, TextureInfo tex)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            byte[] data = null;
            int offset = 0, totalSize = 0;
            int levelSize = GetTextureDataSize(level, Height, Width, true, tex.MipMaps, tex.PixelFormat);

            if (level == 0)
                data = reader.ReadBytes(GetTextureDataSize(level, Height, Width, true, tex.MipMaps, tex.PixelFormat));
            else
            {
                for (int i = 1; i < tex.MipMaps; i++)
                {
                    totalSize += GetTextureDataSize(i, Height, Width, true, tex.MipMaps, tex.PixelFormat);
                }

                for (int i = isSwitchVersion ? 0 : 1; i < level; i++)
                {
                    offset += GetTextureDataSize(i, Height, Width, true, tex.MipMaps, tex.PixelFormat);
                }

                if (isSwitchVersion)
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Current);
                    data = reader.ReadBytes(levelSize);
                }
                else data = reader.ReadBytes(totalSize);
            }

            byte[] textureData = new byte[levelSize];
            Array.Copy(data, (level < 2 || isSwitchVersion) ? 0 : offset, textureData, 0, levelSize);
            //File.WriteAllBytes(@"C:\Users\fumol\OneDrive\Bureau\test.crn", textureData);
            return textureData;
        }

        public static byte[] ReadTextureInfo(IOReader reader, int level, TextureInfo tex)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            VirtualDimension1 = isSwitchVersion ? GetDimension(tex.Height, level) : GetVirtualDimension(tex.Height, level);
            VirtualDimension2 = isSwitchVersion ? GetDimension(tex.Width, level) : GetVirtualDimension(tex.Width, level);
            Height = tex.Height;
            Width = tex.Width;
            CurrentLevel = level;
            CurrentFormat = tex.PixelFormat;

            if (level == 0 || isSwitchVersion)
                reader.BaseStream.Position = tex.TextureDataPointer; //Seek to 1st mip data offset
            else
                reader.BaseStream.Position = tex.MipDataPointer; //Seek to 2nd mip data offset

            byte[] data = GetTextureData(reader, level, tex);
            if (tex.PixelFormat == TextureType.A8R8G8B8)
            {
                return data;
            }

            if (!isSwitchVersion)
            {
                data = ConvertToLinearTexture(data, VirtualDimension2, VirtualDimension1); //Xbox 360 swizzling
            }

            switch (tex.PixelFormat)
            {
                case TextureType.DXT1:
                    data = DecodeDXT1(data, VirtualDimension2, VirtualDimension1);
                    break;
                case TextureType.DXT3:
                    data = DecodeDXT3(data, VirtualDimension2, VirtualDimension1);
                    break;
                case TextureType.DXT5:
                    data = DecodeDXT5(data, VirtualDimension2, VirtualDimension1);
                    break;
                case TextureType.L8:
                    byte[] numArray = new byte[data.Length * 4];
                    for (int index = 0; index < data.Length; ++index)
                    {
                        numArray[index * 4] = data[index];
                        numArray[index * 4 + 1] = data[index];
                        numArray[index * 4 + 2] = data[index];
                        numArray[index * 4 + 3] = byte.MaxValue;
                    }
                    data = numArray;
                    break;
            }
            return data;
        }

        public static BitmapSource LoadImage(byte[] imageData)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            int RealHeight = isSwitchVersion ? GetDimension(Height, CurrentLevel) : GetVirtualDimension(Height, CurrentLevel);
            int RealWidth = isSwitchVersion ? GetDimension(Width, CurrentLevel) : GetVirtualDimension(Width, CurrentLevel);

            int width = RealWidth; int height = RealHeight;
            if ((GetDimension(Height, CurrentLevel) % 128 != 0 || GetDimension(Width, CurrentLevel) % 128 != 0) && !isSwitchVersion)
            {
                height = GetDimension(Height, CurrentLevel);
                width = GetDimension(Width, CurrentLevel);
            }

            BitmapSource source;
            if (CurrentLevel > 0)
                source = BitmapSource.Create(width, height, 96D, 96D, PixelFormats.Bgra32, null, imageData, GetCorrectStride(VirtualDimension2));
            else
                source = BitmapSource.Create(Width, Height, 96D, 96D, PixelFormats.Bgra32, null, imageData, GetCorrectStride(RealWidth));
            return source;
        }

        public static int GetCorrectStride(int width)
        {
            int stride = width * 4;
            var mod = stride % 4;

            if (mod != 0)
            {
                stride += 4 - mod;
            }
            return stride;
        }

        public static byte[] BufferFromImageSource(ImageSource imageSource)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imageSource));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        public static BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            BitmapSource bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Bgr24, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static Image[] GenerateMipMaps(Image img, int levels)
        {
            var imgs = new Image[levels];
            imgs[0] = img;

            for (int i = 1; i < levels; i++)
            {
                int w = img.Width / (int)Math.Pow(2, i);
                int h = img.Height / (int)Math.Pow(2, i);
                imgs[i] = CreateMipMapImage(img, new Size(w, h));
            }
            return imgs;
        }

        private static Image CreateMipMapImage(Image image, Size size)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = (size.Width / (float)sourceWidth);
            nPercentH = (size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap bmp = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(image, 0, 0, destWidth, destHeight);
            g.Dispose();
            return bmp;
        }

        public static void SaveDDS(IOReader er, string filePath, TextureInfo tex)
        {
            string newPath = filePath;
            if (newPath.Contains("memory")) //Some textures contains invalid symbols
            {
                int nameIndex = newPath.LastIndexOf('\\') + 1;
                int validNameIndex = newPath.LastIndexOf(':') + 1;
                string texturename = newPath.Substring(nameIndex, newPath.Length - nameIndex - 4);
                string validTextureName = newPath.Substring(validNameIndex, newPath.Length - validNameIndex - 4);
                newPath = newPath.Replace(texturename, validTextureName);
            }

            BinaryWriter bw = new BinaryWriter(new FileStream(newPath, FileMode.Create, FileAccess.Write));
            new DDS(tex.Height, tex.Width, tex.MipMaps, tex.PixelFormat).Write(bw);

            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            for (int index = 0; index < (isSwitchVersion ? tex.MipMaps : 1); ++index)
            {
                er.BaseStream.Position = tex.TextureDataPointer;
                int virtualDimension1 = isSwitchVersion ? GetDimension(tex.Height, index) : GetVirtualDimension(tex.Height, index);
                int virtualDimension2 = isSwitchVersion ? GetDimension(tex.Width, index) : GetVirtualDimension(tex.Width, index);

                //Make sure the global width and height are correct
                Height = tex.Height;
                Width = tex.Width;

                byte[] buffer = GetTextureData(er, index, tex);

                if (!isSwitchVersion)
                {
                    buffer = StripPadding(ConvertToLinearTexture(buffer, virtualDimension2, virtualDimension1), index, tex);
                }
                if (tex.PixelFormat == TextureType.L8)
                {
                    byte[] numArray = new byte[buffer.Length * 4];
                    for (int i = 0; i < buffer.Length; ++i)
                    {
                        numArray[i * 4] = buffer[i];
                        numArray[i * 4 + 1] = buffer[i];
                        numArray[i * 4 + 2] = buffer[i];
                        numArray[i * 4 + 3] = byte.MaxValue;
                    }
                    buffer = numArray;
                }

                if (tex.PixelFormat != TextureType.A8R8G8B8 && !isSwitchVersion)
                {
                    for (int index2 = 0; index2 < buffer.Length; index2 += 2)
                        Array.Reverse(buffer, index2, 2);
                }
                bw.Write(buffer);
            }
            bw.Close();
        }

        public static (DDS, TextureInfo) InjectDDS(IOReader reader, string FilePath, TextureInfo t, IOWriter writer = null)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            BinaryReader br = new BinaryReader(new FileStream(FilePath, FileMode.Open, FileAccess.Read));

            DDS dds = new DDS();
            dds.Read(br);

            if ((dds.dwFlags2 & DDS.DDPF.FOURCC) != DDS.DDPF.FOURCC)
                throw new Exception("Missing FOURCC compressed texture");
            if ((t.PixelFormat == TextureType.DXT1 && dds.dwFourCC != DDS.FourCC.DXT1) || (t.PixelFormat == TextureType.DXT3 && dds.dwFourCC != DDS.FourCC.DXT3) || (t.PixelFormat == TextureType.DXT5 && dds.dwFourCC != DDS.FourCC.DXT5))
                throw new Exception(string.Format("Wrong DXT format !\n\nExpected: {0}\nDetected: {1}", t.PixelFormat, dds.dwFourCC));

            bool modifyDimensions = false;
            if (dds.dwWidth != t.Width || dds.dwHeight != t.Height)
                throw new Exception(string.Format("Incorrect image dimensions !\n\nDetected: {0}x{1}\nExpected: {2}x{3}", dds.dwWidth, dds.dwHeight, t.Width, t.Height));
            else if ((dds.dwWidth > t.Width || dds.dwHeight > t.Height) && isSwitchVersion)
            {
                //TODO: remove this
                throw new Exception(string.Format("Incorrect image dimensions !\n\nDetected: {0}x{1}\nExpected: {2}x{3}", dds.dwWidth, dds.dwHeight, t.Width, t.Height));

                if (writer == null) //swfTexture
                    throw new Exception(string.Format("Incorrect image dimensions !\n\nDetected: {0}x{1}\nExpected: {2}x{3}", dds.dwWidth, dds.dwHeight, t.Width, t.Height));
                modifyDimensions = true;
            }

            int num = 1;
            if ((dds.dwFlags & DDS.DDSD.MIPMAPCOUNT) == DDS.DDSD.MIPMAPCOUNT && t.MipMaps > 1)
                num = Math.Min(dds.dwMipMapCount, t.MipMaps);

            if (modifyDimensions)
            {
                dds.dwMipMapCount -= 2;
                num = dds.dwMipMapCount;
            }

            var temp = CurrentFormat;
            CurrentFormat = t.PixelFormat;

            //Set up how we're going to write data
            if (modifyDimensions)
                writer.Seek(t.TextureDataPointer, SeekOrigin.Begin);
            else
                reader.Seek(t.TextureDataPointer, SeekOrigin.Begin);

            for (int i = 0; i < num; i++)
            {
                //Xbox 360
                if (!isSwitchVersion)
                {
                    if (i == 1 && t.MipDataPointer != 0)
                        reader.Seek(t.MipDataPointer, SeekOrigin.Begin);
                    if (GetDimension(t.Height, i) <= 16 || GetDimension(t.Width, i) <= 16)
                        continue;
                }

                int virtualDimension1 = isSwitchVersion ? GetDimension(t.Height, i) : GetVirtualDimension(t.Height, i);
                int virtualDimension2 = isSwitchVersion ? GetDimension(t.Width, i) : GetVirtualDimension(t.Width, i);

                //Get the DDS data
                byte[] data = null;
                if (modifyDimensions)
                    data = br.ReadBytes(GetTextureDataSize(i, dds.dwHeight, dds.dwWidth, false, dds.dwMipMapCount, t.PixelFormat));
                else
                    data = br.ReadBytes(GetTextureDataSize(i, t.Height, t.Width, false, t.MipMaps, t.PixelFormat));

                if (!isSwitchVersion)
                {
                    for (int index = 0; index < data.Length; index += 2)
                        Array.Reverse(data, index, 2);
                }

                byte[] buffer = data;
                if (!isSwitchVersion)
                {
                    buffer = AddPadding(data, i, t);
                    buffer = ConvertFromLinearTexture(buffer, virtualDimension2, virtualDimension1);
                }

                //Finally, write the data
                if (modifyDimensions)
                    writer.Write(buffer, 0, buffer.Length);
                else
                    reader.BaseStream.Write(buffer, 0, buffer.Length);
            }
            CurrentFormat = temp;
            return (dds, t);
        }

        public static byte[] StripPadding(byte[] data, int level, TextureInfo tex)
        {
            int dimension1 = GetDimension(Height, level);
            int dimension2 = GetDimension(Width, level);

            if (dimension1 >= 128 && dimension2 >= 128)
                return data;

            int virtualDimension = GetVirtualDimension(dimension2, level);
            int num = (tex.PixelFormat == TextureType.DXT1) ? 2 : 4;
            byte[] numArray = new byte[GetTextureDataSize(level, tex.Height, tex.Width, false, tex.MipMaps, tex.PixelFormat)];

            for (int index = 0; index < dimension1 / 4; ++index)
                Array.Copy(data, index * virtualDimension * num, numArray, index * dimension2 * num, dimension2 * num);

            return numArray;
        }

        public static byte[] AddPadding(byte[] data, int level, TextureInfo tex)
        {
            int dimension1 = GetDimension(tex.Height, level);
            int dimension2 = GetDimension(tex.Width, level);

            if (dimension1 >= 128 && dimension2 >= 128)
                return data;

            int virtualDimension = GetVirtualDimension(dimension2, level);
            int num = (tex.PixelFormat == TextureType.DXT1) ? 2 : 4;
            byte[] numArray = new byte[GetTextureDataSize(level, tex.Height, tex.Width, true, tex.MipMaps, tex.PixelFormat)];

            for (int index = 0; index < numArray.Length; ++index)
                numArray[index] = 205;

            for (int index = 0; index < dimension1 / 4; ++index)
                Array.Copy(data, index * dimension2 * num, numArray, index * virtualDimension * num, dimension2 * num);

            return numArray;
        }

        public static byte[] ConvertToLinearTexture(byte[] data, int width, int height)
        {
            return ModifyLinearTexture(data, width, height, true);
        }

        public static byte[] ConvertFromLinearTexture(byte[] data, int width, int height)
        {
            return ModifyLinearTexture(data, width, height, false);
        }

        private static byte[] ModifyLinearTexture(byte[] data, int width, int height, bool toLinear)
        {
            byte[] numArray = new byte[data.Length];
            int blockSizeRow, texelPitch;

            switch (CurrentFormat)
            {
                case TextureType.L8:
                    blockSizeRow = 2;
                    texelPitch = 1;
                    break;
                case TextureType.A8R8G8B8:
                    blockSizeRow = 8;
                    texelPitch = 4;
                    break;
                case TextureType.DXT1:
                    blockSizeRow = 4;
                    texelPitch = 8;
                    break;
                case TextureType.DXT3:
                case TextureType.DXT5:
                    blockSizeRow = 4;
                    texelPitch = 16;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Bad DXT Type!");
            }

            int blockWidth = width / blockSizeRow;
            int blockHeight = height / blockSizeRow;

            try
            {
                for (int j = 0; j < blockHeight; j++)
                {
                    for (int i = 0; i < blockWidth; i++)
                    {
                        int blockOffset = j * blockWidth + i;

                        int x = XGAddress2DTiledX(blockOffset, blockWidth, texelPitch);
                        int y = XGAddress2DTiledY(blockOffset, blockWidth, texelPitch);

                        int srcOffset = j * blockWidth * texelPitch + i * texelPitch;
                        int destOffset = y * blockWidth * texelPitch + x * texelPitch;

                        if (toLinear)
                            Array.Copy(data, srcOffset, numArray, destOffset, texelPitch);
                        else
                            Array.Copy(data, destOffset, numArray, srcOffset, texelPitch);
                    }
                }
            }
            catch { }
            return numArray;
        }

        internal static int XGAddress2DTiledX(int Offset, int Width, int TexelPitch)
        {
            int num1 = Width + 31 & -32;
            int num2 = (TexelPitch >> 2) + (TexelPitch >> 1 >> (TexelPitch >> 2));
            int num3 = Offset << num2;
            int num4 = ((num3 & -4096) >> 3) + ((num3 & 1792) >> 2) + (num3 & 63);
            return (((num4 >> 7 + num2) % (num1 >> 5) << 2) + ((num4 >> 5 + num2 & 2) + (num3 >> 6) & 3) << 3) + (((num4 >> 1 & -16) + (num4 & 15) & (TexelPitch << 3) - 1) >> num2);
        }

        internal static int XGAddress2DTiledY(int Offset, int Width, int TexelPitch)
        {
            int num1 = Width + 31 & -32;
            int num2 = (TexelPitch >> 2) + (TexelPitch >> 1 >> (TexelPitch >> 2));
            int num3 = Offset << num2;
            int num4 = ((num3 & -4096) >> 3) + ((num3 & 1792) >> 2) + (num3 & 63);
            return (((((num4 >> (7 + num2)) / (num1 >> 5)) << 2) + ((num4 >> (6 + num2)) & 1) + ((num3 & 2048) >> 10)) << 3) + ((num4 & ((TexelPitch << 6) - 1 & -32)) + ((num4 & 15) << 1) >> 3 + num2 & -2) + ((num4 & 16) >> 4);
        }

        #region DXT Decoder
        internal static byte[] DecodeDXT1(byte[] data, int width, int height)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            byte[] pixData = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;

            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 8;
                    uint color0 = 0, color1 = 0;

                    if (!isSwitchVersion)
                    {
                        color0 = ((uint)data[blockDataStart + 0] << 8) + data[blockDataStart + 1];
                        color1 = ((uint)data[blockDataStart + 2] << 8) + data[blockDataStart + 3];
                    }
                    else
                    {
                        color0 = BitConverter.ToUInt16(data, blockDataStart);
                        color1 = BitConverter.ToUInt16(data, blockDataStart + 2);
                    }

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 4);
                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;

                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j;
                        if (!isSwitchVersion)
                            j = k ^ 1; 
                        else
                            j = k;

                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            switch (codeDec)
                            {
                                case 0:
                                    pixData[pixDataStart + 0] = (byte)r0;
                                    pixData[pixDataStart + 1] = (byte)g0;
                                    pixData[pixDataStart + 2] = (byte)b0;
                                    pixData[pixDataStart + 3] = 255;
                                    break;
                                case 1:
                                    pixData[pixDataStart + 0] = (byte)r1;
                                    pixData[pixDataStart + 1] = (byte)g1;
                                    pixData[pixDataStart + 2] = (byte)b1;
                                    pixData[pixDataStart + 3] = 255;
                                    break;
                                case 2:
                                    pixData[pixDataStart + 3] = 255;
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        pixData[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        pixData[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                        pixData[pixDataStart + 3] = 255;
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = 0;
                                        pixData[pixDataStart + 1] = 0;
                                        pixData[pixDataStart + 2] = 0;
                                        pixData[pixDataStart + 3] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }
                }
            }
            return pixData;
        }

        internal static byte[] DecodeDXT3(byte[] data, int width, int height)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            byte[] numArray1 = new byte[width * height * 4];
            int num1 = width / 4;
            int num2 = height / 4;

            for (int index1 = 0; index1 < num2; ++index1)
            {
                for (int index2 = 0; index2 < num1; ++index2)
                {
                    int blockDataStart = (index1 * num1 + index2) * 16;
                    ushort[] alphaData = new ushort[4];

                    if (!isSwitchVersion)
                    {
                        alphaData[0] = (ushort)((data[blockDataStart + 0] << 8) + data[blockDataStart + 1]);
                        alphaData[1] = (ushort)((data[blockDataStart + 2] << 8) + data[blockDataStart + 3]);
                        alphaData[2] = (ushort)((data[blockDataStart + 4] << 8) + data[blockDataStart + 5]);
                        alphaData[3] = (ushort)((data[blockDataStart + 6] << 8) + data[blockDataStart + 7]);
                    }
                    else
                    {
                        alphaData[0] = BitConverter.ToUInt16(data, blockDataStart + 0);
                        alphaData[1] = BitConverter.ToUInt16(data, blockDataStart + 2);
                        alphaData[2] = BitConverter.ToUInt16(data, blockDataStart + 4);
                        alphaData[3] = BitConverter.ToUInt16(data, blockDataStart + 6);
                    }

                    byte[,] numArray3 = new byte[4, 4];
                    for (int index4 = 0; index4 < 4; ++index4)
                    {
                        for (int index5 = 0; index5 < 4; ++index5)
                        {
                            numArray3[index5, index4] = (byte)((alphaData[index4] & 15) * 16);
                            alphaData[index4] >>= 4;
                        }
                    }
                    ushort num3, num4;
                    if (!isSwitchVersion)
                    {
                        num3 = (ushort)(((uint)data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                        num4 = (ushort)(((uint)data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);
                    }
                    else
                    {
                        num3 = BitConverter.ToUInt16(data, blockDataStart + 8);
                        num4 = BitConverter.ToUInt16(data, blockDataStart + 8 + 2);
                    }

                    uint uint32 = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);
                    ushort num5 = (ushort)(8 * (num3 & 31));
                    ushort num6 = (ushort)(4 * (num3 >> 5 & 63));
                    ushort num7 = (ushort)(8 * (num3 >> 11 & 31));
                    ushort num8 = (ushort)(8 * (num4 & 31));
                    ushort num9 = (ushort)(4 * (num4 >> 5 & 63));
                    ushort num10 = (ushort)(8 * (num4 >> 11 & 31));

                    for (int index4 = 0; index4 < 4; ++index4)
                    {
                        int index5;
                        if (!isSwitchVersion)
                            index5 = index4 ^ 1;
                        else
                            index5 = index4;

                        for (int index6 = 0; index6 < 4; ++index6)
                        {
                            int index7 = width * (index1 * 4 + index5) * 4 + (index2 * 4 + index6) * 4;
                            uint num11 = uint32 & 3U;
                            numArray1[index7 + 3] = numArray3[index6, index5];

                            switch (num11)
                            {
                                case 0:
                                    numArray1[index7] = (byte)num5;
                                    numArray1[index7 + 1] = (byte)num6;
                                    numArray1[index7 + 2] = (byte)num7;
                                    break;
                                case 1:
                                    numArray1[index7] = (byte)num8;
                                    numArray1[index7 + 1] = (byte)num9;
                                    numArray1[index7 + 2] = (byte)num10;
                                    break;
                                case 2:
                                    if (num3 > num4)
                                    {
                                        numArray1[index7] = (byte)((2 * num5 + num8) / 3);
                                        numArray1[index7 + 1] = (byte)((2 * num6 + num9) / 3);
                                        numArray1[index7 + 2] = (byte)((2 * num7 + num10) / 3);
                                        break;
                                    }
                                    numArray1[index7] = (byte)((num5 + num8) / 2);
                                    numArray1[index7 + 1] = (byte)((num6 + num9) / 2);
                                    numArray1[index7 + 2] = (byte)((num7 + num10) / 2);
                                    break;
                                case 3:
                                    if (num3 > num4)
                                    {
                                        numArray1[index7] = (byte)((num5 + 2 * num8) / 3);
                                        numArray1[index7 + 1] = (byte)((num6 + 2 * num9) / 3);
                                        numArray1[index7 + 2] = (byte)((num7 + 2 * num10) / 3);
                                        break;
                                    }
                                    numArray1[index7] = 0;
                                    numArray1[index7 + 1] = 0;
                                    numArray1[index7 + 2] = 0;
                                    break;
                            }
                            uint32 >>= 2;
                        }
                    }
                }
            }
            return numArray1;
        }

        internal static byte[] DecodeDXT5(byte[] data, int width, int height)
        {
            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            byte[] pixData = new byte[width * height * 4];
            int xBlocks = width / 4;
            int yBlocks = height / 4;

            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    int blockDataStart = ((y * xBlocks) + x) * 16;
                    uint[] alphas = new uint[8];
                    ulong alphaMask = 0;

                    if (!isSwitchVersion)
                    {
                        alphas[0] = data[blockDataStart + 1];
                        alphas[1] = data[blockDataStart + 0];
                        alphaMask |= data[blockDataStart + 6];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 7];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 4];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 5];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 2];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 3];
                    }
                    else
                    {
                        alphas[0] = data[blockDataStart + 0];
                        alphas[1] = data[blockDataStart + 1];
                        alphaMask |= data[blockDataStart + 7];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 6];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 5];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 4];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 3];
                        alphaMask <<= 8;
                        alphaMask |= data[blockDataStart + 2];
                    }

                    // 8-alpha or 6-alpha block
                    if (alphas[0] > alphas[1])
                    {
                        // 8-alpha block: derive the other 6
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte)((6 * alphas[0] + 1 * alphas[1] + 3) / 7);    // bit code 010
                        alphas[3] = (byte)((5 * alphas[0] + 2 * alphas[1] + 3) / 7);    // bit code 011
                        alphas[4] = (byte)((4 * alphas[0] + 3 * alphas[1] + 3) / 7);    // bit code 100
                        alphas[5] = (byte)((3 * alphas[0] + 4 * alphas[1] + 3) / 7);    // bit code 101
                        alphas[6] = (byte)((2 * alphas[0] + 5 * alphas[1] + 3) / 7);    // bit code 110
                        alphas[7] = (byte)((1 * alphas[0] + 6 * alphas[1] + 3) / 7);    // bit code 111
                    }
                    else
                    {
                        // 6-alpha block.
                        // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                        alphas[2] = (byte)((4 * alphas[0] + 1 * alphas[1] + 2) / 5);    // Bit code 010
                        alphas[3] = (byte)((3 * alphas[0] + 2 * alphas[1] + 2) / 5);    // Bit code 011
                        alphas[4] = (byte)((2 * alphas[0] + 3 * alphas[1] + 2) / 5);    // Bit code 100
                        alphas[5] = (byte)((1 * alphas[0] + 4 * alphas[1] + 2) / 5);    // Bit code 101
                        alphas[6] = 0x00;                                               // Bit code 110
                        alphas[7] = 0xFF;                                               // Bit code 111
                    }

                    byte[,] alpha = new byte[4, 4];

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            alpha[j, i] = (byte)alphas[alphaMask & 7];
                            alphaMask >>= 3;
                        }
                    }

                    ushort color0, color1;
                    if (!isSwitchVersion)
                    {
                        color0 = (ushort)((data[blockDataStart + 8] << 8) + data[blockDataStart + 9]);
                        color1 = (ushort)((data[blockDataStart + 10] << 8) + data[blockDataStart + 11]);
                    }
                    else
                    {
                        color0 = BitConverter.ToUInt16(data, blockDataStart + 8);
                        color1 = BitConverter.ToUInt16(data, blockDataStart + 8 + 2);
                    }

                    uint code = BitConverter.ToUInt32(data, blockDataStart + 8 + 4);
                    ushort r0 = 0, g0 = 0, b0 = 0, r1 = 0, g1 = 0, b1 = 0;

                    r0 = (ushort)(8 * (color0 & 31));
                    g0 = (ushort)(4 * ((color0 >> 5) & 63));
                    b0 = (ushort)(8 * ((color0 >> 11) & 31));

                    r1 = (ushort)(8 * (color1 & 31));
                    g1 = (ushort)(4 * ((color1 >> 5) & 63));
                    b1 = (ushort)(8 * ((color1 >> 11) & 31));

                    for (int k = 0; k < 4; k++)
                    {
                        int j;
                        if (!isSwitchVersion)
                            j = k ^ 1;
                        else
                            j = k;

                        for (int i = 0; i < 4; i++)
                        {
                            int pixDataStart = (width * (y * 4 + j) * 4) + ((x * 4 + i) * 4);
                            uint codeDec = code & 0x3;

                            pixData[pixDataStart + 3] = alpha[i, j];

                            switch (codeDec)
                            {
                                case 0:
                                    pixData[pixDataStart + 0] = (byte)r0;
                                    pixData[pixDataStart + 1] = (byte)g0;
                                    pixData[pixDataStart + 2] = (byte)b0;
                                    break;
                                case 1:
                                    pixData[pixDataStart + 0] = (byte)r1;
                                    pixData[pixDataStart + 1] = (byte)g1;
                                    pixData[pixDataStart + 2] = (byte)b1;
                                    break;
                                case 2:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((2 * r0 + r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((2 * g0 + g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((2 * b0 + b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + r1) / 2);
                                        pixData[pixDataStart + 1] = (byte)((g0 + g1) / 2);
                                        pixData[pixDataStart + 2] = (byte)((b0 + b1) / 2);
                                    }
                                    break;
                                case 3:
                                    if (color0 > color1)
                                    {
                                        pixData[pixDataStart + 0] = (byte)((r0 + 2 * r1) / 3);
                                        pixData[pixDataStart + 1] = (byte)((g0 + 2 * g1) / 3);
                                        pixData[pixDataStart + 2] = (byte)((b0 + 2 * b1) / 3);
                                    }
                                    else
                                    {
                                        pixData[pixDataStart + 0] = 0;
                                        pixData[pixDataStart + 1] = 0;
                                        pixData[pixDataStart + 2] = 0;
                                    }
                                    break;
                            }

                            code >>= 2;
                        }
                    }
                }
            }
            return pixData;
        }

        #endregion

        #region Image Structure

        internal class ImageData
        {
            public int Size { set; get; }
            public int StartOffset { set; get; }
            public int Height { set; get; }
            public int Width { set; get; }
            public int MipMaps { set; get; }
            public TextureType Format { set; get; }
            public string TextureName { set; get; }
            public int MipDataOffset { set; get; }

            public ImageData(int size, int startOffset, int height, int width, int mipmaps, TextureType format, string textureName, int mipDataOffset = 0)
            {
                Size = size;
                StartOffset = startOffset;
                Height = height;
                Width = width;
                MipMaps = mipmaps;
                Format = format;
                TextureName = textureName;
                MipDataOffset = mipDataOffset;
            }
        }

        #endregion

        #region DDS Structure
        public struct DDS
        {
            public int dwMagic;
            public int dwSize;
            public DDSD dwFlags;
            public int dwHeight;
            public int dwWidth;
            public int dwPitchOrLinearSize;
            public int dwDepth;
            public int dwMipMapCount;
            public int[] dwReserved1;
            public int dwSize2;
            public DDPF dwFlags2;
            public FourCC dwFourCC;
            public int dwRGBBitCount;
            public int dwRBitMask;
            public int dwGBitMask;
            public int dwBBitMask;
            public uint dwRGBAlphaBitMask;
            public DDSCAPS dwCaps1;
            public DDSCAPS2 dwCaps2;
            public int[] Reserved2;
            public int dwReserved3;

            internal DDS(int heigth, int width, int mipmaps, TextureType format)
            {
                dwMagic = 542327876;
                dwSize = 124;
                dwFlags = 0;
                dwFlags |= DDSD.CAPS;
                dwFlags |= DDSD.HEIGHT;
                dwFlags |= DDSD.WIDTH;
                dwFlags |= DDSD.PIXELFORMAT;
                dwHeight = heigth;
                dwWidth = width;

                if (format == TextureType.A8R8G8B8)
                {
                    dwFlags |= DDSD.PITCH;
                    dwPitchOrLinearSize = width * 4;
                }
                else
                {
                    dwFlags |= DDSD.LINEARSIZE;
                    dwPitchOrLinearSize = GetTextureDataSize(0, heigth, width, false, mipmaps, format);
                }

                dwDepth = 0;
                dwFlags |= DDSD.MIPMAPCOUNT;
                dwMipMapCount = mipmaps;
                dwReserved1 = new int[11];
                dwSize2 = 32;
                dwFlags2 = 0;
                dwFourCC = FourCC.None;

                switch (format)
                {
                    case TextureType.DXT1:
                        dwFlags2 |= DDPF.FOURCC;
                        dwFourCC = FourCC.DXT1;
                        break;
                    case TextureType.DXT3:
                        dwFlags2 |= DDPF.FOURCC;
                        dwFourCC = FourCC.DXT3;
                        break;
                    case TextureType.DXT5:
                        dwFlags2 |= DDPF.FOURCC;
                        dwFourCC = FourCC.DXT5;
                        break;
                    case TextureType.L8:
                        dwFlags2 |= DDPF.FOURCC;
                        dwFourCC |= FourCC.L8;
                        break;
                    case TextureType.A8R8G8B8:
                        dwFlags2 |= DDPF.RGB;
                        dwFlags2 |= DDPF.ALPHAPIXELS;
                        break;     
                }
                if (format == TextureType.A8R8G8B8)
                {
                    dwRGBBitCount = 32;
                    dwRBitMask = 16711680;
                    dwGBitMask = 65280;
                    dwBBitMask = byte.MaxValue;
                    dwRGBAlphaBitMask = 4278190080U;
                }
                else
                {
                    dwRGBBitCount = 0;
                    dwRBitMask = 0;
                    dwGBitMask = 0;
                    dwBBitMask = 0;
                    dwRGBAlphaBitMask = 0U;
                }
                dwCaps1 = 0;
                dwCaps1 |= DDSCAPS.TEXTURE;
                if (mipmaps > 1)
                {
                    dwCaps1 |= DDSCAPS.COMPLEX;
                    dwCaps1 |= DDSCAPS.MIPMAP;
                }
                dwCaps2 = 0;
                Reserved2 = new int[2];
                dwReserved3 = 0;
            }

            public void Read(BinaryReader br)
            {
                dwMagic = br.ReadInt32(); //542327876
                dwSize = br.ReadInt32(); //124
                dwFlags = (DDSD)br.ReadInt32(); //CAPS, HEIGHT, WIDTH, PIXELFORMAT, LINEARSIZE
                dwHeight = br.ReadInt32();
                dwWidth = br.ReadInt32();
                dwPitchOrLinearSize = br.ReadInt32();
                dwDepth = br.ReadInt32(); //0
                dwMipMapCount = br.ReadInt32(); //0
                dwReserved1 = new int[11];

                for (int index = 0; index < dwReserved1.Length; ++index)
                    dwReserved1[index] = br.ReadInt32();

                dwSize2 = br.ReadInt32(); //32
                dwFlags2 = (DDPF)br.ReadInt32();
                dwFourCC = (FourCC)br.ReadInt32();
                dwRGBBitCount = br.ReadInt32();
                dwRBitMask = br.ReadInt32(); //0
                dwGBitMask = br.ReadInt32(); //0
                dwBBitMask = br.ReadInt32(); //0
                dwRGBAlphaBitMask = br.ReadUInt32(); //0
                dwCaps1 = (DDSCAPS)br.ReadInt32();
                dwCaps2 = (DDSCAPS2)br.ReadInt32();
                Reserved2 = new int[2];

                for (int index = 0; index < Reserved2.Length; ++index)
                    Reserved2[index] = br.ReadInt32();

                dwReserved3 = br.ReadInt32();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(dwMagic);
                bw.Write(dwSize);
                bw.Write((int)dwFlags);
                bw.Write(dwHeight);
                bw.Write(dwWidth);
                bw.Write(dwPitchOrLinearSize);
                bw.Write(dwDepth);
                bw.Write(dwMipMapCount);

                for (int index = 0; index < dwReserved1.Length; ++index)
                    bw.Write(dwReserved1[index]);

                bw.Write(dwSize2);
                bw.Write((int)dwFlags2);
                bw.Write((int)dwFourCC);
                bw.Write(dwRGBBitCount);
                bw.Write(dwRBitMask);
                bw.Write(dwGBitMask);
                bw.Write(dwBBitMask);
                bw.Write(dwRGBAlphaBitMask);
                bw.Write((int)dwCaps1);
                bw.Write((int)dwCaps2);
                for (int index = 0; index < Reserved2.Length; ++index)
                    bw.Write(Reserved2[index]);
                bw.Write(dwReserved3);
            }

            public enum FourCC
            {
                None = 0,
                L8 = 65, // 0x00000041
                DXT1 = 827611204, // 0x31545844
                DXT3 = 861165636, // 0x33545844
                DXT5 = 894720068, // 0x35545844
            }

            [Flags]
            public enum DDSD
            {
                CAPS = 1,
                HEIGHT = 2,
                WIDTH = 4,
                PITCH = 8,
                PIXELFORMAT = 4096, // 0x00001000
                MIPMAPCOUNT = 131072, // 0x00020000
                LINEARSIZE = 524288, // 0x00080000
                DEPTH = 8388608, // 0x00800000
            }

            [Flags]
            public enum DDPF
            {
                ALPHAPIXELS = 1,
                FOURCC = 4,
                RGB = 64, // 0x00000040
            }

            [Flags]
            public enum DDSCAPS
            {
                COMPLEX = 8,
                TEXTURE = 4096, // 0x00001000
                MIPMAP = 4194304, // 0x00400000
            }

            [Flags]
            public enum DDSCAPS2
            {
                CUBEMAP = 512, // 0x00000200
                CUBEMAP_POSITIVEX = 1024, // 0x00000400
                CUBEMAP_NEGATIVEX = 2048, // 0x00000800
                CUBEMAP_POSITIVEY = 4096, // 0x00001000
                CUBEMAP_NEGATIVEY = 8192, // 0x00002000
                CUBEMAP_POSITIVEZ = 16384, // 0x00004000
                CUBEMAP_NEGATIVEZ = 32768, // 0x00008000
                VOLUME = 2097152, // 0x00200000
            }
        }

        #endregion

        #region TextureInfo

        public class TextureInfo
        {
            public string TextureName;
            public int Width;
            public int Height;
            public int MipMaps;
            public uint TextureSize;
            public int TextureDataPointer;
            public int MipDataPointer;
            public TextureType PixelFormat;
        }

        #endregion
    }
}
