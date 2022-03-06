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
            this.buttonBrowseKey = new System.Windows.Forms.Button();
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.buttonSubmitKey = new System.Windows.Forms.Button();
            this.buttonBrowseFileForKey = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonBrowseKey
            // 
            this.buttonBrowseKey.Location = new System.Drawing.Point(26, 70);
            this.buttonBrowseKey.Name = "buttonBrowseKey";
            this.buttonBrowseKey.Size = new System.Drawing.Size(257, 87);
            this.buttonBrowseKey.TabIndex = 0;
            this.buttonBrowseKey.Text = "Browse...";
            this.buttonBrowseKey.UseVisualStyleBackColor = true;
            this.buttonBrowseKey.Click += new System.EventHandler(this.buttonBrowseKey_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(98, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(26, 181);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(363, 68);
            this.textBox1.TabIndex = 2;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(26, 276);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(363, 61);
            this.textBox2.TabIndex = 3;
            this.textBox2.Enter += new System.EventHandler(this.textBox2_Enter);
            this.textBox2.Leave += new System.EventHandler(this.textBox2_Leave);
            // 
            // buttonSubmitKey
            // 
            this.buttonSubmitKey.Location = new System.Drawing.Point(395, 314);
            this.buttonSubmitKey.Name = "buttonSubmitKey";
            this.buttonSubmitKey.Size = new System.Drawing.Size(75, 23);
            this.buttonSubmitKey.TabIndex = 4;
            this.buttonSubmitKey.Text = "Submit Key";
            this.buttonSubmitKey.UseVisualStyleBackColor = true;
            this.buttonSubmitKey.Click += new System.EventHandler(this.buttonSubmitKey_Click);
            // 
            // buttonBrowseFileForKey
            // 
            this.buttonBrowseFileForKey.Location = new System.Drawing.Point(461, 70);
            this.buttonBrowseFileForKey.Name = "buttonBrowseFileForKey";
            this.buttonBrowseFileForKey.Size = new System.Drawing.Size(327, 87);
            this.buttonBrowseFileForKey.TabIndex = 5;
            this.buttonBrowseFileForKey.Text = "button1";
            this.buttonBrowseFileForKey.UseVisualStyleBackColor = true;
            this.buttonBrowseFileForKey.Click += new System.EventHandler(this.buttonBrowseFileForKey_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(461, 181);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(327, 68);
            this.textBox3.TabIndex = 6;
            // 
            // UnlockForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.buttonBrowseFileForKey);
            this.Controls.Add(this.buttonSubmitKey);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonBrowseKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnlockForm";
            this.Text = "UnlockForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowseKey;
        private System.Windows.Forms.OpenFileDialog _openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button buttonSubmitKey;
        private System.Windows.Forms.Button buttonBrowseFileForKey;
        private System.Windows.Forms.TextBox textBox3;
    }
}