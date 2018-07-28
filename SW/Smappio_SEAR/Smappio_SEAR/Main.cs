using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using WebSocketSharp;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Threading;

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

        #region Members
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        string deviceName = "smappio";
        private string filePath = "../../AudioSamples/";
        private string host = "http://192.168.1.1:80";
        private WebSocket client;
        private List<Int32> _fileInts = new List<int>();
        List<byte> _bytes = new List<byte>();
        Stopwatch sw = new Stopwatch();
        long elapsedMilliseconds = 0;
        Socket _socket;

        #endregion
        #region SoundParameters

        //static int _sampleRate = 16000;   // No se esta usando por hacer los calculos en base al tiempo
        static int _seconds = 55;
        static int _bytesDepth = 3;
        static int _bitDepth = _bytesDepth * 8;
        private bool _notified;
        private float _baudRate = 2000000; //(_sampleRate * _bitDepth) * 1.2f;

        #endregion

        #region Transfering methods



        private void btnWifi_Click(object sender, EventArgs e)
        {
            try
            {
                WebSocket client = new WebSocket(host);
                client.Connect();

                SetNotificationLabel("Started");

                client.OnOpen += (ss, ee) =>
                    SetTextBox($"Connected to {host} successfully");

                client.OnError += (ss, ee) =>
                   SetTextBox($"Error: {ee.Message}");

                client.OnMessage += (ss, ee) =>
                {
                    if (sw.ElapsedMilliseconds < (_seconds * 1000)) //_bytes.Count <= (_sampleRate * _bytesDepth * _seconds)
                    {
                        if (!sw.IsRunning)
                            sw.Start();

                        _bytes.AddRange(ee.RawData);
                    }
                    else if (!_notified)
                    {
                        //lblNotification.Text = "Finish";
                        sw.Stop();
                        elapsedMilliseconds = sw.ElapsedMilliseconds;
                        SetNotificationLabel("Finished");
                        _notified = true;
                    }
                };

                client.OnClose += (ss, ee) =>
                   SetTextBox($"Disconnected with {0}");
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private void btnUSB_Click(object sender, EventArgs e)
        {
            EnableFeatures();
            //Silicon Labs CP210x USB to UART Bridge
            try
            {
                serialPort.PortName = "COM4";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
                serialPort.BaudRate = Convert.ToInt32(_baudRate);
                serialPort.Handshake = Handshake.None;

                //ojo con estos flags! sin esto, no recibe!!
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                if (!serialPort.IsOpen)
                    serialPort.Open();

                lblNotification.Text = "Started";
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception ex)
            {
                serialPort.Dispose();
                return;
            }
        }

        private void btnBluetooth_Click(object sender, EventArgs e)
        {
            EnableFeatures();
            try
            {
                serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);
                serialPort.BaudRate = Convert.ToInt32(_baudRate);
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;

                if (!serialPort.IsOpen)
                    serialPort.Open();


                lblNotification.Text = "Started";
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception ex)
            {
                serialPort.Dispose();
                return;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sw.ElapsedMilliseconds < (_seconds * 1000)) //_bytes.Count <= (_sampleRate * _bytesDepth * _seconds)
            {
                var bufferSize = serialPort.BytesToRead;

                byte[] data = new byte[bufferSize];
                serialPort.Read(data, 0, bufferSize);

                if (!sw.IsRunning)
                    sw.Start();

                this._bytes.AddRange(data);
            }
            else if (!_notified)
            {
                //lblNotification.Text = "Finish";
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
                SetNotificationLabel("Finished");
                _notified = true;
            }
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
            _thread.Abort();
            _socket.Disconnect(false);
            _socket.Close();
            if (!_notified)
            {
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
                SetNotificationLabel("Finished");
                _notified = true;
            }


            long sampleRate = _bytes.Count / _bytesDepth / (elapsedMilliseconds / 1000);
            lblSampleRate.Text = sampleRate.ToString();

            long bitRate = (sampleRate * _bytesDepth * 8) / 1000;

            lblTime.Text = elapsedMilliseconds.ToString();
            lblSamplesReceived.Text = (_bytes.Count / _bytesDepth).ToString();
            lblBitRate.Text = bitRate.ToString();

            //little endian!   
            string absolutePath = Path.GetFullPath(filePath);
            string fileName = $"{DateTime.Now.ToString("ddhhmmss")}.wav";
            string fullPath = Path.Combine(absolutePath, fileName);
            File.WriteAllBytes(fullPath, _bytes.ToArray());

            // LOGIC FOR PRINTING THE VALUES IN THE TEXTBOX.

            if (serialPort.IsOpen)
            {
                serialPort.DiscardInBuffer();
                serialPort.Close();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearContents();
        }

        #endregion

        #region private methods
        private void EnableFeatures()
        {
            btnSave.Enabled = true;
        }

        private void ClearContents()
        {
            lblBitRate.Text = lblBitRate.Text = lblSampleRate.Text = lblSamplesReceived.Text = lblTime.Text = "";
            sw = new Stopwatch();

            this._bytes.Clear();

            if (serialPort.IsOpen)
                serialPort.Close();
        }
        #endregion

        private void btnStop_Click(object sender, EventArgs e)
        {
            client.Close();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void btnWifiHTTP_Click(object sender, EventArgs e)
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect("192.168.1.1", 80);
                SetNotificationLabel("Started");
                _thread = new Thread(this.ReceiveData);
                if (!sw.IsRunning)
                    sw.Start();
                _thread.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private byte[] _buffer = new byte[3];
        private Thread _thread;

        private void ReceiveData()
        {            
            while (_socket.Connected)
            {
                _socket.Receive(_buffer, 0, 3, 0);
                _bytes.AddRange(_buffer.ToList());
            }
        }
    }
}
