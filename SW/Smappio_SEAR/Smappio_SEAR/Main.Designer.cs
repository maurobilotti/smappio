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
            this.btnWifi = new System.Windows.Forms.Button();
            this.btnBluetooth = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.lblElapsedTime = new System.Windows.Forms.Label();
            this.txtSerialData = new System.Windows.Forms.TextBox();
            this.btnFileDestination = new System.Windows.Forms.Button();
            this.lblSamplesReceived = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnWifi
            // 
            this.btnWifi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWifi.Image = global::Smappio_SEAR.Properties.Resources.if_wifi_Logo_925806;
            this.btnWifi.Location = new System.Drawing.Point(78, 12);
            this.btnWifi.Name = "btnWifi";
            this.btnWifi.Size = new System.Drawing.Size(48, 48);
            this.btnWifi.TabIndex = 1;
            this.btnWifi.UseVisualStyleBackColor = true;
            this.btnWifi.Click += new System.EventHandler(this.btnWifi_Click);
            // 
            // btnBluetooth
            // 
            this.btnBluetooth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBluetooth.Image = global::Smappio_SEAR.Properties.Resources.if_drop_kbtobexclient_17894;
            this.btnBluetooth.Location = new System.Drawing.Point(12, 12);
            this.btnBluetooth.Name = "btnBluetooth";
            this.btnBluetooth.Size = new System.Drawing.Size(51, 48);
            this.btnBluetooth.TabIndex = 0;
            this.btnBluetooth.UseVisualStyleBackColor = true;
            this.btnBluetooth.Click += new System.EventHandler(this.btnBluetooth_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(151, 36);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // lblElapsedTime
            // 
            this.lblElapsedTime.AutoSize = true;
            this.lblElapsedTime.Location = new System.Drawing.Point(13, 537);
            this.lblElapsedTime.Name = "lblElapsedTime";
            this.lblElapsedTime.Size = new System.Drawing.Size(35, 13);
            this.lblElapsedTime.TabIndex = 3;
            this.lblElapsedTime.Text = "label1";
            // 
            // txtSerialData
            // 
            this.txtSerialData.Location = new System.Drawing.Point(12, 80);
            this.txtSerialData.Multiline = true;
            this.txtSerialData.Name = "txtSerialData";
            this.txtSerialData.Size = new System.Drawing.Size(924, 454);
            this.txtSerialData.TabIndex = 4;
            // 
            // btnFileDestination
            // 
            this.btnFileDestination.Location = new System.Drawing.Point(269, 36);
            this.btnFileDestination.Name = "btnFileDestination";
            this.btnFileDestination.Size = new System.Drawing.Size(124, 23);
            this.btnFileDestination.TabIndex = 5;
            this.btnFileDestination.Text = "File Destination";
            this.btnFileDestination.UseVisualStyleBackColor = true;
            this.btnFileDestination.Click += new System.EventHandler(this.btnFileDestination_Click);
            // 
            // lblSamplesReceived
            // 
            this.lblSamplesReceived.AutoSize = true;
            this.lblSamplesReceived.Location = new System.Drawing.Point(666, 537);
            this.lblSamplesReceived.Name = "lblSamplesReceived";
            this.lblSamplesReceived.Size = new System.Drawing.Size(35, 13);
            this.lblSamplesReceived.TabIndex = 6;
            this.lblSamplesReceived.Text = "label1";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(997, 562);
            this.Controls.Add(this.lblSamplesReceived);
            this.Controls.Add(this.btnFileDestination);
            this.Controls.Add(this.txtSerialData);
            this.Controls.Add(this.lblElapsedTime);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnWifi);
            this.Controls.Add(this.btnBluetooth);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Smappio SEAR";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBluetooth;
        private System.Windows.Forms.Button btnWifi;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Label lblElapsedTime;
        private System.Windows.Forms.TextBox txtSerialData;
        public System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.Button btnFileDestination;
        private System.Windows.Forms.Label lblSamplesReceived;
    }
}

