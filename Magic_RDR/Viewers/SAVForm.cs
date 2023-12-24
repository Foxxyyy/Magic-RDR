using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Magic_RDR.Viewers
{
    public partial class SAVForm : Form
    {
        private uint LanguageHash { get; set; }
        private uint LanguageID { get; set; }
        private byte[] BootBuffer { get; set; }
        private string BootFilePath { get; set; }

        public SAVForm(string directory)
        {
            InitializeComponent();

            this.BootFilePath = directory + "\\PREBOOT.SAV";
            this.BootBuffer = File.ReadAllBytes(this.BootFilePath);

            BinaryReader bootReader = new BinaryReader(new MemoryStream(this.BootBuffer));
            bootReader.BaseStream.Position = 0x4;
            this.LanguageHash = bootReader.ReadUInt32();

            bootReader.BaseStream.Position = 0x24;
            this.LanguageID = bootReader.ReadUInt32();

            string str1 =  this.GetLanguageFromLanguageHash();
            string str2 =  this.GetLanguageFromLanguageID();

            if (str1 == str2)
            {
                this.currentLanguageLabel.Text = "Current language : " + str1;
                this.languageComboBox.Text = str1;

                int languageIndex = -1;
                for (int i = 0; i < this.languageComboBox.Items.Count; i++)
                {
                    if (string.Equals(str1, this.languageComboBox.Items[i].ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        languageIndex = i;
                        break;
                    }
                }
                this.languageComboBox.SelectedIndex = languageIndex;
            }
            else throw new Exception("Corrupted file(s)...");
        }

        private string GetLanguageFromLanguageHash()
        {
            switch (this.LanguageHash)
            {
                case 30116437U:
                    return "English";
                case 3991934154U:
                    return "French";
                case 130546859U:
                    return "Korean";
                case 1884892763U:
                    return "Portuguese";
                case 2674166715U:
                    return "Russian";
                default:
                    return "Unknown";
            }
        }

        private string GetLanguageFromLanguageID()
        {
            switch (this.LanguageID)
            {
                case 0:
                    return "English";
                case 2:
                    return "French";
                case 9:
                    return "Korean";
                case 11:
                    return "Portuguese";
                case 13:
                    return "Russian";
                default:
                    return "Unknown";
            }
        }

        private uint GetLanguageHashFromLanguageID()
        {
            switch (this.LanguageID)
            {
                default:
                case 0:
                    return 30116437U;
                case 2:
                    return 3991934154U;
                case 9:
                    return 130546859U;
                case 11:
                    return 1884892763U;
                case 13:
                    return 2674166715U;
            }
        }

        private uint GetLanguageIDFromComboBoxID()
        {
            switch (this.languageComboBox.SelectedIndex)
            {
                default:
                case 0:
                    return 0;
                case 1:
                    return 2;
                case 2:
                    return 9;
                case 3:
                    return 11;
                case 4:
                    return 13;
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.LanguageID = this.GetLanguageIDFromComboBoxID();
                this.LanguageHash = this.GetLanguageHashFromLanguageID();

                //Save BOOT.SAV
                BinaryWriter bootWriter = new BinaryWriter(new MemoryStream(this.BootBuffer));
                bootWriter.BaseStream.Position = 0x4;
                bootWriter.Write(this.LanguageHash);
                bootWriter.BaseStream.Position = 0x24;
                bootWriter.Write(this.LanguageID);
                File.WriteAllBytes(this.BootFilePath, this.BootBuffer);

                MessageBox.Show("Successfully saved file!\n\nMake sure to close MagicRDR before running emulators.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't save files :\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
