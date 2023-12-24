using System.Drawing;
using System.Windows.Media.Media3D;

namespace Magic_RDR.Application
{
    public class Geometry
    {
        public ModelViewer.Mesh Mesh { get; set; }
        public string TextureName { get; set; }
        public long VertexsPointer { get; set; }
        public ulong FormatIndices { get; set; }
        public uint FormatMask { get; set; }
        public int IndexCount { get; set; }
        public int FaceCount { get; set; }
        public int VertexCount { get; set; }
        public Vector3[] Vertexs { get; set; }
        public Vector2[] UVs { get; set; }
        public Vector3[] Normals { get; set; }
        public Vector3[] Tangents { get; set; }
        public Color[] VertexColors { get; set; }
        public ushort[] Indices { get; set; }

        public Geometry(ModelViewer.Mesh mesh, string textureName, int indexCount, int faceCount, int vertexCount, Vector3[] vertexs, Vector2[] uvs, ushort[] indices, Vector3[] normals, Vector3[] tangents, Color[] vertexColors, long vertexsPointer, ulong formatIndices, uint mask)
        {
            Mesh = mesh;
            TextureName = textureName;
            IndexCount = indexCount;
            FaceCount = faceCount;
            VertexCount = vertexCount;
            Vertexs = vertexs;
            UVs = uvs;
            Indices = indices;
            Normals = normals;
            Tangents = tangents;
            VertexColors = vertexColors;
            VertexsPointer = vertexsPointer;
            FormatMask = mask;
            FormatIndices = formatIndices;
        }
    }

    public static class Helper3D
    {
        public static Point3DCollection GetModelBoundsPoints(this Model3D model)
        {
            Rect3D bounds = model.Bounds;
            Point3DCollection linesPoints = new Point3DCollection();

            double lengthX = bounds.Size.X;
            double lengthY = bounds.Size.Y;
            double lengthZ = bounds.Size.Z;
            double posX = model.Bounds.X + lengthX / 2;
            double posY = model.Bounds.Y + lengthY / 2;
            double posZ = model.Bounds.Z + lengthZ / 2;

            Point3D a = new Point3D(posX - (lengthX / 2), posY - (lengthY / 2), posZ - (lengthZ / 2));
            Point3D b = new Point3D(posX - (lengthX / 2), posY - (lengthY / 2), posZ + (lengthZ / 2));
            Point3D c = new Point3D(posX - (lengthX / 2), posY + (lengthY / 2), posZ + (lengthZ / 2));
            Point3D d = new Point3D(posX - (lengthX / 2), posY + (lengthY / 2), posZ - (lengthZ / 2));

            Point3D e = new Point3D(posX + (lengthX / 2), posY + (lengthY / 2), posZ + (lengthZ / 2));
            Point3D f = new Point3D(posX + (lengthX / 2), posY + (lengthY / 2), posZ - (lengthZ / 2));
            Point3D g = new Point3D(posX + (lengthX / 2), posY - (lengthY / 2), posZ + (lengthZ / 2));
            Point3D h = new Point3D(posX + (lengthX / 2), posY - (lengthY / 2), posZ - (lengthZ / 2));

            //Vertical lines
            linesPoints.Add(a);
            linesPoints.Add(b);
            linesPoints.Add(c);
            linesPoints.Add(d);
            linesPoints.Add(e);
            linesPoints.Add(f);
            linesPoints.Add(g);
            linesPoints.Add(h);

            //Horizontal lines
            linesPoints.Add(a);
            linesPoints.Add(d);
            linesPoints.Add(b);
            linesPoints.Add(c);
            linesPoints.Add(e);
            linesPoints.Add(g);
            linesPoints.Add(f);
            linesPoints.Add(h);
            linesPoints.Add(a);
            linesPoints.Add(h);
            linesPoints.Add(b);
            linesPoints.Add(g);
            linesPoints.Add(c);
            linesPoints.Add(e);
            linesPoints.Add(d);
            linesPoints.Add(f);

            return linesPoints;
        }
    }
}
