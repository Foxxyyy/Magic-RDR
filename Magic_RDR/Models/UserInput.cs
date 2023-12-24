using System.Windows.Forms;

namespace Magic_RDR.Models
{
    public partial class UserInput : Form
    {
        public static float PositionX, PositionY, PositionZ;

        private void validatedButton_Click(object sender, System.EventArgs e)
        {
            try
            {
                float x = float.Parse(textBoxX.Text);
                float y = float.Parse(textBoxY.Text);
                float z = float.Parse(textBoxZ.Text);

                PositionX = x;
                PositionY = y;
                PositionZ = z;
            }
            catch
            {
                MessageBox.Show("A value contains invalid symbols", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public UserInput()
        {
            InitializeComponent();
        }
    }
}
