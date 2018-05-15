using NAudio.Wave;
using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace Smappio_SEAR
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();            
                                   
        }

        BluetoothManager bluetoothManager;
        string deviceName = "HC-05";
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;

        private void btnBluetooth_Click(object sender, EventArgs e)
        {
            bluetoothManager = new BluetoothManager();

            serialPort.PortName = BluetoothHelper.GetBluetoothPort(deviceName);

            serialPort.BaudRate = bluetoothManager.GetBaudRate();
            if (!serialPort.IsOpen)
                serialPort.Open();           

            serialPort.DataReceived += SerialPort_DataReceived;
        }

        private int count = 0;
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                if (outputDevice == null)
                {
                    outputDevice = new WaveOutEvent();
                    outputDevice.PlaybackStopped += OnPlaybackStopped;
                }


                //string text = $"Data {count} : {sp.ReadLine()} ";
                //SetText(text);
                //count++;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFile.Dispose();
            audioFile = null;
        }

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
                txtSerialData.AppendText(Environment.NewLine);
            }
        }


        /*
        //private WasapiCapture _soundIn;
        //private ISoundOut _soundOut;
        //private IWaveSource _source;
        //private PitchShifter _pitchShifter;
        
        //private float[] _fileArray;

        //private readonly Bitmap _bitmap = new Bitmap(2000, 600);
        

        //private void btnOpenFile_Click(object sender, EventArgs e)
        //{
        //    var openFileDialog = new OpenFileDialog()
        //    {
        //        Filter = CodecFactory.SupportedFilesFilterEn,
        //        Title = "Select a file..."
        //    };
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {                

        //        ////open the selected file
        //        //ISampleSource source = CodecFactory.Instance.GetCodec(openFileDialog.FileName)
        //        //    .ToSampleSource()
        //        //    .AppendSource(x => new PitchShifter(x), out _pitchShifter);

                

        //        _fileArray = FloatArrayFromFile(openFileDialog.FileName);

        //        //play the audio
        //        _soundOut = new WasapiOut();
        //        _soundOut.Initialize(_source);
        //        _soundOut.Play();

        //        timer1.Start();                     
        //    }
        //}      

        //public float[] FloatArrayFromFile(string fileName)
        //{
        //    byte[] buff = null;
        //    FileStream fs = new FileStream(fileName,
        //                                   FileMode.Open,
        //                                   FileAccess.Read);
        //    BinaryReader br = new BinaryReader(fs);
        //    long numBytes = new FileInfo(fileName).Length;
        //    buff = br.ReadBytes((int)numBytes);
        //    return FloatArrayFromByteArray(buff);
        //}

        //public float[] FloatArrayFromByteArray(byte[] input)
        //{
        //    float[] output = new float[input.Length / 4];
        //    for (int i = 0; i < output.Length; i++)
        //    {
        //        output[i] = BitConverter.ToSingle(input, i * 4);
        //    }
        //    return output;
        //}
        */

    }
}
