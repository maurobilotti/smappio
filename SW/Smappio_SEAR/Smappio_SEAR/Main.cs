﻿using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Smappio_SEAR.Serial;
using Smappio_SEAR.Wifi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Smappio_SEAR
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            txtPath.Text = filePath;
            btnPlay.Enabled = false;
            btnStop.Enabled = false;
            btnSave.Enabled = false;
            btnClear.Enabled = false;
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
        public UIParams UIParams { get; private set; }

        #region Transfering methods

        private void btnUSB_Click(object sender, EventArgs e)
        {
            InvokeReceiver(new SerialReceiver(ref _serialPort));
        }

        private void btnBluetooth_Click(object sender, EventArgs e)
        {

        }

        private void btnUdp_Click(object sender, EventArgs e)
        {
            InvokeReceiver(new UdpReceiver());
        }

        private void btnAuscultate_Click(object sender, EventArgs e)
        {
            InvokeReceiver(new TcpReceiver(UIParams));
            btnSave.Enabled = true;
            btnClear.Enabled = true;
        }

        private bool InvokeReceiver(Receiver receiver)
        {
            Receiver = receiver;

            if (!Receiver.Connected)
            {
                SetNotificationLabel(Receiver.PortName + " Not Available");
                return false;
            }

            Receiver.Receive();

            SetButtonStatus();
            SetNotificationLabel(Receiver.PortName + " Running");

            if (!sw.IsRunning)
                sw.Start();

            return true;
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

        //delegate void ClearWaveFormPainterCallback();
        //private void ClearWaveFormPainter()
        //{
        //    // InvokeRequired required compares the thread ID of the
        //    // calling thread to the thread ID of the creating thread.
        //    // If these threads are different, it returns true.

        //    if (this.waveformPainter.InvokeRequired)
        //    {
        //        ClearWaveFormPainterCallback d = new ClearWaveFormPainterCallback();
        //        this.Invoke(d, new object[] {});
        //    }
        //    else
        //    {
        //        waveformPainter.Refresh();
        //    }
        //}


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

            Receiver.SaveFile();
            Receiver.ClearAndClose();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearContents();
        }

        private void ClearContents()
        {
            if (Receiver != null)
                Receiver.ClearAndClose();

            SetButtonStatus(true);
            lblBitRate.Text = lblNotification.Text = lblBitRate.Text = lblSampleRate.Text = lblSamplesReceived.Text = lblTime.Text = "";
            for (int i = 0; i < 2000; i++)
            {
                this.waveformPainter.AddMax(0);
            }

            this.volumeMeter.Amplitude = 0;

        }

        private void SetButtonStatus(bool status = false)
        {
            btnBluetooth.Enabled = btnAuscultate.Enabled = btnSerial.Enabled = status;
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
                btnPlay.Enabled = true;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = true;
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
            postVolumeMeter.SamplesPerNotification = 80;
            return postVolumeMeter;
        }

        private void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            waveformPainter.AddMax(e.MaxSampleValues[0] * 5);
        }
        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            cbEncoding.SelectedIndex = 1;
            this.UIParams = new UIParams(ref this.waveformPainter, ref this.volumeMeter, (PCMAudioFormat)cbEncoding.SelectedIndex);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Receiver != null)
                Receiver.ClearAndClose();
        }

        private void cbEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UIParams = new UIParams(ref this.waveformPainter, ref this.volumeMeter, (PCMAudioFormat)cbEncoding.SelectedIndex);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            waveOut.Stop();
            ClearContents();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            this.UIParams = new UIParams(ref this.waveformPainter, ref this.volumeMeter, (PCMAudioFormat)cbEncoding.SelectedIndex, Mode.Test);
            try
            {
                bool result;
                result = InvokeReceiver(new TcpReceiver(UIParams));

                if (!result)
                {
                    MessageBox.Show("Issue: The device is not connected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    Thread.Sleep(2000);
                    if (this.Receiver.TestResult)
                    {
                        MessageBox.Show("OK: The data was received correctly and the device is responding as expected.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SetNotificationLabel("Test PASSED.");
                    }
                    else
                    {
                        MessageBox.Show("Error: There are connection errors. The data was not received correctly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SetNotificationLabel("Test not passed.");
                    }                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetButtonStatus(true);
        }

        private void btnLogs_Click(object sender, EventArgs e)
        {
            this.UIParams = new UIParams(ref this.waveformPainter, ref this.volumeMeter, (PCMAudioFormat)cbEncoding.SelectedIndex, Mode.Logs);
            try
            {
                var result = InvokeReceiver(new TcpReceiver(UIParams));

                if (!result)
                    MessageBox.Show("Issue: The device is not connected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                throw;
            }
            SetButtonStatus(true);
        }
    }
}
