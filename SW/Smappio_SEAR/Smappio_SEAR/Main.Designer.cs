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
            this.btnStop = new System.Windows.Forms.Button();
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.lblNotification = new System.Windows.Forms.Label();
            this.txtSerialData = new System.Windows.Forms.TextBox();
            this.btnFileDestination = new System.Windows.Forms.Button();
            this.lblSamplesReceived = new System.Windows.Forms.Label();
            this.btnUSB = new System.Windows.Forms.Button();
            this.btnWifi = new System.Windows.Forms.Button();
            this.btnBluetooth = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblBitRate = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(309, 36);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblNotification
            // 
            this.lblNotification.AutoSize = true;
            this.lblNotification.Location = new System.Drawing.Point(91, 503);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(13, 13);
            this.lblNotification.TabIndex = 3;
            this.lblNotification.Text = "_";
            // 
            // txtSerialData
            // 
            this.txtSerialData.Location = new System.Drawing.Point(12, 80);
            this.txtSerialData.Multiline = true;
            this.txtSerialData.Name = "txtSerialData";
            this.txtSerialData.Size = new System.Drawing.Size(924, 341);
            this.txtSerialData.TabIndex = 4;
            // 
            // btnFileDestination
            // 
            this.btnFileDestination.Location = new System.Drawing.Point(390, 36);
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
            this.lblSamplesReceived.Location = new System.Drawing.Point(774, 521);
            this.lblSamplesReceived.Name = "lblSamplesReceived";
            this.lblSamplesReceived.Size = new System.Drawing.Size(13, 13);
            this.lblSamplesReceived.TabIndex = 6;
            this.lblSamplesReceived.Text = "_";
            // 
            // btnUSB
            // 
            this.btnUSB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUSB.Image = global::Smappio_SEAR.Properties.Resources.if_usb_925801;
            this.btnUSB.Location = new System.Drawing.Point(141, 11);
            this.btnUSB.Name = "btnUSB";
            this.btnUSB.Size = new System.Drawing.Size(48, 48);
            this.btnUSB.TabIndex = 7;
            this.btnUSB.UseVisualStyleBackColor = true;
            this.btnUSB.Click += new System.EventHandler(this.btnUSB_Click);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(669, 521);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Samples Received:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 503);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Notification:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 521);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Time:";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(91, 521);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(13, 13);
            this.lblTime.TabIndex = 11;
            this.lblTime.Text = "_";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(669, 540);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Bit rate:";
            // 
            // lblBitRate
            // 
            this.lblBitRate.AutoSize = true;
            this.lblBitRate.Location = new System.Drawing.Point(774, 540);
            this.lblBitRate.Name = "lblBitRate";
            this.lblBitRate.Size = new System.Drawing.Size(13, 13);
            this.lblBitRate.TabIndex = 13;
            this.lblBitRate.Text = "_";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(997, 562);
            this.Controls.Add(this.lblBitRate);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUSB);
            this.Controls.Add(this.lblSamplesReceived);
            this.Controls.Add(this.btnFileDestination);
            this.Controls.Add(this.txtSerialData);
            this.Controls.Add(this.lblNotification);
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
        private System.Windows.Forms.Label lblNotification;
        private System.Windows.Forms.TextBox txtSerialData;
        public System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.Button btnFileDestination;
        private System.Windows.Forms.Label lblSamplesReceived;
        private System.Windows.Forms.Button btnUSB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblBitRate;
    }
}

