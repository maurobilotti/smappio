namespace Smappio_SEAR
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.button2 = new System.Windows.Forms.Button();
            this.btnBluetooth = new System.Windows.Forms.Button();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.lblMessages = new System.Windows.Forms.Label();
            this.txtSerialData = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Image = global::Smappio_SEAR.Properties.Resources.if_wifi_Logo_925806;
            this.button2.Location = new System.Drawing.Point(78, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(48, 48);
            this.button2.TabIndex = 1;
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnBluetooth
            // 
            this.btnBluetooth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBluetooth.Image = global::Smappio_SEAR.Properties.Resources.if_drop_kbtobexclient_17894;
            this.btnBluetooth.Location = new System.Drawing.Point(12, 12);
            this.btnBluetooth.Name = "btnBluetooth";
            this.btnBluetooth.Size = new System.Drawing.Size(48, 48);
            this.btnBluetooth.TabIndex = 0;
            this.btnBluetooth.UseVisualStyleBackColor = true;
            this.btnBluetooth.Click += new System.EventHandler(this.btnBluetooth_Click);
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(151, 36);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(75, 23);
            this.btnOpenFile.TabIndex = 2;
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            // 
            // lblMessages
            // 
            this.lblMessages.AutoSize = true;
            this.lblMessages.Location = new System.Drawing.Point(13, 537);
            this.lblMessages.Name = "lblMessages";
            this.lblMessages.Size = new System.Drawing.Size(35, 13);
            this.lblMessages.TabIndex = 3;
            this.lblMessages.Text = "label1";
            // 
            // txtSerialData
            // 
            this.txtSerialData.Location = new System.Drawing.Point(12, 95);
            this.txtSerialData.Multiline = true;
            this.txtSerialData.Name = "txtSerialData";
            this.txtSerialData.Size = new System.Drawing.Size(924, 405);
            this.txtSerialData.TabIndex = 4;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(997, 562);
            this.Controls.Add(this.txtSerialData);
            this.Controls.Add(this.lblMessages);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnBluetooth);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Smappio SEAR";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBluetooth;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.TextBox txtSerialData;
        public System.IO.Ports.SerialPort serialPort;
    }
}

