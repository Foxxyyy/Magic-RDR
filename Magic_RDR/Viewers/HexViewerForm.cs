using Be.Windows.Forms;
using Magic_RDR.RPF;
using System;
using System.Windows.Forms;
using static Magic_RDR.RPF6.RPF6TOC;

namespace Magic_RDR.Viewers
{
    public partial class HexViewerForm : Form
    {
        public HexViewerForm(TOCSuperEntry entry)
        {
            InitializeComponent();
            Text = string.Format("MagicRDR - Simple Hex Viewer [{0}]", entry.Entry.Name);
            charCountLabel.Text = string.Format("{0} bytes", entry.Entry.AsFile.SizeInArchive);

            var file = entry.Entry.AsFile;
            RPFFile.RPFIO.Position = file.GetOffset();

            byte[] data;
            if (file.FlagInfo.IsResource)
                data = ResourceUtils.ResourceInfo.GetDataFromResourceBytes(RPFFile.RPFIO.ReadBytes(file.SizeInArchive));
            else if (file.FlagInfo.IsCompressed)
            {
                if (AppGlobals.Platform == AppGlobals.PlatformEnum.Switch)
                    data = DataUtils.DecompressZStandard(RPFFile.RPFIO.ReadBytes(file.SizeInArchive));
                else
                    data = DataUtils.DecompressDeflate(RPFFile.RPFIO.ReadBytes(file.SizeInArchive), file.FlagInfo.GetTotalSize());
            }
            else data = RPFFile.RPFIO.ReadBytes(file.SizeInArchive);

            try
            {
                var byteProvider = new DynamicByteProvider(data);
                hexBox.ByteProvider = byteProvider;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured while reading file :\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
    }
}
