using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ModelViewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace Magic_RDR.Viewers
{
    public partial class DirectXForm : Form
    {
		private Device device = null;
		private float moveSpeed = 0.5f;
		private float turnSpeed = 0.005f;
		private float rotY = 0;
		private float tempY = 0;
		private float rotXZ = 0;
		private float tempXZ = 0;
		private Vector3 camPosition, camLookAt, camUp;
		public List<List<Mesh>> meshs;
		private bool isRightMouseDown = false;
		private bool invalidating = true;

		public DirectXForm(object magicMeshs)
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true); //this needs to be set so the invalidate() function in the paint event works properly
			InitializeComponent();

			PresentParameters pp = new PresentParameters();
			pp.Windowed = true;
			pp.SwapEffect = SwapEffect.Discard; //just how the stuff should be moved from the backbuffer to the front buffer
			pp.EnableAutoDepthStencil = true;
			pp.AutoDepthStencilFormat = DepthFormat.D16;

			device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, pp);
			meshs = (List<List<Mesh>>)magicMeshs;

			camPosition = new Vector3((float)meshs[0][0].MeshGeometry.Positions[0].X, (float)meshs[0][0].MeshGeometry.Positions[0].Y + 80f, (float)meshs[0][0].MeshGeometry.Positions[0].Z);
			camUp = new Vector3(0, 1, 0);

			InitializeEventHandler();
		}

		private void InitializeEventHandler()
		{
			KeyDown += new KeyEventHandler(OnKeyDown);
			MouseWheel += new MouseEventHandler(OnMouseScroll);
			MouseMove += new MouseEventHandler(OnMouseMove);
			MouseDown += new MouseEventHandler(OnMouseDown);
			MouseUp += new MouseEventHandler(OnMouseUp);
		}

		private void SetupCamera()
		{
			camLookAt.X = (float)Math.Sin(rotY) + camPosition.X + (float)(Math.Sin(rotXZ) * Math.Sin(rotY));     
			camLookAt.Y = (float) Math.Sin(rotXZ) + camPosition.Y;  // Bind the camera lookAt somehow with the camera position, so once we move around we also move the lookAt
			camLookAt.Z = (float)Math.Cos(rotY) + camPosition.Z + (float)(Math.Sin(rotXZ) * Math.Cos(rotY));

			device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, Width / Height, 1.0f, 1000.0f);
			device.Transform.View = Matrix.LookAtLH(camPosition, camLookAt, camUp);

			device.RenderState.Lighting = false;
			device.RenderState.CullMode = Cull.CounterClockwise;
			device.RenderState.FillMode = FillMode.Solid;

			label1.Text = "PosX " + camPosition.X.ToString();
			label2.Text = "PosY " + camPosition.Y.ToString();
			label3.Text = "PosZ " + camPosition.Z.ToString();
			label4.Text = "RotX " + camLookAt.X.ToString();
			label5.Text = "RotY " + camLookAt.Y.ToString();
			label6.Text = "RotZ " + camLookAt.Z.ToString();
			System.Windows.Forms.Application.DoEvents();
		}

		private void DirectXForm_Paint(object sender, PaintEventArgs e)
		{
			device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Aqua, 1, 0);
			device.VertexFormat = CustomVertex.PositionColored.Format;
			SetupCamera();

			device.BeginScene();

			for (int s = 0; s < meshs.Count; s++)
            {
				for (int mesh = 0; mesh < meshs[s].Count; mesh++)
				{
					var data = new CustomVertex.PositionColored[meshs[s][mesh].MeshGeometry.Positions.Count];
					Random rd = new Random();
					for (int v = 0; v < data.Length; v++)
					{
						data[v].Position = new Vector3((float)meshs[s][mesh].MeshGeometry.Positions[v].X, (float)meshs[s][mesh].MeshGeometry.Positions[v].Y, (float)meshs[s][mesh].MeshGeometry.Positions[v].Z);
						data[v].Color = Color.Black.ToArgb();
					}

					var indices = new ushort[meshs[s][mesh].MeshGeometry.TriangleIndices.Count];
					for (int f = 0; f < indices.Length; f++)
					{
						indices[f] = (ushort)meshs[s][mesh].MeshGeometry.TriangleIndices[f];
					}

					VertexBuffer vBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), data.Length, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
					vBuffer.SetData(data, 0, LockFlags.None);

					IndexBuffer iBuffer = new IndexBuffer(typeof(ushort), indices.Length, device, Usage.WriteOnly, Pool.Default);
					iBuffer.SetData(indices, 0, LockFlags.None);

					device.SetStreamSource(0, vBuffer, 0);
					device.Indices = iBuffer;
					device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, data.Length, 0, indices.Length / 3);

					vBuffer.Dispose();
					iBuffer.Dispose();
					data = null;
					indices = null;
				}
			}

			device.EndScene();
			device.Present();

			if (invalidating)
			{
				Invalidate(); //this makes the form redraw itself over and over so that we have some kind of a loop
			}
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Up:
					{
						camPosition.X += moveSpeed * (float)Math.Sin(rotY);
						camPosition.Z += moveSpeed * (float)Math.Cos(rotY);
						break;
					}
				case Keys.Down:
					{
						camPosition.X -= moveSpeed * (float)Math.Sin(rotY);
						camPosition.Z -= moveSpeed * (float)Math.Cos(rotY);
						break;
					}
				case Keys.Right:
                    {
						camPosition.X += moveSpeed * (float)Math.Sin(rotY + Math.PI / 2);
						camPosition.Z += moveSpeed * (float)Math.Cos(rotY + Math.PI / 2);
						break;
					}
				case Keys.Left:
                    {
						camPosition.X -= moveSpeed * (float)Math.Sin(rotY + Math.PI / 2);
						camPosition.Z -= moveSpeed * (float)Math.Cos(rotY + Math.PI / 2);
						break;
					}
			}
		}

		private void OnMouseScroll(object sender, MouseEventArgs e)
		{
			camPosition.Y -= e.Delta * 0.005f;
		}

        private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (isRightMouseDown)
			{
				rotY = tempY + (e.X * turnSpeed);

				float tmp = tempXZ - (e.Y * turnSpeed / 4);
				if (tmp < Math.PI / 2 && tmp > -Math.PI / 2)
				{
					rotXZ = tmp;
				}
			}
		}

        private void OnMouseDown(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Right:
					{
						tempY = rotY - e.X * turnSpeed;
						tempXZ = rotXZ + e.Y * turnSpeed / 4;

						isRightMouseDown = true;
						break;
					}
			}
		}

		private void OnMouseUp(object sender, MouseEventArgs e)
		{
			switch (e.Button)
			{
				case MouseButtons.Right:
					{
						isRightMouseDown = false;
						break;
					}
			}
		}
	}
}