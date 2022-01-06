
namespace BackTrackFileViewer
{
    partial class GetDataForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.archDirTextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.aesKeyTextbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.aesIvTextbox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Archive Directory:";
            // 
            // archDirTextbox
            // 
            this.archDirTextbox.Location = new System.Drawing.Point(15, 25);
            this.archDirTextbox.Name = "archDirTextbox";
            this.archDirTextbox.Size = new System.Drawing.Size(369, 20);
            this.archDirTextbox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "AES Key:";
            // 
            // aesKeyTextbox
            // 
            this.aesKeyTextbox.Location = new System.Drawing.Point(15, 64);
            this.aesKeyTextbox.Name = "aesKeyTextbox";
            this.aesKeyTextbox.Size = new System.Drawing.Size(369, 20);
            this.aesKeyTextbox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "AES IV:";
            // 
            // aesIvTextbox
            // 
            this.aesIvTextbox.Location = new System.Drawing.Point(15, 103);
            this.aesIvTextbox.Name = "aesIvTextbox";
            this.aesIvTextbox.Size = new System.Drawing.Size(369, 20);
            this.aesIvTextbox.TabIndex = 5;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(309, 137);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // GetDataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 172);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.aesIvTextbox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.aesKeyTextbox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.archDirTextbox);
            this.Controls.Add(this.label1);
            this.Name = "GetDataForm";
            this.Text = "GetDataForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox archDirTextbox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox aesKeyTextbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox aesIvTextbox;
        private System.Windows.Forms.Button okButton;
    }
}