using System;
using System.IO;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class UnlockForm : Form
    {
        static readonly FileInfo ExecutablePath = new FileInfo(Application.ExecutablePath);
        static readonly FileInfo EncryptedExecutablePath = new FileInfo(Path.Combine(ExecutablePath.Directory.FullName, Path.ChangeExtension(ExecutablePath.Name, Path.GetExtension(ExecutablePath.Name) + ".enc")));
        public UnlockForm()
        {
            InitializeComponent();
            Crypto MyCrypto = new Crypto();

            this.textBoxRequestCode.Text = Convert.ToBase64String((MyCrypto.GetEncryptedKey(EncryptedExecutablePath)).Item1);
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
            string SubmittedKey = this.textBoxAnswer.Text;
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

        private void enableSubmitKeyButton(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.textBoxAnswer.Text) && this.textBoxAnswer.Text.Length == 44)
            {
                this.buttonSubmitKey.Enabled = true;
            }
        }


    }
}
