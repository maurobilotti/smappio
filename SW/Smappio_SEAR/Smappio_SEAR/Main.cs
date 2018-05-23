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
        string deviceName = "HC-05";
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private VolumeWaveProvider16 volumeProvider;
        private IMp3FrameDecompressor decompressor = null;
        private string filePath;
        private string fileBinary;
        Stopwatch sw = new Stopwatch();
        int samplesReceived = 0;


        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }
        #endregion


        private void btnBluetooth_Click(object sender, EventArgs e)
        {
            try
            {
                bluetoothManager = new BluetoothManager();

                serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);
                serialPort.BaudRate = 9600;
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;

                if (!serialPort.IsOpen)
                    serialPort.Open();

                sw.Start();
                playbackState = StreamingPlaybackState.Playing;
                serialPort.DataReceived += SerialPort_DataReceived;

                if (playbackState == StreamingPlaybackState.Stopped)
                {
                    playbackState = StreamingPlaybackState.Buffering;
                    bufferedWaveProvider = null;
                    //ThreadPool.QueueUserWorkItem(StreamAudioFromSmappio);
                    //waveOut = new WaveOut();
                    //timer.Enabled = true;
                }
                else if (playbackState == StreamingPlaybackState.Paused)
                {
                    playbackState = StreamingPlaybackState.Buffering;
                }
            }
            catch (Exception ex)
            {
                serialPort.Dispose();
                return;
            }
        }


        /// <summary>
        /// Invoked every time the bluetooth module sents data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (playbackState == StreamingPlaybackState.Playing)
            {
                SerialPort sp = (SerialPort)sender;

                //var buffer = new byte[sp.BytesToRead];
                string raw = sp.ReadExisting();
                var responseValues = raw.Split(' ');
                samplesReceived += responseValues.Length;
                foreach (var item in responseValues)
                {
                    if (string.IsNullOrWhiteSpace(item))
                        continue;
                    var byteVal = Convert.ToInt16(item);
                    var value = Convert.ToString(byteVal, 2).PadLeft(16, '0');
                    fileBinary += value;

                    SetText(item);
                }
            }
        }

        public byte[] ConvertWavToMp3(byte[] wavFile)
        {

            using (var retMs = new MemoryStream())
            using (var ms = new MemoryStream(wavFile))
            using (var rdr = new WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(retMs, rdr.WaveFormat, 128))
            {
                rdr.CopyTo(wtr);
                return retMs.ToArray();
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (waveOut == null && bufferedWaveProvider != null)
                {
                    waveOut = new WaveOut();
                    waveOut.PlaybackStopped += OnPlaybackStopped;
                    volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                    volumeProvider.Volume = 1.0f; //maximum volume
                    waveOut.Init(volumeProvider);
                }
                else if (bufferedWaveProvider != null)
                {
                    var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded)
                    {
                        Pause();
                    }
                    else if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering)
                    {
                        Play();
                    }
                    else if (fullyDownloaded && bufferedSeconds == 0)
                    {
                        StopPlayback();
                    }
                }
            }

        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                MessageBox.Show(String.Format("Playback Error {0}", e.Exception.Message));
            }
        }

        private void Play()
        {
            waveOut.Play();
            playbackState = StreamingPlaybackState.Playing;
        }

        private void Pause()
        {
            playbackState = StreamingPlaybackState.Buffering;
            waveOut.Pause();
        }

        private void StopPlayback()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Stopped;
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }
                timer.Enabled = false;
                // n.b. streaming thread may not yet have exited
                Thread.Sleep(500);
            }
        }

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
                txtSerialData.AppendText(text);
            }
        }

        #endregion

        private void btnStop_Click(object sender, EventArgs e)
        {
            sw.Stop();
            playbackState = StreamingPlaybackState.Stopped;
            lblElapsedTime.Text = sw.Elapsed.ToString();
            lblSamplesReceived.Text = samplesReceived.ToString();
            byte[] fileBytes = fileBinary.GetBytesFromBinaryString();
            File.WriteAllBytes(filePath, fileBytes);

            if (serialPort.IsOpen)
            {
                serialPort.DiscardInBuffer();
                serialPort.Close();
            }


            //this.Close();

            //if(waveWriter != null)
            //{
            //    waveWriter.Flush();
            //    waveWriter.Close();                
            //    waveWriter.Dispose();                
            //}

            //playbackState = StreamingPlaybackState.Stopped;
        }

        private void btnWifi_Click(object sender, EventArgs e)
        {

        }

        private void btnFileDestination_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PCM (*.pcm) | *.pcm";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                filePath = sfd.FileName;
            }
        }
    }
}
