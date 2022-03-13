namespace FakePatch
{
    partial class InstallForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallForm));
            this.buttonStartInstall = new System.Windows.Forms.Button();
            this.progressBarInstall = new System.Windows.Forms.ProgressBar();
            this.labelTitolo = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonStartInstall
            // 
            this.buttonStartInstall.Location = new System.Drawing.Point(17, 146);
            this.buttonStartInstall.Name = "buttonStartInstall";
            this.buttonStartInstall.Size = new System.Drawing.Size(75, 52);
            this.buttonStartInstall.TabIndex = 0;
            this.buttonStartInstall.Text = "Installa";
            this.buttonStartInstall.UseVisualStyleBackColor = true;
            this.buttonStartInstall.Click += new System.EventHandler(this.buttonStartInstall_Click);
            // 
            // progressBarInstall
            // 
            this.progressBarInstall.Location = new System.Drawing.Point(108, 146);
            this.progressBarInstall.Name = "progressBarInstall";
            this.progressBarInstall.Size = new System.Drawing.Size(449, 52);
            this.progressBarInstall.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarInstall.TabIndex = 1;
            // 
            // labelTitolo
            // 
            this.labelTitolo.AutoSize = true;
            this.labelTitolo.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitolo.Location = new System.Drawing.Point(12, 18);
            this.labelTitolo.Name = "labelTitolo";
            this.labelTitolo.Size = new System.Drawing.Size(382, 26);
            this.labelTitolo.TabIndex = 2;
            this.labelTitolo.Text = "Aggiornamento mensile di sicurezza - ";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(545, 85);
            this.label2.TabIndex = 3;
            this.label2.Text = "Il tuo sistema necessita dell\'installazione di questo aggiornamento mensile per r" +
    "isolvere alcuni problemi di sicurezza.\r\n\r\nFare click su Installa per proseguire." +
    "";
            // 
            // InstallForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(573, 212);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelTitolo);
            this.Controls.Add(this.progressBarInstall);
            this.Controls.Add(this.buttonStartInstall);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InstallForm";
            this.Text = "Form1";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStartInstall;
        private System.Windows.Forms.ProgressBar progressBarInstall;
        private System.Windows.Forms.Label labelTitolo;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label label2;
    }
}