using Magic_RDR.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static Magic_RDR.RPF6.RPF6TOC;

namespace Magic_RDR.Viewers
{
    public partial class AudioViewerForm : Form
    {
        private string awcPath;
        private string[] wavPath;
        private int SampleCount;
        private List<int> AudioIds = new List<int>();
        private bool IsSwitch { get; set; }

        public AudioViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            this.Text = string.Format("MagicRDR - Audio Player [{0}]", entry.Entry.Name);

            this.IsSwitch = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();

            byte[] buffer = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            IOReader reader = new IOReader(new MemoryStream(buffer), this.IsSwitch ? IOReader.Endian.Little : IOReader.Endian.Big);

            if (this.IsSwitch)
            {
                wavPath = AWC_ReadData(reader, file.Name);
                if (wavPath == null || wavPath.Length == 0)
                    return;

                List<ListViewItem> listItem = wavPath.Select(s => new ListViewItem(Path.GetFileName(s).Replace(".wav", ""))).ToList();
                columnHeaderAudio.Text = string.Format("Audio Files ({0})", wavPath.Length);
                listView.BeginUpdate();
                listView.Items.Clear();
                listView.Items.AddRange(listItem.ToArray());
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listView.EndUpdate();

                audioPlayer.URL = wavPath[0].Substring(0, wavPath[0].IndexOf(" "));
                audioPlayer.Ctlcontrols.play();
                return;
            }

            if (!File.Exists(System.Windows.Forms.Application.StartupPath + "\\Assemblies\\" + "libutils.exe"))
            {
                MessageBox.Show("Required file 'libutils.exe' is missing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            byte[] bytes = new byte[4];
            Array.Copy(buffer, 0x8, bytes, 0, 4);

            SampleCount = BitConverter.ToInt32(bytes, 0).Swap();
            string assemblyPath = System.Windows.Forms.Application.StartupPath + "\\Assemblies\\";
            awcPath = assemblyPath + entry.Entry.Name;

            File.WriteAllBytes(awcPath, buffer);
            if (!File.Exists(awcPath))
            {
                MessageBox.Show("An error occured while decoding audio", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            List<ListViewItem> list = new List<ListViewItem>();
            wavPath = new string[SampleCount];
            for (int i = 0; i < SampleCount; i++)
            {
                wavPath[i] = string.Format("{0}_{1}.wav", awcPath.Substring(0, awcPath.LastIndexOf(".")), i + 1);

                Process process = new Process();
                process.StartInfo.FileName = assemblyPath + "libutils.exe";
                process.StartInfo.Arguments = string.Format("-s {0} -o \"{1}\" \"{2}\"", i + 1, wavPath[i], awcPath);
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.UseShellExecute = true;
                process.Start();

                while (!process.HasExited)
                    Thread.Sleep(500);

                ListViewItem item = new ListViewItem();
                item.Text = Path.GetFileNameWithoutExtension(awcPath) + "_" + (i + 1).ToString();
                list.Add(item);
            }
            columnHeaderAudio.Text = string.Format("Audio Files ({0})", SampleCount);
            listView.BeginUpdate();
            listView.Items.Clear();
            listView.Items.AddRange(list.ToArray());
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView.EndUpdate();

            Thread.Sleep(1000);
            if (!File.Exists(wavPath[0]))
            {
                MessageBox.Show("An error occured while decoding audio", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }

        #region Controls

        private void AudioViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(awcPath))
            {
                if (File.Exists(awcPath))
                {
                    try { File.Delete(awcPath); }
                    catch { }
                }
            }

            if (wavPath == null)
                return;

            foreach (string path in wavPath)
            {
                if (File.Exists(path))
                {
                    try { File.Delete(path); }
                    catch { }
                }
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count <= 0)
                return;

            string wavFileName = listView.SelectedItems[0].Text;
            foreach (string path in wavPath)
            {
                if (path.Contains(wavFileName))
                    wavFileName = path;
            }
            if (!File.Exists(wavFileName))
            {
                return;
            }

            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
            {
                wavFileName = wavFileName.Substring(0, wavPath[0].IndexOf(" "));
            }
            audioPlayer.URL = wavFileName;
            audioPlayer.Ctlcontrols.play();
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (wavPath.Length <= 0)
            {
                MessageBox.Show("There's nothing to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string currentAudioFile;
            if (listView.SelectedItems.Count <= 0)
                currentAudioFile = wavPath[0];
            else
                currentAudioFile = listView.SelectedItems[0].Text;

            foreach (string file in wavPath)
            {
                if (File.Exists(file) && file.Contains(currentAudioFile))
                {
                    currentAudioFile = file;
                }
            }

            if (!File.Exists(currentAudioFile))
            {
                MessageBox.Show("An error occured while exporting", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "WAV Audio File (*.wav)|*.wav";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(dialog.FileName))
                {
                    File.Delete(dialog.FileName);
                }
                File.Copy(currentAudioFile, dialog.FileName);
                MessageBox.Show("Successfully export file !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void exportAllButton_Click(object sender, EventArgs e)
        {
            if (wavPath.Length <= 0)
            {
                MessageBox.Show("There's nothing to export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select a destination to export audio files";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int count = 0;
                foreach (string file in wavPath)
                {
                    if (File.Exists(file))
                    {
                        File.Copy(file, dialog.SelectedPath + "\\\\" + Path.GetFileName(file));
                        count++;
                    }
                }
                MessageBox.Show(string.Format("Successfully export {0} file(s) !", count), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        private AwcChunkType Tag(string s)
        {
            return Tag(DataUtils.GetHash(s));
        }

        private AwcChunkType Tag(uint hash)
        {
            return (AwcChunkType)(hash & 0xff);
        }

        private string[] AWC_ReadData(IOReader reader, string entryName)
        {
            uint magic = reader.ReadUInt32(); //TADA
            uint versionAndFlags = reader.ReadUInt32(); //4278255617
            short version = (short)(versionAndFlags & 0xFFFF); //1
            ushort flags = (ushort)((versionAndFlags >> 16) & 0xFFFF); //65281
            int streamsCount = (int)reader.ReadUInt32(); //16
            int streamsInfoOffset = (int)reader.ReadUInt32(); //496

            if ((flags >> 8) != 0xFF)
            {
                throw new Exception("Unsupported flag");
            }

            //First bit - means that there are unknown word for each stream after this header
            //Second bit - I think that it means that not all the tags are in the start of the file, but all the tags of a stream are near the data tag
            //Third bit - Multi channel audio
            if ((flags & 0xF8) != 0)
            {
                throw new Exception("Unsupported flag");
            }

            bool singleChannelEncryptFlag = (flags & 2) == 2;
            bool multiChannel = (flags & 4) == 4;
            bool multiChannelEncryptFlag = (flags & 8) == 8;

            reader.BaseStream.Seek(this.IsSwitch ? 0x10 : 0x10 + ((flags & 1) == 1 ? (2 * streamsCount) : 0), SeekOrigin.Begin);
            long offset = reader.BaseStream.Position;

            if (this.IsSwitch)
            {
                if ((flags & 0x00010000) == 0)
                {
                    offset += 0x2 * streamsCount;
                }
                uint layer = reader.ReadUInt32();
                bool isMusic = (layer & 0x1FFFFFFF) == 0x0;
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            }

            List<StreamInfo> info = new List<StreamInfo>();
            List<string> wavPaths = new List<string>();
            Dictionary<int, Dictionary<AwcChunkType, ChunkInfo>> streamsChunks = new Dictionary<int, Dictionary<AwcChunkType, ChunkInfo>>();

            for (int i = 0; i < streamsCount; ++i)
            {
                info.Add(new StreamInfo(reader));
            }

            for (int i = 0; i < streamsCount; ++i)
            {
                streamsChunks[info[i].Id] = new Dictionary<AwcChunkType, ChunkInfo>();
                for (int j = 0; j < info[i].TagsCount; ++j)
                {
                    ChunkInfo chunk = new ChunkInfo(reader);
                    streamsChunks[info[i].Id][chunk.Tag] = chunk;
                }
            }

            bool firstInfo = true;
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].TagsCount > 1 && i != 0)
                    firstInfo = false;
            }

            if (!multiChannel)
            {
                var chunkList = new List<FormatChunk>();
                var chunkFormatList = new List<ChannelsInfoChunkHeader>();

                for (int i = 0; i < streamsCount; i++)
                {
                    try
                    {
                        if (streamsChunks.TryGetValue(info[i].Id, out Dictionary<AwcChunkType, ChunkInfo> codecInfo))
                        {
                            codecInfo.TryGetValue(Tag("data"), out ChunkInfo chunkDataInfo);
                            codecInfo.TryGetValue(Tag("streamformat"), out ChunkInfo chunkStreamFormatInfo);
                            codecInfo.TryGetValue(Tag("format"), out ChunkInfo chunkFormatInfo);

                            //Format
                            FormatChunk chunk = null;
                            if (chunkFormatInfo != null)
                            {
                                reader.BaseStream.Position = chunkFormatInfo.Offset;
                                chunk = new FormatChunk(reader);
                                chunkList.Add(chunk);
                            }

                            //StreamFormat
                            ChannelsInfoChunkHeader chunkFormat = null;
                            if (chunkStreamFormatInfo != null)
                            {
                                reader.BaseStream.Position = chunkStreamFormatInfo.Offset;
                                chunkFormat = new ChannelsInfoChunkHeader(reader);
                                chunkFormatList.Add(chunkFormat);
                            }

                            //Data
                            if (chunkDataInfo != null && (chunk != null || chunkFormat != null))
                            {
                                if (info[i].Id == 0 && !firstInfo)
                                    continue;

                                reader.BaseStream.Position = chunkDataInfo.Offset;
                                byte[] data = new byte[chunkDataInfo.Size];
                                if (chunkDataInfo.Tag == AwcChunkType.data)
                                {
                                    var header = reader.ReadBytes(0x60);
                                    data = reader.ReadBytes(chunkDataInfo.Size - 0x60);

                                    if (chunk != null) data = DecodeADPCM(data, (int)chunk.Samples);
                                    else data = DecodeADPCM(data, (int)chunkFormat.Channels[0].Samples);

                                    string hexString = info[i].Id.ToString("X");
                                    string desiredFileName = string.Format("0x{0}.wav", hexString.Length == 7 ? ("0" + hexString) : hexString);
                                    if (desiredFileName == "0x0.wav")
                                    {
                                        desiredFileName = entryName.Replace(".awc", ".wav");
                                    }

                                    string tempFile = Path.Combine(Path.GetTempPath(), desiredFileName);
                                    using (var fs = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                    {
                                        if (chunk != null)
                                            WAVFromPCM(new MemoryStream(data), fs, 1, chunk.SamplesPerSecond, 16, (int)chunk.Samples);
                                        else
                                            WAVFromPCM(new MemoryStream(data), fs, 1, chunkFormat.Channels[0].SamplesPerSecond, 16, (int)chunkFormat.Channels[0].Samples);
                                    }
                                    wavPaths.Add(tempFile + " - " + (chunk != null ? chunk.SamplesPerSecond : chunkFormat.Channels[0].SamplesPerSecond) + " Hz");
                                }
                            }
                        }
                    }
                    catch
                    {
                        
                    }
                }
            }
            return wavPaths.ToArray();
        }

        public struct AdpcmState
        {
            public short valprev;
            public byte index;
        }

        private static int[] ima_index_table =
        {
            -1, -1, -1, -1, 2, 4, 6, 8,
            -1, -1, -1, -1, 2, 4, 6, 8
        };

        private static short[] ima_step_table =
        {
            7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
            19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
            50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
            130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
            337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
            876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
            2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
            5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
            15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
        };

        private static int clip(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public byte[] DecodeADPCM(byte[] data, int sampleCount)
        {
            byte[] dataPCM = new byte[data.Length * 4];
            int predictor = 0, stepIndex = 0;
            int readingOffset = 0, writingOffset = 0, bytesInBlock = 0;

            void parseNibble(byte nibble)
            {
                var step = ima_step_table[stepIndex];
                int diff = ((((nibble & 7) << 1) + 1) * step) >> 3;
                if ((nibble & 8) != 0) diff = -diff;
                predictor = predictor + diff;
                stepIndex = clip(stepIndex + ima_index_table[nibble], 0, 88);
                int samplePCM = clip(predictor, -32768, 32767);

                dataPCM[writingOffset] = (byte)(samplePCM & 0xFF);
                dataPCM[writingOffset + 1] = (byte)((samplePCM >> 8) & 0xFF);
                writingOffset += 2;
            }

            while ((readingOffset < data.Length) && (sampleCount > 0))
            {
                if (bytesInBlock == 0)
                {
                    stepIndex = clip(data[readingOffset], 0, 88);
                    predictor = BitConverter.ToInt16(data, readingOffset + 2);
                    bytesInBlock = 2044;
                    readingOffset += 4;
                }
                else
                {
                    parseNibble((byte)(data[readingOffset] & 0x0F));
                    parseNibble((byte)((data[readingOffset] >> 4) & 0x0F));
                    bytesInBlock--;
                    sampleCount -= 2;
                    readingOffset++;
                }
            }
            return dataPCM;
        }

        public void WAVFromPCM(Stream input, Stream output, short channels, int samplesPerSec, int bitsPerSample, int samples = 0)
        {
            short sample_size = (short)((bitsPerSample / 8) * channels);

            using (BinaryWriter writer = new BinaryWriter(output))
            {
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write((int)0); // Skip size of wave file
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write((int)16); // Size of header
                writer.Write((short)1); // Format tag - PCM
                writer.Write(channels);
                writer.Write(samplesPerSec);
                writer.Write((int)(sample_size * samplesPerSec)); // average bytes per sec
                writer.Write(sample_size); // full sample size..
                writer.Write((short)bitsPerSample);
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write((int)0); // Skip size of data

                if (samples != 0)
                {
                    int count = this.CopyToCount(input, output, samples * sample_size);
                    if (count != samples * sample_size)
                    {
                        // Check output size
                        //throw new Exception("Invalid WAV size");
                    }
                }
                else
                {
                    // Write the pcm
                    input.CopyTo(output);
                }

                // Write the size
                output.Seek(4, SeekOrigin.Begin);
                writer.Write((int)(output.Length - 8));

                // Write the size
                output.Seek(40, SeekOrigin.Begin);
                writer.Write((int)(output.Length - 44));
            }
        }

        public int CopyToCount(Stream stream, Stream output, int count)
        {
            byte[] buffer = new byte[32768];
            int start_count = count;
            int read = buffer.Length;

            if (read > count)
            {
                read = count;
            }

            while (read > 0 && (read = stream.Read(buffer, 0, read)) > 0)
            {
                output.Write(buffer, 0, read);
                count -= read;
                read = buffer.Length;

                if (read > count)
                {
                    read = count;
                }
            }
            return start_count - count;
        }
    }

    public class StreamInfo
    {
        public int TagsCount;
        public int Id;

        public StreamInfo(IOReader reader)
        {
            uint info = reader.ReadUInt32();
            Id = (int)(info & 0x1fffffff);
            TagsCount = (int)(info >> 29);
        }
    }

    public class ChunkInfo
    {
        public AwcChunkType Tag;
        public int Offset;
        public int Size;

        public ChunkInfo(IOReader reader)
        {
            ulong info = reader.ReadUInt64();
            Tag = (AwcChunkType)(info >> 56);
            Offset = (int)(info & 0x0fffffff);
            Size = (int)((info >> 28) & 0x0fffffff);
        }
    }

    public enum AwcChunkType : byte
    {
        //Should be the last byte of the hash of the name.
        data = 0x55,            // 0x5EB5E655
        format = 0xFA,          // 0x6061D4FA
        animation = 0x5C,       // 0x938C925C   not correct
        peak = 0x36,            // 0x8B946236
        mid = 0x68,             // 0x71DE4C68
        gesture = 0x2B,         // 0x23097A2B
        granulargrains = 0x5A,  // 0xE787895A
        granularloops = 0xD9,   // 0x252C20D9
        markers = 0xBD,         // 0xD4CB98BD
        streamformat = 0x48,    // 0x81F95048
        seektable = 0xA3,       // 0x021E86A3
    }

    public enum AwcCodecType
    {
        PCM_16BIT_LITTLE_ENDIAN = 0, //Max Payne 3 PC
        PCM_16BIT_BIG_ENDIAN = 1,
        PCM_32BIT_LITTLE_ENDIAN = 2,
        PCM_32BIT_BIG_ENDIAN = 3,
        ADPCM = 4, //IMA PC
        XMA2 = 5, //Xbox 360
        XWMA = 6, //Xbox 360
        MP3 = 7, //PS3
        OGG = 8, //PC only
        AAC = 9, //PC only
        WMA = 10, //PC only
        ATRAC9 = 11, //Orbis
        UNKNOWN = 12
    }

    public class FormatChunk
    {
        public uint Samples;
        public int LoopPoint;
        public ushort SamplesPerSecond;
        public ushort Headroom;
        public ushort Loopbegin;
        public ushort Loopend;
        public ushort PlayEnd;
        public byte PlayBegin;
        public AwcCodecType Codec;

        public FormatChunk(IOReader reader)
        {
            Samples = reader.ReadUInt32();
            LoopPoint = reader.ReadInt32();
            SamplesPerSecond = reader.ReadUInt16();
            Headroom = reader.ReadUInt16();
            Loopbegin = reader.ReadUInt16();
            Loopend = reader.ReadUInt16();
            PlayEnd = reader.ReadUInt16();
            PlayBegin = reader.ReadByte();
            Codec = (AwcCodecType)reader.ReadByte();
        }

        public FormatChunk(ChannelsInfoChunkItem channelInfo)
        {
            Samples = channelInfo.Samples;
            LoopPoint = -1;
            SamplesPerSecond = channelInfo.SamplesPerSecond;
            Headroom = channelInfo.Headroom;
            Loopbegin = channelInfo.unknownWord2;
            Loopend = 0;
            PlayEnd = 0;
            PlayBegin = 0;
            Codec = channelInfo.Codec;
        }
    }

    public class ChannelsInfoChunkHeader
    {
        public int BlockCount; //Small number
        public int BigChunkSize;
        public int ChannelsCount;
        public ChannelsInfoChunkItem[] Channels { get; set; }

        public ChannelsInfoChunkHeader(IOReader reader)
        {
            BlockCount = reader.ReadInt32();
            BigChunkSize = reader.ReadInt32();
            ChannelsCount = reader.ReadInt32();

            var channels = new List<ChannelsInfoChunkItem>();
            for (int i = 0; i < ChannelsCount; i++)
            {
                var itemInfo = new ChannelsInfoChunkItem(reader);
                channels.Add(itemInfo);
            }
            Channels = channels.ToArray();
        }
    }

    public class ChannelsInfoChunkItem
    {
        public int Id;
        public uint Samples; //Big number
        public ushort Headroom;
        public ushort SamplesPerSecond;
        public AwcCodecType Codec;
        public byte RoundSize;
        public ushort unknownWord2;

        public ChannelsInfoChunkItem(IOReader reader)
        {
            Id = reader.ReadInt32();
            Samples = reader.ReadUInt32();
            Headroom = reader.ReadUInt16();
            SamplesPerSecond = reader.ReadUInt16();
            Codec = (AwcCodecType)reader.ReadByte();
            RoundSize = reader.ReadByte();
            unknownWord2 = reader.ReadUInt16();
        }
    }
}