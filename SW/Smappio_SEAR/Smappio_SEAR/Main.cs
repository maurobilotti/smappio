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
        List<byte> _receivedBytes = new List<byte>();
        Stopwatch sw = new Stopwatch();
        long elapsedMilliseconds = 0;
        Socket _socket;
        private TransmissionMethod _transmissionMethod = TransmissionMethod.Wifi;
        #endregion
        #region SoundParameters

        //static int _sampleRate = 16000;   // No se esta usando por hacer los calculos en base al tiempo
        static int _seconds = 10;
        static int _bytesDepth = 3;
        static int _sampleRate = 64000;
        static int _bitDepth = _bytesDepth * 8;
        private bool _notified;
        private float _baudRate = 2000000; //(_sampleRate * _bitDepth) * 1.2f;

        #endregion

        #region Transfering methods

        private void btnUSB_Click(object sender, EventArgs e)
        {
            EnableFeatures();
            this._transmissionMethod = TransmissionMethod.Serial;
            //Silicon Labs CP210x USB to UART Bridge
            try
            {
                serialPort.PortName = "COM7";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
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

                this._receivedBytes.AddRange(data);
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
            if (this._transmissionMethod == TransmissionMethod.Wifi)
            {
                //_thread.Abort();
                //_socket.Disconnect(false);
                //_socket.Close();
                if (!_notified)
                {
                    sw.Stop();
                    elapsedMilliseconds = sw.ElapsedMilliseconds;
                    SetNotificationLabel("Finished");
                    _notified = true;
                }
            }


            long sampleRate = _receivedBytes.Count / _bytesDepth / (elapsedMilliseconds / 1000);
            lblSampleRate.Text = sampleRate.ToString();

            long bitRate = (sampleRate * _bytesDepth * 8) / 1000;

            lblTime.Text = elapsedMilliseconds.ToString();
            lblSamplesReceived.Text = (_receivedBytes.Count / _bytesDepth).ToString();
            lblBitRate.Text = bitRate.ToString();

            //little endian!   
            string absolutePath = Path.GetFullPath(filePath);
            string fileName = $"{DateTime.Now.ToString("yyyy-MM-dd_hhmmss")}.pcm";
            string fullPath = Path.Combine(absolutePath, fileName);
            //string text = _bytes.ByteListToString();
            File.WriteAllBytes(fullPath, _receivedBytes.ToArray());

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

            this._receivedBytes.Clear();

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

        #region Wifi Logic
        private Thread _threadReceive;
        private Thread _threadCopy;
        private Thread _threadPlay;
        private TcpClient _tcp;
        bool firstData = true;
        int offset = 0;
        private WaveOut _waveOut = new WaveOut();
        private List<byte> _bufferedBytes = new List<byte>();
        private Mutex _mutex = new Mutex();
        private int _playingLength = 64000;
        private Semaphore _playingSemaphore = new Semaphore(0, 5000);
        private Semaphore _bufferingSemaphore = new Semaphore(0, 5000);

        private void btnWifiHTTP_Click(object sender, EventArgs e)
        {
            try
            {
                _tcp = new TcpClient("192.168.1.1", 80);

                //_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //_socket.Connect("192.168.1.1", 80);
                SetNotificationLabel("Started");
                _threadReceive = new Thread(this.ReceiveData);
                _threadCopy = new Thread(this.CopyData);
                _threadPlay = new Thread(this.PlayAudio);
                if (!sw.IsRunning)
                    sw.Start();
                _threadReceive.Start();
                _threadCopy.Start();
                _threadPlay.Start();
                SetNotificationLabel("Threads Running");
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// Order 1
        /// </summary>
        private void ReceiveData()
        {
            NetworkStream netStream = _tcp.GetStream();
            byte[] buffer = new byte[_tcp.ReceiveBufferSize];
            while (_tcp.Connected)
            {
                if (netStream.CanRead)
                {
                    int readed = netStream.Read(buffer, 0, _tcp.ReceiveBufferSize);
                    var subBuffer = buffer.Take(readed);

                    _receivedBytes.AddRange(subBuffer);
                    if (_receivedBytes.Count >= offset + _playingLength)
                        _bufferingSemaphore.Release();

                    //if (firstData)
                    //{
                    //    _receivedBytes.RemoveAt(0);
                    //    _receivedBytes.RemoveAt(1);
                    //    firstData = false;
                    //}
                }
            }
        }

        /// <summary>
        /// Order 2
        /// </summary>
        private void CopyData()
        {
            while (_tcp.Connected)
            {
                //if (_receivedBytes.Count >= offset + arrayLength)
                //{
                    _bufferingSemaphore.WaitOne();
                    _mutex.WaitOne();
                    _bufferedBytes = _receivedBytes.GetRange(offset, _playingLength);
                    _mutex.ReleaseMutex();
                    offset += _playingLength;
                    _playingSemaphore.Release();
                //}
            }
        }

        /// <summary>
        /// Order 3
        /// </summary>
        private void PlayAudio()
        {
            while (_tcp.Connected)
            {
                _playingSemaphore.WaitOne();
                _mutex.WaitOne();
                var bufferedBytesArray = _bufferedBytes.ToArray();
                _bufferedBytes.Clear();
                _mutex.ReleaseMutex();

                IWaveProvider provider = new RawSourceWaveStream(new MemoryStream(bufferedBytesArray), new WaveFormat(_sampleRate, 24, 1));

                _waveOut.Init(provider);
                _waveOut.Play();

            }
        }
        
        #endregion
    }
}
