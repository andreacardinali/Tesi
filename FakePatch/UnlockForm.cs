using System;
using System.IO;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class UnlockForm : Form
    {
        static Crypto MyCrypto = new Crypto();
        static readonly FileInfo ExecutablePath = new FileInfo(Application.ExecutablePath);
        static readonly FileInfo EncryptedExecutablePath = MyCrypto.GetEncryptedFilePath(ExecutablePath);

        public UnlockForm()
        {
            InitializeComponent();

            this.textBoxRequestCode.Text = Convert.ToBase64String(MyCrypto.GetEncryptedKey(EncryptedExecutablePath).Item1);
        }
        private void textBoxAnswer_Enter(object sender, EventArgs e)
        {
            ActiveForm.AcceptButton = buttonSubmitKey; // Button1 will be 'clicked' when user presses return
        }

        private void textBoxAnswer_Leave(object sender, EventArgs e)
        {
            ActiveForm.AcceptButton = null; // remove "return" button behavior
        }

        private void buttonSubmitKey_Click(object sender, EventArgs e)
        {
            string SubmittedKey = this.textBoxAnswer.Text;
            Log("[buttonSubmitKey_Click] Submitted key: " + SubmittedKey);

            try
            {
                if (MyCrypto.ValidateKeyString(SubmittedKey, EncryptedExecutablePath))
                {
                    MessageBox.Show("Il codice fornito è valido! Avvio disinstallazione");
                    Log("[buttonSubmitKey_Click] Supplied code is valid. Starting patch uninstall");
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
                        string message = string.Format("[buttonSubmitKey_Click] Exception occurred: \n {0} \n {1} ", ex.Message, ex.ToString());
                        Log(message, LogLevel.Error);
                    }
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                MessageBox.Show("Il codice fornito non è valido, riprova.");
            }
        }

        private void enableSubmitKeyButton(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.textBoxAnswer.Text) && this.textBoxAnswer.Text.Length == 44)
            {
                this.buttonSubmitKey.Enabled = true;
            }
        }


    }
}
