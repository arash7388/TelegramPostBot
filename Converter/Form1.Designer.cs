namespace Converter
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnCambridge = new System.Windows.Forms.Button();
            this.btnPhotoWithText = new System.Windows.Forms.Button();
            this.btnEslFast = new System.Windows.Forms.Button();
            this.btnListenAMin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(122, 43);
            this.button1.TabIndex = 0;
            this.button1.Text = "Convert";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnCambridge
            // 
            this.btnCambridge.Location = new System.Drawing.Point(252, 46);
            this.btnCambridge.Name = "btnCambridge";
            this.btnCambridge.Size = new System.Drawing.Size(122, 43);
            this.btnCambridge.TabIndex = 0;
            this.btnCambridge.Text = "Cambridge Vocab";
            this.btnCambridge.UseVisualStyleBackColor = true;
            this.btnCambridge.Click += new System.EventHandler(this.btnCambridge_Click);
            // 
            // btnPhotoWithText
            // 
            this.btnPhotoWithText.Location = new System.Drawing.Point(252, 95);
            this.btnPhotoWithText.Name = "btnPhotoWithText";
            this.btnPhotoWithText.Size = new System.Drawing.Size(122, 43);
            this.btnPhotoWithText.TabIndex = 0;
            this.btnPhotoWithText.Text = "PhotoWithText";
            this.btnPhotoWithText.UseVisualStyleBackColor = true;
            this.btnPhotoWithText.Click += new System.EventHandler(this.btnPhotoWithText_Click);
            // 
            // btnEslFast
            // 
            this.btnEslFast.Location = new System.Drawing.Point(252, 144);
            this.btnEslFast.Name = "btnEslFast";
            this.btnEslFast.Size = new System.Drawing.Size(122, 43);
            this.btnEslFast.TabIndex = 0;
            this.btnEslFast.Text = "ESLFast";
            this.btnEslFast.UseVisualStyleBackColor = true;
            this.btnEslFast.Click += new System.EventHandler(this.btnEslFast_Click);
            // 
            // btnListenAMin
            // 
            this.btnListenAMin.Location = new System.Drawing.Point(252, 193);
            this.btnListenAMin.Name = "btnListenAMin";
            this.btnListenAMin.Size = new System.Drawing.Size(122, 43);
            this.btnListenAMin.TabIndex = 0;
            this.btnListenAMin.Text = "ListenAMin";
            this.btnListenAMin.UseVisualStyleBackColor = true;
            this.btnListenAMin.Click += new System.EventHandler(this.btnListenAMin_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 298);
            this.Controls.Add(this.btnListenAMin);
            this.Controls.Add(this.btnEslFast);
            this.Controls.Add(this.btnPhotoWithText);
            this.Controls.Add(this.btnCambridge);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "EngCafe";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnCambridge;
        private System.Windows.Forms.Button btnPhotoWithText;
        private System.Windows.Forms.Button btnEslFast;
        private System.Windows.Forms.Button btnListenAMin;
    }
}

