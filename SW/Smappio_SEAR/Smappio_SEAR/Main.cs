﻿using Smappio_SEAR.Serial;
using Smappio_SEAR.Wifi;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Smappio_SEAR
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            txtPath.Text = filePath;
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
        } 

        private string filePath = "../../AudioSamples/";        
        Stopwatch sw = new Stopwatch();
        long elapsedMilliseconds = 0;
        string deviceName = "smappio";
        private bool _notified;
        
        public Receiver Receiver;        

        #region Transfering methods

        private void btnUSB_Click(object sender, EventArgs e)
        {
            Receiver = new SerialReceiver(_serialPort);

            if(!Receiver.Connected)
            {
                SetNotificationLabel("Can't connect serial.");
            }
            ((SerialReceiver)Receiver).Notify();
            Receiver.Receive();
            SetNotificationLabel("Serial started");            

            if (!sw.IsRunning)
                sw.Start();

            Receiver.Play();
        }

        private void btnBluetooth_Click(object sender, EventArgs e)
        {
            //EnableFeatures();
            //try
            //{
            //    _serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);
            //    _serialPort.BaudRate = Convert.ToInt32(_baudRate);
            //    _serialPort.DtrEnable = true;
            //    _serialPort.RtsEnable = true;

            //    if (!_serialPort.IsOpen)
            //        _serialPort.Open();


            //    lblNotification.Text = "Started";
            //    _serialPort.DataReceived += SerialPort_DataReceived;
            //}
            //catch (Exception ex)
            //{
            //    _serialPort.Dispose();
            //    return;
            //}
        }        

        private void btnUdp_Click(object sender, EventArgs e)
        {
            InvokeWifiReceiver(new UdpReceiver());
        }

        private void btnTcp_Click(object sender, EventArgs e)
        {
            InvokeWifiReceiver(new TcpReceiver());
        }

        public void InvokeWifiReceiver(Receiver receiver)
        {
            Receiver = receiver;

            if (!Receiver.Connected)
            {
                SetNotificationLabel("Not Available");
                return;
            }

            Receiver.Receive();
            Receiver.Play();

            SetButtonStatus();
            SetNotificationLabel("Threads Running");
        }


        #endregion

        #region TextBox_Methods
        delegate void SetTextCallback(string text);
        private void SetTextBox(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.

            if (this.txtSerialData.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetTextBox);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                txtSerialData.AppendText(text + " ");
            }
        }

        delegate void SetSetNotificationLabelCallback(string text);
        private void SetNotificationLabel(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.

            if (this.txtSerialData.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetNotificationLabel);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                lblNotification.Text = text;
            }
        }

        #endregion

        #region Save file methods
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!_notified)
            {
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
                SetNotificationLabel("Finished");
                _notified = true;
            }

            if (elapsedMilliseconds == 0)
                elapsedMilliseconds = 1000; // Para que no estalle

            long sampleRate = Receiver.GetReceivedBytes() / Receiver.GetBytesDepth() / (elapsedMilliseconds / 1000);
            lblSampleRate.Text = sampleRate.ToString();

            long bitRate = (sampleRate * Receiver.GetBytesDepth() * 8) / 1000;

            lblTime.Text = elapsedMilliseconds.ToString();
            lblSamplesReceived.Text = (Receiver.GetReceivedBytes() / Receiver.GetBytesDepth()).ToString();
            lblBitRate.Text = bitRate.ToString();

            Receiver.SaveFile();
            Receiver.Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearContents();
        }

        #endregion

        #region private methods
       
        private void ClearContents()
        {
            SetButtonStatus(true);
            lblBitRate.Text = lblBitRate.Text = lblSampleRate.Text = lblSamplesReceived.Text = lblTime.Text = "";
            sw = new Stopwatch();

            Receiver.ClearAndClose();

        }

        private void SetButtonStatus(bool status = false)
        {
            btnBluetooth.Enabled = btnTcp.Enabled = btnUdp.Enabled = btnSerial.Enabled = status;
        }
        #endregion       

        private void Main_Load(object sender, EventArgs e)
        {

        }
    }
}
