using HelixToolkit.Wpf;
using Magic_RDR.Application;
using Magic_RDR.RPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using static Magic_RDR.RPF6.RPF6TOC;
using static ModelViewer.ModelView;
using Geometry = Magic_RDR.Application.Geometry;
using Texture = Magic_RDR.RPF.Texture;
using Mesh = ModelViewer.Mesh;
using Material = System.Windows.Media.Media3D.Material;
using static Magic_RDR.RPF.Texture;
using System.Reflection;
using Microsoft.DirectX.Direct3D;

namespace Magic_RDR
{
    public partial class FragViewerForm : Form
    {
        private IOReader Reader;
        private TOCSuperEntry Entry;
        private List<Mesh[]> ModelBackup;
        public static byte[] HeaderData;
        private bool[] ModelDefaultMesh;
        private bool NewColorChange, UseFrag;
        private int CurrentImageIndex = 0;
        private System.Drawing.Image currentImage;
        private string FileName;
        private bool fileEdited = false;

        public FragViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Text = string.Format("MagicRDR - Model Viewer - [{0}]", entry.Entry.Name);
            Entry = entry;
            FileName = entry.Entry.Name;

            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();
            byte[] compressedData = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            byte[] fileData = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(compressedData);
            Reader = new IOReader(new MemoryStream(fileData), isSwitchVersion ? IOReader.Endian.Little : IOReader.Endian.Big);

            HeaderData = new byte[0x18];
            for (int i = 0; i < HeaderData.Length; i++)
            {
                HeaderData[i] = compressedData[i];
            }
            Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

            byte rscVersion = file.ResourceType;
            FragType FragTypeData = null;
            FragDrawable FragDrawableData = null;

            if (rscVersion == 138)
                FragTypeData = new FragType(Reader, file);
            else if (rscVersion == 1)
                FragDrawableData = new FragDrawable(Reader, file);

            UpdateTabControls(FragTypeData, FragDrawableData);
        }

        private void UpdateTabControls(FragType frag, FragDrawable drawable)
        {
            UseFrag = frag != null;
            ModelBackup = CurrentModelMesh;
            ModelUseProps = false;

            labelGeometryCount.Text = "* Geometry Count : " + (UseFrag ? FragType.TotalGeometryCount : FragDrawable.TotalGeometryCount);
            labelParentMeshCount.Text = "* Mesh Parent Count : " + (UseFrag ? FragType.TotalMeshParent : FragDrawable.TotalMeshParent);
            labelVerticesCount.Text = "* Vertices Count : " + (UseFrag ? FragType.TotalVerticeCount : FragDrawable.TotalVerticeCount);
            labelPolygonsCount.Text = "* Polygons Count : " + (UseFrag ? FragType.TotalFaceCount : FragDrawable.TotalFaceCount);
            
            if (UseFrag)
            {
                FragDrawable.ImagesSource = null;
                labelPosition.Text = string.Format("* Position : ({0}; {1}; {2})", FragType.ModelBoundsCenter.X, FragType.ModelBoundsCenter.Y, FragType.ModelBoundsCenter.Z);
                labelBoundsMin.Text = string.Format("* Bounds Min : ({0}; {1}; {2})", FragType.ModelBoundsMin.X, FragType.ModelBoundsMin.Y, FragType.ModelBoundsMin.Z);
                labelBoundsMax.Text = string.Format("* Bounds Max : ({0}; {1}; {2})", FragType.ModelBoundsMax.X, FragType.ModelBoundsMax.Y, FragType.ModelBoundsMax.Z);
            }
            else
            {
                FragType.ImagesSource = null;
                labelPosition.Text = string.Format("* Position : ({0}; {1}; {2})", FragDrawable.ModelBoundsCenter.X, FragDrawable.ModelBoundsCenter.Y, FragDrawable.ModelBoundsCenter.Z);
                labelBoundsMin.Text = string.Format("* Bounds Min : ({0}; {1}; {2})", FragDrawable.ModelBoundsMin.X, FragDrawable.ModelBoundsMin.Y, FragDrawable.ModelBoundsMin.Z);
                labelBoundsMax.Text = string.Format("* Bounds Max : ({0}; {1}; {2})", FragDrawable.ModelBoundsMax.X, FragDrawable.ModelBoundsMax.Y, FragDrawable.ModelBoundsMax.Z);

            }

            //Geometries
            int GeometryCount = UseFrag ? (int)FragType.TotalGeometryCount : (int)FragDrawable.TotalGeometryCount;
            panel1.RowCount = GeometryCount;

            int count = 0;
            ModelDefaultMesh = new bool[GeometryCount];
            for (int i = 0; i < GeometryCount; i++)
            {
                CheckBox box = new CheckBox();
                box.Checked = true;
                box.Dock = DockStyle.Top;
                box.Name = string.Format("GeometryCheckBox{0}", i + 1);
                box.CheckedChanged += GeometryCheckBox_Checked;

                if (UseFrag)
                    box.Text = string.Format("Geometry {0} (Vertexs: {1}, Faces: {2}, Indexs: {3})", i + 1, FragType.ModelGeometry[i].VertexCount, FragType.ModelGeometry[i].FaceCount, FragType.ModelGeometry[i].IndexCount);
                else
                    box.Text = string.Format("Geometry {0} (Vertexs: {1}, Faces: {2}, Indexs: {3})", i + 1, FragDrawable.ModelGeometry[i].VertexCount, FragDrawable.ModelGeometry[i].FaceCount, FragDrawable.ModelGeometry[i].IndexCount);

                box.Margin = new Padding(5, 0, 0, 0);
                panel1.Controls.Add(box, 0, i);
                count++;
            }

            //Textures
            int index = -1, ImageCount = UseFrag ? FragType.ImagesSource.Length : FragDrawable.ImagesSource.Length;
            ImageSource[] Textures = UseFrag ? FragType.ImagesSource : FragDrawable.ImagesSource;
            TextureInfo[] images = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;

            for (int i = 0; i < ImageCount; i++)
            {
                if (Textures[i] != null)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                imageContainer.Image = System.Drawing.Image.FromStream(new MemoryStream(BufferFromImageSource(Textures[index])));
                labelCurrentMipmap.Text = string.Format("MipMap 1 - Size {0}x{1}", images[CurrentImageIndex].Height, images[CurrentImageIndex].Width);
                trackBar1.Maximum = images[index].MipMaps;
                currentImage = imageContainer.Image;
                CurrentImageIndex = index;
            }

            List<ListViewItem> list = new List<ListViewItem>();
            for (int i = 0; i < ImageCount; i++)
            {
                if (Textures[i] == null)
                {
                    continue;
                }

                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = images[i].TextureName;
                listViewItem.SubItems.Add(string.Format("{0}x{1}", images[i].Height, images[i].Width));
                listViewItem.SubItems.Add(images[i].PixelFormat.ToString());
                listViewItem.SubItems.Add(images[i].MipMaps.ToString());
                list.Add(listViewItem);
            }
            listViewTextures.BeginUpdate();
            listViewTextures.Items.Clear();
            listViewTextures.Items.AddRange(list.ToArray());
            listViewTextures.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewTextures.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewTextures.EndUpdate();
            list.Clear();

            //Skeleton
            int BoneCount = UseFrag ? FragType.BoneNames.Length : FragDrawable.BoneNames.Length;
            string[] BoneNames = UseFrag ? FragType.BoneNames : FragDrawable.BoneNames;
            Vector4[] BonePositions = UseFrag ? FragType.BonePositions : FragDrawable.BonePositions;
            
            for (int i = 0; i < BoneCount; i++)
            {
                int parent = UseFrag ? FragType.BoneParentOffset[i] : FragDrawable.BoneParentOffset[i];
                int boneIndex = (parent == 0) ? 0 : Array.IndexOf(UseFrag ? FragType.BoneFileOffset : FragDrawable.BoneFileOffset, parent);

                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = BoneNames[i];
                listViewItem.SubItems.Add(BoneNames[boneIndex]);
                listViewItem.SubItems.Add(string.Format("{0}; {1}; {2}", BonePositions[i].X.ToString("F3"), BonePositions[i].Y.ToString("F3"), BonePositions[i].Z.ToString("F3")).Replace(",", "."));
                list.Add(listViewItem);
            }
            columnHeader3.Text = string.Format("Bones ({0})", BoneCount);
            listViewSkeleton.BeginUpdate();
            listViewSkeleton.Items.Clear();
            listViewSkeleton.Items.AddRange(list.ToArray());
            listViewSkeleton.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewSkeleton.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewSkeleton.EndUpdate();

            if (RPF6FileNameHandler.UseCustomColor)
            {
                NewFirstGradientColor = RPF6FileNameHandler.CustomColor1;
                NewSecondGradientColor = RPF6FileNameHandler.CustomColor2;
            }
        }

        #region FragType

        public class FragType
        {
            private IOReader Reader;
            private FileEntry Entry;
            private MeshGeometry3D ModelMesh;
            private Mesh[] Meshs;
            private List<Mesh[]> MeshsList = new List<Mesh[]>();
            public static List<Geometry> ModelGeometry = new List<Geometry>();
            private Model3DGroup ModelGroup = new Model3DGroup();

            public static string ModelName;
            private int[] m_Models = new int[4];
            private int BoneCount;
            public static uint TotalVerticeCount, TotalFaceCount, TotalGeometryCount, TotalMeshParent;

            private string[] ShaderTextureMappingNames;
            private List<ushort> ShaderTextureMapping = new List<ushort>();
            private List<string> ShaderTextureMappingNamesOrdered = new List<string>();

            private int DrawablePointer;
            private int BoundsPointer;
            private int TextureDataPointer;
            private int ShaderGroupPointer;
            private int SkeletonPointer;
            private int BoneInfoPointer;

            public static Vector3 ModelBoundsCenter;
            public static Vector3 ModelBoundsMin;
            public static Vector3 ModelBoundsMax;

            public static Vector4[] BonePositions;
            public static string[] BoneNames;
            public static int[] BoneFileOffset, BoneParentOffset;

            private Vector3[] Vertices;
            private Vector2[] UV;
            private Vector3[] Normals;
            private Vector3[] Tangents;
            private System.Drawing.Color[] VertexsColors;

            public static ImageSource[] ImagesSource;
            public static TextureInfo[] TexInfos;
            private RGTexture[] ImagesData;

            public FragType(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;

                //Reset static variables
                ModelName = "";
                TotalVerticeCount = 0;
                TotalFaceCount = 0;
                TotalGeometryCount = 0;
                TotalMeshParent = 0;
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelGeometry.Clear();
                ShaderTextureMapping.Clear();
                ShaderTextureMappingNamesOrdered.Clear();

                //Read model data
                XFT_ReadMainStructure();
                XFT_ReadBoundsStructure();

                (int, int[]) TextureInfo = XFT_ReadTextureStructure(); //returns (TextureCount, TexturePointer)
                if (TextureInfo.Item1 > 0)
                {
                    XFT_ReadTextureData(TextureInfo.Item1, TextureInfo.Item2);
                }
                else
                {
                    TexInfos = new TextureInfo[0];
                    ImagesSource = new ImageSource[0];
                    ImagesData = new RGTexture[0];
                }
                XFT_ReadDrawableStructure();
                XFT_ReadSkeletonData();
                XFT_ReadBoneData();
                XFT_ReadDrawableData();

                //Add model
                NewModel = ModelGroup;
                Vector3D axis = new Vector3D(1, 0, 0);
                Matrix3D matrix = NewModel.Transform.Value;
                matrix.Rotate(new Quaternion(axis, 90));
                NewModel.Transform = new MatrixTransform3D(matrix);

                NewPoints = NewModel.GetModelBoundsPoints();
                NewModelName = ModelName;
                CurrentModelMesh = MeshsList;
            }

            private void XFT_ReadMainStructure()
            {
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());
                Vector3 Unknown0x08 = Reader.ReadVector3();
                Vector3 Unknown0x14 = Reader.ReadVector3();
                Vector4 Unknown0x20 = Reader.ReadVector4();
                Vector4 Unknown0x30 = Reader.ReadVector4();
                Vector4 m_vUnbrokenCGOffset = Reader.ReadVector4();
                Vector4 m_vDampingLinearC = Reader.ReadVector4();
                Vector4 m_vDampingLinearV = Reader.ReadVector4();
                Vector4 m_vDampingLinearV2 = Reader.ReadVector4();
                Vector4 m_vDampingAngularC = Reader.ReadVector4();
                Vector4 m_vDampingAngularV = Reader.ReadVector4();
                Vector4 m_vDampingAngularV2 = Reader.ReadVector4();

                int ModelNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                ModelName = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, ModelNamePointer, true);
                DrawablePointer = Reader.ReadOffset(Reader.ReadInt32());

                uint Unknown0xB8 = Reader.ReadUInt32();
                uint Unknown0xBC = Reader.ReadUInt32();
                uint Unknown0xC0 = Reader.ReadUInt32();
                uint Unknown0xC4 = Reader.ReadUInt32();
                uint Unknown0xC8 = Reader.ReadUInt32();
                int GroupNamesPointer = Reader.ReadOffset(Reader.ReadInt32());
                int Unknown0xD0 = Reader.ReadOffset(Reader.ReadInt32());
                int ChildrensPointer = Reader.ReadOffset(Reader.ReadInt32());

                int unkArray = Reader.ReadOffset(Reader.ReadInt32());
                ushort unkArrayCount = Reader.ReadUInt16();
                ushort unkArraySize = Reader.ReadUInt16();
                uint unkPtr = Reader.ReadUInt32();
                int archetype = Reader.ReadOffset(Reader.ReadInt32());
                int archetype2 = Reader.ReadOffset(Reader.ReadInt32());
                BoundsPointer = Reader.ReadOffset(Reader.ReadInt32());

                Reader.BaseStream.Position += 256;
                TextureDataPointer = Reader.ReadOffset(Reader.ReadInt32());
            }

            private void XFT_ReadBoundsStructure()
            {
                if (BoundsPointer == 0x0)
                    return;

                Reader.BaseStream.Seek(BoundsPointer + 0x14, SeekOrigin.Begin);
                byte BoundsType = Reader.ReadByte();
                byte Unknown1 = Reader.ReadByte();
                ushort Unknown2 = Reader.ReadUInt16();
                float SphereRadius = Reader.ReadFloat();
                float WorldRadius = Reader.ReadFloat();
                Vector4 BoxMax = Reader.ReadVector4();
                Vector4 BoxMin = Reader.ReadVector4();
                Vector4 BoxCenter = Reader.ReadVector4();
                Vector4 Unknown3 = Reader.ReadVector4();
                Vector4 SphereCenter = Reader.ReadVector4();
                Vector4 Unknown4 = Reader.ReadVector4();
                Vector3 Margin = Reader.ReadVector3();
                uint RefCount = Reader.ReadUInt32();

                int ChildrenPtr = Reader.ReadOffset(Reader.ReadInt32());
                int ChildrenTransform1Ptr = Reader.ReadOffset(Reader.ReadInt32());
                int ChildrenTransform2Ptr = Reader.ReadOffset(Reader.ReadInt32());
                int ChildrenBoundingBoxesPtr = Reader.ReadOffset(Reader.ReadInt32());
                int UnkPtr1 = Reader.ReadOffset(Reader.ReadInt32());
                int UnkPtr2 = Reader.ReadOffset(Reader.ReadInt32());
                ushort ChildrenCount1 = Reader.ReadUInt16();
                ushort ChildrenCount2 = Reader.ReadUInt16();
            }

            private (int, int[]) XFT_ReadTextureStructure()
            {
                if (TextureDataPointer == 0x0)
                    return (0, null);

                Reader.BaseStream.Seek(TextureDataPointer, SeekOrigin.Begin);
                uint Unknown_0x00 = Reader.ReadUInt32();
                int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
                byte Unknown_0x08 = Reader.ReadByte();
                byte Unknown_0x09 = Reader.ReadByte();
                ushort Unknown_0x0A = Reader.ReadUInt16();
                uint Unknown_0x0C = Reader.ReadUInt32();

                if (Unknown_0x0C != 1)
                    return (0, null);

                RGArray TextureNameHashs;
                TextureNameHashs.Offset = Reader.ReadOffset(Reader.ReadInt32());
                TextureNameHashs.Count = Reader.ReadUInt16();
                TextureNameHashs.Size = Reader.ReadUInt16();

                RGArray Textures;
                Textures.Offset = Reader.ReadOffset(Reader.ReadInt32());
                Textures.Count = Reader.ReadUInt16();
                Textures.Size = Reader.ReadUInt16();

                TexInfos = new TextureInfo[Textures.Count];
                ImagesSource = new ImageSource[Textures.Count];
                ImagesData = new RGTexture[Textures.Count];

                //TextureNameHashs
                Reader.BaseStream.Seek(TextureNameHashs.Offset, SeekOrigin.Begin);
                uint[] Hashs = new uint[TextureNameHashs.Count];
                for (int i = 0; i < TextureNameHashs.Count; i++)
                    Hashs[i] = Reader.ReadUInt32();

                //Textures
                Reader.BaseStream.Seek(Textures.Offset, SeekOrigin.Begin);
                int[] TexturesPointer = new int[Textures.Count];
                for (int i = 0; i < Textures.Count; i++)
                    TexturesPointer[i] = Reader.ReadOffset(Reader.ReadInt32());

                return (Textures.Count, TexturesPointer);
            }

            private void XFT_ReadTextureData(int textureCount, int[] texturePointer)
            {
                for (int i = 0; i < textureCount; i++)
                {
                    Reader.BaseStream.Seek(texturePointer[i], SeekOrigin.Begin);
                    uint Unknown_0x00 = Reader.ReadUInt32();
                    int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
                    byte Unknown_0x08 = Reader.ReadByte();
                    byte Unknown_0x09 = Reader.ReadByte();
                    ushort Unknown_0x0A = Reader.ReadUInt16();
                    uint Unknown_0x0C = Reader.ReadUInt32();

                    if (Unknown_0x0A != 1)
                        return;

                    uint Unknown_0x10 = ImagesData[i].Resource.Unknown_0x00 = Reader.ReadUInt32();
                    uint TextureSize = ImagesData[i].DataSize = Reader.ReadUInt32();
                    int TextureNamePointer = ImagesData[i].Resource.NameTexturePointer = Reader.ReadOffset(Reader.ReadInt32());
                    int D3DStructurePointer = ImagesData[i].Resource.BaseTexturePointer = Reader.ReadOffset(Reader.ReadInt32());
                    ushort Width = ImagesData[i].Resource.Width = Reader.ReadUInt16();
                    ushort Height = ImagesData[i].Resource.Height = Reader.ReadUInt16();
                    int MipMapCount = ImagesData[i].Resource.MipMapCount = Reader.ReadInt32();
                    Vector3 Unknown_0x24 = ImagesData[i].Resource.Unknown_0x18 = Reader.ReadVector3();
                    Vector3 Unknown_0x30 = ImagesData[i].Resource.Unknown_0x24 = Reader.ReadVector3();
                    string Name = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer, true);

                    //BaseAdress
                    Reader.BaseStream.Seek(D3DStructurePointer + 0x20, SeekOrigin.Begin);
                    uint Value = Reader.ReadUInt32();
                    uint BaseAdress = Value >> 0xC;
                    int CurrentTextureDataPointer = ((int)(BaseAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                    //MipAdress
                    Reader.BaseStream.Seek(D3DStructurePointer + 0x30, SeekOrigin.Begin);
                    uint Value_ = Reader.ReadUInt32();
                    uint MipMapAdress = Value_ >> 0xC;
                    int CurrentMipDataPointer = ((int)(MipMapAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                    //Read texture data
                    Reader.BaseStream.Seek(D3DStructurePointer + 0x23, SeekOrigin.Begin);
                    Texture.TextureType Format = (Texture.TextureType)(Value & byte.MaxValue);

                    try
                    {
                        TexInfos[i] = new TextureInfo()
                        {
                            TextureName = Name,
                            Width = Width,
                            Height = Height,
                            MipMaps = MipMapCount,
                            TextureSize = TextureSize,
                            TextureDataPointer = CurrentTextureDataPointer,
                            MipDataPointer = CurrentMipDataPointer,
                            PixelFormat = Format
                        };

                        byte[] buffer = ReadTextureInfo(Reader, 0, TexInfos[i]);
                        ImagesSource[i] = LoadImage(buffer);
                    }
                    catch { TexInfos[i] = null; ImagesSource[i] = null; }

                    if (CurrentMipDataPointer == 0)
                    {
                        TexInfos[i] = null;
                        ImagesSource[i] = null;
                        continue;
                    }
                }
            }

            public void XFT_ReadDrawableStructure()
            {
                Reader.BaseStream.Seek(DrawablePointer, SeekOrigin.Begin);
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());

                ////////////// ShaderGroup //////////////
                ShaderGroupPointer = Reader.ReadOffset(Reader.ReadInt32());
                XFT_ReadShaderGroup();

                ////////////// Skeleton //////////////
                Reader.BaseStream.Seek(DrawablePointer + 0xC, SeekOrigin.Begin);
                SkeletonPointer = Reader.ReadOffset(Reader.ReadInt32());

                ////////////// LODGroup //////////////
                Vector4 m_vCenter = Reader.ReadVector4();
                Vector4 m_vBoundsMin = Reader.ReadVector4();
                Vector4 m_vBoundsMax = Reader.ReadVector4();
                ModelBoundsCenter = new Vector3(m_vCenter.X, m_vCenter.Y, m_vCenter.Z);
                ModelBoundsMin = new Vector3(m_vBoundsMin.X, m_vBoundsMin.Y, m_vBoundsMin.Z);
                ModelBoundsMax = new Vector3(m_vBoundsMax.X, m_vBoundsMax.Y, m_vBoundsMax.Z);

                for (int i = 0; i < 4; i++)
                    m_Models[i] = Reader.ReadOffset(Reader.ReadInt32());

                Vector4 Unknown0x40 = Reader.ReadVector4();
                uint[] m_dwShaderUseMask = new uint[4];
                for (int i = 0; i < 4; i++)
                    m_dwShaderUseMask[i] = Reader.ReadUInt32();

                float m_fRadius = Reader.ReadFloat();
                uint Unknown0x64 = Reader.ReadUInt32();
                int Unknown0x68 = Reader.ReadOffset(Reader.ReadInt32());
                int Unknown0x6C = Reader.ReadOffset(Reader.ReadInt32());
            }

            private void XFT_ReadShaderGroup()
            {
                Reader.BaseStream.Seek(ShaderGroupPointer, SeekOrigin.Begin);
                uint Unknown0x00 = Reader.ReadUInt32();
                uint Unknown0x04 = Reader.ReadUInt32();

                RGArray Unknown_0x08;
                Unknown_0x08.Offset = Reader.ReadOffset(Reader.ReadInt32());
                Unknown_0x08.Count = Reader.ReadUInt16();
                Unknown_0x08.Size = Reader.ReadUInt16();

                uint[] Unknown0x10 = new uint[4];
                for (int i = 0; i < Unknown0x10.Length; i++)
                    Unknown0x10[i] = Reader.ReadUInt32();

                int[] ShaderParamsPointers = new int[Unknown_0x08.Count];
                Reader.BaseStream.Seek(Unknown_0x08.Offset, SeekOrigin.Begin);
                for (int i = 0; i < Unknown_0x08.Count; i++)
                    ShaderParamsPointers[i] = Reader.ReadOffset(Reader.ReadInt32());

                ShaderTextureMappingNames = new string[ShaderParamsPointers.Length];
                for (int i = 0; i < ShaderParamsPointers.Length; i++)
                {
                    Reader.BaseStream.Seek(ShaderParamsPointers[i], SeekOrigin.Begin);
                    XFT_ReadShaderFX(i);
                }
            }

            private void XFT_ReadShaderFX(int index)
            {
                int ParameterPointer = Reader.ReadOffset(Reader.ReadInt32());
                uint ShaderNameHash = Reader.ReadUInt32();
                string ShaderName = ((ShaderNameTerrains)Enum.Parse(typeof(ShaderNameTerrains), ShaderNameHash.ToString(), true)).ToString();

                byte ParameterCount = Reader.ReadByte();
                byte RenderBucket = Reader.ReadByte();
                ushort Unknown0xA = Reader.ReadUInt16();
                ushort ParameterSize = Reader.ReadUInt16();
                ushort ParameterDataSize = Reader.ReadUInt16();
                uint ShaderHash = Reader.ReadUInt32();
                uint Unknown_0x14 = Reader.ReadUInt32();
                uint RenderBucketMask = Reader.ReadUInt32();
                uint Unknown_0x1C = Reader.ReadUInt32();

                int attempt = -1;
                long Offset = Reader.BaseStream.Position;
            Redo:
                attempt++;

                if (attempt > 0)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position + 0x8, SeekOrigin.Begin);
                    Offset = Reader.BaseStream.Position;
                }

                for (int i = 0; i < 1; i++) //ParameterCount, we're only using the first item
                {
                    byte RegisterCount = Reader.ReadByte();
                    byte RegisterIndex = Reader.ReadByte();
                    byte DataType = Reader.ReadByte();
                    _ = Reader.ReadByte();

                    Vector4[] ShaderItemParameter = new Vector4[RegisterCount];
                    int ShaderMappingPointer = Reader.ReadOffset(Reader.ReadInt32());
                    if (ShaderMappingPointer == 0x0)
                    {
                        Reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
                        goto Redo;
                    }
                    long currentOffset = Reader.BaseStream.Position;

                    if (RegisterCount > 0)
                    {
                        Reader.BaseStream.Seek(ShaderMappingPointer, SeekOrigin.Begin);
                        for (int data = 0; data < RegisterCount; data++)
                        {
                            ShaderItemParameter[data] = Reader.ReadVector4(); //+0xC0
                        }
                        Reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                    }
                    else
                    {
                        Reader.BaseStream.Seek(ShaderMappingPointer + 0x18, SeekOrigin.Begin);
                        int ShaderTextureNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                        ShaderTextureMappingNames[index] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, ShaderTextureNamePointer, true);
                        Reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                    }
                }

                bool EndReached = false;
                /*while (!EndReached) //Shader item parameters, skipping because of unknown parameters //I have to redo this one day
                {
                    uint ShaderItemHash = Reader.ReadUInt32();
                    if (ShaderItemHash == 726757629 || ShaderItemHash == 1186448975 || ShaderItemHash == 4134611841) //DiffuseSampler, BumpSampler, bumpiness
                    {
                        EndReached = true;
                        Reader.BaseStream.Seek(Reader.BaseStream.Position - 0x4, SeekOrigin.Begin);
                    }
                }*/
            }

            private void XFT_ReadSkeletonData()
            {
                Reader.BaseStream.Seek(SkeletonPointer, SeekOrigin.Begin);
                BoneInfoPointer = Reader.ReadOffset(Reader.ReadInt32());
                int ParentIndices = Reader.ReadOffset(Reader.ReadInt32());
                int ModelTransforms = Reader.ReadOffset(Reader.ReadInt32());
                int ModelInverses = Reader.ReadOffset(Reader.ReadInt32());
                int RelativeTransforms = Reader.ReadOffset(Reader.ReadInt32());
                int AbsoluteTransforms = Reader.ReadOffset(Reader.ReadInt32());
                BoneCount = Reader.ReadUInt16();
                ushort Unknown0x1A = Reader.ReadUInt16();
                ushort Unknown0x1C = Reader.ReadUInt16();
                ushort Unknown0x1E = Reader.ReadUInt16();
                uint Unknown0x20 = Reader.ReadUInt32();

                RGArray BoneStructure;
                BoneStructure.Offset = Reader.ReadOffset(Reader.ReadInt32());
                BoneStructure.Count = Reader.ReadUInt16();
                BoneStructure.Size = Reader.ReadUInt16();
                uint Unknown0x2C = Reader.ReadUInt32();
            }

            private void XFT_ReadBoneData()
            {
                BoneNames = new string[BoneCount];
                BoneFileOffset = new int[BoneCount];
                BoneParentOffset = new int[BoneCount];
                BonePositions = new Vector4[BoneCount];
                Reader.BaseStream.Seek(BoneInfoPointer, SeekOrigin.Begin);

                for (int i = 0; i < BoneCount; i++)
                {
                    BoneFileOffset[i] = (int)Reader.BaseStream.Position; //Saving the current bone offset
                    int m_pszBoneName = Reader.ReadOffset(Reader.ReadInt32());
                    BoneNames[i] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, m_pszBoneName, true);

                    uint m_dwFlags = Reader.ReadUInt32();
                    int m_pNextSibling = Reader.ReadOffset(Reader.ReadInt32());
                    int m_pFirstChild = Reader.ReadOffset(Reader.ReadInt32());
                    BoneParentOffset[i] = Reader.ReadOffset(Reader.ReadInt32()); //Saving the current bone parent offset
                    ushort m_wBoneIndex = Reader.ReadUInt16();
                    ushort m_wBoneId = Reader.ReadUInt16();
                    ushort m_wMirror = Reader.ReadUInt16();
                    byte m_nbTransFlags = Reader.ReadByte();
                    byte m_nbRotFlags = Reader.ReadByte();
                    byte m_nbScaleFlags = Reader.ReadByte();

                    byte[] __pad_1D = new byte[3];
                    for (int i1 = 0; i1 < 3; i1++)
                        __pad_1D[i1] = Reader.ReadByte();

                    Vector3 m_pos = Reader.ReadVector3();
                    uint m_dwKey = Reader.ReadUInt32();
                    Vector4 m_vRotationEuler = Reader.ReadVector4();
                    Vector4 m_vRotationQuaternion = Reader.ReadVector4();
                    Vector4 m_vScale = Reader.ReadVector4();
                    BonePositions[i] = Reader.ReadVector4();
                    Vector4 m_vOrient = Reader.ReadVector4();
                    Vector4 m_vSorient = Reader.ReadVector4();
                    Vector4 m_vTransMin = Reader.ReadVector4();
                    Vector4 m_vTransMax = Reader.ReadVector4();
                    Vector4 m_vRotMin = Reader.ReadVector4();
                    Vector4 m_vRotMax = Reader.ReadVector4();

                    int m_pJointData = Reader.ReadOffset(Reader.ReadInt32());
                    uint _fD4 = Reader.ReadUInt32();
                    uint _fD8 = Reader.ReadUInt32();
                    uint _fDC = Reader.ReadUInt32();
                }
            }

            private void XFT_ReadDrawableData()
            {
                int m_ModelCount = 0, m_ModelIndex = 0;
                for (int i = 0; i < m_Models.Length; i++)
                {
                    if (m_Models[i] == 0)
                        continue;

                    m_ModelCount++;
                    m_ModelIndex = i;
                }

                int index = 0;
                for (int model = 0; model < m_ModelCount; ++model)
                {
                    Reader.BaseStream.Seek(m_Models[m_ModelIndex], SeekOrigin.Begin);

                    RGArray CurrentModelMesh;
                    CurrentModelMesh.Offset = Reader.ReadOffset(Reader.ReadInt32());
                    TotalMeshParent = CurrentModelMesh.Count = Reader.ReadUInt16();
                    CurrentModelMesh.Size = Reader.ReadUInt16();

                    Reader.BaseStream.Seek(CurrentModelMesh.Offset, SeekOrigin.Begin);
                    int[] CurrentModelMeshPointer = new int[CurrentModelMesh.Count];
                    for (int i = 0; i < CurrentModelMesh.Count; i++)
                    {
                        CurrentModelMeshPointer[i] = Reader.ReadOffset(Reader.ReadInt32());
                    }

                    for (uint modelMesh = 0; modelMesh < CurrentModelMesh.Count; modelMesh++)
                    {
                        Reader.BaseStream.Seek(CurrentModelMeshPointer[modelMesh], SeekOrigin.Begin);
                        RGModel ModelGeometryData;
                        ModelGeometryData.Unknown0x00 = Reader.ReadUInt32();
                        ModelGeometryData.Geometry.Offset = Reader.ReadOffset(Reader.ReadInt32());
                        ModelGeometryData.Geometry.Count = Reader.ReadUInt16();
                        ModelGeometryData.Geometry.Size = Reader.ReadUInt16();
                        ModelGeometryData.Unknown0x0C = Reader.ReadOffset(Reader.ReadInt32());
                        ModelGeometryData.Unknown0x10 = Reader.ReadOffset(Reader.ReadInt32());

                        uint Unknown0x14 = Reader.ReadUInt32();
                        ushort Unknown0x18 = Reader.ReadUInt16();
                        ushort ReferenceCount = Reader.ReadUInt16();
                        Reader.BaseStream.Seek(ModelGeometryData.Unknown0x10, SeekOrigin.Begin);

                        for (int i = 0; i < ReferenceCount; i++)
                        {
                            ShaderTextureMapping.Add(Reader.ReadUInt16());
                        }

                        RGArray ModelCurrentGeometry = ModelGeometryData.Geometry;
                        Reader.BaseStream.Seek(ModelCurrentGeometry.Offset, SeekOrigin.Begin);

                        int[] ModelGeometryDataPointer = new int[ModelGeometryData.Geometry.Count];
                        for (int i = 0; i < ModelCurrentGeometry.Count; ++i)
                            ModelGeometryDataPointer[i] = Reader.ReadOffset(Reader.ReadInt32());

                        TotalGeometryCount += ModelCurrentGeometry.Count;
                        XFT_ReadGeometryData(ModelCurrentGeometry.Count, ModelGeometryDataPointer, ref index);
                    }
                }
            }

            private void XFT_ReadGeometryData(int geometryCount, int[] modelGeometryDataPointer, ref int index)
            {
                Meshs = new Mesh[geometryCount];
                for (int count = 0; count < geometryCount; ++count)
                {
                    Reader.BaseStream.Seek(modelGeometryDataPointer[count], SeekOrigin.Begin);
                    (uint[], byte[]) Model3DInfo = XFT_ReadModel3DData(count); //0 = IndexCount, 1 = FaceCount, 2 = Indices, 3 = VertexColor
                    ModelMesh = new MeshGeometry3D();

                    ////////////// Vertices //////////////
                    foreach (Vector3 vertex in Vertices)
                    {
                        ModelMesh.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
                    }

                    ////////////// Faces //////////////
                    ushort[] indices = new ushort[Model3DInfo.Item1[1] * 3];
                    for (int i = 0; i < Model3DInfo.Item1[1] * 3; i++)
                    {
                        indices[i] = BitConverter.ToUInt16(Model3DInfo.Item2, i * 2).Swap();
                    }
                    for (int i = 0; i < Model3DInfo.Item1[1]; i++)
                    {
                        ModelMesh.TriangleIndices.Add(indices[i * 3 + 0]);
                        ModelMesh.TriangleIndices.Add(indices[i * 3 + 1]);
                        ModelMesh.TriangleIndices.Add(indices[i * 3 + 2]);
                    }

                    ////////////// Texture Coordinates //////////////
                    for (int i = 0; i < UV.Length; i++)
                    {
                        ModelMesh.TextureCoordinates.Add(new System.Windows.Point(UV[i].X, UV[i].Y));
                    }

                    ////////////// Normals //////////////
                    for (int i = 0; i < Normals.Length; i++)
                    {
                        ModelMesh.Normals.Add(new Vector3D(Normals[i].X, Normals[i].Y, Normals[i].Z));
                    }

                    ////////////// Material //////////////
                    Random random = new Random();
                    Material material = MaterialHelper.CreateMaterial(Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255)));
                    ushort CurrentSubMeshShaderIndex;
                    string CurrentSubMeshShaderName = "";

                    try
                    {
                        CurrentSubMeshShaderIndex = ShaderTextureMapping[index];
                        CurrentSubMeshShaderName = ShaderTextureMappingNames[CurrentSubMeshShaderIndex];
                    }
                    catch { }


                    if (string.IsNullOrEmpty(CurrentSubMeshShaderName))
                        ShaderTextureMappingNamesOrdered.Add("Unknown");
                    else
                        ShaderTextureMappingNamesOrdered.Add(CurrentSubMeshShaderName);

                    if (CurrentSubMeshShaderName != null)
                    {
                        for (int i = 0; i < TexInfos.Count(); i++)
                        {
                            if (TexInfos[i].TextureName.Contains(CurrentSubMeshShaderName.ToLower()))
                            {
                                ImageBrush brush = new ImageBrush
                                {
                                    ImageSource = ImagesSource[i],
                                    TileMode = TileMode.Tile,
                                    ViewportUnits = BrushMappingMode.Absolute
                                };

                                if (brush.ImageSource != null)
                                    material = new DiffuseMaterial(brush);
                            }
                        }
                    }

                    ////////////// Set up meshs //////////////
                    Model3DGroup Group = new Model3DGroup();
                    GeometryModel3D GeometryModel = new GeometryModel3D(ModelMesh, material)
                    {
                        BackMaterial = material
                    };
                    Mesh mesh = new Mesh(ModelMesh, material);
                    ModelGeometry.Add(new Geometry(mesh, CurrentSubMeshShaderName ?? "Unknown", (int)Model3DInfo.Item1[0], (int)Model3DInfo.Item1[1], Vertices.Length, Vertices, UV, indices, Normals, Tangents, VertexsColors, 0, 0, 0));
                    Group.Children.Add(GeometryModel);
                    ModelGroup.Children.Add(Group);
                    Meshs[count] = mesh;
                    index++;
                }
                MeshsList.Add(Meshs);
            }

            public (uint[], byte[]) XFT_ReadModel3DData(int Index)
            {
                uint Unknown0x00 = Reader.ReadUInt32();
                uint DeclarationIndex = Reader.ReadUInt32();
                uint Unknown0x08 = Reader.ReadUInt32();

                int[] VertexBuffer = new int[4];
                for (int i = 0; i < 4; i++)
                    VertexBuffer[i] = Reader.ReadOffset(Reader.ReadInt32());

                int[] IndexBuffer = new int[4];
                for (int i = 0; i < 4; i++)
                    IndexBuffer[i] = Reader.ReadOffset(Reader.ReadInt32());

                uint IndexCount = Reader.ReadUInt32();
                uint FaceCount = Reader.ReadUInt32();

                ushort VertexCount = Reader.ReadUInt16();
                byte PrimitiveType = Reader.ReadByte(); //Triangles
                byte Unknown0x37 = Reader.ReadByte();
                int JointMapPointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort VertexStride = Reader.ReadUInt16();
                ushort JointCount = Reader.ReadUInt16();
                int DataOffset = Reader.ReadInt32();
                int VertexDataPointer = Reader.GetDataOffset(DataOffset);

                TotalVerticeCount += VertexCount;
                TotalFaceCount += FaceCount;
                Reader.BaseStream.Seek(VertexBuffer[0], SeekOrigin.Begin);

                RGVertexBuffer vertexBuffer;
                vertexBuffer.Unknown0x00 = Reader.ReadUInt32();
                vertexBuffer.VertexCount = Reader.ReadUInt16();
                vertexBuffer.IsLocked = Reader.ReadByte();
                vertexBuffer.Unknown0x06 = Reader.ReadByte();
                vertexBuffer.LockedData = Reader.GetDataOffset(Reader.ReadInt32());
                vertexBuffer.Unknown0x08 = Reader.ReadUInt16();
                vertexBuffer.VertexStride = Reader.ReadUInt16();
                vertexBuffer.VertexData = Reader.GetDataOffset(Reader.ReadInt32());
                vertexBuffer.LockThreadID = Reader.ReadUInt32();
                vertexBuffer.VertexFormat = Reader.ReadOffset(Reader.ReadInt32());
                vertexBuffer.VertexBuffer = Reader.ReadOffset(Reader.ReadInt32());
                Reader.BaseStream.Seek(vertexBuffer.VertexFormat, SeekOrigin.Begin);

                RGVertexFormat format;
                format.Mask = Reader.ReadUInt32();
                format.VertexSize = Reader.ReadByte();
                format.Unknown0x05 = Reader.ReadByte();
                format.Unknnown0x06 = Reader.ReadByte();
                format.DeclarationCount = Reader.ReadByte();
                format.FVFIndices = Reader.ReadUInt64();

                Vertices = new Vector3[VertexCount];
                UV = new Vector2[VertexCount];
                Normals = new Vector3[VertexCount];
                Tangents = new Vector3[VertexCount];
                VertexsColors = new System.Drawing.Color[VertexCount];
                Reader.BaseStream.Seek(DataOffset >> 28 == 6 ? Entry.FlagInfo.BaseResourceSizeV + VertexDataPointer : Entry.FlagInfo.BaseResourceSizeP + VertexDataPointer, SeekOrigin.Begin);

                ////////////// Vertexs Layouts //////////////
                for (int vertex = 0; vertex < VertexCount; ++vertex)
                {
                    for (int usageIdx = 0; usageIdx < 16; ++usageIdx)
                    {
                        if (((format.Mask >> usageIdx) & 1) == 0)
                            continue;

                        int typeIdx = (int)((format.FVFIndices >> (usageIdx << 2)) & 0x0F);

                        switch (usageIdx)
                        {
                            case 0: //Vertices
                                {
                                    float x = Reader.ReadFloat();
                                    float y = Reader.ReadFloat();
                                    float z = Reader.ReadFloat();
                                    Vertices[vertex] = new Vector3(x, y, z);
                                    break;
                                }
                            case 3: //Normals
                                {
                                    Vector3 vector = DataUtils.GetVector3FromDec3N(Reader.ReadUInt32());
                                    Normals[vertex] = new Vector3(vector.X, vector.Y, vector.Z);
                                    break;
                                }
                            case 4: //Color
                                {
                                    uint rgba = Reader.ReadUInt32();
                                    VertexsColors[vertex] = System.Drawing.Color.FromArgb((int)rgba);
                                    break;
                                }
                            case 6: //Texture coordinates
                                {
                                    float x = DataUtils.GetHalfFloat(Reader.ReadByte(), Reader.ReadByte());
                                    float y = DataUtils.GetHalfFloat(Reader.ReadByte(), Reader.ReadByte());
                                    UV[vertex] = new Vector2(x, y);
                                    break;
                                }
                            case 14: //Tangents
                                {
                                    Vector3 vector = DataUtils.GetVector3FromDec3N(Reader.ReadUInt32());
                                    Tangents[vertex] = new Vector3(vector.X, vector.Y, vector.Z);
                                    break;
                                }
                            default:
                                Reader.BaseStream.Seek(Reader.BaseStream.Position + DataUtils.VertexSizeMapping[typeIdx], SeekOrigin.Begin);
                                break;
                        }
                    }
                }

                ////////////// Indices //////////////
                Reader.BaseStream.Seek(IndexBuffer[0], SeekOrigin.Begin);
                uint Unknown0x00_ = Reader.ReadUInt32();
                uint IndexCount_ = Reader.ReadUInt32();
                int IndexDataPointer = Reader.GetDataOffset(Reader.ReadInt32());
                int IndexBufferPointer = Reader.ReadOffset(Reader.ReadInt32());
                Reader.BaseStream.Seek(Entry.FlagInfo.BaseResourceSizeV + IndexDataPointer, SeekOrigin.Begin);
 
                byte[] Indices = Reader.ReadBytes(FaceCount * 6);
                return (new uint[] { IndexCount, FaceCount }, Indices);
            }

            public enum TextureType
            {
                L8 = 2,
                DXT1 = 82, // 0x00000052
                DXT3 = 83, // 0x00000053
                DXT5 = 84, // 0x00000054
                A8R8G8B8 = 134, // 0x00000086
            }
        }

        #endregion

        #region FragDrawable

        public class FragDrawable
        {
            private IOReader Reader { get; set; }
            private FileEntry Entry { get; set; }

            private Mesh[] Meshs;
            private List<Mesh[]> MeshsList = new List<Mesh[]>();
            private MeshGeometry3D ModelMesh;
            public static List<Geometry> ModelGeometry = new List<Geometry>();
            private Model3DGroup ModelGroup = new Model3DGroup();

            private int[] m_Models = new int[4];
            private int BoneCount;
            public static uint TotalVerticeCount, TotalFaceCount, TotalGeometryCount, TotalMeshParent;

            private string[] ShaderTextureMappingNames;
            private List<ushort> ShaderTextureMapping = new List<ushort>();
            private List<string> ShaderTextureMappingNamesOrdered = new List<string>();

            private int ShaderGroupPointer;
            private int SkeletonPointer;
            private int BoneInfoPointer;
            private int TextureDataPointer;

            public static Vector3 ModelBoundsCenter;
            public static Vector3 ModelBoundsMin;
            public static Vector3 ModelBoundsMax;

            public static Vector4[] BonePositions;
            public static string[] BoneNames;
            public static int[] BoneFileOffset, BoneParentOffset;

            private Vector3[] Vertices;
            private Vector2[] UV;
            private Vector3[] Normals;
            private Vector3[] Tangents;
            private System.Drawing.Color[] VertexsColors;

            public static TextureInfo[] TexInfos;
            public static ImageSource[] ImagesSource;
            private RGTexture[] ImagesData;

            public FragDrawable(object reader, object entry)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)entry;

                TotalVerticeCount = 0;
                TotalFaceCount = 0;
                TotalGeometryCount = 0;
                TotalMeshParent = 0;
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelGeometry.Clear();
                ShaderTextureMapping.Clear();
                ShaderTextureMappingNamesOrdered.Clear();

                long offset = Reader.BaseStream.Position;
                XFD_ReadMainStructure(offset);
                (int, int[]) TextureInfo = XFT_ReadTextureStructure(); //returns (TextureCount, TexturePointer)

                if (TextureInfo.Item1 > 0)
                {
                    XFD_ReadTextureData(TextureInfo.Item1, TextureInfo.Item2);
                }
                else
                {
                    TexInfos = new TextureInfo[0];
                    ImagesSource = new ImageSource[0];
                    ImagesData = new RGTexture[0];
                }

                Reader.BaseStream.Seek(offset + 0x8, SeekOrigin.Begin);
                XFD_ReadDrawableStructure();
                //XFD_ReadShaderGroup();
                XFD_ReadSkeletonData();
                XFD_ReadBoneData();
                XFD_ReadDrawableData();

                //Add model
                NewModel = ModelGroup;
                Vector3D axis = new Vector3D(1, 0, 0);
                Matrix3D matrix = NewModel.Transform.Value;
                matrix.Rotate(new Quaternion(axis, 90));
                NewModel.Transform = new MatrixTransform3D(matrix);

                NewPoints = NewModel.GetModelBoundsPoints();
                NewModelName = "";
                CurrentModelMesh = MeshsList;
            }

            private void XFD_ReadMainStructure(long offset)
            {
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());

                Reader.BaseStream.Seek(offset + 0xC, SeekOrigin.Begin);
                TextureDataPointer = Reader.ReadOffset(Reader.ReadInt32());
            }

            private (int, int[]) XFT_ReadTextureStructure()
            {
                if (TextureDataPointer == 0x0)
                    return (0, null);

                Reader.BaseStream.Seek(TextureDataPointer, SeekOrigin.Begin);
                uint Unknown_0x00 = Reader.ReadUInt32();
                int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
                byte Unknown_0x08 = Reader.ReadByte();
                byte Unknown_0x09 = Reader.ReadByte();
                ushort Unknown_0x0A = Reader.ReadUInt16();
                uint Unknown_0x0C = Reader.ReadUInt32();

                if (Unknown_0x0C != 1)
                    return (0, null);

                RGArray TextureNameHashs;
                TextureNameHashs.Offset = Reader.ReadOffset(Reader.ReadInt32());
                TextureNameHashs.Count = Reader.ReadUInt16();
                TextureNameHashs.Size = Reader.ReadUInt16();

                RGArray Textures;
                Textures.Offset = Reader.ReadOffset(Reader.ReadInt32());
                Textures.Count = Reader.ReadUInt16();
                Textures.Size = Reader.ReadUInt16();

                TexInfos = new TextureInfo[Textures.Count];
                ImagesSource = new ImageSource[Textures.Count];
                ImagesData = new RGTexture[Textures.Count];

                //TextureNameHashs
                Reader.BaseStream.Seek(TextureNameHashs.Offset, SeekOrigin.Begin);
                uint[] Hashs = new uint[TextureNameHashs.Count];
                for (int i = 0; i < TextureNameHashs.Count; i++)
                    Hashs[i] = Reader.ReadUInt32();

                //Textures
                Reader.BaseStream.Seek(Textures.Offset, SeekOrigin.Begin);
                int[] TexturesPointer = new int[Textures.Count];
                for (int i = 0; i < Textures.Count; i++)
                {
                    TexturesPointer[i] = Reader.ReadOffset(Reader.ReadInt32());
                }
                return (Textures.Count, TexturesPointer);
            }

            private void XFD_ReadTextureData(int textureCount, int[] texturePointer)
            {
                for (int i = 0; i < textureCount; i++)
                {
                    Reader.BaseStream.Seek(texturePointer[i], SeekOrigin.Begin);
                    uint Unknown_0x00 = Reader.ReadUInt32();
                    int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
                    byte Unknown_0x08 = Reader.ReadByte();
                    byte Unknown_0x09 = Reader.ReadByte();
                    ushort Unknown_0x0A = Reader.ReadUInt16();
                    uint Unknown_0x0C = Reader.ReadUInt32();

                    if (Unknown_0x0A != 1)
                        return;

                    uint Unknown_0x10 = ImagesData[i].Resource.Unknown_0x00 = Reader.ReadUInt32();
                    uint TextureSize = ImagesData[i].DataSize = Reader.ReadUInt32();
                    int TextureNamePointer = ImagesData[i].Resource.NameTexturePointer = Reader.ReadOffset(Reader.ReadInt32());
                    int D3DStructurePointer = ImagesData[i].Resource.BaseTexturePointer = Reader.ReadOffset(Reader.ReadInt32());
                    ushort Width = ImagesData[i].Resource.Width = Reader.ReadUInt16();
                    ushort Height = ImagesData[i].Resource.Height = Reader.ReadUInt16();
                    int MipMapCount = ImagesData[i].Resource.MipMapCount = Reader.ReadInt32();
                    Vector3 Unknown_0x24 = ImagesData[i].Resource.Unknown_0x18 = Reader.ReadVector3();
                    Vector3 Unknown_0x30 = ImagesData[i].Resource.Unknown_0x24 = Reader.ReadVector3();
                    string Name = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer, true);

                    //BaseAdress
                    Reader.BaseStream.Seek(D3DStructurePointer + 0x20, SeekOrigin.Begin);
                    uint Value = Reader.ReadUInt32();
                    uint BaseAdress = Value >> 0xC;
                    int CurrentTextureDataPointer = ((int)(BaseAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                    //MipAdress
                    Reader.BaseStream.Seek(D3DStructurePointer + 0x30, SeekOrigin.Begin);
                    uint Value_ = Reader.ReadUInt32();
                    uint MipMapAdress = Value_ >> 0xC;
                    int CurrentMipDataPointer = ((int)(MipMapAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                    //Read texture data
                    Reader.BaseStream.Seek(D3DStructurePointer + 0x23, SeekOrigin.Begin);
                    Texture.TextureType Format = Reader.ReadByte() == 0x52 ? Texture.TextureType.DXT1 : Texture.TextureType.DXT5;

                    try
                    {
                        TexInfos[i] = new TextureInfo()
                        {
                            TextureName = Name,
                            Width = Width,
                            Height = Height,
                            MipMaps = MipMapCount,
                            TextureSize = TextureSize,
                            TextureDataPointer = CurrentTextureDataPointer,
                            MipDataPointer = CurrentMipDataPointer,
                            PixelFormat = Format
                        };

                        byte[] buffer = ReadTextureInfo(Reader, 0, TexInfos[i]);
                        ImagesSource[i] = LoadImage(buffer);
                    }
                    catch { TexInfos[i] = null; ImagesSource[i] = null; }

                    if (CurrentMipDataPointer == 0)
                    {
                        TexInfos[i] = null;
                        ImagesSource[i] = null;
                        continue;
                    }
                }
            }

            public void XFD_ReadDrawableStructure()
            {
                Reader.BaseStream.Seek(Reader.ReadOffset(Reader.ReadInt32()), SeekOrigin.Begin);
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());

                ////////////// ShaderGroup //////////////
                ShaderGroupPointer = Reader.ReadOffset(Reader.ReadInt32());

                ////////////// Skeleton //////////////
                SkeletonPointer = Reader.ReadOffset(Reader.ReadInt32());

                ////////////// LODGroup //////////////
                Vector4 m_vCenter = Reader.ReadVector4();
                Vector4 m_vBoundsMin = Reader.ReadVector4();
                Vector4 m_vBoundsMax = Reader.ReadVector4();
                ModelBoundsCenter = new Vector3(m_vCenter.X, m_vCenter.Y, m_vCenter.Z);
                ModelBoundsMin = new Vector3(m_vBoundsMin.X, m_vBoundsMin.Y, m_vBoundsMin.Z);
                ModelBoundsMax = new Vector3(m_vBoundsMax.X, m_vBoundsMax.Y, m_vBoundsMax.Z);

                for (int i = 0; i < 4; i++)
                    m_Models[i] = Reader.ReadOffset(Reader.ReadInt32());

                Vector4 Unknown0x40 = Reader.ReadVector4();
                uint[] m_dwShaderUseMask = new uint[4];
                for (int i = 0; i < 4; i++)
                    m_dwShaderUseMask[i] = Reader.ReadUInt32();

                float m_fRadius = Reader.ReadFloat();
                uint Unknown0x64 = Reader.ReadUInt32();
                int Unknown0x68 = Reader.ReadOffset(Reader.ReadInt32());
                int Unknown0x6C = Reader.ReadOffset(Reader.ReadInt32());
            }

            public void XFD_ReadShaderGroup()
            {
                Reader.BaseStream.Seek(ShaderGroupPointer, SeekOrigin.Begin);

                uint Unknown0x00 = Reader.ReadUInt32();
                uint Unknown0x04 = Reader.ReadUInt32();
                RGArray ShadersData;
                ShadersData.Offset = Reader.ReadOffset(Reader.ReadInt32());
                ShadersData.Count = Reader.ReadUInt16();
                ShadersData.Size = Reader.ReadUInt16();

                uint[]Unknown0x10 = new uint[4];
                for (int i = 0; i < Unknown0x10.Length; i++)
                    Unknown0x10[i] = Reader.ReadUInt32();

                int[] ShaderPointers = new int[ShadersData.Count];
                Reader.BaseStream.Seek(ShadersData.Offset, SeekOrigin.Begin);
                for (int i = 0; i < ShadersData.Count; i++)
                    ShaderPointers[i] = Reader.ReadOffset(Reader.ReadInt32());

                ShaderTextureMappingNames = new string[ShaderPointers.Length];
                for (int i = 0; i < ShaderPointers.Length; i++)
                {
                    Reader.BaseStream.Seek(ShaderPointers[i], SeekOrigin.Begin);
                    XFD_ReadShaderFX(i);
                }
            }

            public void XFD_ReadShaderFX(int index)
            {
                int ParametersPointer = Reader.ReadOffset(Reader.ReadInt32()); //Useless
                uint ShaderNameHash = Reader.ReadUInt32(); //Hash
                string ShaderName = ((ShaderNameTerrains)Enum.Parse(typeof(ShaderNameTerrains), ShaderNameHash.ToString(), true)).ToString();
                byte ParameterCount = Reader.ReadByte();
                byte Unknown0x09 = Reader.ReadByte();
                byte Unknown0x0A = Reader.ReadByte();
                byte Unknown0x0B = Reader.ReadByte();
                ushort Unknown0x0C = Reader.ReadUInt16();
                ushort Unknown0x0E = Reader.ReadUInt16();

                uint[] Unknown0x10 = new uint[4];
                for (int i = 0; i < Unknown0x10.Length; i++)
                    Unknown0x10[i] = Reader.ReadUInt32();

                int attempt = -1;
                long Offset = Reader.BaseStream.Position;
                Redo:
                attempt++;

                if (attempt > 0)
                {
                    Reader.BaseStream.Seek(Reader.BaseStream.Position + 0x8, SeekOrigin.Begin);
                    Offset = Reader.BaseStream.Position;
                }

                for (int i = 0; i < 1; i++) //ParameterCount, we're only using the first item
                {
                    byte RegisterCount = Reader.ReadByte();
                    byte RegisterIndex = Reader.ReadByte();
                    byte DataType = Reader.ReadByte();
                    _ = Reader.ReadByte();

                    Vector4[] ShaderItemParameter = new Vector4[RegisterCount];
                    int ShaderMappingPointer = Reader.ReadOffset(Reader.ReadInt32());
                    if (ShaderMappingPointer == 0x0)
                    {
                        Reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
                        goto Redo;
                    }
                    long currentOffset = Reader.BaseStream.Position;

                    if (RegisterCount > 0)
                    {
                        Reader.BaseStream.Seek(ShaderMappingPointer, SeekOrigin.Begin);
                        for (int data = 0; data < RegisterCount; data++)
                        {
                            ShaderItemParameter[data] = Reader.ReadVector4(); //+0xC0
                        }
                        Reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                    }
                    else
                    {
                        Reader.BaseStream.Seek(ShaderMappingPointer + 0x18, SeekOrigin.Begin);
                        int ShaderTextureNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                        ShaderTextureMappingNames[index] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, ShaderTextureNamePointer, true);
                        Reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                    }
                }
            }

            public void XFD_ReadSkeletonData()
            {
                Reader.BaseStream.Seek(SkeletonPointer, SeekOrigin.Begin);
                BoneInfoPointer = Reader.ReadOffset(Reader.ReadInt32());
                int ParentIndices = Reader.ReadOffset(Reader.ReadInt32());
                int ModelTransforms = Reader.ReadOffset(Reader.ReadInt32());
                int ModelInverses = Reader.ReadOffset(Reader.ReadInt32());
                int RelativeTransforms = Reader.ReadOffset(Reader.ReadInt32());
                int AbsoluteTransforms = Reader.ReadOffset(Reader.ReadInt32());
                BoneCount = Reader.ReadUInt16();
                ushort Unknown0x1A = Reader.ReadUInt16();
                ushort Unknown0x1C = Reader.ReadUInt16();
                ushort Unknown0x1E = Reader.ReadUInt16();
                uint Unknown0x20 = Reader.ReadUInt32();

                RGArray BoneStructure;
                BoneStructure.Offset = Reader.ReadOffset(Reader.ReadInt32());
                BoneStructure.Count = Reader.ReadUInt16();
                BoneStructure.Size = Reader.ReadUInt16();
                uint Unknown0x2C = Reader.ReadUInt32();
            }

            public void XFD_ReadBoneData()
            {
                BoneNames = new string[BoneCount];
                BoneFileOffset = new int[BoneCount];
                BoneParentOffset = new int[BoneCount];
                BonePositions = new Vector4[BoneCount];
                Reader.BaseStream.Seek(BoneInfoPointer, SeekOrigin.Begin);

                for (int i = 0; i < BoneCount; i++)
                {
                    BoneFileOffset[i] = (int)Reader.BaseStream.Position; //Saving the current bone offset
                    int m_pszBoneName = Reader.ReadOffset(Reader.ReadInt32());
                    BoneNames[i] = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, m_pszBoneName, true);

                    uint m_dwFlags = Reader.ReadUInt32();
                    int m_pNextSibling = Reader.ReadOffset(Reader.ReadInt32());
                    int m_pFirstChild = Reader.ReadOffset(Reader.ReadInt32());
                    BoneParentOffset[i] = Reader.ReadOffset(Reader.ReadInt32()); //Saving the current bone parent offset
                    ushort m_wBoneIndex = Reader.ReadUInt16();
                    ushort m_wBoneId = Reader.ReadUInt16();
                    ushort m_wMirror = Reader.ReadUInt16();
                    byte m_nbTransFlags = Reader.ReadByte();
                    byte m_nbRotFlags = Reader.ReadByte();
                    byte m_nbScaleFlags = Reader.ReadByte();

                    byte[] __pad_1D = new byte[3];
                    for (int i1 = 0; i1 < 3; i1++)
                        __pad_1D[i1] = Reader.ReadByte();

                    Vector3 m_pos = Reader.ReadVector3();
                    uint m_dwKey = Reader.ReadUInt32();
                    Vector4 m_vRotationEuler = Reader.ReadVector4();
                    Vector4 m_vRotationQuaternion = Reader.ReadVector4();
                    Vector4 m_vScale = Reader.ReadVector4();
                    BonePositions[i] = Reader.ReadVector4();
                    Vector4 m_vOrient = Reader.ReadVector4();
                    Vector4 m_vSorient = Reader.ReadVector4();
                    Vector4 m_vTransMin = Reader.ReadVector4();
                    Vector4 m_vTransMax = Reader.ReadVector4();
                    Vector4 m_vRotMin = Reader.ReadVector4();
                    Vector4 m_vRotMax = Reader.ReadVector4();

                    int m_pJointData = Reader.ReadOffset(Reader.ReadInt32());
                    uint _fD4 = Reader.ReadUInt32();
                    uint _fD8 = Reader.ReadUInt32();
                    uint _fDC = Reader.ReadUInt32();
                }
            }

            public void XFD_ReadDrawableData()
            {
                int m_ModelCount = 0, m_ModelIndex = 0;
                for (int i = 0; i < m_Models.Length; i++)
                {
                    if (m_Models[i] == 0)
                        continue;

                    m_ModelCount++;
                    m_ModelIndex = i;
                }

                int index = 0;
                for (int model = 0; model < m_ModelCount; ++model)
                {
                    Reader.BaseStream.Seek(m_Models[m_ModelIndex], SeekOrigin.Begin);

                    RGArray CurrentModelMesh;
                    CurrentModelMesh.Offset = Reader.ReadOffset(Reader.ReadInt32());
                    TotalMeshParent = CurrentModelMesh.Count = Reader.ReadUInt16();
                    CurrentModelMesh.Size = Reader.ReadUInt16();

                    Reader.BaseStream.Seek(CurrentModelMesh.Offset, SeekOrigin.Begin);
                    int[] CurrentModelMeshPointer = new int[CurrentModelMesh.Count];
                    for (int i = 0; i < CurrentModelMesh.Count; i++)
                    {
                        CurrentModelMeshPointer[i] = Reader.ReadOffset(Reader.ReadInt32());
                    }

                    for (uint modelMesh = 0; modelMesh < CurrentModelMesh.Count; modelMesh++)
                    {
                        Reader.BaseStream.Seek(CurrentModelMeshPointer[modelMesh], SeekOrigin.Begin);
                        RGModel ModelGeometryData;
                        ModelGeometryData.Unknown0x00 = Reader.ReadUInt32();
                        ModelGeometryData.Geometry.Offset = Reader.ReadOffset(Reader.ReadInt32());
                        ModelGeometryData.Geometry.Count = Reader.ReadUInt16();
                        ModelGeometryData.Geometry.Size = Reader.ReadUInt16();
                        ModelGeometryData.Unknown0x0C = Reader.ReadOffset(Reader.ReadInt32());
                        ModelGeometryData.Unknown0x10 = Reader.ReadOffset(Reader.ReadInt32());

                        uint Unknown0x14 = Reader.ReadUInt32();
                        ushort Unknown0x18 = Reader.ReadUInt16();
                        ushort ReferenceCount = Reader.ReadUInt16();
                        Reader.BaseStream.Seek(ModelGeometryData.Unknown0x10, SeekOrigin.Begin);

                        for (int i = 0; i < ReferenceCount; i++)
                        {
                            ShaderTextureMapping.Add(Reader.ReadUInt16());
                        }

                        RGArray ModelCurrentGeometry = ModelGeometryData.Geometry;
                        Reader.BaseStream.Seek(ModelCurrentGeometry.Offset, SeekOrigin.Begin);

                        int[] ModelGeometryDataPointer = new int[ModelGeometryData.Geometry.Count];
                        for (int i = 0; i < ModelCurrentGeometry.Count; ++i)
                            ModelGeometryDataPointer[i] = Reader.ReadOffset(Reader.ReadInt32());

                        TotalGeometryCount += ModelCurrentGeometry.Count;
                        XFD_ReadGeometryData(ModelCurrentGeometry.Count, ModelGeometryDataPointer, ref index);
                    }
                }
            }

            public void XFD_ReadGeometryData(int geometryCount, int[] modelGeometryDataPointer, ref int index)
            {
                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                Meshs = new Mesh[geometryCount];

                for (int count = 0; count < geometryCount; ++count)
                {
                    Reader.BaseStream.Seek(modelGeometryDataPointer[count], SeekOrigin.Begin);
                    (uint[], byte[]) Model3DInfo = XFD_ReadModel3DData(count); //0 = IndexCount, 1 = FaceCount, 2 = Indices, 3 = VertexColor
                    ModelMesh = new MeshGeometry3D();

                    ////////////// Vertices //////////////
                    foreach (Vector3 vertex in Vertices)
                    {
                        ModelMesh.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
                    }

                    ////////////// Faces //////////////
                    ushort[] indices = new ushort[Model3DInfo.Item1[1] * 3];
                    for (int i = 0; i < Model3DInfo.Item1[1] * 3; i++)
                    {
                        indices[i] = BitConverter.ToUInt16(Model3DInfo.Item2, i * 2);

                        if (!isSwitchVersion)
                        {
                            indices[i] = indices[i].Swap();
                        }
                    }
                    for (int i = 0; i < Model3DInfo.Item1[1]; i++)
                    {
                        ModelMesh.TriangleIndices.Add(indices[i * 3 + 0]);
                        ModelMesh.TriangleIndices.Add(indices[i * 3 + 1]);
                        ModelMesh.TriangleIndices.Add(indices[i * 3 + 2]);
                    }

                    ////////////// Texture Coordinates //////////////
                    PointCollection coords = new PointCollection();
                    for (int i = 0; i < UV.Length; i++)
                    {
                        ModelMesh.TextureCoordinates.Add(new System.Windows.Point(UV[i].X, UV[i].Y));
                    }

                    ////////////// Normals //////////////
                    for (int i = 0; i < Normals.Length; i++)
                    {
                        ModelMesh.Normals.Add(new Vector3D(Normals[i].X, Normals[i].Y, Normals[i].Z));
                    }

                    ////////////// Material //////////////
                    Random random = new Random();
                    Material material = MaterialHelper.CreateMaterial(Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255)));
                    ushort CurrentSubMeshShaderIndex;
                    string CurrentSubMeshShaderName = "";

                    if (!isSwitchVersion)
                    {
                        try
                        {
                            CurrentSubMeshShaderIndex = ShaderTextureMapping[index];
                            CurrentSubMeshShaderName = ShaderTextureMappingNames[CurrentSubMeshShaderIndex];
                        }
                        catch { }


                        if (string.IsNullOrEmpty(CurrentSubMeshShaderName))
                            ShaderTextureMappingNamesOrdered.Add("Unknown");
                        else
                            ShaderTextureMappingNamesOrdered.Add(CurrentSubMeshShaderName);

                        if (CurrentSubMeshShaderName != null)
                        {
                            for (int i = 0; i < TexInfos.Count(); i++)
                            {
                                if (TexInfos[i].TextureName.Contains(CurrentSubMeshShaderName.ToLower()))
                                {
                                    ImageBrush brush = new ImageBrush
                                    {
                                        ImageSource = ImagesSource[i],
                                        TileMode = TileMode.Tile,
                                        ViewportUnits = BrushMappingMode.Absolute
                                    };

                                    if (brush.ImageSource != null)
                                        material = new DiffuseMaterial(brush);
                                }
                            }
                        }
                    }       

                    ////////////// Add submesh to global mesh //////////////
                    Model3DGroup Group = new Model3DGroup();
                    GeometryModel3D GeometryModel = new GeometryModel3D(ModelMesh, material)
                    {
                        BackMaterial = material
                    };

                    Mesh mesh = new Mesh(ModelMesh, material);
                    ModelGeometry.Add(new Geometry(mesh, CurrentSubMeshShaderName ?? "Unknown", (int)Model3DInfo.Item1[0], (int)Model3DInfo.Item1[1], Vertices.Length, Vertices, UV, indices, Normals, Tangents, VertexsColors, 0, 0, 0));
                    Group.Children.Add(GeometryModel);
                    ModelGroup.Children.Add(Group);
                    Meshs[count] = mesh;
                    index++;
                }
                MeshsList.Add(Meshs);
            }

            public (uint[], byte[]) XFD_ReadModel3DData(int Index)
            {
                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;

                uint Unknown0x00 = Reader.ReadUInt32();
                uint DeclarationIndex = Reader.ReadUInt32();
                uint Unknown0x08 = Reader.ReadUInt32();

                int[] VertexBuffer = new int[4];
                for (int i = 0; i < 4; i++)
                    VertexBuffer[i] = Reader.ReadOffset(Reader.ReadInt32());

                int[] IndexBuffer = new int[4];
                for (int i = 0; i < 4; i++)
                    IndexBuffer[i] = Reader.ReadOffset(Reader.ReadInt32());

                uint IndexCount = Reader.ReadUInt32();
                uint FaceCount = Reader.ReadUInt32();
                ushort VertexCount = Reader.ReadUInt16();
                byte PrimitiveType = Reader.ReadByte(); //Triangles
                byte Unknown0x37 = Reader.ReadByte();
                int JointMapPointer = Reader.ReadOffset(Reader.ReadInt32());
                ushort VertexStride = Reader.ReadUInt16();
                ushort JointCount = Reader.ReadUInt16();
                int DataOffset = Reader.ReadInt32();
                int VertexDataPointer = Reader.GetDataOffset(DataOffset);

                TotalVerticeCount += VertexCount;
                TotalFaceCount += FaceCount;
                Reader.BaseStream.Seek(VertexBuffer[0], SeekOrigin.Begin);

                RGVertexBuffer vertexBuffer;
                vertexBuffer.Unknown0x00 = Reader.ReadUInt32();
                vertexBuffer.VertexCount = Reader.ReadUInt16();
                vertexBuffer.IsLocked = Reader.ReadByte();
                vertexBuffer.Unknown0x06 = Reader.ReadByte();
                vertexBuffer.LockedData = Reader.GetDataOffset(Reader.ReadInt32());
                vertexBuffer.Unknown0x08 = Reader.ReadUInt16();
                vertexBuffer.VertexStride = Reader.ReadUInt16();
                vertexBuffer.VertexData = Reader.GetDataOffset(Reader.ReadInt32());
                vertexBuffer.LockThreadID = Reader.ReadUInt32();
                vertexBuffer.VertexFormat = Reader.ReadOffset(Reader.ReadInt32());
                vertexBuffer.VertexBuffer = Reader.ReadOffset(Reader.ReadInt32());
                Reader.BaseStream.Seek(vertexBuffer.VertexFormat, SeekOrigin.Begin);

                RGVertexFormat format;
                format.Mask = Reader.ReadUInt32();
                format.VertexSize = Reader.ReadByte();
                format.Unknown0x05 = Reader.ReadByte();
                format.Unknnown0x06 = Reader.ReadByte();
                format.DeclarationCount = Reader.ReadByte();
                format.FVFIndices = Reader.ReadUInt64();

                Vertices = new Vector3[VertexCount];
                UV = new Vector2[VertexCount];
                Normals = new Vector3[VertexCount];
                Tangents = new Vector3[VertexCount];
                VertexsColors = new System.Drawing.Color[VertexCount];

                long VertexOffset = DataOffset >> 28 == 6 ? Entry.FlagInfo.BaseResourceSizeV + VertexDataPointer : Entry.FlagInfo.BaseResourceSizeP + VertexDataPointer;
                Reader.BaseStream.Seek(isSwitchVersion ? vertexBuffer.VertexData : VertexOffset, SeekOrigin.Begin);

                ////////////// Vertexs Layouts //////////////
                for (int vertex = 0; vertex < VertexCount; ++vertex)
                {
                    for (int usageIdx = 0; usageIdx < 16; ++usageIdx)
                    {
                        if (((format.Mask >> usageIdx) & 1) == 0)
                            continue;

                        int typeIdx = (int)((format.FVFIndices >> (usageIdx << 2)) & 0x0F);

                        switch (usageIdx)
                        {
                            case 0: //Vertices
                                {
                                    float x = Reader.ReadFloat();
                                    float y = Reader.ReadFloat();
                                    float z = Reader.ReadFloat();
                                    Vertices[vertex] = new Vector3(x, y, z);
                                    break;
                                }
                            case 3: //Normals
                                {
                                    Vector3 vector = DataUtils.GetVector3FromDec3N(Reader.ReadUInt32());
                                    Normals[vertex] = new Vector3(vector.X, vector.Y, vector.Z);
                                    break;
                                }
                            case 4: //Color
                                {
                                    uint rgba = Reader.ReadUInt32();
                                    VertexsColors[vertex] = System.Drawing.Color.FromArgb((int)rgba);
                                    break;
                                }
                            case 6: //Texture coordinates
                                {
                                    float x = DataUtils.GetHalfFloat(Reader.ReadByte(), Reader.ReadByte());
                                    float y = DataUtils.GetHalfFloat(Reader.ReadByte(), Reader.ReadByte());
                                    UV[vertex] = new Vector2(x, y);
                                    break;
                                }
                            case 14: //Tangents
                                {
                                    Vector3 vector = DataUtils.GetVector3FromDec3N(Reader.ReadUInt32());
                                    Tangents[vertex] = new Vector3(vector.X, vector.Y, vector.Z);
                                    break;
                                }
                            default:
                                Reader.BaseStream.Seek(Reader.BaseStream.Position + DataUtils.VertexSizeMapping[typeIdx], SeekOrigin.Begin);
                                break;
                        }
                    }
                }

                ////////////// Indices //////////////
                Reader.BaseStream.Seek(IndexBuffer[0], SeekOrigin.Begin);
                uint Unknown0x00_ = Reader.ReadUInt32();
                uint IndexCount_ = Reader.ReadUInt32();
                int IndexDataPointer = Reader.GetDataOffset(Reader.ReadInt32());
                int IndexBufferPointer = Reader.ReadOffset(Reader.ReadInt32());
                Reader.BaseStream.Seek(Entry.FlagInfo.BaseResourceSizeV + IndexDataPointer, SeekOrigin.Begin);
                byte[] Indices = Reader.ReadBytes(FaceCount * 6);

                return (new uint[] { IndexCount, FaceCount }, Indices);
            }

            public enum TextureType
            {
                L8 = 2,
                DXT1 = 82, // 0x00000052
                DXT3 = 83, // 0x00000053
                DXT5 = 84, // 0x00000054
                A8R8G8B8 = 134, // 0x00000086
            }
        }

        #endregion

        #region Controls

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewTextures.SelectedItems.Count <= 0)
                return;

            string textureName = listViewTextures.SelectedItems[0].Text;
            var images = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;

            CurrentImageIndex = -1;
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].TextureName == textureName)
                    CurrentImageIndex = i;
            }

            bool UseFrags = FragType.ImagesSource != null;
            if ((FragType.ImagesSource == null && FragDrawable.ImagesSource == null) || CurrentImageIndex == -1) //Shouldn't be the case
                return;
            if (UseFrags ? FragType.ImagesSource[CurrentImageIndex] == null : FragDrawable.ImagesSource[CurrentImageIndex] == null)
                return;

            imageContainer.Image = System.Drawing.Image.FromStream(new MemoryStream(Texture.BufferFromImageSource(UseFrags ? FragType.ImagesSource[CurrentImageIndex] : FragDrawable.ImagesSource[CurrentImageIndex])));
            trackBar1.Maximum = images[CurrentImageIndex].MipMaps;
            trackBar1.Value = 1;
            currentImage = imageContainer.Image;
            labelCurrentMipmap.Text = string.Format("MipMap 1 - Size {0}x{1}", images[CurrentImageIndex].Height, images[CurrentImageIndex].Width);
        }

        private void checkBoxShowBounds_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxShowBounds.Checked)
                ModelChangedFlags = 0x2;
            else
                ModelChangedFlags = 0x3;
        }

        private void checkBoxShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            NewGridVisibility = checkBoxShowGrid.Checked;
        }

        private void comboBoxGridSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            NewGridSize = Convert.ToInt32(comboBoxGridSize.Items[comboBoxGridSize.SelectedIndex]);
        }

        private void checkBoxWireframeChanged(object sender, EventArgs e)
        {
            UseWireframe = checkBoxWireframe.Checked;
        }

        private void comboBoxSelectColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBoxSelectColor.SelectedIndex;
            ColorDialog dialog = new ColorDialog();

            switch (index)
            {
                case 0:
                    {
                        dialog.AllowFullOpen = true;
                        dialog.ShowHelp = true;

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            NewFirstGradientColor = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                            RPF6FileNameHandler.CustomColor1 = NewFirstGradientColor;
                            RPF6FileNameHandler.SaveSettings();

                            if (!NewColorChange)
                            {
                                MessageBox.Show("Background color succesfully changed.\n\nMake sure \"Use Custom Color\" is checked to fully save colors.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                NewColorChange = true;
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        dialog.AllowFullOpen = true;
                        dialog.ShowHelp = true;

                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            NewFirstGradientColor = Color.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
                            RPF6FileNameHandler.CustomColor1 = NewFirstGradientColor;
                            RPF6FileNameHandler.SaveSettings();

                            if (!NewColorChange)
                            {
                                MessageBox.Show("Background color succesfully changed.\n\nMake sure \"Use Custom Color\" is checked to fully save colors.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                NewColorChange = true;
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        NewFirstGradientColor = Color.FromArgb(255, 104, 138, 213);
                        NewSecondGradientColor = Color.FromArgb(255, 66, 92, 148);
                        RPF6FileNameHandler.SaveSettings();
                        MessageBox.Show("Colors were successfully reset to default.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    break;
                default:
                    break;
            }
            BeginInvoke((MethodInvoker)delegate { comboBoxSelectColor.Text = "Select Colors"; });
        }

        private void ModelViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            comboBoxGridSize.Text = "8";
            comboBoxSelectColor.Text = "Select Colors";
            checkBoxShowGrid.Checked = false;
            checkBoxShowBounds.Checked = false;
            checkBoxWireframe.Checked = false;
            checkBoxVertices.Checked = false;
        }

        private void checkBoxVertices_CheckedChanged(object sender, EventArgs e)
        {
            UseVertices = checkBoxVertices.Checked;
        }

        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.Filter = "WaveFront Object File|*.obj";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FileStream file = new FileStream(dialog.FileName, FileMode.Create);
                string mtlPath = Path.ChangeExtension(file.Name, ".mtl");
                string ddsPath = Path.ChangeExtension(file.Name, ".dds");
                ObjExporter obj = new ObjExporter
                {
                    SwitchYZ = true,
                    ExportNormals = true,
                    Comment = "Exported with MagicRDR by Im Foxxyyy",
                    MaterialsFile = mtlPath
                };
                obj.Export(NewModel, file);

                ImageSource[] images = UseFrag ? FragType.ImagesSource : FragDrawable.ImagesSource;
                TextureInfo[] texInfos = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;
                StreamWriter mtlWriter = new StreamWriter(mtlPath);
                List<Geometry> geometry = UseFrag ? FragType.ModelGeometry : FragDrawable.ModelGeometry;

                for (int i = 0; i < geometry.Count; i++)
                {
                    if (geometry[i] == null)
                        continue;
                    if (geometry[i].TextureName == "Unknown")
                        continue;

                    mtlWriter.WriteLine("newmtl mat" + (i + 1).ToString());
                    mtlWriter.WriteLine("map_Kd " + geometry[i].TextureName.Replace(".dds", ".png"));
                }
                mtlWriter.Close();

                //Make sure we can read the material path
                string[] buffer = File.ReadAllLines(file.Name);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].Contains("mtllib"))
                    {
                        buffer[i] = buffer[i].Replace("./", "");
                        break;
                    }
                }
                File.WriteAllLines(file.Name, buffer);

                if (images == null)
                {
                    MessageBox.Show("Successfully exported model!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else if (MessageBox.Show("Successfully exported model!\n\nWould you like to also export texture(s) ?", "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                {
                    return;
                }

                //Export all textures
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null)
                    {
                        continue;
                    }

                    int index = ddsPath.LastIndexOf('\\') + 1;
                    string userInput = ddsPath.Substring(index, ddsPath.Length - index - 4) + ".dds";
                    string texturePath = ddsPath.Replace(userInput, texInfos[i].TextureName).Replace(".dds", ".png");

                    using (var fileStream = new FileStream(texturePath, FileMode.Create))
                    {
                        
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)images[i]));
                        encoder.Save(fileStream);
                    }
                }
                MessageBox.Show("All textures exported!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (imageContainer.Image == null)
            {
                return;
            }

            int value = trackBar1.Value; //Current mipmap
            TextureInfo[] images = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;

            if (images == null || images[CurrentImageIndex] == null)
                return;

            try
            {
                byte[] buffer = ReadTextureInfo(Reader, value - 1, images[CurrentImageIndex]);
                BitmapSource image = LoadImage(buffer);

                if (image != null)
                {
                    imageContainer.Image = System.Drawing.Image.FromStream(new MemoryStream(BufferFromImageSource(image)));
                    labelCurrentMipmap.Text = string.Format("MipMap {0} - Size {1}x{2}", value, image.Height, image.Width);
                }
            }
            catch { labelCurrentMipmap.Text = "MipMap Error!"; }
        }

        private void exportCurrentTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextureInfo[] texInfos = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export",
                Filter = "Portable Network Graphics File|*.png",
                FileName = texInfos[CurrentImageIndex].TextureName.Substring(0, texInfos[CurrentImageIndex].TextureName.Length - 4) ?? ""
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImageSource[] images = UseFrag ? FragType.ImagesSource : FragDrawable.ImagesSource;
                int index = dialog.FileName.LastIndexOf('\\') + 1;
                string userInput = dialog.FileName.Substring(index, dialog.FileName.Length - index - 4) + ".dds";
                string texturePath = dialog.FileName.Replace(userInput, texInfos[CurrentImageIndex].TextureName).Replace(".dds", ".png");

                using (var fileStream = new FileStream(texturePath, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)images[CurrentImageIndex]));
                    encoder.Save(fileStream);
                }
                MessageBox.Show("Successfully exported current texture", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void exportAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Choose a destination to export textures";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImageSource[] images = UseFrag ? FragType.ImagesSource : FragDrawable.ImagesSource;
                TextureInfo[] texInfos = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;

                int exported = 0;
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null)
                    {
                        continue;
                    }

                    string texturePath = dialog.SelectedPath + "\\\\" + texInfos[i].TextureName.Replace(".dds", ".png");
                    using (var fileStream = new FileStream(texturePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)images[i]));
                        encoder.Save(fileStream);
                        exported++;
                    }
                }
                MessageBox.Show(string.Format("Successfully exported {0} textures", exported), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void rebuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!fileEdited)
            {
                MessageBox.Show("You didn't import any textures, no need to save !", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                //Recompress into a single block
                Reader.BaseStream.Position = 0x0;
                byte[] newData = Reader.ReadBytes(Reader.BaseStream.Length);
                newData = DataUtils.CompressLZX(newData);
                byte[] buffer = new byte[HeaderData.Length + newData.Length];

                byte[] dataLength = BitConverter.GetBytes(newData.Length.Swap());
                HeaderData[20] = dataLength[0];
                HeaderData[21] = dataLength[1];
                HeaderData[22] = dataLength[2];
                HeaderData[23] = dataLength[3];

                Buffer.BlockCopy(HeaderData, 0, buffer, 0, HeaderData.Length);
                Buffer.BlockCopy(newData, 0, buffer, HeaderData.Length, newData.Length);

                TOCSuperEntry selectedReplaceFile = Entry;
                TOCSuperEntry selectedFile = Entry;
                NewImportReplaceForm importReplaceForm = new NewImportReplaceForm(true, Entry.SuperParent, selectedFile, selectedReplaceFile, null, Entry.Entry.Name, false, new MemoryStream(buffer));

                if (!importReplaceForm.IsDisposed)
                    importReplaceForm.ShowDialog();
                if (importReplaceForm.TOCResult == null)
                    return;

                selectedFile.CustomDataStream?.Close();
                selectedFile.CustomDataStream = importReplaceForm.TOCResult.CustomDataStream;
                selectedFile.Entry = importReplaceForm.TOCResult.Entry;
                selectedFile.OldEntry = importReplaceForm.TOCResult.OldEntry;
                selectedFile.ReadBackFromRPF = false;

                MessageBox.Show("Succesfully rebuilt the file !\nMake sure to save the .RPF", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while saving the file :\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void importTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageContainer.Image == null)
            {
                MessageBox.Show("Current image is invalid", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Import";
            dialog.Filter = "Direct Draw Surface File|*.dds";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    int index = listViewTextures.FocusedItem.Index;
                    TextureInfo[] texInfos = UseFrag ? FragType.TexInfos : FragDrawable.TexInfos;
                    InjectDDS(Reader, dialog.FileName, texInfos[index]);

                    fileEdited = true;
                    MessageBox.Show("Succesfully imported texture.\nMake sure to rebuild the file!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while importing texture :\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GeometryCheckBox_Checked(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            string name = Regex.Match(checkbox.Name, @"\d+").Value;
            int index = Convert.ToInt32(name) - 1;

            Model3DGroup group = new Model3DGroup();
            Geometry CurrentGeometry = null;
            List<Geometry> geometryList = UseFrag ? FragType.ModelGeometry : FragDrawable.ModelGeometry;

            for (int i = 0; i < geometryList.Count; i++)
            {
                if (geometryList[i] == null || i != index)
                {
                    continue;
                }
                CurrentGeometry = geometryList[i];
                break;
            }

            int newIndex = 0;
            foreach (Mesh[] t in CurrentModelMesh)
            {
                foreach (Mesh t1 in t)
                {
                    GeometryModel3D geometry = new GeometryModel3D(t1.MeshGeometry, t1.Material)
                    {
                        BackMaterial = t1.Material
                    };

                    if (!checkbox.Checked)
                    {
                        if (t1 != CurrentGeometry.Mesh && !ModelDefaultMesh[newIndex])
                            group.Children.Add(geometry);
                        else
                            ModelDefaultMesh[newIndex] = true;
                    }
                    else
                    {
                        if (!ModelDefaultMesh[newIndex])
                            group.Children.Add(geometry);
                        else if (t1 == CurrentGeometry.Mesh)
                        {
                            group.Children.Add(geometry);
                            ModelDefaultMesh[newIndex] = false;
                        }
                    }
                    newIndex++;
                }
            }
            NewModel = group;
            Vector3D axis = new Vector3D(1, 0, 0);
            Matrix3D matrix = NewModel.Transform.Value;
            matrix.Rotate(new Quaternion(axis, 90));
            NewModel.Transform = new MatrixTransform3D(matrix);
        }
    }

    #endregion
}
