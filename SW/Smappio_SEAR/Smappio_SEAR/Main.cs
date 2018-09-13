using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Smappio_SEAR.Provider;
using Smappio_SEAR.Serial;
using Smappio_SEAR.Wifi;
using System;
using System.Collections.Generic;
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
        private bool _notified;
        public Receiver Receiver;
        private string fileName;
        public WaveOut waveOut = new WaveOut();
        private AudioFileReader audioFileReader;
        private Action<float> setVolumeDelegate;

        #region Transfering methods

        private void btnUSB_Click(object sender, EventArgs e)
        {
            InvokeReceiver(new SerialReceiver(ref _serialPort));
        }

        private void btnBluetooth_Click(object sender, EventArgs e)
        {
            //EnableFeatures();
            //try
            //{
            //    _serialPort.PortName = BluetoothHelper.GetBluetoothPort("smappio");
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
            InvokeReceiver(new UdpReceiver());
        }

        private void btnTcp_Click(object sender, EventArgs e)
        {
            InvokeReceiver(new TcpReceiver(ref waveformPainter));
        }

        private void InvokeReceiver(Receiver receiver)
        {
            Receiver = receiver;

            if (!Receiver.Connected)
            {
                SetNotificationLabel(Receiver.PortName + " Not Available");
                return;
            }

            Receiver.Receive();
            Receiver.Play();

            SetButtonStatus();
            SetNotificationLabel(Receiver.PortName + " Running");

            if (!sw.IsRunning)
                sw.Start();
        }

        #endregion

        #region Program features

        #region TextBox_Methods
        

        delegate void SetNotificationLabelCallback(string text);
        private void SetNotificationLabel(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.

            if (this.lblNotification.InvokeRequired)
            {
                SetNotificationLabelCallback d = new SetNotificationLabelCallback(SetNotificationLabel);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                lblNotification.Text = text;
            }
        }

        #endregion
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!_notified)
            {
                sw.Stop();
                elapsedMilliseconds = sw.ElapsedMilliseconds;
                SetNotificationLabel("Finished");
                _notified = true;
                SetButtonStatus(true);
            }

            if (elapsedMilliseconds == 0)
                elapsedMilliseconds = 1000; // Para que no estalle

            long sampleRate = Receiver.GetReceivedBytes() / Receiver.GetBytesDepth() / (elapsedMilliseconds / 1000);
            lblSampleRate.Text = sampleRate.ToString();

            long bitRate = (sampleRate * Receiver.GetBytesDepth() * 8) / 1000;

            lblTime.Text = elapsedMilliseconds.ToString();
            lblSamplesReceived.Text = (Receiver.GetReceivedBytes() / Receiver.GetBytesDepth()).ToString();
            lblBitRate.Text = bitRate.ToString();

            int badSequencesCounter = 0;
            int niceSequencesCounter = 0;
            IList<int> badSequencesIndexes = new List<int>();
            for (int i = 1; i < Receiver.bufferSeqNums.Count; i++)
            {
                if ((Receiver.bufferSeqNums[i - 1] + 1 == Receiver.bufferSeqNums[i]) || (Receiver.bufferSeqNums[i] == 0 && Receiver.bufferSeqNums[i - 1] == 63))
                {
                    niceSequencesCounter++;
                }
                else
                {
                    badSequencesCounter++;
                    badSequencesIndexes.Add(i);
                }
            }

            Receiver.Close();
            Receiver.SaveFile();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearContents();
        }

        private void ClearContents()
        {
            Receiver.ClearAndClose();

            SetButtonStatus(true);
            lblBitRate.Text = lblNotification.Text = lblBitRate.Text = lblSampleRate.Text = lblSamplesReceived.Text = lblTime.Text = "";            
        }

        private void SetButtonStatus(bool status = false)
        {
            btnBluetooth.Enabled = btnTcp.Enabled = btnUdp.Enabled = btnSerial.Enabled = status;
        }

        #endregion

        #region File Methods
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            string allExtensions = "*.wav";
            openFileDialog.Filter = String.Format("All Supported Files|{0}|All Files (*.*)|*.*", allExtensions);
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog.FileName;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    return;
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                }
            }

            ISampleProvider sampleProvider;
            try
            {
                sampleProvider = CreateInputStream(fileName);
            }
            catch (Exception createException)
            {
                MessageBox.Show(String.Format("{0}", createException.Message), "Error Loading File");
                return;
            }


            try
            {
                waveOut.Init(sampleProvider);
            }
            catch (Exception initException)
            {
                MessageBox.Show(String.Format("{0}", initException.Message), "Error Initializing Output");
                return;
            }

            waveOut.Play();
        }

        private ISampleProvider CreateInputStream(string fileName)
        {
            audioFileReader = new AudioFileReader(fileName);
            var sampleChannel = new SampleChannel(audioFileReader, false);
            setVolumeDelegate = vol => sampleChannel.Volume = vol;
            var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
            postVolumeMeter.StreamVolume += OnPostVolumeMeter;


            return postVolumeMeter;
        }

        private void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            waveformPainter.AddMax(e.MaxSampleValues[0]);
        }
        #endregion

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Receiver.ClearAndClose();
        }
    }
}
