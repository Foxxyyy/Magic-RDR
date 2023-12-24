using HelixToolkit.Wpf;
using Magic_RDR.Application;
using Magic_RDR.Models;
using Magic_RDR.RPF;
using ModelViewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml;
using static Magic_RDR.RPF.Texture;
using static Magic_RDR.RPF6.RPF6TOC;
using static ModelViewer.ModelView;
using Geometry = Magic_RDR.Application.Geometry;

namespace Magic_RDR
{
    public partial class VolumeViewerForm : Form, IDisposable
    {
        private IOReader Reader;
        private TOCSuperEntry Entry;
        public volatile List<List<Mesh>> meshs;
        public static string FileName;
        private bool[] ModelDefaultMesh;
        public static bool NewColorChange, FormIsClosing;
        private int CurrentImageIndex = 0;
        private System.Drawing.Image currentImage;
        private byte[] HeaderData;

        public VolumeViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Entry = entry;
            FileName = entry.Entry.Name;
            Text = string.Format("MagicRDR - Model Viewer - [{0}]", entry.Entry.Name);
            FormIsClosing = false;
            NewModel = new Model3DGroup();
            NewModelName = "";

            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();
            byte[] compressedData = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            byte[] fileData = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(compressedData);

            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                HeaderData = new byte[0xC];
            else
                HeaderData = new byte[0x18];

            for (int i = 0; i < HeaderData.Length; i++)
            {
                HeaderData[i] = compressedData[i];
            }

            Reader = new IOReader(new MemoryStream(fileData), AppGlobals.Platform == AppGlobals.PlatformEnum.Switch ? IOReader.Endian.Little : IOReader.Endian.Big);
            Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

            VolumeData data = new VolumeData(Reader, file);
            UpdateTabControls();
        }

        private void UpdateTabControls()
        {
            labelGeometryCount.Text = "* Geometry Count : " + VolumeData.TotalGeometryCount;
            labelParentMeshCount.Text = "* Mesh Parent Count : " + VolumeData.TotalMeshParent;
            labelVerticesCount.Text = "* Vertices Count : " + VolumeData.TotalVerticeCount;
            labelPolygonsCount.Text = "* Polygons Count : " + VolumeData.TotalFaceCount;
            labelPosition.Text = string.Format("* Position : ({0}; {1}; {2})", VolumeData.ModelBoundsCenter.X, VolumeData.ModelBoundsCenter.Y, VolumeData.ModelBoundsCenter.Z);
            labelBoundsMin.Text = string.Format("* Bounds Min : ({0}; {1}; {2})", VolumeData.ModelBoundsMin.X, VolumeData.ModelBoundsMin.Y, VolumeData.ModelBoundsMin.Z);
            labelBoundsMax.Text = string.Format("* Bounds Max : ({0}; {1}; {2})", VolumeData.ModelBoundsMax.X, VolumeData.ModelBoundsMax.Y, VolumeData.ModelBoundsMax.Z);

            //Geometries
            int GeometryCount = (int)VolumeData.TotalGeometryCount;
            panel1.RowCount = GeometryCount;

            int count = 0;
            ModelDefaultMesh = new bool[GeometryCount];
            for (int i = 0; i < GeometryCount; i++)
            {
                CheckBox box = new CheckBox
                {
                    Checked = true,
                    Dock = DockStyle.Top,
                    Name = string.Format("GeometryCheckBox{0}", i + 1)
                };
                box.CheckedChanged += GeometryCheckBox_Checked;
                box.Text = string.Format("Geometry {0} (Vertexs: {1}, Faces: {2}, Indexs: {3})", i + 1, VolumeData.ModelGeometry[i].VertexCount, VolumeData.ModelGeometry[i].FaceCount, VolumeData.ModelGeometry[i].IndexCount);

                box.Margin = new Padding(5, 0, 0, 0);
                panel1.Controls.Add(box, 0, i);
                count++;
            }

            //Textures
            int index = -1, ImageCount = VolumeData.ImagesSource.Length;
            ImageSource[] Textures = VolumeData.ImagesSource;
            TextureInfo[] TexInfos = VolumeData.TexInfos;

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
                labelCurrentMipmap.Text = string.Format("MipMap 1 - Size {0}x{1}", TexInfos[index].Height, TexInfos[index].Width);
                trackBar1.Maximum = TexInfos[index].MipMaps;
                currentImage = imageContainer.Image;
                CurrentImageIndex = index;
            }

            List<ListViewItem> list = new List<ListViewItem>();
            for (int i = 0; i < ImageCount; i++)
            {
                if (Textures[i] == null)
                    continue;

                ListViewItem listViewItem = new ListViewItem
                {
                    Text = TexInfos[i].TextureName
                };
                listViewItem.SubItems.Add(string.Format("{0}x{1}", TexInfos[i].Height, TexInfos[i].Width));
                listViewItem.SubItems.Add(TexInfos[i].PixelFormat.ToString());
                listViewItem.SubItems.Add(TexInfos[i].MipMaps.ToString());
                list.Add(listViewItem);
            }
            columnHeader1.Text = string.Format("Name ({0})", ImageCount);
            listViewTextures.BeginUpdate();
            listViewTextures.Items.Clear();
            listViewTextures.Items.AddRange(list.ToArray());
            listViewTextures.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewTextures.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewTextures.EndUpdate();
            list.Clear();

            //Mapping
            var mapping = VolumeData.ShaderTextureMappingNamesOrdered;
            for (int i = 0; i < mapping.Count; i++)
            {
                if (mapping[i] == null)
                    continue;

                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = (i + 1).ToString();
                listViewItem.SubItems.Add(mapping[i]);

                for (int c = 0; c < ImageCount; c++)
                {
                    if (Textures[c] == null)
                        continue;

                    if (TexInfos[c].TextureName.Contains(mapping[i].ToLower()))
                    {
                        listViewItem.SubItems.Add("True");
                        break;
                    }
                    else if (c == (ImageCount - 1))
                    {
                        listViewItem.SubItems.Add("False");
                        break;
                    }
                }
                if (ImageCount == 0)
                    listViewItem.SubItems.Add("False");
                list.Add(listViewItem);
            }
            listViewShaders.BeginUpdate();
            listViewShaders.Items.Clear();
            listViewShaders.Items.AddRange(list.ToArray());
            listViewShaders.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewShaders.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listViewShaders.EndUpdate();
            list.Clear();

            //Shaders
            var shaders = VolumeData.ShaderList;
            var lod = VolumeData.LODDistance;
            var renderMask = VolumeData.RenderMaskFlags;
            treeViewShaders.Nodes.Clear();

            TreeNode lodGroup = treeViewShaders.Nodes.Add("LOD");
            lodGroup.Nodes.Add("LOD Distance High : " + lod.X.ToString());
            lodGroup.Nodes.Add("LOD Distance Medium: " + lod.Y.ToString());
            lodGroup.Nodes.Add("LOD Distance Low: " + lod.Z.ToString());
            lodGroup.Nodes.Add("LOD Distance Very Low: " + lod.W.ToString());
            lodGroup.Nodes.Add("RenderMaskHigh : " + renderMask[0].ToString());
            lodGroup.Nodes.Add("RenderMaskMed: " + renderMask[1].ToString());
            lodGroup.Nodes.Add("RenderMaskLow: " + renderMask[2].ToString());
            lodGroup.Nodes.Add("RenderMaskVlow: " + renderMask[3].ToString());

            byte[] t = BitConverter.GetBytes(lod.X);

            if (shaders != null)
            {
                for (int i = 0; i < shaders.Length; i++)
                {
                    if (shaders[i] == null)
                        continue;

                    TreeNode currentShader = treeViewShaders.Nodes.Add("Shader_" + (i + 1).ToString());
                    currentShader.Nodes.Add("Name : " + shaders[i].ShaderName);
                    currentShader.Nodes.Add("Parameter Count : " + shaders[i].ParameterCount);
                    currentShader.Nodes.Add("RenderBucket : " + shaders[i].RenderBucket);
                    currentShader.Nodes.Add("RenderBucketMask : " + shaders[i].RenderBucketMask);

                    TreeNode vertexType = currentShader.Nodes.Add("Vertex Type");
                    (string[], string) structure = ShaderParam.GetVertexType(shaders[i].ShaderName);
                    foreach (string type in structure.Item1)
                    {
                        vertexType.Nodes.Add(type);
                    }

                    TreeNode parametersType = currentShader.Nodes.Add("Parameters Type");
                    string[] types = ShaderParam.GetParameterType(structure.Item2);
                    foreach (string type in types)
                    {
                        parametersType.Nodes.Add(type);
                    }

                    TreeNode parameters = currentShader.Nodes.Add("Parameters");
                    foreach (var param in shaders[i].Parameters)
                    {
                        if (param.Param is string)
                            parameters.Nodes.Add("Texture : " + param.Param.ToString());
                        else
                        {
                            Vector4[] vectors = (Vector4[])param.Param;
                            if (vectors.Length == 1)
                                parameters.Nodes.Add(string.Format("Vector : {0}; {1}; {2}; {3}", vectors[0].X, vectors[0].Y, vectors[0].Z, vectors[0].W).Replace(",", "."));
                            else
                            {
                                TreeNode array = parameters.Nodes.Add("Vector Array");
                                array.Nodes.Add(string.Format("HardAlphaBlend : {0}; {1}; {2}; {3}", vectors[0].X, vectors[0].Y, vectors[0].Z, vectors[0].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("useTessellation : {0}; {1}; {2}; {3}", vectors[1].X, vectors[1].Y, vectors[1].Z, vectors[1].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("wetnessMultiplier : {0}; {1}; {2}; {3}", vectors[2].X, vectors[2].Y, vectors[2].Z, vectors[2].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("bumpiness : {0}; {1}; {2}; {3}", vectors[3].X, vectors[3].Y, vectors[3].Z, vectors[3].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("specularIntensityMult : {0}; {1}; {2}; {3}", vectors[4].X, vectors[4].Y, vectors[4].Z, vectors[4].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("specularFalloffMult : {0}; {1}; {2}; {3}", vectors[5].X, vectors[5].Y, vectors[5].Z, vectors[5].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("specularFresnel : {0}; {1}; {2}; {3}", vectors[6].X, vectors[6].Y, vectors[6].Z, vectors[6].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("globalAnimUV1 : {0}; {1}; {2}; {3}", vectors[7].X, vectors[7].Y, vectors[7].Z, vectors[7].W).Replace(",", "."));
                                array.Nodes.Add(string.Format("globalAnimUV0 : {0}; {1}; {2}; {3}", vectors[8].X, vectors[8].Y, vectors[8].Z, vectors[8].W).Replace(",", "."));
                            }
                        }
                    }
                }
            }

            if (RPF6FileNameHandler.UseCustomColor)
            {
                NewFirstGradientColor = RPF6FileNameHandler.CustomColor1;
                NewSecondGradientColor = RPF6FileNameHandler.CustomColor2;
            }
        }

        #region Volume Data

        public class VolumeData
        {
            private IOReader Reader;
            private FileEntry Entry;
            public volatile MeshGeometry3D ModelMesh;
            private Mesh[] Meshs;
            public static List<Mesh[]> MeshsList = new List<Mesh[]>();
            public static List<Geometry> ModelGeometry = new List<Geometry>();
            public static Model3DGroup ModelGroup = new Model3DGroup(), SeparatedGroup;

            private int[] m_Models = new int[4];
            public static uint TotalVerticeCount, TotalFaceCount, TotalGeometryCount, TotalMeshParent;

            private List<ushort> ShaderTextureMapping = new List<ushort>();
            public static List<string> ShaderNameList = new List<string>();
            public static List<string> ShaderTextureMappingNamesOrdered = new List<string>();
            public static List<Vector3> AllVertices = new List<Vector3>();

            private int TextureDataPointer;
            public static int[] DrawablesPointers;
            private int ShaderGroupPointer;
            private int SkeletonPointer;
            public static int LODGroupPointer;

            public static Vector3 ModelBoundsCenter;
            public static Vector3 ModelBoundsMin;
            public static Vector3 ModelBoundsMax;
            public static Vector4 LODDistance;
            public static uint[] RenderMaskFlags = new uint[4];

            private Vector3[] Vertices;
            private Vector2[] UV;
            private Vector3[] Normals;
            private Vector3[] Tangents;
            private System.Drawing.Color[] VertexColors;

            public static Shader[] ShaderList;
            public static TextureInfo[] TexInfos;
            public static ImageSource[] ImagesSource;

            public VolumeData(object reader, object file)
            {
                Reader = (IOReader)reader;
                Entry = (FileEntry)file;

                //Reset static variables
                TotalVerticeCount = 0;
                TotalFaceCount = 0;
                TotalGeometryCount = 0;
                TotalMeshParent = 0;
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                ModelBoundsCenter = new Vector3(0F, 0F, 0F);
                DrawablesPointers = null;
                ModelGeometry.Clear();
                ShaderNameList.Clear();
                ShaderTextureMapping.Clear();
                ShaderTextureMappingNamesOrdered.Clear();

                ModelGroup.Children.Clear();
                MeshsList.Clear();

                Vertices = null; //For purpose only
                UV = null; //For purpose only
                Normals = null; //For purpose only
                Tangents = null; //For purpose only
                VertexColors = null; //For purpose only
                ShaderList = null; //For purpose only
                ImagesSource = null; //For purpose only

                XVD_ReadMainStructure();
                if (DrawablesPointers != null) //Skip this part for parents since they only have textures
                {
                    for (int i = 0; i < DrawablesPointers.Length; i++)
                    {
                        Reader.BaseStream.Seek(DrawablesPointers[i], SeekOrigin.Begin);
                        XVD_ReadDrawableGroup();
                        XVD_ReadShaderGroup();
                        XVD_ReadSkeleton();
                        XVD_ReadLODGroup();
                        XVD_ReadDrawableData();
                    }

                    NewModel = ModelGroup;
                    NewModel.Transform = new MatrixTransform3D();
                    Matrix3D matrix = NewModel.Transform.Value;
                    matrix.Rotate(new Quaternion(new Vector3D(1, 0, 0), 90));
                    NewModel.Transform = new MatrixTransform3D(matrix);
                    NewPoints = NewModel.GetModelBoundsPoints();
                    CurrentModelMesh = MeshsList;     
                }
            }

            private void XVD_ReadMainStructure()
            {
                long Offset = Reader.BaseStream.Position;
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());

                TextureDataPointer = (int)(Offset + 0x28);
                (int, int[]) TextureInfo = XVD_ReadTextureStructure(); //returns (TextureCount, TexturePointer)
                if (TextureInfo.Item1 > 0)
                {
                    XVD_ReadTextureData(TextureInfo.Item1, TextureInfo.Item2);
                }
                else
                {
                    TexInfos = new TextureInfo[0];
                    ImagesSource = new ImageSource[0];
                }
                Reader.BaseStream.Seek(Offset + 0x8, SeekOrigin.Begin);
                XVD_ReadTileDictionary();
            }

            private (int, int[]) XVD_ReadTextureStructure()
            {
                if (TextureDataPointer == 0x0)
                    return (0, null);

                Reader.BaseStream.Seek(TextureDataPointer, SeekOrigin.Begin);
                Reader.BaseStream.Seek(Reader.ReadOffset(Reader.ReadInt32()), SeekOrigin.Begin);
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

            private void XVD_ReadTextureData(int textureCount, int[] texturePointer)
            {
                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                for (int i = 0; i < textureCount; i++)
                {
                    Reader.BaseStream.Seek(texturePointer[i], SeekOrigin.Begin);
                    uint Unknown_0x00 = Reader.ReadUInt32();
                    int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
                    byte Unknown_0x08 = Reader.ReadByte();
                    byte Unknown_0x09 = Reader.ReadByte();
                    ushort Unknown_0x0A = Reader.ReadUInt16();
                    uint Unknown_0x0C = Reader.ReadUInt32();

                    if (Unknown_0x0A != 1 && !isSwitchVersion)
                        return;

                    uint Unknown_0x10 = Reader.ReadUInt32();
                    uint TextureSize = Reader.ReadUInt32();
                    int TextureNamePointer = Reader.ReadOffset(Reader.ReadInt32());
                    int D3DStructurePointer = Reader.ReadOffset(Reader.ReadInt32());
                    ushort Width = Reader.ReadUInt16();
                    ushort Height = Reader.ReadUInt16();

                    int CurrentTextureDataPointer, CurrentMipDataPointer = 0, MipMapCount;
                    Texture.TextureType format;

                    //Nintendo Switch
                    if (isSwitchVersion)
                    {
                        string sFormat = Reader.ReadString(IOReader.StringType.ASCII, 4);
                        switch (sFormat)
                        {
                            case "DXT1":
                                format = Texture.TextureType.DXT1;
                                break;
                            case "DXT3":
                                format = Texture.TextureType.DXT3;
                                break;
                            case "DXT5":
                                format = Texture.TextureType.DXT5;
                                break;
                            default:
                                //string test = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer);
                                //throw new Exception("Unsupported format");
                                format = Texture.TextureType.A8R8G8B8; //Likely
                                break;
                        }
                        byte pad = Reader.ReadByte();
                        ushort unkValue = Reader.ReadUInt16();
                        MipMapCount = (int)Reader.ReadByte();
                        float unknownFloat1 = Reader.ReadSingle(); //1.0f
                        float unknownFloat2 = Reader.ReadSingle(); //1.0f
                        float unknownFloat3 = Reader.ReadSingle(); //1.0f
                        float unknownFloat4 = Reader.ReadSingle(); //0.0f
                        float unknownFloat5 = Reader.ReadSingle(); //0.0f
                        float unknownFloat6 = Reader.ReadSingle(); //0.0f
                        int prevTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                        int nextTextureOffset = Reader.ReadOffset(Reader.ReadInt32());
                        CurrentTextureDataPointer = Reader.ReadOffset(Reader.ReadInt32()); //533 504
                    }
                    else //Xbox 360
                    {
                        MipMapCount = Reader.ReadInt32();
                        Vector3 Unknown_0x24 = Reader.ReadVector3();
                        Vector3 Unknown_0x30 = Reader.ReadVector3();

                        //BaseAdress
                        Reader.BaseStream.Seek(D3DStructurePointer + 0x20, SeekOrigin.Begin);
                        uint Value = Reader.ReadUInt32();
                        uint BaseAdress = Value >> 0xC;
                        CurrentTextureDataPointer = ((int)(BaseAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                        //MipAdress
                        Reader.BaseStream.Seek(D3DStructurePointer + 0x30, SeekOrigin.Begin);
                        uint Value_ = Reader.ReadUInt32();
                        uint MipMapAdress = Value_ >> 0xC;
                        CurrentMipDataPointer = ((int)(MipMapAdress << 12) & 0xFFFFFFF) + Entry.FlagInfo.BaseResourceSizeV;

                        //Read texture data
                        Reader.BaseStream.Seek(D3DStructurePointer + 0x23, SeekOrigin.Begin);
                        format = Reader.ReadByte() == 0x52 ? Texture.TextureType.DXT1 : Texture.TextureType.DXT5;
                    }

                    string name = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, TextureNamePointer, true);

                    try
                    {
                        TexInfos[i] = new TextureInfo()
                        {
                            TextureName = name,
                            Width = Width,
                            Height = Height,
                            MipMaps = MipMapCount,
                            TextureSize = TextureSize,
                            TextureDataPointer = CurrentTextureDataPointer,
                            MipDataPointer = CurrentMipDataPointer,
                            PixelFormat = format
                        };

                        byte[] buffer = ReadTextureInfo(Reader, 0, TexInfos[i]);
                        ImagesSource[i] = LoadImage(buffer);
                    }
                    catch { TexInfos[i] = null; ImagesSource[i] = null; }

                    if (CurrentMipDataPointer == 0 && !isSwitchVersion)
                    {
                        TexInfos[i] = null;
                        ImagesSource[i] = null;
                        continue;
                    }
                }
            }

            private void XVD_ReadTileDictionary()
            {
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());
                byte Unknown0x08 = Reader.ReadByte();
                byte Unknown0x09 = Reader.ReadByte();
                ushort Unknown0x0A = Reader.ReadUInt16();
                uint Unknown0x0C = Reader.ReadUInt32();

                RGArray NameHashs;
                NameHashs.Offset = Reader.ReadOffset(Reader.ReadInt32());
                NameHashs.Count = Reader.ReadUInt16();
                NameHashs.Size = Reader.ReadUInt16();

                RGArray Drawables;
                Drawables.Offset = Reader.ReadOffset(Reader.ReadInt32());
                Drawables.Count = Reader.ReadUInt16();
                Drawables.Size = Reader.ReadUInt16();

                Reader.BaseStream.Seek(NameHashs.Offset, SeekOrigin.Begin);
                uint[] hashs = new uint[NameHashs.Count];
                for (int i = 0; i < NameHashs.Count; i++)
                {
                    hashs[i] = Reader.ReadUInt32();
                }

                if (Drawables.Count == 0)
                {
                    if (ImagesSource.Length >= 1)
                        MessageBox.Show("This file doesn't contains any model but texture(s) were found", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show("Invalid terrain file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //ModelUseProps = (Drawables.Count > 1);
                DrawablesPointers = new int[Drawables.Count];
                Reader.BaseStream.Seek(Drawables.Offset, SeekOrigin.Begin);
                for (int i = 0; i < Drawables.Count; i++)
                {
                    DrawablesPointers[i] = Reader.ReadOffset(Reader.ReadInt32());
                }
            }

            private void XVD_ReadDrawableGroup()
            {
                uint Unknown0x00 = Reader.ReadUInt32();
                int Unknown0x04 = Reader.ReadOffset(Reader.ReadInt32());
                ShaderGroupPointer = Reader.ReadOffset(Reader.ReadInt32());
                SkeletonPointer = Reader.ReadOffset(Reader.ReadInt32()); //Always 0
                LODGroupPointer = (int)Reader.BaseStream.Position;
            }

            private void XVD_ReadShaderGroup()
            {
                Reader.BaseStream.Seek(ShaderGroupPointer, SeekOrigin.Begin);
                uint Unknown0x00 = Reader.ReadUInt32();
                uint Unknown0x04 = Reader.ReadUInt32();

                RGArray Shaders;
                Shaders.Offset = Reader.ReadOffset(Reader.ReadInt32());
                Shaders.Count = Reader.ReadUInt16();
                Shaders.Size = Reader.ReadUInt16();

                uint[] Unknown0x10 = new uint[4];
                for (int i = 0; i < Unknown0x10.Length; i++)
                    Unknown0x10[i] = Reader.ReadUInt32();

                int[] ShaderPointers = new int[Shaders.Count];
                Reader.BaseStream.Seek(Shaders.Offset, SeekOrigin.Begin);
                for (int i = 0; i < Shaders.Count; i++)
                    ShaderPointers[i] = Reader.ReadOffset(Reader.ReadInt32());

                ShaderList = new Shader[ShaderPointers.Length];
                for (int i = 0; i < ShaderPointers.Length; i++)
                {
                    Reader.BaseStream.Seek(ShaderPointers[i], SeekOrigin.Begin);
                    XVD_ReadShaderFX(i);
                }
            }

            private void XVD_ReadShaderFX(int index)
            {
                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                int ParameterPointer = Reader.ReadOffset(Reader.ReadInt32());
                uint ShaderNameHash = Reader.ReadUInt32();
                string ShaderName = Enum.Parse(typeof(ShaderNameTerrains), ShaderNameHash.ToString(), true).ToString();
                byte ParameterCount = Reader.ReadByte();
                byte RenderBucket = Reader.ReadByte();
                ushort Unknown0xA = Reader.ReadUInt16();
                ushort ParameterSize = Reader.ReadUInt16();
                ushort ParameterDataSize = Reader.ReadUInt16();
                uint ShaderHash = Reader.ReadUInt32();
                uint Unknown_0x14 = Reader.ReadUInt32();
                uint RenderBucketMask = Reader.ReadUInt32();
                uint Unknown_0x1C = Reader.ReadUInt32();

                if (isSwitchVersion) //Not finished yet
                    return;

                ShaderParam[] Paramaters = new ShaderParam[ParameterCount];
                ShaderList[index] = new Shader(Paramaters, ParameterCount, RenderBucket, ParameterSize, ParameterDataSize, ShaderHash, ShaderName, RenderBucketMask);

                if (isSwitchVersion)
                {
                    int shaderParameterBlock = Reader.ReadOffset(Reader.ReadInt32());
                    Reader.BaseStream.Seek(shaderParameterBlock, SeekOrigin.Begin);
                }

                for (int i = 0; i < ParameterCount; i++)
                {
                    byte RegisterCount = Reader.ReadByte();
                    byte RegisterIndex = Reader.ReadByte();
                    byte DataType = Reader.ReadByte();
                    _ = Reader.ReadByte();

                    int ParameterValuesOffset = Reader.ReadOffset(Reader.ReadInt32());
                    long Offset = Reader.BaseStream.Position;
                    Reader.BaseStream.Seek((RegisterCount == 0) ? ParameterValuesOffset + 0x18 : ParameterValuesOffset, SeekOrigin.Begin);

                    string ShaderTexture = "";
                    Vector4[] ShaderVector = new Vector4[RegisterCount];

                    if (RegisterCount == 0) //Texture
                    {
                        ShaderTexture = Reader.ReadCustomString(IOReader.StringType.ASCII_NULL_TERMINATED, 0, Reader.ReadOffset(Reader.ReadInt32()), true);
                    }
                    else //Vector
                    {
                        for (int v = 0; v < RegisterCount; v++)
                        {
                            ShaderVector[v] = Reader.ReadVector4();
                        }
                    }

                    object param;
                    if (ShaderTexture != "")
                        param = ShaderTexture;
                    else
                        param = ShaderVector;

                    Paramaters[i] = new ShaderParam(RegisterCount, RegisterIndex, DataType, param);
                    Reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
                }
                ShaderList[index].Parameters = Paramaters;
            }

            private void XVD_ReadSkeleton()
            {
                return;
            }

            private void XVD_ReadLODGroup()
            {
                if (LODGroupPointer == 0)
                {
                    return;
                }

                Reader.BaseStream.Seek(LODGroupPointer, SeekOrigin.Begin);
                Vector4 m_vCenter = Reader.ReadVector4();
                Vector4 m_vBoundsMin = Reader.ReadVector4();
                Vector4 m_vBoundsMax = Reader.ReadVector4();

                for (int i = 0; i < 4; i++)
                {
                    m_Models[i] = Reader.ReadOffset(Reader.ReadInt32());
                }

                LODDistance = Reader.ReadVector4();
                RenderMaskFlags = new uint[4];
                for (int i = 0; i < 4; i++)
                {
                    RenderMaskFlags[i] = Reader.ReadUInt32();
                }
                float m_fRadius = Reader.ReadFloat();
                uint Unknown0x64 = Reader.ReadUInt32();
                int Unknown0x68 = Reader.ReadOffset(Reader.ReadInt32());
                int Unknown0x6C = Reader.ReadOffset(Reader.ReadInt32());

                if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                {
                    m_vCenter = DataUtils.SwapEndian(m_vCenter);
                    m_vBoundsMin = DataUtils.SwapEndian(m_vBoundsMin);
                    m_vBoundsMax = DataUtils.SwapEndian(m_vBoundsMax);
                    LODDistance = DataUtils.SwapEndian(LODDistance);
                }
                ModelBoundsCenter = new Vector3(m_vCenter.X, m_vCenter.Y, m_vCenter.Z);
                ModelBoundsMin = new Vector3(m_vBoundsMin.X, m_vBoundsMin.Y, m_vBoundsMin.Z);
                ModelBoundsMax = new Vector3(m_vBoundsMax.X, m_vBoundsMax.Y, m_vBoundsMax.Z);
            }

            private void XVD_ReadDrawableData()
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
                SeparatedGroup = new Model3DGroup();
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

                    AllVertices.Clear();
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
                        XVD_ReadGeometryData(ModelCurrentGeometry.Count, ModelGeometryDataPointer, ref index);
                    }
                }
                ModelGroup.Children.Add(SeparatedGroup);
            }

            private void XVD_ReadGeometryData(int geometryCount, int[] modelGeometryDataPointer, ref int index)
            {
                bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;
                Meshs = new Mesh[geometryCount];

                for (int count = 0; count < geometryCount; ++count)
                {
                    Reader.BaseStream.Seek(modelGeometryDataPointer[count], SeekOrigin.Begin);
                    var Model3DInfo = XVD_ReadModel3DData(count); //0 = IndexCount, 1 = FaceCount, 2 = Indices, 3 = VertexColor
                    ModelMesh = new MeshGeometry3D();

                    ////////////// Vertices //////////////
                    foreach (Vector3 vertex in Vertices)
                    {
                        Vector3 pos = new Vector3(vertex.X, vertex.Y, vertex.Z);
                        ModelMesh.Positions.Add(new Point3D(pos.X, pos.Y, pos.Z));
                        AllVertices.Add(vertex);
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
                    ushort CurrentSubMeshShaderIndex; string CurrentSubMeshShaderName = ""; int CurrentSubMeshShaderParamCount = 0;

                    if (!isSwitchVersion)
                    {
                        try
                        {
                            CurrentSubMeshShaderIndex = ShaderTextureMapping[index];
                            CurrentSubMeshShaderName = (string)ShaderList[CurrentSubMeshShaderIndex].Parameters[0].Param;
                            CurrentSubMeshShaderParamCount = ShaderList[CurrentSubMeshShaderIndex].ParameterCount;
                        }
                        catch { }


                        if (string.IsNullOrEmpty(CurrentSubMeshShaderName))
                            ShaderTextureMappingNamesOrdered.Add("Unknown");
                        else
                            ShaderTextureMappingNamesOrdered.Add(CurrentSubMeshShaderName);

                        if (CurrentSubMeshShaderName != null && TexInfos != null)
                        {
                            for (int i = 0; i < TexInfos.Count(); i++)
                            {
                                if (TexInfos[i] == null)
                                    continue;

                                if (TexInfos[i].TextureName.Contains(CurrentSubMeshShaderName))
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

                    ////////////// Set up meshs //////////////
                    Model3DGroup Group = new Model3DGroup();
                    GeometryModel3D GeometryModel = new GeometryModel3D(ModelMesh, material)
                    {
                        BackMaterial = material
                    };
                    Mesh mesh = new Mesh(ModelMesh, material);
                    ModelGeometry.Add(new Geometry(mesh, CurrentSubMeshShaderName ?? "Unknown", (int)Model3DInfo.Item1[0], (int)Model3DInfo.Item1[1], Vertices.Length, Vertices, UV, indices, Normals, Tangents, VertexColors, Model3DInfo.Item3, Model3DInfo.Item4, Model3DInfo.Item5));
                    Group.Children.Add(GeometryModel);
                    SeparatedGroup.Children.Add(Group);
                    Meshs[count] = mesh;

                    //ModelMesh = null;
                    GeometryModel = null;
                    Group = null;
                    indices = null;
                    Vertices = null;
                    VertexColors = null;
                    UV = null;
                    Tangents = null;
                    Normals = null;
                    index++;
                }
                MeshsList.Add(Meshs);
            }

            public (uint[], byte[], long, ulong, uint) XVD_ReadModel3DData(int Index)
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
                VertexColors = new System.Drawing.Color[VertexCount];
                long VertexOffset = (DataOffset >> 28 == 6) ? Entry.FlagInfo.BaseResourceSizeV + VertexDataPointer : Entry.FlagInfo.BaseResourceSizeP + VertexDataPointer;
                Reader.BaseStream.Seek(isSwitchVersion ? vertexBuffer.VertexData : VertexOffset, SeekOrigin.Begin);

                var test = new List<ushort>();
                var test1 = new List<ushort>();
                ////////////// Vertexs Layouts //////////////
                for (int vertex = 0; vertex < VertexCount; vertex++)
                {
                    for (int usageIdx = 0; usageIdx < 16; usageIdx++)
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
                                    VertexColors[vertex] = System.Drawing.Color.FromArgb((int)rgba);
                                    break;
                                }
                            case 7: //Texture coordinates
                            /*case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 13:*/
                                {
                                    if (typeIdx == 14)
                                    {
                                        float x = Reader.ReadUInt16() * (float)3.05185094e-005;
                                        float y = Reader.ReadUInt16() * (float)3.05185094e-005;
                                        /*float minU = 0.625f;
                                        float maxU = 0.75f;
                                        x = (x - minU) / (maxU - minU);
                                        y = (y - minU) / (maxU - minU);*/
                                        UV[vertex] = new Vector2(x, y);
                                    }
                                    else
                                    {
                                        float x = DataUtils.GetHalfFloat(Reader.ReadByte(), Reader.ReadByte());
                                        float y = DataUtils.GetHalfFloat(Reader.ReadByte(), Reader.ReadByte());
                                        UV[vertex] = new Vector2(x, y);
                                    }
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

                /*if (test.Count != 0 && test1.Count != 0)
                {
                    ushort max = test.Max();
                    ushort max1 = test1.Max();

                    for (int i = 0; i < test.Count; i++)
                    {
                        float u = (float)test[i] / max;
                        float v = (float)test1[i] / max1;
                        UV[i] = new Vector2(u, v);
                    }
                }*/

                ////////////// Indices //////////////
                Reader.BaseStream.Seek(IndexBuffer[0], SeekOrigin.Begin);
                uint Unknown0x00_ = Reader.ReadUInt32();
                uint IndexCount_ = Reader.ReadUInt32();
                int IndexDataPointer = Reader.GetDataOffset(Reader.ReadInt32());
                int IndexBufferPointer = Reader.ReadOffset(Reader.ReadInt32());

                if (isSwitchVersion)
                    Reader.BaseStream.Seek(IndexDataPointer, SeekOrigin.Begin);
                else
                    Reader.BaseStream.Seek(Entry.FlagInfo.BaseResourceSizeV + IndexDataPointer, SeekOrigin.Begin);

                byte[] Indices = Reader.ReadBytes(FaceCount * 6);

                return (new uint[] { IndexCount, FaceCount }, Indices, isSwitchVersion ? vertexBuffer.VertexData : VertexOffset, format.FVFIndices, format.Mask);
            }

            private float ConvertFrom(ushort valueOne, ushort valueTwo)
            {
                byte[][] final = Array.ConvertAll(new ushort[] { valueOne, valueTwo }, delegate (ushort item) { return BitConverter.GetBytes(item); });
                return BitConverter.ToSingle(new byte[4] { final[0][0], final[0][1], final[1][0], final[1][1] }, 0);
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

        private void importTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageContainer.Image == null)
            {
                MessageBox.Show("Current image is invalid", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Open";
            dialog.Filter = "Direct Draw Surface File (*.dds)|*.dds";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextureInfo[] TexInfos = VolumeData.TexInfos;
                string textureName = listViewTextures.SelectedItems[0].Text;
                int index = -1;

                for (int i = 0; i < TexInfos.Length; i++)
                {
                    if (TexInfos[i] == null)
                        continue;
                    if (TexInfos[i].TextureName == textureName)
                        index = i;
                }

                try
                {
                    InjectDDS(Reader, dialog.FileName, TexInfos[index]);
                    MessageBox.Show("Succesfully imported texture.\nMake sure to rebuild the file!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while replacing texture\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void checkBoxShowBounds_CheckedChanged(object sender, EventArgs e)
        {
            if (VolumeData.DrawablesPointers == null && !FormIsClosing)
            {
                MessageBox.Show("This file doesn't contain any models", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (checkBoxShowBounds.Checked)
                ModelChangedFlags = 0x2;
            else
                ModelChangedFlags = 0x3;
        }

        private void checkBoxWireframe_CheckedChanged(object sender, EventArgs e)
        {
            if (VolumeData.DrawablesPointers == null && !FormIsClosing)
            {
                MessageBox.Show("This file doesn't contain any models", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
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

        private void VolumeViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FormIsClosing = true;
            comboBoxSelectColor.Text = "Select Colors";
            checkBoxShowBounds.Checked = false;
            checkBoxWireframe.Checked = false;
            checkBoxVertices.Checked = false;
        }

        private void checkBoxVertices_CheckedChanged(object sender, EventArgs e)
        {
            if (VolumeData.DrawablesPointers == null && !FormIsClosing)
            {
                MessageBox.Show("This file doesn't contain any models", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            UseVertices = checkBoxVertices.Checked;
        }

        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VolumeData.DrawablesPointers == null)
            {
                MessageBox.Show("This file doesn't contain any models", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.Filter = "WaveFront Object File|*.obj";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FileStream file = new FileStream(dialog.FileName, FileMode.Create);
                string mtlPath = Path.ChangeExtension(file.Name, ".mtl");
                string ddsPath = Path.ChangeExtension(file.Name, ".dds");
                ObjExporter obj = new ObjExporter();

                obj.SwitchYZ = false;
                obj.ExportNormals = true;
                obj.Comment = "Exported with MagicRDR by Im Foxxyyy";
                obj.MaterialsFile = mtlPath;
                obj.Export(NewModel, file);

                ImageSource[] images = VolumeData.ImagesSource;
                StreamWriter mtlWriter = new StreamWriter(mtlPath);
                List<Geometry> geometry = VolumeData.ModelGeometry;

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
                    string texturePath = ddsPath.Replace(userInput, VolumeData.TexInfos[i].TextureName).Replace(".dds", ".png");
                    texturePath = texturePath.Substring(texturePath.LastIndexOf(":") + 1);

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

            int value = trackBar1.Value - 1; //Current mipmap
            ImageSource[] textures = VolumeData.ImagesSource;
            System.Drawing.Image[] currentImageMipmaps = Texture.GenerateMipMaps(currentImage, VolumeData.TexInfos[CurrentImageIndex].MipMaps);

            if (currentImageMipmaps[value] != null)
            {
                imageContainer.Image = currentImageMipmaps[value];
                labelCurrentMipmap.Text = string.Format("MipMap {0} - Size {1}x{2}", value + 1, currentImageMipmaps[value].Height, currentImageMipmaps[value].Width);
            }
        }

        private void exportCurrentTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageContainer.Image == null)
            {
                MessageBox.Show("Can't find any texture", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var texInfos = VolumeData.TexInfos;
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Export",
                Filter = "Portable Network Graphics (*.png)|*.png|DDS Image (*.dds)|*.dds",
                FileName = texInfos[CurrentImageIndex].TextureName.Substring(0, texInfos[CurrentImageIndex].TextureName.Length - 4) ?? ""
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ImageSource[] images = VolumeData.ImagesSource;
                int index = dialog.FileName.LastIndexOf('\\') + 1;
                string userInput = dialog.FileName.Substring(index, dialog.FileName.Length - index - 4) + ".dds";
                string texturePath = dialog.FileName.Replace(userInput, texInfos[CurrentImageIndex].TextureName);

                if (dialog.FileName.EndsWith(".dds"))
                {
                    try
                    {
                        SaveDDS(Reader, dialog.FileName, texInfos[CurrentImageIndex]);
                        MessageBox.Show("Successfully exported texture", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occured while saving texture :\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    texturePath = texturePath.Replace(".dds", ".png");
                    using (var fileStream = new FileStream(texturePath, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)images[CurrentImageIndex]));
                        encoder.Save(fileStream);
                        MessageBox.Show("Successfully exported current texture", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void exportAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageContainer.Image == null)
            {
                MessageBox.Show("Can't find any texture", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string extension = ".png";
            if (MessageBox.Show("Would you like to export all textures in .png format ?\n\nOtherwise it will be .dds", "Choose a format", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.No)
            {
                extension = ".dds";
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select a destination to export textures";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var texInfos = VolumeData.TexInfos;
                ImageSource[] images = VolumeData.ImagesSource;

                int exported = 0;
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i] == null)
                    {
                        continue;
                    }

                    var texInfo = VolumeData.TexInfos[i];
                    string texturePath = dialog.SelectedPath + "\\\\" + texInfo.TextureName.Substring(texInfo.TextureName.LastIndexOf(":") + 1, texInfo.TextureName.Length - texInfo.TextureName.LastIndexOf(":") - 1).Replace(".dds", extension);
                    
                    if (texturePath.EndsWith(".dds"))
                    {
                        try
                        {
                            SaveDDS(Reader, texturePath, texInfos[i]);
                            exported++;
                        }
                        catch { } //Just in case
                    }
                    else
                    {
                        using (var fileStream = new FileStream(texturePath, FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)images[i]));
                            encoder.Save(fileStream);
                            exported++;
                        }
                    }
                }
                MessageBox.Show(string.Format("Successfully exported {0} textures", exported), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void setModelPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
            {
                MessageBox.Show("Not supported on the Nintendo Switch version", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            UserInput input = new UserInput();
            var result = input.ShowDialog();

            if (result != DialogResult.OK)
                return;

            float x = UserInput.PositionX;
            float y = UserInput.PositionY;
            float z = UserInput.PositionZ;

            Vector3 newBoundsCenter = new Vector3(x, y, z);
            Vector3 newBoundsMin = Vector3.Substract(newBoundsCenter, Vector3.Substract(VolumeData.ModelBoundsCenter, VolumeData.ModelBoundsMin));
            Vector3 newBoundsMax = Vector3.Substract(newBoundsCenter, Vector3.Substract(VolumeData.ModelBoundsCenter, VolumeData.ModelBoundsMax));

            foreach (Geometry geo in VolumeData.ModelGeometry) //-2251,4   19,1   2600,6
            {
                long pointer = geo.VertexsPointer;
                Reader.BaseStream.Seek(pointer, SeekOrigin.Begin);

                for (int vertex = 0; vertex < geo.VertexCount; ++vertex)
                {
                    Vector3 pos = Vector3.Substract(new Vector3(geo.Vertexs[vertex].X, geo.Vertexs[vertex].Y, geo.Vertexs[vertex].Z), new Vector3(VolumeData.ModelBoundsCenter.X, VolumeData.ModelBoundsCenter.Y, VolumeData.ModelBoundsCenter.Z));
                    float newX = x + pos.X;
                    float newY = y + pos.Y;
                    float newZ = z + pos.Z;
                    byte[] bytesX = GetBytes(newX);
                    byte[] bytesY = GetBytes(newY);
                    byte[] bytesZ = GetBytes(newZ);

                    for (int usageIdx = 0; usageIdx < 16; ++usageIdx)
                    {
                        if (((geo.FormatMask >> usageIdx) & 1) == 0)
                            continue;

                        int typeIdx = (int)((geo.FormatIndices >> (usageIdx << 2)) & 0x0F);
                        switch (usageIdx)
                        {
                            case 0: //Vertices
                                {
                                    Reader.BaseStream.Write(bytesX, 0, 4);
                                    Reader.BaseStream.Write(bytesY, 0, 4);
                                    Reader.BaseStream.Write(bytesZ, 0, 4);
                                    break;
                                }
                            default:
                                Reader.BaseStream.Seek(Reader.BaseStream.Position + DataUtils.VertexSizeMapping[typeIdx], SeekOrigin.Begin);
                                break;
                        }
                    }
                }
            }

            if (VolumeData.LODGroupPointer == 0)
            {
                return;
            }
            Reader.BaseStream.Seek(VolumeData.LODGroupPointer, SeekOrigin.Begin);
            Reader.BaseStream.Write(GetBytes(newBoundsCenter), 0, 12);
            _ = Reader.ReadUInt32();
            Reader.BaseStream.Write(GetBytes(newBoundsMin), 0, 12);
            _ = Reader.ReadUInt32();
            Reader.BaseStream.Write(GetBytes(newBoundsMax), 0, 12);
        }

        private byte[] GetBytes(uint value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            Array.Reverse(temp);
            return temp;
        }

        private byte[] GetBytes(float value)
        {
            byte[] temp = BitConverter.GetBytes(value);
            Array.Reverse(temp);
            return temp;
        }

        private byte[] GetBytes(Vector3 value)
        {
            byte[] buffer = new byte[12];
            Buffer.BlockCopy(GetBytes(value.X), 0, buffer, 0, 4);
            Buffer.BlockCopy(GetBytes(value.Y), 0, buffer, 4, 4);
            Buffer.BlockCopy(GetBytes(value.Z), 0, buffer, 8, 4);
            return buffer;
        }

        private void saveRebuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will save any modifications made to the file\nMaking backups is recommended !\n\nContinue ?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            bool isSwitchVersion = AppGlobals.Platform == AppGlobals.PlatformEnum.Switch;

            //Recompress into a single block
            Reader.BaseStream.Position = 0x0;
            byte[] newData = Reader.ReadBytes(Reader.BaseStream.Length);

            if (isSwitchVersion)
                newData = DataUtils.CompressZStandard(newData);
            else
                newData = DataUtils.CompressLZX(newData);

            byte[] buffer = new byte[HeaderData.Length + newData.Length];
            if (!isSwitchVersion)
            {
                byte[] dataLength = BitConverter.GetBytes(newData.Length.Swap());
                HeaderData[20] = dataLength[0];
                HeaderData[21] = dataLength[1];
                HeaderData[22] = dataLength[2];
                HeaderData[23] = dataLength[3];
            }

            Buffer.BlockCopy(HeaderData, 0, buffer, 0, HeaderData.Length);
            Buffer.BlockCopy(newData, 0, buffer, HeaderData.Length, newData.Length);

            try
            {
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
            }
            catch
            {
                if (MessageBox.Show("An error occured while overwriting the current file...\n\nDo you want to try to export it ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Title = "Export";
                    dialog.FileName = FileName;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(dialog.FileName, buffer);
                        MessageBox.Show("File saved !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                return;
            }
            MessageBox.Show("Successfully rebuilt the file !\nMake sure to save the .RPF", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void GeometryCheckBox_Checked(object sender, EventArgs e)
        {
            if (VolumeData.DrawablesPointers == null && !FormIsClosing) //How is this possible??
            {
                MessageBox.Show("This file doesn't contain any models", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CheckBox checkbox = (CheckBox)sender;
            string name = Regex.Match(checkbox.Name, @"\d+").Value;
            int index = System.Convert.ToInt32(name) - 1;

            Model3DGroup group = new Model3DGroup();
            Geometry CurrentGeometry = null;
            List<Geometry> geometryList = VolumeData.ModelGeometry;

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

        private void listViewTextures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewTextures.SelectedItems.Count <= 0)
                return;

            string textureName = listViewTextures.SelectedItems[0].Text;
            var images = VolumeData.TexInfos;

            CurrentImageIndex = -1;
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] == null)
                    continue;
                if (images[i].TextureName == textureName)
                    CurrentImageIndex = i;
            }

            if (VolumeData.ImagesSource == null || CurrentImageIndex == -1) //Shouldn't be the case
                return;
            if (VolumeData.ImagesSource[CurrentImageIndex] == null)
                return;

            imageContainer.Image = System.Drawing.Image.FromStream(new MemoryStream(Texture.BufferFromImageSource(VolumeData.ImagesSource[CurrentImageIndex])));
            trackBar1.Maximum = images[CurrentImageIndex].MipMaps;
            trackBar1.Value = 1;
            currentImage = imageContainer.Image;
            labelCurrentMipmap.Text = string.Format("MipMap 1 - Size {0}x{1}", images[CurrentImageIndex].Height, images[CurrentImageIndex].Width);
        }

        private void exportToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
            {
                MessageBox.Show("Not supported on the Nintendo Switch version", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (VolumeData.DrawablesPointers == null)
            {
                MessageBox.Show("This file doesn't contain any models", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (ModelUseProps || VolumeData.ImagesSource.Length == 0)
            {
                MessageBox.Show("This file is not supported yet", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This will try to export the model to .xml for CodeWalker\n");
            sb.AppendLine("Until I research all RDR shaders, this will only works with only a few GTAV shaders\n");
            sb.AppendLine("\nContinue ?");

            if (MessageBox.Show(sb.ToString(), "Convert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Title = "Export";
            dialog.Filter = "GTAV Drawable File|*.ydr.xml";
            dialog.FileName = FileName;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportToXML(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occured while exporting :\n\n" + ex.ToString(), "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MessageBox.Show("File was exported !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region XML Export

        public void ExportToXML(string path)
        {
            string[] fileName = path.Split('\\')[path.Split('\\').Length - 1].Split('.');
            Vector3 ExportScale = new Vector3(1.0F, 1.0F, 1.0F);
            Vector3 boundsCenter = Vector3.Multiply(new Vector3(VolumeData.ModelBoundsCenter.X, VolumeData.ModelBoundsCenter.Y, VolumeData.ModelBoundsCenter.Z), ExportScale);
            Vector3 boundsMin = Vector3.Multiply(new Vector3(VolumeData.ModelBoundsMin.X, VolumeData.ModelBoundsMin.Y, VolumeData.ModelBoundsMin.Z), ExportScale);
            Vector3 boundsMax = Vector3.Multiply(new Vector3(VolumeData.ModelBoundsMax.X, VolumeData.ModelBoundsMax.Y, VolumeData.ModelBoundsMax.Z), ExportScale);
            Vector3 boundsMinRel = Vector3.Substract(boundsMin, boundsCenter);
            Vector3 boundsMaxRel = Vector3.Substract(boundsMax, boundsCenter);
            float radius = Math.Max(Math.Max(boundsMaxRel.X - boundsMinRel.X, boundsMaxRel.Y - boundsMinRel.Y), boundsMaxRel.Z - boundsMinRel.Z);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<Drawable>");
            sb.AppendLine("  <Name>" + fileName[0] + "</Name>");
            sb.AppendLine(string.Format("  <BoundingSphereCenter x=\"{0}\" y=\"{1}\" z=\"{2}\" />", 0.0f, 0.0f, 0.0f));
            sb.AppendLine(string.Format("  <BoundingSphereRadius value=\"{0}\" />", FormatFloat(radius)));
            sb.AppendLine(string.Format("  <BoundingBoxMin x=\"{0}\" y=\"{1}\" z=\"{2}\" />", FormatFloat(boundsMinRel.X), FormatFloat(-boundsMaxRel.Z), FormatFloat(boundsMinRel.Y)));
            sb.AppendLine(string.Format("  <BoundingBoxMax x=\"{0}\" y=\"{1}\" z=\"{2}\" />", FormatFloat(boundsMaxRel.X), FormatFloat(-boundsMinRel.Z), FormatFloat(boundsMaxRel.Y)));
            sb.AppendLine("  <LodDistHigh value=\"9998\" />");
            sb.AppendLine("  <LodDistMed value=\"9998\" />");
            sb.AppendLine("  <LodDistLow value=\"9998\" />");
            sb.AppendLine("  <LodDistVlow value=\"9998\" />");
            sb.AppendLine("  <FlagsHigh value=\"" + VolumeData.TotalGeometryCount.ToString() + "\" />");
            sb.AppendLine("  <FlagsMed value=\"0\" />");
            sb.AppendLine("  <FlagsLow value=\"0\" />");
            sb.AppendLine("  <FlagsVlow value=\"0\" />");
            sb.AppendLine("  <Unknown9A value=\"0\" />");
            sb.AppendLine("  <ShaderGroup>");
            sb.AppendLine("    <Unknown30 value=\"0\" />");
            sb.AppendLine("    <Shaders>");

            List<Geometry> geometries = VolumeData.ModelGeometry;
            var textures = VolumeData.ShaderTextureMappingNamesOrdered;
            for (int i = 0; i < geometries.Count; i++)
            {
                sb.AppendLine("      <Item>");
                sb.AppendLine("        <Name>normal</Name>");
                sb.AppendLine("        <FileName>gta_normal.sps</FileName>");
                sb.AppendLine("        <RenderBucket value=\"0\" />");
                sb.AppendLine("        <Parameters>");
                sb.AppendLine("          <Item name=\"DiffuseSampler\" type=\"Texture\">");
                sb.AppendLine("            <Name>" + textures[i].ToLower() + "</Name>");
                sb.AppendLine("          </Item>");
                sb.AppendLine("          <Item name=\"BumpSampler\" type=\"Texture\">");
                sb.AppendLine("            <Name>" + ((textures[i] == "Unknown") ? "unknown_n.dds" : textures[i].Insert(textures[i].LastIndexOf("."), "_n").ToLower()) + "</Name>");
                sb.AppendLine("          </Item>");
                sb.AppendLine("          <Item name=\"HardAlphaBlend\" type =\"Vector\" x =\"1\" y =\"0\" z =\"0\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"useTessellation\" type =\"Vector\" x =\"1\" y =\"0.5\" z =\"0\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"wetnessMultiplier\" type =\"Vector\" x =\"1\" y =\"1\" z =\"0\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"bumpiness\" type =\"Vector\" x =\"0\" y =\"1\" z =\"0\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"specularIntensityMult\" type =\"Vector\" x =\"0\" y =\"1\" z =\"1\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"specularFalloffMult\" type =\"Vector\" x =\"0\" y =\"0\" z =\"1\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"specularFresnel\" type =\"Vector\" x =\"0.5\" y =\"0\" z =\"1\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"globalAnimUV1\" type =\"Vector\" x =\"1\" y =\"0\" z =\"1\" w =\"1\" />");
                sb.AppendLine("          <Item name=\"globalAnimUV0\" type =\"Vector\" x =\"1\" y =\"1\" z =\"1\" w =\"1\" />");
                sb.AppendLine("        </Parameters>");
                sb.AppendLine("      </Item>");
            }

            sb.AppendLine("    </Shaders>");
            sb.AppendLine("  </ShaderGroup>");
            sb.AppendLine("  <DrawableModelsHigh>");

            for (int i = 0; i < geometries.Count; i++)
            {
                Mesh mesh = geometries[i].Mesh;
                Point3DCollection Vertices = mesh.MeshGeometry.Positions;
                Int32Collection Indices = mesh.MeshGeometry.TriangleIndices;
                PointCollection UV = mesh.MeshGeometry.TextureCoordinates;
                Vector3[] Normals = geometries[i].Normals;
                Vector3[] Tangents = geometries[i].Tangents;
                Rect3D Bounds = mesh.MeshGeometry.Bounds;
                Vector3 BoundsMin = Vector3.Multiply(new Vector3((float)(Bounds.X - Bounds.SizeX), (float)(Bounds.Y - Bounds.SizeY), (float)(Bounds.Z - Bounds.SizeZ)), ExportScale);
                Vector3 BoundsMax = Vector3.Multiply(new Vector3((float)(Bounds.X + Bounds.SizeX), (float)(Bounds.Y + Bounds.SizeY), (float)(Bounds.Z + Bounds.SizeZ)), ExportScale);
                Vector3 bMinRel = Vector3.Substract(BoundsMin, boundsCenter);
                Vector3 bMaxRel = Vector3.Substract(BoundsMax, boundsCenter);

                sb.AppendLine("    <Item>");
                sb.AppendLine("      <RenderMask value=\"255\" />");
                sb.AppendLine("      <Flags value=\"255\" />");
                sb.AppendLine("      <HasSkin value=\"0\" />");
                sb.AppendLine("      <BoneIndex value=\"0\" />");
                sb.AppendLine("      <Unknown1 value=\"0\" />");
                sb.AppendLine("      <Geometries>");
                sb.AppendLine("        <Item>");
                sb.AppendLine("          <ShaderIndex value=\"" + i.ToString() + "\" />");
                sb.AppendLine(string.Format("          <BoundingBoxMin x=\"{0}\" y=\"{1}\" z=\"{2}\" w=\"0\" />", FormatFloat(bMinRel.X), FormatFloat(-bMaxRel.Z), FormatFloat(bMinRel.Y)));
                sb.AppendLine(string.Format("          <BoundingBoxMax x=\"{0}\" y=\"{1}\" z=\"{2}\" w=\"0\" />", FormatFloat(bMaxRel.X), FormatFloat(-bMinRel.Z), FormatFloat(bMaxRel.Y)));
                sb.AppendLine("          <VertexBuffer>");
                sb.AppendLine("            <Flags value=\"0\" />");
                sb.AppendLine("            <Layout type=\"GTAV1\" >");
                sb.AppendLine("              <Position />");
                sb.AppendLine("              <Normal />");
                sb.AppendLine("              <Colour0 />");
                sb.AppendLine("              <TexCoord0 />");
                sb.AppendLine("              <Tangent />");
                sb.AppendLine("            </Layout>");
                sb.AppendLine("            <Data2>");
                for (int v = 0; v < Vertices.Count; v++)
                {
                    Vector3 pos = Vector3.Substract(Vector3.Multiply(new Vector3((float)Vertices[v].X, (float)Vertices[v].Y, (float)Vertices[v].Z), ExportScale), boundsCenter);
                    Vector3 normals = (v < Normals.Length) ? Normals[v] : new Vector3(1f, 1f, -1f);
                    sb.AppendLine(string.Format("              {0}   {1}   163 0 161 255   {2}   {3} 1", FormatVector3(pos), FormatVector3(normals), FormatVector2(UV[v]), FormatVector3(Tangents[v])));
                }
                sb.AppendLine("            </Data2>");
                sb.AppendLine("          </VertexBuffer>");
                sb.AppendLine("          <IndexBuffer>");
                sb.AppendLine("            <Data>");
                for (int v = 0; v < Indices.Count; v++)
                {
                    sb.Append(string.Format("{0} ", Indices[v]));
                }
                sb.AppendLine("\n            </Data>");
                sb.AppendLine("          </IndexBuffer>");
                sb.AppendLine("        </Item>");
                sb.AppendLine("      </Geometries>");
                sb.AppendLine("    </Item>");
            }
            sb.AppendLine("  </DrawableModelsHigh>");
            sb.AppendLine("</Drawable>");
            File.WriteAllText(path, sb.ToString());
        }

        public void CreateShaderAttribute(XmlNode node, XmlDocument doc, string[] param, string[] value)
        {
            for (int i = 0; i < param.Length; i++)
            {
                node.Attributes.Append(doc.CreateAttribute(param[i])).Value = value[i];
            }
        }

        public string FormatFloat(float value)
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }

        public string FormatVector2(System.Windows.Point str)
        {
            return string.Format("{0} {1}", str.X.ToString("G", CultureInfo.InvariantCulture), str.Y.ToString("G", CultureInfo.InvariantCulture));
        }

        public string FormatVector3(Vector3 str) //XZY
        {
            return string.Format("{0} {1} {2}", str.X.ToString("G", CultureInfo.InvariantCulture), (-str.Z).ToString("G", CultureInfo.InvariantCulture), str.Y.ToString("G", CultureInfo.InvariantCulture));
        }

        #endregion
    }
}