using System;
using System.IO;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;
using FakePatch;

namespace KeyGenerator
{
    public partial class KeyGenForm : Form
    {
        static FileInfo ExecutablePath = new FileInfo(Application.ExecutablePath);
        static FileInfo EncryptedExecutablePath = new FileInfo(Path.Combine(ExecutablePath.Directory.FullName, Path.ChangeExtension(ExecutablePath.Name, Path.GetExtension(ExecutablePath.Name) + ".enc")));
        public KeyGenForm()
        {
            InitializeComponent();
            Crypto MyCrypto = new Crypto();

            //this.textBox1.Text = Convert.ToBase64String((MyCrypto.GetEncryptedKey(EncryptedExecutablePath)).Item1);
        }

        private void buttonBrowseKey_Click(object sender, EventArgs e)
        {
            Log(Path.GetDirectoryName(Application.ExecutablePath));
            Log(String.Format(@"""{0}""", Path.GetDirectoryName(Application.ExecutablePath)));
            // Display a dialog box to select the encrypted file.
            FileInfo fName = browseForFileOpen(Path.GetDirectoryName(Application.ExecutablePath), "All files|*.*");

            Crypto MyCrypto = new Crypto();
            try
            {
                if (MyCrypto.ValidateKeyFile(fName, EncryptedExecutablePath))
                {
                    MessageBox.Show("The file supplied is valid. Starting uninstall");
                    Log("Starting patch uninstall");
                    Install Install = new Install();
                    Install.CopyFileExactly(fName.FullName, Path.Combine(gKeyWatchPath, fName.Name));
                    System.Windows.Forms.Application.ExitThread();
                }
                else
                {
                    throw new Exception();
                    //MessageBox.Show("The file supplied is not valid. Please retry");
                    //buttonBrowseKey_Click(null, null);
                }
            }
            catch
            {
                MessageBox.Show("The file supplied is not valid. Please retry");
            }

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

            Crypto MyCrypto = new Crypto();
            try
            {
                string DecryptedKey = MyCrypto.GenerateKeyString(this.textBox1.Text, fName);
                if (!String.IsNullOrWhiteSpace(DecryptedKey))
                {
                    this.textBox3.Text = DecryptedKey;
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
    }
}
