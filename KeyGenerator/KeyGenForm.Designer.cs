namespace KeyGenerator
{
    partial class KeyGenForm
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
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.buttonBrowseFileForKey = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.buttonValidateKey = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonBrowseFileForKey
            // 
            this.buttonBrowseFileForKey.Location = new System.Drawing.Point(461, 70);
            this.buttonBrowseFileForKey.Name = "buttonBrowseFileForKey";
            this.buttonBrowseFileForKey.Size = new System.Drawing.Size(327, 87);
            this.buttonBrowseFileForKey.TabIndex = 5;
            this.buttonBrowseFileForKey.Text = "Browse for key file...";
            this.buttonBrowseFileForKey.UseVisualStyleBackColor = true;
            this.buttonBrowseFileForKey.Click += new System.EventHandler(this.buttonBrowseFileForKey_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(511, 227);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.Size = new System.Drawing.Size(277, 68);
            this.textBoxOutput.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Key file not loaded";
            // 
            // textBoxInput
            // 
            this.textBoxInput.Location = new System.Drawing.Point(29, 227);
            this.textBoxInput.Multiline = true;
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(375, 68);
            this.textBoxInput.TabIndex = 2;
            this.textBoxInput.TextChanged += new System.EventHandler(this.enableValidateKeyButton);
            // 
            // buttonValidateKey
            // 
            this.buttonValidateKey.Enabled = false;
            this.buttonValidateKey.Location = new System.Drawing.Point(410, 272);
            this.buttonValidateKey.Name = "buttonValidateKey";
            this.buttonValidateKey.Size = new System.Drawing.Size(75, 23);
            this.buttonValidateKey.TabIndex = 7;
            this.buttonValidateKey.Text = "Validate Key";
            this.buttonValidateKey.UseVisualStyleBackColor = true;
            this.buttonValidateKey.Click += new System.EventHandler(this.buttonValidateKey_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 211);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Input";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(508, 211);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Output";
            // 
            // KeyGenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonValidateKey);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.buttonBrowseFileForKey);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyGenForm";
            this.Text = "Key Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog _openFileDialog;
        private System.Windows.Forms.Button buttonBrowseFileForKey;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Button buttonValidateKey;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}