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
        private List<Int32> _fileInts = new List<int>();
        List<byte> _receivedBytes = new List<byte>();
        Stopwatch sw = new Stopwatch();
        long elapsedMilliseconds = 0;
        Socket _socket;
        private TransmissionMethod _transmissionMethod = TransmissionMethod.Wifi;
        #endregion

        #region SoundParameters

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
                _serialPort.PortName = "COM13";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
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
        int _offset = 0;
        private WaveOut _waveOut = new WaveOut();
        private int _playingLength = 3000;// 3 * 20400;
        private MemoryStream _memoryStream = new MemoryStream();
        BufferedWaveProvider _provider = new BufferedWaveProvider(new WaveFormat(_sampleRate, 24, 1));
        /// <summary>
        /// Variable utilizada para generar un colchon de datos para reproducir
        /// </summary>
        private int _cache = 1;
        private bool _release = false;

        private void btnWifiHTTP_Click(object sender, EventArgs e)
        {
            _tcp = new TcpClient("192.168.1.2", 80);
            
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
        string bytePacketsReceived = "";
        int readedAux = 0;
        int readed = 0;
        private void ReceiveData()
        {
            NetworkStream netStream = _tcp.GetStream();
            byte[] buffer = new byte[_playingLength];
            byte[] buffertmp = new byte[_playingLength];
            int acumDiscardedBytes = 0;

            while (_tcp.Connected)
            {
                if (netStream.CanRead)
                {
                    if (_tcp.Client.Available < _playingLength)
                        continue;

                    acumDiscardedBytes = 0;
                    readedAux = netStream.Read(buffertmp, 0, _playingLength);

                    #region Algoritmo de control de datos
                    //Verificar que lo que se lee cumpla con la secuencia 01, 10, 11
                    int i = 0;
                    readed = 0;
                    while (i < readedAux - 3)
                    {
                        int bitsControlPrimerByte = buffertmp[i] >> 6;
                        int bitsControlSegundoByte = buffertmp[i + 1] >> 6;
                        int bitsControlTercerByte = buffertmp[i + 2] >> 6;
                        int discartedBytes = 0;

                        if (bitsControlPrimerByte != 1)
                        {
                            if (bitsControlPrimerByte == 2)
                                discartedBytes += 2;
                            else if (bitsControlPrimerByte == 3)
                                discartedBytes += 1;
                        }
                        else if (bitsControlSegundoByte != 2)
                        {
                            if (bitsControlSegundoByte == 1)
                                discartedBytes += 1;
                            else if (bitsControlSegundoByte == 3)
                                discartedBytes += 2;
                        }
                        else if (bitsControlTercerByte != 3)
                        {
                            if (bitsControlTercerByte == 1)
                                discartedBytes += 2;
                            else if (bitsControlTercerByte == 2)
                                discartedBytes += 3;
                        }
                        else
                        {
                            //Vuelvo a armar las muetras originales
                            //Byte 1 => ultimos 6 bits del primer byte + 2 últimos bits del segundo byte
                            buffer[i - acumDiscardedBytes] = (byte)((buffertmp[i + 1] & 3) << 6);
                            buffer[i - acumDiscardedBytes] = (byte)(buffer[i] | (buffertmp[i] & 63));

                            //Byte 2 => 4 bits del medio del segundo byte + 4 úlitmos bits del último byte
                            buffer[i + 1 - acumDiscardedBytes] = (byte)((buffertmp[i + 2] & 15) << 4);
                            buffer[i + 1 - acumDiscardedBytes] = (byte)(buffer[i + 1] | ((buffertmp[i + 1] >> 2) & 15));

                            //Byte 3 => 1 bit (el 4to de izq a derecha)
                            buffer[i + 2 - acumDiscardedBytes] = (byte)((buffertmp[i + 2] >> 4) & 1);

                            //Byte 3 => 5 bits para el signo(depende del 3ero de izq a derecha)
                            // Si el bit mas significativo del samlpe es '1' quiere decir que el numero es negativo, entonces se
                            // agrega un padding a la izquierda de '7' unos, caso contrario, se deja el padding 0 que ya habia
                            byte signBit = (byte)((buffertmp[i + 2] >> 5) & 1);
                            if (signBit == 1)
                                buffer[i + 2 - acumDiscardedBytes] = (byte)(buffer[i + 2 - acumDiscardedBytes] | 254);

                            readed += 3;
                        }

                        if (discartedBytes == 0)
                            i += 3;
                        else
                        {
                            i += discartedBytes;
                            acumDiscardedBytes += discartedBytes;
                        }
                    }

                    #endregion

                    _receivedBytes.AddRange(buffer.Take(readed).ToList());

                    if (!sw.IsRunning)
                        sw.Start();

                    if (_receivedBytes.Count < _playingLength * 4)
                        continue;

                    AddSamples();
                }
            }
        }
        
        public void AddSamples()
        {
            var bufferForPlaying = _receivedBytes.GetRange(_offset, readed).ToArray();

            _offset += readed;

            _provider.AddSamples(bufferForPlaying, 0, bufferForPlaying.Length);
        }

        #endregion
    }
}
