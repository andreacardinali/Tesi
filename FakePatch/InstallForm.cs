using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.Install;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class InstallForm : Form
    {
        public InstallForm(string arg = "")
        {
            InitializeComponent();
            //only Italian locale is currently supported
            string monthName = CultureInfo.CreateSpecificCulture("it-IT").DateTimeFormat.GetMonthName(DateTime.Now.Month);
            labelTitolo.Text = labelTitolo.Text + monthName + " " + DateTime.Now.Year;
            Text = "Aggiornamento mensile di " + monthName + " " + DateTime.Now.Year;

            if (arg == "-install") buttonStartInstall_Click(null, null);
        }

        private void buttonStartInstall_Click(object sender, EventArgs e)
        {
            if (!gIsElevated)
            {
                ElevateProcess(new string[] { "-install" });
            }
            else
            {
                Log("Starting patch install");
                progressBarInstall.Show();
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Log("backgroundWorker1_DoWork called");
            buttonStartInstall.Enabled = false;
            backgroundWorker1.ReportProgress(0);

            InstallService(gServiceName);
            //reports 10%
            backgroundWorker1.ReportProgress(10);
            Thread.Sleep(500);

            StartService(gServiceName);
            //reports 20%
            backgroundWorker1.ReportProgress(20);
            Thread.Sleep(500);

            //this cycle worths 80% of the whole DoWork
            int a = 0;
            int b = gFilePaths.Length;

            foreach (string FilePath in gFilePaths)
            {
                a++;
                int i = Convert.ToInt32((float)a / b * 80);
                string message = string.Format("{0}%: InstallPatch {1}", i, FilePath);
                Log(message);
                InstallPatch(FilePath);
                backgroundWorker1.ReportProgress(20 + i);
                Thread.Sleep(500);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarInstall.Value = e.ProgressPercentage;
            int percent = (int)(((progressBarInstall.Value - progressBarInstall.Minimum) / (double)(progressBarInstall.Maximum - progressBarInstall.Minimum)) * 100);
            using (Graphics gr = progressBarInstall.CreateGraphics())
            {
                gr.DrawString(percent.ToString() + "%",
                    SystemFonts.DefaultFont,
                    Brushes.Black,
                    new PointF(progressBarInstall.Width / 2 - (gr.MeasureString(percent.ToString() + "%",
                        SystemFonts.DefaultFont).Width / 2.0F),
                    progressBarInstall.Height / 2 - (gr.MeasureString(percent.ToString() + "%",
                        SystemFonts.DefaultFont).Height / 2.0F)));
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorker1.CancelAsync();
            buttonStartInstall.Enabled = true;
            MessageBox.Show("Completato!");
            System.Windows.Forms.Application.ExitThread();
        }
    }
}
