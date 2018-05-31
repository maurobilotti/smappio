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
            var bufferSize = serialPort.BytesToRead;

            if (bufferSize <= 4)
                return;

            byte[] data = new byte[bufferSize];
            serialPort.Read(data, 0, bufferSize);

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
        #endregion

        #region TextBox_Methods
        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.

            if (this.txtSerialData.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
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
            //little endian!               
            File.WriteAllBytes(filePath, _bytes.ToArray());
            lblSamplesReceived.Text = _bytes.Count.ToString();

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
