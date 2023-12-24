using HelixToolkit.Wpf;
using Magic_RDR.Application;
using Magic_RDR.RPF;
using ModelViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static Magic_RDR.RPF6.RPF6TOC;
using static ModelViewer.ModelView;

namespace Magic_RDR.Models
{
    public partial class BoundViewerForm : Form
    {
        private TOCSuperEntry Entry;
        private IOReader Reader;
        private string FileName;
        private byte[] HeaderData;
        private bool[] ModelDefaultMesh;
        private bool NotTerrainCollision;

        public BoundViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            FileName = entry.Entry.Name;
            Text = string.Format("MagicRDR - Collision Viewer - [{0}]", entry.Entry.Name);
            Entry = entry;
            saveButton.Enabled = entry.Entry.Name.EndsWith(".xbd");

            FileEntry file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();
            byte[] compressedData = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);
            byte[] fileData = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(compressedData);

            HeaderData = new byte[0x18];
            for (int i = 0; i < HeaderData.Length; i++)
            {
                HeaderData[i] = compressedData[i];
            }

            Reader = new IOReader(new MemoryStream(fileData), IOReader.Endian.Big);
            Reader.BaseStream.Seek(file.FlagInfo.RSC85_ObjectStart, SeekOrigin.Begin);

            if (entry.Entry.Name.EndsWith(".xtb"))
                new XTB_CollisionData(Reader, file);
            else
            {
                NotTerrainCollision = true;
                new XBD_CollisionData(Reader, file);
            }

            uint verticesCount = 0;
            for (int i = 0; i < (NotTerrainCollision ? XBD_CollisionData.VerticesCount.Length : XTB_CollisionData.VerticesCount.Length); i++)
            {
                verticesCount += NotTerrainCollision ? XBD_CollisionData.VerticesCount[i] : XTB_CollisionData.VerticesCount[i];
            }

            uint polygonsCount = 0;
            for (int i = 0; i < (NotTerrainCollision ? XBD_CollisionData.PolygonsCount.Length : XTB_CollisionData.PolygonsCount.Length); i++)
            {
                polygonsCount += NotTerrainCollision ? XBD_CollisionData.PolygonsCount[i] : XTB_CollisionData.PolygonsCount[i];
            }
            verticeCountLabel.Text = "* Vertices Count : " + verticesCount;
            polygonCountLabel.Text = "* Polygons Count : " + polygonsCount;

            ModelDefaultMesh = new bool[NotTerrainCollision ? XBD_CollisionData.BoundsOffset.Length : XTB_CollisionData.BoundsOffset.Length];
            for (int i = 0; i < ModelDefaultMesh.Length; i++)
            {
                int vertexs = NotTerrainCollision ? XBD_CollisionData.ModelGeometry[i].VertexCount : XTB_CollisionData.ModelGeometry[i].VertexCount;
                int polygons = NotTerrainCollision ? XBD_CollisionData.ModelGeometry[i].FaceCount : XTB_CollisionData.ModelGeometry[i].FaceCount;

                CheckBox box = new CheckBox();
                box.Checked = true;
                box.Dock = DockStyle.Top;
                box.Name = string.Format("GeometryCheckBox{0}", i + 1);
                box.CheckedChanged += GeometryCheckBox_Checked;
                box.Text = string.Format("[BVH] : Geometry {0} (Vertexs: {1}, Polygons: {2})", i + 1, vertexs, polygons);

                box.Margin = new Padding(10, 5, 0, 5);
                panel1.Controls.Add(box);
            }
        }

        private void GeometryCheckBox_Checked(object sender, EventArgs e)
        {
            if (NewModel == null) //How ?
            {
                MessageBox.Show("Invalid collisions...", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CheckBox checkbox = (CheckBox)sender;
            string name = Regex.Match(checkbox.Name, @"\d+").Value;
            int index = Convert.ToInt32(name) - 1;

            Model3DGroup group = new Model3DGroup();
            Application.Geometry CurrentGeometry = null;
            List<Application.Geometry> geometryList = NotTerrainCollision ? XBD_CollisionData.ModelGeometry : XTB_CollisionData.ModelGeometry;

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

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (NewModel == null)
            {
                return;
            }

            if (MessageBox.Show("This will overwrite the file and remove all collisions that are unchecked. You won't be able to re-enable those after that. Making backups is recommended !\n\nContinue ?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            foreach (Control control in panel1.Controls)
            {
                if (!control.Name.Contains("GeometryCheckBox"))
                    continue;

                if (((CheckBox)control).Checked)
                    continue;

                int.TryParse(control.Name.Substring(16), out int geometryIndex);
                Reader.BaseStream.Seek(NotTerrainCollision ? XBD_CollisionData.PolygonsCountOffset[geometryIndex - 1] : XTB_CollisionData.PolygonsCountOffset[geometryIndex - 1], SeekOrigin.Begin);
                Reader.BaseStream.Write(BitConverter.GetBytes(0), 0, 4);

                Reader.BaseStream.Seek(NotTerrainCollision ? XBD_CollisionData.PolygonsOffset[geometryIndex - 1] : XTB_CollisionData.PolygonsOffset[geometryIndex - 1], SeekOrigin.Begin);
                for (int i = 0; i < (NotTerrainCollision ? XBD_CollisionData.PolygonsCount[geometryIndex - 1] : XTB_CollisionData.PolygonsCount[geometryIndex - 1]); i++)
                {
                    float pad = Reader.ReadFloat();
                    Reader.BaseStream.Write(BitConverter.GetBytes(0), 0, 4);
                    Reader.BaseStream.Write(BitConverter.GetBytes(0), 0, 4);
                    Reader.BaseStream.Write(BitConverter.GetBytes(0), 0, 4);
                }
            }

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

            try
            {
                TOCSuperEntry selectedReplaceFile = Entry;
                TOCSuperEntry selectedFile = Entry;
                NewImportReplaceForm importReplaceForm = new NewImportReplaceForm(true, Entry.SuperParent, selectedFile, selectedReplaceFile, null, Entry.Entry.Name, false, new MemoryStream(buffer));
                
                if (!importReplaceForm.IsDisposed)
                    importReplaceForm.ShowDialog();
                if (importReplaceForm.TOCResult == null)
                    return;

                if (selectedFile.CustomDataStream != null)
                {
                    selectedFile.CustomDataStream.Close();
                }
                selectedFile.CustomDataStream = importReplaceForm.TOCResult.CustomDataStream;
                selectedFile.Entry = importReplaceForm.TOCResult.Entry;
                selectedFile.OldEntry = importReplaceForm.TOCResult.OldEntry;
                selectedFile.ReadBackFromRPF = false;
            }
            catch
            {
                if (MessageBox.Show("An error occured while overwriting the current file...\n\nDo you want to export it ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes && buffer != null)
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
            MessageBox.Show("Successfully rebuilt file !", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class XBD_CollisionData
    {
        private IOReader Reader;
        private FileEntry Entry;
        public static object[] CollisionData;
        public static Model3DGroup ModelGroup = new Model3DGroup();
        private List<Mesh[]> MeshsList = new List<Mesh[]>();
        public static List<Application.Geometry> ModelGeometry = new List<Application.Geometry>();

        public static int[] BoundsOffset, PolygonsOffset, VerticesPointer, BVHDataOffset, BVHNodeOffset;
        public static uint[] VerticesCount, PolygonsCount;
        public static long[] PolygonsCountOffset;

        private float SphereRadius, SphereMargin;
        private Vector4 BoundsMax, BoundsMin, BoundsCenter, Quantum, SphereCenter, GeometryCenter;
        private List<short[]> Polygons, EdgeIndexs;
        private List<float> TriAreas;
        private List<Vector3> Vertices;

        public XBD_CollisionData(object reader, object file)
        {
            Reader = (IOReader)reader;
            Entry = (FileEntry)file;

            BoundsOffset = null;
            PolygonsCountOffset = null;
            PolygonsOffset = null;
            VerticesPointer = null;
            BVHDataOffset = null;
            VerticesCount = null;
            PolygonsCount = null;

            XBD_ReadBoundsDictionary();
            if (BoundsOffset == null)
            {
                return;
            }

            ModelGeometry.Clear();
            PolygonsCountOffset = new long[BoundsOffset.Length];
            PolygonsOffset = new int[BoundsOffset.Length];
            VerticesPointer = new int[BoundsOffset.Length];
            BVHDataOffset = new int[BoundsOffset.Length];
            BVHNodeOffset = new int[BoundsOffset.Length];
            VerticesCount = new uint[BoundsOffset.Length];
            PolygonsCount = new uint[BoundsOffset.Length];
            CollisionData = new XBD_Data[BoundsOffset.Length];

            for (int i = 0; i < BoundsOffset.Length; i++)
            {
                Polygons = new List<short[]>();
                EdgeIndexs = new List<short[]>();
                TriAreas = new List<float>();
                Vertices = new List<Vector3>();

                Reader.BaseStream.Seek(BoundsOffset[i], SeekOrigin.Begin);
                XBD_ReadBoundsData(i);

                if (BVHDataOffset[i] <= 0)
                    continue;

                Reader.BaseStream.Seek(BVHDataOffset[i], SeekOrigin.Begin);
                uint NodeCount = XBD_ReadBVHData(i);

                if (BVHNodeOffset[i] <= 0)
                    continue;

                Reader.BaseStream.Seek(BVHNodeOffset[i], SeekOrigin.Begin);
                XBD_ReadBVHNodeData(i, NodeCount);
                XBD_GetModelData(i);
                CollisionData[i] = new XBD_Data(BoundsMin, BoundsMax, BoundsCenter, SphereCenter, GeometryCenter, SphereRadius, SphereMargin, Quantum, Vertices, TriAreas, EdgeIndexs, Polygons);
            }

            NewModel = ModelGroup;
            Vector3D axis = new Vector3D(1, 0, 0);
            Matrix3D matrix = NewModel.Transform.Value;
            matrix.Rotate(new Quaternion(axis, 90));
            NewModel.Transform = new MatrixTransform3D(matrix);
            CurrentModelMesh = MeshsList;
        }

        private void XBD_ReadBoundsDictionary()
        {
            uint Magic = Reader.ReadUInt32();
            RGArray Unknown0x04;
            Unknown0x04.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown0x04.Count = Reader.ReadUInt16();
            Unknown0x04.Size = Reader.ReadUInt16();
            uint Unknown0xC = Reader.ReadUInt32();

            RGArray HashsCollectionOffset;
            HashsCollectionOffset.Offset = Reader.ReadOffset(Reader.ReadInt32());
            HashsCollectionOffset.Count = Reader.ReadUShort();
            HashsCollectionOffset.Size = Reader.ReadUShort();

            RGArray BoundsCollectionOffset;
            BoundsCollectionOffset.Offset = Reader.ReadOffset(Reader.ReadInt32());
            BoundsCollectionOffset.Count = Reader.ReadUShort();
            BoundsCollectionOffset.Size = Reader.ReadUShort();

            Reader.BaseStream.Seek(HashsCollectionOffset.Offset, SeekOrigin.Begin);
            uint[] Hashs = new uint[HashsCollectionOffset.Count];
            for (int i = 0; i < HashsCollectionOffset.Count; i++)
            {
                Hashs[i] = Reader.ReadUInt32();
            }

            BoundsOffset = new int[BoundsCollectionOffset.Count];
            Reader.BaseStream.Seek(BoundsCollectionOffset.Offset, SeekOrigin.Begin);
            for (int i = 0; i < BoundsCollectionOffset.Count; i++)
            {
                BoundsOffset[i] = Reader.ReadOffset(Reader.ReadInt32());
            }
        }

        private void XBD_ReadBoundsData(int index)
        {
            uint VTable = Reader.ReadUInt32();
            _ = Reader.ReadVector4();
            byte BoundsType = Reader.ReadByte(); //Composite
            byte Unknown_0x15 = Reader.ReadByte();
            ushort Unknown_0x16 = Reader.ReadUInt16();
            SphereRadius = Reader.ReadFloat();
            SphereMargin = Reader.ReadFloat();
            BoundsMax = Reader.ReadVector4();
            BoundsMin = Reader.ReadVector4();
            BoundsCenter = Reader.ReadVector4();
            Vector4 Unknown_0x50 = Reader.ReadVector4();
            SphereCenter = Reader.ReadVector4();
            Vector4 Transforms = Reader.ReadVector4();
            Quantum = Reader.ReadVector4();
            Vector3 Unknown_0x90 = Reader.ReadVector3();
            PolygonsOffset[index] = Reader.ReadOffset(Reader.ReadInt32());
            Vector4 Unknown_0xA0 = Reader.ReadVector4();
            GeometryCenter = Reader.ReadVector4();
            VerticesPointer[index] = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_0xC4 = Reader.ReadUInt32();
            ushort Unknown_0xC8 = Reader.ReadUShort();
            VerticesCount[index] = Reader.ReadUShort();
            Vector3 Unknown_0xCC = Reader.ReadVector3();
            uint VerticesCount2 = Reader.ReadUInt32();
            PolygonsCountOffset[index] = Reader.BaseStream.Position;
            PolygonsCount[index] = Reader.ReadUInt32();
            int Unknown_0xE0 = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_0xE4 = Reader.ReadUInt32();
            int Unknown_Ex8 = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_0xEC = Reader.ReadUInt32();
            BVHDataOffset[index] = Reader.ReadOffset(Reader.ReadInt32());
        }

        private uint XBD_ReadBVHData(int index) //phBVH - 0x70
        {
            BVHNodeOffset[index] = Reader.ReadOffset(Reader.ReadInt32());
            uint NodeCount = Reader.ReadUInt32();
            uint Size = Reader.ReadUInt32();
            uint m_depth = Reader.ReadUInt32();
            BoundsMin = Reader.ReadVector4(); //m_aabbMin
            BoundsMax = Reader.ReadVector4(); //m_aabbMax
            BoundsCenter = Reader.ReadVector4(); //m_center
            Vector4 m_divisor = Reader.ReadVector4(); //m_scale = (1.0f / m_divisor)
            Vector4 m_scale = Reader.ReadVector4();
            return NodeCount;
        }

        private void XBD_ReadBVHNodeData(int index, uint nodeCount)
        {
            short[] m_quantizedAabbMin = new short[3];
            short[] m_quantizedAabbMax = new short[3];

            for (int i = 0; i < nodeCount; i++)
            {
                m_quantizedAabbMin = new short[] { Reader.ReadShort(), Reader.ReadShort(), Reader.ReadShort() };
                m_quantizedAabbMax = new short[] { Reader.ReadShort(), Reader.ReadShort(), Reader.ReadShort() };
                ushort m_escapeIndexOrTriangleIndex = Reader.ReadUShort();
                byte m_count = Reader.ReadByte();
                byte m_pad = Reader.ReadByte();
            }
        }

        private void XBD_GetModelData(int index)
        {
            MeshGeometry3D ModelMesh = new MeshGeometry3D();
            Reader.BaseStream.Seek(VerticesPointer[index], SeekOrigin.Begin);

            for (int i = 0; i < VerticesCount[index]; i++)
            {
                short x = Reader.ReadShort();
                short y = Reader.ReadShort();
                short z = Reader.ReadShort();
                ModelMesh.Positions.Add(new Point3D(x / 256.0f, y / 256.0f, z / 256.0f));
                Vertices.Add(new Vector3(x / 256.0f, y / 256.0f, z / 256.0f));
            }

            Reader.BaseStream.Seek(PolygonsOffset[index], SeekOrigin.Begin);
            for (int i = 0; i < PolygonsCount[index]; i++)
            {
                TriAreas.Add(Reader.ReadFloat());
                short x = Reader.ReadShort();
                short y = Reader.ReadShort();
                short z = Reader.ReadShort();
                ModelMesh.TriangleIndices.Add(x);
                ModelMesh.TriangleIndices.Add(y);
                ModelMesh.TriangleIndices.Add(z);
                Polygons.Add(new short[] { x, y, z });
                EdgeIndexs.Add(new short[] { Reader.ReadShort(), Reader.ReadShort(), Reader.ReadShort() });
            }

            Random random = new Random();
            Material material = MaterialHelper.CreateMaterial(Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255)));
            Model3DGroup Group = new Model3DGroup();
            Mesh mesh = new Mesh(ModelMesh, material);
            MeshsList.Add(new Mesh[] { mesh });

            GeometryModel3D geometryModel = new GeometryModel3D(ModelMesh, material)
            {
                BackMaterial = material
            };
            Group.Children.Add(geometryModel);
            ModelGroup.Children.Add(Group);
            ModelGeometry.Add(new Application.Geometry(mesh, "Unknown", 0, (int)PolygonsCount[index], (int)VerticesCount[index], null, null, null, null, null, null, 0, 0, 0));
        }

        public class XBD_Data
        {
            public Vector4 BoundsMin;
            public Vector4 BoundsMax;
            public Vector4 BoxCenter;
            public float SphereRadius;
            public float Margin;
            public Vector4 Quantum;
            public Vector4 SphereCenter;
            public Vector4 GeometryCenter;
            public List<Vector3> Vertices;
            public List<float> TriArea;
            public List<short[]> EdgeIndexs;
            public List<short[]> Polygons;

            public XBD_Data(Vector4 boundsMin, Vector4 boundsMax, Vector4 boxCenter, Vector4 sphereCenter, Vector4 geoCenter, float sphereRadius, float margin, Vector4 quantum, List<Vector3> vertices, List<float> triArea, List<short[]> edges, List<short[]> polygons)
            {
                BoundsMin = boundsMin;
                BoundsMax = boundsMax;
                BoxCenter = boxCenter;
                SphereCenter = sphereCenter;
                SphereRadius = sphereRadius;
                Margin = margin;
                Quantum = quantum;
                GeometryCenter = geoCenter;
                Vertices = vertices;
                TriArea = triArea;
                EdgeIndexs = edges;
                Polygons = polygons;
            }
        }
    }

    public class XTB_CollisionData
    {
        private IOReader Reader;
        private FileEntry Entry;
        public static Model3DGroup ModelGroup = new Model3DGroup();
        private List<Mesh[]> MeshsList = new List<Mesh[]>();
        public static List<Application.Geometry> ModelGeometry = new List<Application.Geometry>();

        private int TerrainBoundsOffset = -1;
        public static int[] BoundsOffset, PolygonsOffset, VerticesPointer, BVHDataOffset, BVHNodeOffset;
        public static uint[] VerticesCount, PolygonsCount;
        public static long[] PolygonsCountOffset;

        public XTB_CollisionData(object reader, object file)
        {
            Reader = (IOReader)reader;
            Entry = (FileEntry)file;

            BoundsOffset = null;
            PolygonsCountOffset = null;
            PolygonsOffset = null;
            VerticesPointer = null;
            BVHDataOffset = null;
            VerticesCount = null;
            PolygonsCount = null;

            XTB_ReadFileStructure();
            XTB_ReadTerrainBounds();

            ModelGeometry.Clear();
            PolygonsCountOffset = new long[BoundsOffset.Length];
            PolygonsOffset = new int[BoundsOffset.Length];
            VerticesPointer = new int[BoundsOffset.Length];
            BVHDataOffset = new int[BoundsOffset.Length];
            BVHNodeOffset = new int[BoundsOffset.Length];
            VerticesCount = new uint[BoundsOffset.Length];
            PolygonsCount = new uint[BoundsOffset.Length];

            for (int i = 0; i < BoundsOffset.Length; i++)
            {
                Reader.BaseStream.Seek(BoundsOffset[i], SeekOrigin.Begin);
                int BoundsChildOffset = Reader.ReadOffset(Reader.ReadInt32());

                Reader.BaseStream.Seek(BoundsChildOffset, SeekOrigin.Begin);
                XTB_ReadBoundsData(i);

                if (BVHDataOffset[i] <= 0)
                {
                    ModelGeometry.Add(new Application.Geometry(null, "Unknown", 0, 0, 0, null, null, null, null, null, null, 0, 0, 0));
                    continue;
                }
                XTB_GetModelData(i);
            }
            NewModel = ModelGroup;
            Vector3D axis = new Vector3D(1, 0, 0);
            Matrix3D matrix = NewModel.Transform.Value;
            matrix.Rotate(new Quaternion(axis, 90));
            NewModel.Transform = new MatrixTransform3D(matrix);
            CurrentModelMesh = MeshsList;
        }

        private void XTB_ReadFileStructure()
        {
            uint Magic = Reader.ReadUInt32();
            int Unknown_0x04 = Reader.ReadOffset(Reader.ReadInt32());
            TerrainBoundsOffset = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_Hash = Reader.ReadUInt32();
            uint Unknown_Size1 = Reader.ReadUInt32();
            uint Unknown_Size2 = Reader.ReadUInt32();
            float Unknown_0x18 = Reader.ReadFloat(); //Sphere center?
            float Unknown_0x1C = Reader.ReadFloat(); //Margin?

            RGArray Unknown_0x20; //Quantization???
            Unknown_0x20.Offset = Reader.ReadOffset(Reader.ReadInt32());
            Unknown_0x20.Count = Reader.ReadUShort();
            Unknown_0x20.Size = Reader.ReadUShort();
        }

        private void XTB_ReadTerrainBounds()
        {
            if (TerrainBoundsOffset <= -1)
            {
                return;
            }

            Reader.BaseStream.Seek(TerrainBoundsOffset + 0x18, SeekOrigin.Begin); //Skipping some infos, mainly hashs
            RGArray BoundsOffsetCollection;
            BoundsOffsetCollection.Offset = Reader.ReadOffset(Reader.ReadInt32());
            BoundsOffsetCollection.Count = Reader.ReadUShort();
            BoundsOffsetCollection.Size = Reader.ReadUShort();

            Reader.BaseStream.Seek(BoundsOffsetCollection.Offset, SeekOrigin.Begin);
            BoundsOffset = new int[BoundsOffsetCollection.Count];
            for (int i = 0; i < BoundsOffsetCollection.Count; i++)
            {
                BoundsOffset[i] = Reader.ReadOffset(Reader.ReadInt32());
            }
        }

        private void XTB_ReadBoundsData(int index)
        {
            uint VTable = Reader.ReadUInt32();
            _ = Reader.ReadVector4();
            byte BoundsType = Reader.ReadByte(); //Composite

            if (BoundsType != 10)
                return;

            byte Unknown_0x15 = Reader.ReadByte();
            ushort Unknown_0x16 = Reader.ReadUInt16();
            float SphereRadius = Reader.ReadFloat();
            float Margin = Reader.ReadFloat();
            Vector4 BoundsMax = Reader.ReadVector4();
            Vector4 BoundsMin = Reader.ReadVector4();
            Vector4 BoundsCenter = Reader.ReadVector4();
            Vector4 Unknown_0x50 = Reader.ReadVector4();
            Vector4 Unknown_0x60 = Reader.ReadVector4();
            Vector4 Transforms = Reader.ReadVector4();
            Vector4 Quantum = Reader.ReadVector4();
            Vector3 Unknown_0x90 = Reader.ReadVector3();
            PolygonsOffset[index] = Reader.ReadOffset(Reader.ReadInt32());
            Vector4 Unknown_0xA0 = Reader.ReadVector4();
            Vector4 Unknown_0xB0 = Reader.ReadVector4(); //Maybe geometry center
            VerticesPointer[index] = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_0xC4 = Reader.ReadUInt32();
            ushort Unknown_0xC8 = Reader.ReadUShort();
            VerticesCount[index] = Reader.ReadUShort();
            Vector3 Unknown_0xCC = Reader.ReadVector3();
            uint VerticesCount2 = Reader.ReadUInt32();
            PolygonsCountOffset[index] = Reader.BaseStream.Position;
            PolygonsCount[index] = Reader.ReadUInt32();
            int Unknown_0xE0 = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_0xE4 = Reader.ReadUInt32();
            int Unknown_Ex8 = Reader.ReadOffset(Reader.ReadInt32());
            uint Unknown_0xEC = Reader.ReadUInt32();
            BVHDataOffset[index] = Reader.ReadOffset(Reader.ReadInt32());
        }

        private void XTB_GetModelData(int index)
        {
            MeshGeometry3D ModelMesh = new MeshGeometry3D();
            List<short[]> polygons = new List<short[]>();
            List<short[]> edgeIndexs = new List<short[]>();
            List<float> triAreas = new List<float>();

            Reader.BaseStream.Seek(VerticesPointer[index], SeekOrigin.Begin);
            for (int i = 0; i < VerticesCount[index]; i++)
            {
                short x = Reader.ReadShort();
                short y = Reader.ReadShort();
                short z = Reader.ReadShort();
                ModelMesh.Positions.Add(new Point3D(x / 256.0f, y / 256.0f, z / 256.0f));
            }

            Reader.BaseStream.Seek(PolygonsOffset[index], SeekOrigin.Begin);
            for (int i = 0; i < PolygonsCount[index]; i++)
            {
                triAreas.Add(Reader.ReadFloat());
                short x = Reader.ReadShort();
                short y = Reader.ReadShort();
                short z = Reader.ReadShort();
                ModelMesh.TriangleIndices.Add(x);
                ModelMesh.TriangleIndices.Add(y);
                ModelMesh.TriangleIndices.Add(z);
                polygons.Add(new short[] { x, y, z });
                edgeIndexs.Add(new short[] { Reader.ReadShort(), Reader.ReadShort(), Reader.ReadShort() });
            }

            Random random = new Random();
            Material material = MaterialHelper.CreateMaterial(Color.FromArgb(255, (byte)random.Next(0, 255), (byte)random.Next(0, 255), (byte)random.Next(0, 255)));
            Model3DGroup Group = new Model3DGroup();
            Mesh mesh = new Mesh(ModelMesh, material);
            MeshsList.Add(new Mesh[] { mesh });

            GeometryModel3D geometryModel = new GeometryModel3D(ModelMesh, material)
            {
                BackMaterial = material
            };
            Group.Children.Add(geometryModel);
            ModelGroup.Children.Add(Group);
            ModelGeometry.Add(new Application.Geometry(mesh, "Unknown", 0, (int)PolygonsCount[index], (int)VerticesCount[index], null, null, null, null, null, null, 0, 0, 0));
        }
    }
}
