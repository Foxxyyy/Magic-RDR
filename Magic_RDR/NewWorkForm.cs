using Magic_RDR.RPF;
using Pik.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_RDR
{
    public partial class NewWorkForm : Form
    {
        public Exception OpenRPFException;
        public Exception SaveRPFException;
        public RPF6 OpenRPF6Return;
        public RPF6 RPFToSave;
        public RPF6 RPFToExportFilesFrom;
        public RPF6 RPFToLoadDirFrom;
        public Stream OpenRPFStream;
        public Stream SaveRPFStream;
        public Progress<RPF6Report> Prog;

        public bool Done { get; set; }
        public IProgress<RPF6Report> IProg => Prog;

        public NewWorkForm(Stream openRPFStream)
        {
            InitializeComponent();

            if (MainForm.CommandLine)
            {
                foreach (Control control in this.Controls)
                {
                    control.Visible = false;
                }
            }

            Prog = new Progress<RPF6Report>(new Action<RPF6Report>(ProgReport));
            OpenRPFStream = openRPFStream;
            DoOpenRPF();
        }

        public NewWorkForm(Stream saveRPFStream, RPF6 rpfToSave)
        {
            InitializeComponent();

            if (MainForm.CommandLine)
            {
                foreach (Control control in this.Controls)
                {
                    control.Visible = false;
                }
            }

            Prog = new Progress<RPF6Report>(new Action<RPF6Report>(ProgReport));
            SaveRPFStream = saveRPFStream;
            RPFToSave = rpfToSave;
            DoSaveRPF();
        }

        private async void DoOpenRPF()
        {
            try
            {
                LoadEvents();
                await Task.Factory.StartNew(() =>
                {
                    if (!RPF6.RPF6Header.HasIdentifier(new PikIO(OpenRPFStream, PikIO.Endianess.Big)))
                    {
                        throw new Exception("RPF File doesn't have valid header magic.");
                    }
                    OpenRPF6Return = new RPF6(OpenRPFStream);
                });
                ClearEvents();
                Done = true;
                Close();
            }
            catch (Exception ex)
            {
                OpenRPFException = ex;
                ClearEvents();
            }
        }

        private async void DoSaveRPF()
        {
            try
            {
                LoadEvents();
                await Task.Factory.StartNew(() => RPFToSave.Write(SaveRPFStream));
                ClearEvents();
                Done = true;
                Close();
            }
            catch (Exception ex)
            {
                SaveRPFException = ex;
                ClearEvents();
            }
        }

        public void OnOperation(object sender, RPF6Report e) => IProg.Report(e);

        private void LoadEvents() => RPF6.OnOperation += new EventHandler<RPF6Report>(OnOperation);

        private void ClearEvents() => RPF6.OnOperation -= new EventHandler<RPF6Report>(OnOperation);

        public void ProgReport(RPF6Report progreport)
        {
            if (!MainForm.CommandLine)
            {
                titleLabel.Text = progreport.TitleText;
                progBar.Value = progreport.Percent;
                titleLabel.Invalidate();
                progBar.Invalidate();
                titleLabel.Update();
                progBar.Update();
            }
        }
    }
}
