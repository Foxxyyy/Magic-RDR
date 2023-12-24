using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ModelViewer
{
    public partial class ModelView : INotifyPropertyChanged
    {
        private bool _isInvalidated = true;
        private bool _isUpdating, HasWireframeBeenSet, HasVerticesBeenSet;
        private int count = 0;
        private object updateLock = "Foxxyyy";
        private LinesVisual3D wireframe;
        private PointsVisual3D vertices;

        public static List<Mesh[]> CurrentModelMesh;
        public static bool UseWireframe, UseVertices, ModelUseProps;
        public static int ModelChangedFlags = 0x0;

        public static Model3D NewModel;
        public Model3D _Model;
        public Model3D Model
        {
            get => _Model;
            set
            {
                _Model = value;
                OnPropertyChanged("Model");
            }
        }

        public static Point3DCollection NewPoints;
        public Point3DCollection _Points;
        public Point3DCollection Points
        {
            get => _Points;
            set
            {
                _Points = value;
                OnPropertyChanged("Points");
            }
        }

        public static int NewGridSize;
        public int _GridSize;
        public int GridSize
        {
            get => _GridSize;
            set
            {
                _GridSize = value;
                OnPropertyChanged("GridSize");
            }
        }

        public static string NewModelName;
        public string _ModelName;
        public string ModelName
        {
            get => _ModelName;
            set
            {
                _ModelName = value;
                OnPropertyChanged("ModelName");
            }
        }

        public Vector3D _CameraDirection;
        public Vector3D CameraDirection
        {
            get => _CameraDirection;
            set
            {
                _CameraDirection = value;
                OnPropertyChanged("CameraDirection");
            }
        }

        public static Color NewFirstGradientColor;
        public Color _FirstGradientColor;
        public Color FirstGradientColor
        {
            get => _FirstGradientColor;
            set
            {
                _FirstGradientColor = value;
                OnPropertyChanged("FirstGradientColor");
            }
        }

        public static Color NewSecondGradientColor;
        public Color _SecondGradientColor;
        public Color SecondGradientColor
        {
            get => _SecondGradientColor;
            set
            {
                _SecondGradientColor = value;
                OnPropertyChanged("SecondGradientColor");
            }
        }

        public static bool NewGridVisibility;
        public bool _GridVisibility;
        public bool GridVisibility
        {
            get => _GridVisibility;
            set
            {
                _GridVisibility = value;
                OnPropertyChanged("GridVisibility");
            }
        }

        public bool _ModelHasChanged;
        public bool ModelHasChanged
        {
            get => _ModelHasChanged;
            set
            {
                _ModelHasChanged = value;
                OnPropertyChanged("ModelHasChanged");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ModelView()
        {
            InitializeComponent();
            CameraHelper.Reset(View.Camera);

            CompositionTarget.Rendering += RefreshLoop;
            DataContext = this;
            PropertyChanged += ModelChanged;
        }

        private void ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            lock (updateLock)
            {
                _isInvalidated = true;
            }
        }

        private void RefreshLoop(object sender, EventArgs argss)
        {
            if (_isInvalidated && NewModel != null)
               BeginUpdateModel(); //Async

            ModelHasChanged = true;
        }

        private void BeginUpdateModel()
        {
            lock (updateLock)
            {
                if (!_isUpdating)
                {
                    _isInvalidated = false;
                    _isUpdating = true;
                    Dispatcher.Invoke(new Action(UpdateModel));
                }
            }
        }

        private void SetModelWireframe()
        {
            Point3DCollection points = new Point3DCollection();
            for (int i = 0; i < CurrentModelMesh.Count; i++)
            {
                if (CurrentModelMesh[i] == null)
                    continue;

                Mesh[] newMesh = CurrentModelMesh[i];
                for (int m = 0; m < newMesh.Length; m++)
                {
                    if (newMesh[m] == null)
                        continue;

                    MeshGeometry3D meshGeometry = newMesh[m].MeshGeometry;
                    for (int index = 0; index < meshGeometry.TriangleIndices.Count; index += 3)
                    {
                        points.Add(meshGeometry.Positions[meshGeometry.TriangleIndices[index]]);
                        points.Add(meshGeometry.Positions[meshGeometry.TriangleIndices[index + 1]]);
                        points.Add(meshGeometry.Positions[meshGeometry.TriangleIndices[index + 2]]);
                        points.Add(meshGeometry.Positions[meshGeometry.TriangleIndices[index]]);
                    }
                }
            }
            wireframe = new LinesVisual3D();
            wireframe.Points = points;
            wireframe.Color = Colors.LightGreen;
            wireframe.Thickness = 1;

            Vector3D axis = new Vector3D(1, 0, 0);
            Matrix3D matrix = wireframe.Transform.Value;
            matrix.Rotate(new Quaternion(axis, 90));
            wireframe.Transform = new MatrixTransform3D(matrix);
            View.Children.Add(wireframe);
            HasWireframeBeenSet = true;
        }

        private void RemoveModelWireframe()
        {
            if (!View.Children.Contains(wireframe))
                return;

            View.Children.Remove(wireframe);
            wireframe = null;
            HasWireframeBeenSet = false;
        }

        private void SetModelVertices()
        {
            Point3DCollection points = new Point3DCollection();
            for (int i = 0; i < CurrentModelMesh.Count; i++)
            {
                if (CurrentModelMesh[i] == null)
                    continue;

                Mesh[] newMesh = CurrentModelMesh[i];
                for (int m = 0; m < newMesh.Length; m++)
                {
                    if (newMesh[m] == null)
                        continue;

                    MeshGeometry3D meshGeometry = newMesh[m].MeshGeometry;
                    for (int index = 0; index < meshGeometry.Positions.Count; index++)
                        points.Add(meshGeometry.Positions[index]);
                }
            }
            vertices = new PointsVisual3D();
            vertices.Points = points;
            vertices.Color = Colors.Red;
            vertices.Size = 3;

            Vector3D axis = new Vector3D(1, 0, 0);
            Matrix3D matrix = vertices.Transform.Value;
            matrix.Rotate(new Quaternion(axis, 90));
            vertices.Transform = new MatrixTransform3D(matrix);
            View.Children.Add(vertices);
            HasVerticesBeenSet = true;
        }

        private void RemoveModelVertices()
        {
            if (!View.Children.Contains(vertices))
                return;

            View.Children.Remove(vertices);
            vertices = null;
            HasVerticesBeenSet = false;
        }

        private void UpdateModel()
        {
            if (UseWireframe && !HasWireframeBeenSet)
                SetModelWireframe();
            else if (!UseWireframe && HasWireframeBeenSet)
                RemoveModelWireframe();

            if (UseVertices && !HasVerticesBeenSet)
                SetModelVertices();
            else if (!UseVertices && HasVerticesBeenSet)
                RemoveModelVertices();

            Model = NewModel;
            if (ModelUseProps)
                View.LookAt(new Point3D(0, 0, 0), 100);
            else
                CameraDirection = new Vector3D(Model.Bounds.X + (Model.Bounds.SizeX / 2), Model.Bounds.Y + (Model.Bounds.SizeY / 2), Model.Bounds.Z + (Model.Bounds.SizeZ / 2));

            ModelName = NewModelName;
            GridVisibility = NewGridVisibility;
            GridSize = NewGridSize == 0 ? 8 : NewGridSize;
            FirstGradientColor = (NewFirstGradientColor == Color.FromArgb(0, 0, 0, 0)) ? Color.FromArgb(255, 104, 138, 213) : NewFirstGradientColor;
            SecondGradientColor = (NewSecondGradientColor == Color.FromArgb(0, 0, 0, 0)) ? Color.FromArgb(255, 66, 92, 148) : NewSecondGradientColor;

            if (count <= 100 && !ModelUseProps)
                View.ZoomExtents();
            else if (ModelChangedFlags == 2)
                Points = NewPoints;
            else if (ModelChangedFlags == 3)
                Points = new Point3DCollection();

            ModelHasChanged = false;
            _isUpdating = false;
            count++;
        }
    }

    public class Mesh
    {
        public MeshGeometry3D MeshGeometry { get; set; }
        public Material Material { get; set; }

        public Mesh(MeshGeometry3D meshGeometry, Material material)
        {
            MeshGeometry = meshGeometry;
            Material = material;
        }
    }
}
