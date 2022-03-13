using FakePatch;
using System;
using System.IO;
using System.Windows.Forms;
using static FakePatch.LogHelper;

namespace KeyGenerator
{
    public partial class KeyGenForm : Form
    {
        static FileInfo KeyFile;
        public KeyGenForm()
        {
            InitializeComponent();
        }

        public FileInfo browseForFileOpen(string InitialDirectory = @"C:\", string Filter = "All files|*.*", int FilterIndex = 1)
        {

            if (!Directory.Exists(InitialDirectory)) { InitialDirectory = Path.GetPathRoot(Environment.SystemDirectory); }

            _openFileDialog.InitialDirectory = InitialDirectory;
            _openFileDialog.Filter = Filter;
            _openFileDialog.FilterIndex = FilterIndex;
            _openFileDialog.RestoreDirectory = true;
            _openFileDialog.CheckFileExists = true;
            _openFileDialog.CheckPathExists = true;
            _openFileDialog.ValidateNames = true;

            FileInfo fName = null;
            Log("browseForFileOpen " + InitialDirectory);
            if (_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fName = new FileInfo(_openFileDialog.FileName);
                Log("browseForFileOpen - selected file: " + fName.FullName);
            }
            return fName;
        }

        private void buttonBrowseFileForKey_Click(object sender, EventArgs e)
        {
            Log(Path.GetDirectoryName(Application.ExecutablePath));
            Log(String.Format(@"""{0}""", Path.GetDirectoryName(Application.ExecutablePath)));
            // Display a dialog box to select the encrypted file.
            FileInfo fName = browseForFileOpen(Path.GetDirectoryName(Application.ExecutablePath), "All files|*.*");
            if (fName.Exists)
            {
                KeyFile = fName;
                this.label1.Text = "Key file selected: " + fName.FullName;
                if (!String.IsNullOrEmpty(this.textBoxInput.Text))
                {
                    this.buttonValidateKey.Enabled = true;
                }
            }
        }

        private void buttonValidateKey_Click(object sender, EventArgs e)
        {
            Crypto MyCrypto = new Crypto();
            try
            {
                string DecryptedKey = MyCrypto.GenerateKeyString(this.textBoxInput.Text, KeyFile);
                if (!String.IsNullOrWhiteSpace(DecryptedKey))
                {
                    this.textBoxOutput.Text = DecryptedKey;
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                MessageBox.Show("The file supplied is not valid. Please retry");
            }
        }

        private void enableValidateKeyButton(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.textBoxInput.Text) && KeyFile.Exists)
            {
                this.buttonValidateKey.Enabled = true;
            }
        }
    }
}
