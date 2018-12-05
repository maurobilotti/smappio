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
            this._serialPort = new System.IO.Ports.SerialPort(this.components);
            this.lblNotification = new System.Windows.Forms.Label();
            this.lblSamplesReceived = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSampleRate = new System.Windows.Forms.Label();
            this.lblBitRate = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.volumeMeter = new NAudio.Gui.VolumeMeter();
            this.cbEncoding = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnAuscultate = new System.Windows.Forms.Button();
            this.toolTipAuscultar = new System.Windows.Forms.ToolTip(this.components);
            this.toolTipGuardar = new System.Windows.Forms.ToolTip(this.components);
            this.btnSave = new System.Windows.Forms.Button();
            this.toolTipStop = new System.Windows.Forms.ToolTip(this.components);
            this.btnClear = new System.Windows.Forms.Button();
            this.toolTipTest = new System.Windows.Forms.ToolTip(this.components);
            this.btnTest = new System.Windows.Forms.Button();
            this.toolTipLogs = new System.Windows.Forms.ToolTip(this.components);
            this.btnLogs = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.waveformPainter = new NAudio.Gui.WaveformPainter();
            this.btnSerial = new System.Windows.Forms.Button();
            this.btnBluetooth = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblNotification
            // 
            this.lblNotification.AutoSize = true;
            this.lblNotification.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNotification.Location = new System.Drawing.Point(112, 11);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(105, 18);
            this.lblNotification.TabIndex = 3;
            this.lblNotification.Text = "Not connected";
            // 
            // lblSamplesReceived
            // 
            this.lblSamplesReceived.AutoSize = true;
            this.lblSamplesReceived.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSamplesReceived.Location = new System.Drawing.Point(555, 11);
            this.lblSamplesReceived.Name = "lblSamplesReceived";
            this.lblSamplesReceived.Size = new System.Drawing.Size(16, 18);
            this.lblSamplesReceived.TabIndex = 6;
            this.lblSamplesReceived.Text = "_";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(414, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 18);
            this.label1.TabIndex = 8;
            this.label1.Text = "Samples Received:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(15, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 18);
            this.label2.TabIndex = 9;
            this.label2.Text = "Notification:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(15, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 18);
            this.label3.TabIndex = 10;
            this.label3.Text = "Time:";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.Location = new System.Drawing.Point(76, 45);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(16, 18);
            this.lblTime.TabIndex = 11;
            this.lblTime.Text = "_";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(414, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 18);
            this.label4.TabIndex = 12;
            this.label4.Text = "Sample rate:";
            // 
            // lblSampleRate
            // 
            this.lblSampleRate.AutoSize = true;
            this.lblSampleRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSampleRate.Location = new System.Drawing.Point(524, 45);
            this.lblSampleRate.Name = "lblSampleRate";
            this.lblSampleRate.Size = new System.Drawing.Size(16, 18);
            this.lblSampleRate.TabIndex = 13;
            this.lblSampleRate.Text = "_";
            // 
            // lblBitRate
            // 
            this.lblBitRate.AutoSize = true;
            this.lblBitRate.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBitRate.Location = new System.Drawing.Point(732, 11);
            this.lblBitRate.Name = "lblBitRate";
            this.lblBitRate.Size = new System.Drawing.Size(16, 18);
            this.lblBitRate.TabIndex = 15;
            this.lblBitRate.Text = "_";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Red;
            this.label6.Location = new System.Drawing.Point(656, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 18);
            this.label6.TabIndex = 14;
            this.label6.Text = "Bit rate:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(788, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(40, 18);
            this.label7.TabIndex = 16;
            this.label7.Text = "kbps";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(594, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 18);
            this.label5.TabIndex = 17;
            this.label5.Text = "Hz";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(112, 45);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 18);
            this.label8.TabIndex = 18;
            this.label8.Text = "ms";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(673, 39);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(217, 20);
            this.txtPath.TabIndex = 19;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gainsboro;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblNotification);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lblTime);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.lblSamplesReceived);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lblBitRate);
            this.panel1.Controls.Add(this.lblSampleRate);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Location = new System.Drawing.Point(41, 94);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(994, 82);
            this.panel1.TabIndex = 27;
            // 
            // volumeMeter
            // 
            this.volumeMeter.Amplitude = 0F;
            this.volumeMeter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.volumeMeter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.volumeMeter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.volumeMeter.Location = new System.Drawing.Point(13, 179);
            this.volumeMeter.MaxDb = 18F;
            this.volumeMeter.MinDb = -60F;
            this.volumeMeter.Name = "volumeMeter";
            this.volumeMeter.Size = new System.Drawing.Size(23, 414);
            this.volumeMeter.TabIndex = 28;
            this.volumeMeter.Text = "volumeMeter";
            // 
            // cbEncoding
            // 
            this.cbEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEncoding.FormattingEnabled = true;
            this.cbEncoding.Items.AddRange(new object[] {
            "PCM 24 bit",
            "PCM 32 bit Float",
            "PCM 16 bit"});
            this.cbEncoding.Location = new System.Drawing.Point(509, 39);
            this.cbEncoding.Name = "cbEncoding";
            this.cbEncoding.Size = new System.Drawing.Size(138, 21);
            this.cbEncoding.TabIndex = 30;
            this.cbEncoding.SelectedIndexChanged += new System.EventHandler(this.cbEncoding_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Black;
            this.panel2.Controls.Add(this.btnAuscultate);
            this.panel2.Location = new System.Drawing.Point(209, 8);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(81, 79);
            this.panel2.TabIndex = 31;
            // 
            // btnAuscultate
            // 
            this.btnAuscultate.BackgroundImage = global::Smappio_SEAR.Properties.Resources.auscultate;
            this.btnAuscultate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAuscultate.Location = new System.Drawing.Point(3, 2);
            this.btnAuscultate.Name = "btnAuscultate";
            this.btnAuscultate.Size = new System.Drawing.Size(75, 74);
            this.btnAuscultate.TabIndex = 22;
            this.btnAuscultate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTipAuscultar.SetToolTip(this.btnAuscultate, "Auscultar");
            this.btnAuscultate.UseVisualStyleBackColor = true;
            this.btnAuscultate.Click += new System.EventHandler(this.btnAuscultate_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImage = global::Smappio_SEAR.Properties.Resources.save;
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(912, 22);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(50, 50);
            this.btnSave.TabIndex = 0;
            this.toolTipGuardar.SetToolTip(this.btnSave, "Guardar");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackgroundImage = global::Smappio_SEAR.Properties.Resources.stop;
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(984, 22);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(50, 50);
            this.btnClear.TabIndex = 20;
            this.toolTipStop.SetToolTip(this.btnClear, "Finalizar");
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnTest
            // 
            this.btnTest.BackgroundImage = global::Smappio_SEAR.Properties.Resources.test;
            this.btnTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnTest.Location = new System.Drawing.Point(42, 9);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 75);
            this.btnTest.TabIndex = 23;
            this.btnTest.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTipTest.SetToolTip(this.btnTest, "Test Smappio");
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnLogs
            // 
            this.btnLogs.BackgroundImage = global::Smappio_SEAR.Properties.Resources.logs2;
            this.btnLogs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLogs.Location = new System.Drawing.Point(125, 9);
            this.btnLogs.Name = "btnLogs";
            this.btnLogs.Size = new System.Drawing.Size(75, 75);
            this.btnLogs.TabIndex = 32;
            this.btnLogs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTipLogs.SetToolTip(this.btnLogs, "Obtener Logs");
            this.btnLogs.UseVisualStyleBackColor = true;
            this.btnLogs.Click += new System.EventHandler(this.btnLogs_Click);
            // 
            // btnStop
            // 
            this.btnStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStop.BackgroundImage")));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStop.Location = new System.Drawing.Point(444, 24);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(48, 49);
            this.btnStop.TabIndex = 33;
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.BackgroundImage = global::Smappio_SEAR.Properties.Resources.play1;
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPlay.Location = new System.Drawing.Point(390, 24);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(48, 49);
            this.btnPlay.TabIndex = 26;
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackgroundImage = global::Smappio_SEAR.Properties.Resources.folder;
            this.btnBrowse.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBrowse.Location = new System.Drawing.Point(334, 24);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(50, 49);
            this.btnBrowse.TabIndex = 25;
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // waveformPainter
            // 
            this.waveformPainter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveformPainter.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.waveformPainter.BackgroundImage = global::Smappio_SEAR.Properties.Resources.eje;
            this.waveformPainter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.waveformPainter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.waveformPainter.Location = new System.Drawing.Point(41, 181);
            this.waveformPainter.Name = "waveformPainter";
            this.waveformPainter.Size = new System.Drawing.Size(994, 414);
            this.waveformPainter.TabIndex = 24;
            // 
            // btnSerial
            // 
            this.btnSerial.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSerial.Image = global::Smappio_SEAR.Properties.Resources.if_usb_925801;
            this.btnSerial.Location = new System.Drawing.Point(-2, 558);
            this.btnSerial.Name = "btnSerial";
            this.btnSerial.Size = new System.Drawing.Size(48, 48);
            this.btnSerial.TabIndex = 7;
            this.btnSerial.UseVisualStyleBackColor = true;
            this.btnSerial.Visible = false;
            this.btnSerial.Click += new System.EventHandler(this.btnUSB_Click);
            // 
            // btnBluetooth
            // 
            this.btnBluetooth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBluetooth.Image = global::Smappio_SEAR.Properties.Resources.Bluetooth;
            this.btnBluetooth.Location = new System.Drawing.Point(-2, 558);
            this.btnBluetooth.Name = "btnBluetooth";
            this.btnBluetooth.Size = new System.Drawing.Size(51, 48);
            this.btnBluetooth.TabIndex = 0;
            this.btnBluetooth.UseVisualStyleBackColor = true;
            this.btnBluetooth.Visible = false;
            this.btnBluetooth.Click += new System.EventHandler(this.btnBluetooth_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(1054, 605);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnLogs);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.cbEncoding);
            this.Controls.Add(this.volumeMeter);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.waveformPainter);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnSerial);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnBluetooth);
            this.Controls.Add(this.panel2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Smappio SEAR";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBluetooth;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblNotification;
        public System.IO.Ports.SerialPort _serialPort;
        private System.Windows.Forms.Label lblSamplesReceived;
        private System.Windows.Forms.Button btnSerial;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSampleRate;
        private System.Windows.Forms.Label lblBitRate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnAuscultate;
        private NAudio.Gui.WaveformPainter waveformPainter;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Panel panel1;
        private NAudio.Gui.VolumeMeter volumeMeter;
        private System.Windows.Forms.ComboBox cbEncoding;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnLogs;
        private System.Windows.Forms.ToolTip toolTipAuscultar;
        private System.Windows.Forms.ToolTip toolTipLogs;
        private System.Windows.Forms.ToolTip toolTipTest;
        private System.Windows.Forms.ToolTip toolTipStop;
        private System.Windows.Forms.ToolTip toolTipGuardar;
        private System.Windows.Forms.Button btnStop;
    }
}

