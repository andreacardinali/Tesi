﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using static FakePatch.Globals;
using static FakePatch.LogHelper;

namespace FakePatch
{
    public partial class InstallForm : Form
    {
        public InstallForm(string arg = "")
        {
            InitializeComponent();
            if (arg == "-install") buttonStartInstall_Click(null, null);
        }

        private void buttonStartInstall_Click(object sender, EventArgs e)
        {
            Install Install = new Install();

            if (!gIsElevated)
            {
                Install.ElevateProcess(new string[] { "-install" });
            }
            else
            {
                Log("Starting patch install");
                progressBarInstall.Show();
                backgroundWorker1.RunWorkerAsync();

                //buttonCompleted.Hide();
                //ShowDialog();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Log("backgroundWorker1_DoWork called");
            buttonStartInstall.Enabled = false;
            backgroundWorker1.ReportProgress(0);
            Install Install = new Install();

            Install.InstallService(gServiceName);
            //reports 10%
            backgroundWorker1.ReportProgress(10);
            Thread.Sleep(500);

            Install.StartService(gServiceName);
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
                Install.InstallPatch(FilePath);
                backgroundWorker1.ReportProgress(20 + i);
                Thread.Sleep(500);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarInstall.Value = e.ProgressPercentage;
            progressBarInstall.Text = e.ProgressPercentage.ToString();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //progressBarInstall.Hide();
            //iw.buttonCompleted.Show();
            //iw.buttonCompleted.Text = "Completato";
            backgroundWorker1.CancelAsync();
            buttonStartInstall.Enabled = true;
            MessageBox.Show("Completato!");
            System.Windows.Forms.Application.ExitThread();
        }
    }
}
