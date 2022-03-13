namespace FakePatch
{
    partial class UnlockForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnlockForm));
            this.labelTitolo = new System.Windows.Forms.Label();
            this.textBoxRequestCode = new System.Windows.Forms.TextBox();
            this.textBoxAnswer = new System.Windows.Forms.TextBox();
            this.buttonSubmitKey = new System.Windows.Forms.Button();
            this.labelIstruzioni = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelTitolo
            // 
            this.labelTitolo.AutoSize = true;
            this.labelTitolo.Font = new System.Drawing.Font("Kristen ITC", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitolo.Location = new System.Drawing.Point(23, 9);
            this.labelTitolo.MaximumSize = new System.Drawing.Size(450, 0);
            this.labelTitolo.Name = "labelTitolo";
            this.labelTitolo.Size = new System.Drawing.Size(426, 29);
            this.labelTitolo.TabIndex = 1;
            this.labelTitolo.Text = "Questo programma è stato bloccato.";
            // 
            // textBoxRequestCode
            // 
            this.textBoxRequestCode.Location = new System.Drawing.Point(26, 181);
            this.textBoxRequestCode.Multiline = true;
            this.textBoxRequestCode.Name = "textBoxRequestCode";
            this.textBoxRequestCode.ReadOnly = true;
            this.textBoxRequestCode.Size = new System.Drawing.Size(580, 90);
            this.textBoxRequestCode.TabIndex = 2;
            // 
            // textBoxAnswer
            // 
            this.textBoxAnswer.Location = new System.Drawing.Point(26, 316);
            this.textBoxAnswer.Multiline = true;
            this.textBoxAnswer.Name = "textBoxAnswer";
            this.textBoxAnswer.Size = new System.Drawing.Size(454, 61);
            this.textBoxAnswer.TabIndex = 3;
            this.textBoxAnswer.TextChanged += new System.EventHandler(this.enableSubmitKeyButton);
            this.textBoxAnswer.Enter += new System.EventHandler(this.textBox2_Enter);
            this.textBoxAnswer.Leave += new System.EventHandler(this.textBox2_Leave);
            // 
            // buttonSubmitKey
            // 
            this.buttonSubmitKey.Enabled = false;
            this.buttonSubmitKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSubmitKey.Image = ((System.Drawing.Image)(resources.GetObject("buttonSubmitKey.Image")));
            this.buttonSubmitKey.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.buttonSubmitKey.Location = new System.Drawing.Point(486, 316);
            this.buttonSubmitKey.Name = "buttonSubmitKey";
            this.buttonSubmitKey.Size = new System.Drawing.Size(120, 61);
            this.buttonSubmitKey.TabIndex = 4;
            this.buttonSubmitKey.Text = "SBLOCCA";
            this.buttonSubmitKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSubmitKey.UseVisualStyleBackColor = true;
            this.buttonSubmitKey.Click += new System.EventHandler(this.buttonSubmitKey_Click);
            // 
            // labelIstruzioni
            // 
            this.labelIstruzioni.AutoSize = true;
            this.labelIstruzioni.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelIstruzioni.Location = new System.Drawing.Point(25, 50);
            this.labelIstruzioni.Name = "labelIstruzioni";
            this.labelIstruzioni.Size = new System.Drawing.Size(550, 65);
            this.labelIstruzioni.TabIndex = 5;
            this.labelIstruzioni.Text = resources.GetString("labelIstruzioni.Text");
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(25, 165);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Codice richiesta";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(25, 300);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(168, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Inserisci il codice di sblocco";
            // 
            // UnlockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 392);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelIstruzioni);
            this.Controls.Add(this.buttonSubmitKey);
            this.Controls.Add(this.textBoxAnswer);
            this.Controls.Add(this.textBoxRequestCode);
            this.Controls.Add(this.labelTitolo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnlockForm";
            this.Text = "Programma bloccato";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelTitolo;
        private System.Windows.Forms.TextBox textBoxRequestCode;
        private System.Windows.Forms.TextBox textBoxAnswer;
        private System.Windows.Forms.Button buttonSubmitKey;
        private System.Windows.Forms.Label labelIstruzioni;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}