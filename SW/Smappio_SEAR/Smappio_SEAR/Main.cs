﻿using NAudio.Wave;
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
        private List<Int32> _fileInts = new List<int>();
        List<byte> _receivedBytes = new List<byte>();
        Stopwatch sw = new Stopwatch();
        long elapsedMilliseconds = 0;
        Socket _socket;
        private TransmissionMethod _transmissionMethod = TransmissionMethod.Wifi;
        #endregion

        #region SoundParameters

        //static int _sampleRate = 16000;   // No se esta usando por hacer los calculos en base al tiempo
        static int _bytesDepth = 3;
        static int _sampleRate = 32000;// No modificar, pues modifica el audio escuchado.
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
                _serialPort.PortName = "COM7";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
                _serialPort.BaudRate = Convert.ToInt32(_baudRate);
                _serialPort.Handshake = Handshake.None;

                //ojo con estos flags! sin esto, no recibe!!
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = true;
                if (!_serialPort.IsOpen)
                    _serialPort.Open();

                lblNotification.Text = "Started";
                _serialPort.Write("s");

                if (!sw.IsRunning)
                    sw.Start();

                _serialPort.DataReceived += SerialPort_DataReceived;

                this.Play();
            }
            catch (Exception ex)
            {
                _serialPort.Dispose();
                return;
            }
        }

        private void btnBluetooth_Click(object sender, EventArgs e)
        {
            EnableFeatures();
            try
            {
                _serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);
                _serialPort.BaudRate = Convert.ToInt32(_baudRate);
                _serialPort.DtrEnable = true;
                _serialPort.RtsEnable = true;

                if (!_serialPort.IsOpen)
                    _serialPort.Open();


                lblNotification.Text = "Started";
                _serialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception ex)
            {
                _serialPort.Dispose();
                return;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bufferSize = _serialPort.BytesToRead;

            byte[] data = new byte[bufferSize];
            _serialPort.Read(data, 0, bufferSize);

            if (!sw.IsRunning)
                sw.Start();

            this._receivedBytes.AddRange(data);

            if (_cache != 0)
                _cache--;
            else
                _release = true;

            bool releaseCondition = _receivedBytes.Count >= _offset + _playingLength;

            if (firstData)
            {
                _receivedBytes.RemoveAt(0);
                _receivedBytes.RemoveAt(1);
                firstData = false;
            }

            if (_receivedBytes.Count < _offset + _playingLength)
                return;

            AddSamples();
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

            if (_serialPort.IsOpen)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.Close();
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

            if (_serialPort.IsOpen)
                _serialPort.Close();
        }
        #endregion

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_transmissionMethod == TransmissionMethod.Wifi)
            {
                _tcp.Close();
                _tcp.Dispose();
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        #region Wifi Logic
        private Thread _threadReceive;
        private TcpClient _tcp = new TcpClient();
        bool firstData = true;
        int _offset = 0;
        private WaveOut _waveOut = new WaveOut();
        private int _playingLength = 8000 * 3;// 3 * 20400;
        private MemoryStream _memoryStream = new MemoryStream();
        BufferedWaveProvider _provider = new BufferedWaveProvider(new WaveFormat(_sampleRate, 24, 1));
        /// <summary>
        /// Variable utilizada para generar un colchon de datos para reproducir
        /// </summary>
        private int _cache = 1;
        private bool _release = false;

        private void btnWifiHTTP_Click(object sender, EventArgs e)
        {
            try
            {
                _tcp = new TcpClient("192.168.1.1", 80);

            }
            catch (Exception ex)
            {

                throw;
            }
            //_tcp.NoDelay = false;
            _threadReceive = new Thread(this.ReceiveData);

            _threadReceive.Start();

            SetNotificationLabel("Threads Running");
            Play();
        }

        private void Play()
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _waveOut.Init(_provider);
                _waveOut.Play();
            }
        }
        string cantidades = "";
        private void ReceiveData()
        {
            NetworkStream netStream = _tcp.GetStream();
            byte[] buffer = new byte[_tcp.ReceiveBufferSize];
            while (_tcp.Connected)
            {
                if (netStream.CanRead)
                {
                    int readed = netStream.Read(buffer, 0, _tcp.ReceiveBufferSize);
                    List<byte> subBuffer = buffer.Take(readed).ToList();

                    cantidades += readed.ToString() + " ";

                    _receivedBytes.AddRange(subBuffer);

                    if (firstData)
                    {
                        if (!sw.IsRunning)
                            sw.Start();
                        _receivedBytes.RemoveAt(0);
                        _receivedBytes.RemoveAt(1);
                        firstData = false;
                        readed -= 2;
                    }
                    

                    if (_receivedBytes.Count < _offset + _playingLength)
                        continue;

                    AddSamples();
                }
            }
        }

        public void AddSamples()
        {
            //var newBytesSize = _receivedBytes.Count - _offset;
            //var bufferForPlaying = _receivedBytes.GetRange(_offset, newBytesSize).ToArray();

            //_offset += newBytesSize;

            var bufferForPlaying = _receivedBytes.GetRange(_offset, _playingLength).ToArray();

            _offset += _playingLength;

            _provider.AddSamples(bufferForPlaying, 0, bufferForPlaying.Length);
        }

        #endregion
    }
}
