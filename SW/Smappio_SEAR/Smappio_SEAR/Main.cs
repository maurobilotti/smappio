using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smappio_SEAR
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        #region Members
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        BluetoothManager bluetoothManager;
        string deviceName = "smappio_PCM";
        private string filePath;
        private List<Int32> _fileInts = new List<int>();
        List<byte> _bytes = new List<byte>();
        Stopwatch sw = new Stopwatch();
        long elapsedMilliseconds = 0;
        #region SoundParameters
        int _bytesDepth = 3;
        int _sampleRate = 32000;
        int _seconds = 5;
        
        private bool _notified;

        #endregion

        //Stopwatch sp = new Stopwatch();


        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        #endregion

        #region Transfering methods
        private void btnWifi_Click(object sender, EventArgs e)
        {
            //logica para Wi-fi
        }

        private void btnUSB_Click(object sender, EventArgs e)
        {
            //Silicon Labs CP210x USB to UART Bridge
            try
            {
                serialPort.PortName = "COM4";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
                serialPort.BaudRate = 960000;
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
            try
            {
                bluetoothManager = new BluetoothManager();

                serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);
                serialPort.BaudRate = 115200;
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
            if(_bytes.Count <= (_sampleRate * _bytesDepth * _seconds))
            {
                                
                var bufferSize = serialPort.BytesToRead;

                //USED FOR PRINTING INT32 IN TEXTBOX
                //if (bufferSize <= 4)
                //    return;

                byte[] data = new byte[bufferSize];
                serialPort.Read(data, 0, bufferSize);

                if (!sw.IsRunning)
                    sw.Start();

                //LOGIC FOR PRINTING THE VALUES IN THE TEXTBOX.
                //int i = 0;
                //Int32 temp = 0;
                //while (i <= (bufferSize - 4))
                //{
                //    temp = (Int32)BitConverter.ToInt32(data, i);

                //    SetText(temp.ToString());  

                //    i += 4;
                //}

                this._bytes.AddRange(data);
            }
            else if(!_notified)
            {
                //lblNotification.Text = "Finish";
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
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

        #endregion

        #region Save file methods
        private void btnStop_Click(object sender, EventArgs e)
        {

            lblTime.Text = elapsedMilliseconds.ToString();
            int samplesReceived = _bytes.Count / _bytesDepth;
            lblSamplesReceived.Text = samplesReceived.ToString();
            long bitRate = samplesReceived / (elapsedMilliseconds / 1000);
            lblBitRate.Text = bitRate.ToString();

            lblNotification.Text = "Finished";

            //little endian!               
            File.WriteAllBytes(filePath, _bytes.ToArray());
            

            if (serialPort.IsOpen)
            {
                serialPort.DiscardInBuffer();
                serialPort.Close();
            }
        }

        private void btnFileDestination_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "WAV (*.wav) | *.wav";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                filePath = sfd.FileName;
            }
        }
        #endregion
    }
}
