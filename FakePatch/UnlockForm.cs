using System;
using System.IO;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class UnlockForm : Form
    {
        static FileInfo ExecutablePath = new FileInfo(Application.ExecutablePath);
        static FileInfo EncryptedExecutablePath = new FileInfo(Path.Combine(ExecutablePath.Directory.FullName, Path.ChangeExtension(ExecutablePath.Name, Path.GetExtension(ExecutablePath.Name) + ".enc")));
        public UnlockForm()
        {
            InitializeComponent();
            Crypto MyCrypto = new Crypto();

            this.textBox1.Text = Convert.ToBase64String((MyCrypto.GetEncryptedKey(EncryptedExecutablePath)).Item1);
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

        private void textBox2_Enter(object sender, EventArgs e)
        {
            ActiveForm.AcceptButton = buttonSubmitKey; // Button1 will be 'clicked' when user presses return
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            ActiveForm.AcceptButton = null; // remove "return" button behavior
        }

        private void buttonSubmitKey_Click(object sender, EventArgs e)
        {
            string SubmittedKey = this.textBox2.Text;
            Log(SubmittedKey);

            Crypto MyCrypto = new Crypto();
            try
            {
                if (MyCrypto.ValidateKeyString(SubmittedKey, EncryptedExecutablePath))
                {
                    MessageBox.Show("The key supplied is valid. Starting uninstall");
                    Log("Starting patch uninstall");
                    try
                    {
                        FileInfo KeyFile = new FileInfo(Path.Combine(gKeyWatchPath, "key.txt"));
                        if (!Directory.Exists(KeyFile.DirectoryName))
                        {
                            Directory.CreateDirectory(KeyFile.DirectoryName);
                        }
                        if (KeyFile.Exists) { KeyFile.Delete(); }
                        FileStream KeyFileWriter = new FileStream(KeyFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                        using (var sw = new StreamWriter(KeyFileWriter))
                        {
                            sw.Write(SubmittedKey);
                        }
                        System.Windows.Forms.Application.ExitThread();
                    }
                    catch (Exception ex)
                    {
                        string message = String.Format("Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                        Log(message, LogLevel.Error);
                    }
                }
                else
                {
                    throw new InvalidDataException();
                    //MessageBox.Show("The file supplied is not valid. Please retry");
                    //buttonBrowseKey_Click(null, null);
                }
            }
            catch
            {
                MessageBox.Show("The key supplied is not valid. Please retry");
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
    }
}
