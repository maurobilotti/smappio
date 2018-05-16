using NAudio.Wave;
using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

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
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private VolumeWaveProvider16 volumeProvider;
        private IMp3FrameDecompressor decompressor = null;

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
            bluetoothManager = new BluetoothManager();

            serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);

            serialPort.BaudRate = bluetoothManager.GetBaudRate();
            if (!serialPort.IsOpen)
                serialPort.Open();

            serialPort.DataReceived += SerialPort_DataReceived;

            if (playbackState == StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Buffering;
                bufferedWaveProvider = null;
                //ThreadPool.QueueUserWorkItem(StreamAudioFromSmappio);
                //timer.Enabled = true;
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }
        }


        /// <summary>
        /// Invoked every time the bluetooth module sents data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame
            try
            {
                SerialPort sp = (SerialPort)sender;

                //reads the bytes while they're available.
                string text = sp.ReadExisting();
                SetText(text);
                var readFullyStream = sp.ReadExisting().ToStream();
                //do
                //{
                //    if (IsBufferNearlyFull)
                //    {
                //        Thread.Sleep(500);
                //    }
                //    else
                //    {
                //        Mp3Frame frame;
                //        try
                //        {
                //            frame = Mp3Frame.LoadFromStream(readFullyStream);
                //        }
                //        catch (Exception ex)
                //        {
                //            fullyDownloaded = true;
                //            // reached the end of the MP3 file / stream
                //            break;
                //        }

                //        if (frame == null) break;
                //        if (decompressor == null)
                //        {
                //            // don't think these details matter too much - just help ACM select the right codec
                //            // however, the buffered provider doesn't know what sample rate it is working at
                //            // until we have a frame
                //            decompressor = CreateFrameDecompressor(frame);
                //            bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                //            bufferedWaveProvider.BufferDuration =
                //                TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                //                                          //this.bufferedWaveProvider.BufferedDuration = 250;
                //        }
                //        int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                //        //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                //        bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                //    }

                //} while (playbackState != StreamingPlaybackState.Stopped);
                //// was doing this in a finally block, but for some reason
                //// we are hanging on response stream .Dispose so never get there
                ////decompressor.Dispose();
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
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
                this.txtSerialData.AppendText(text);                
            }
        }

        #endregion

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (decompressor != null)
            {
                decompressor.Dispose();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
                serialPort.Close();
        }
    }
}
