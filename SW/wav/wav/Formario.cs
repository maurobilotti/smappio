using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Media;
//using NAudio.Wave;

namespace wav
{
    public partial class Formario : Form
    {
        public Formario()
        {
            InitializeComponent();
        }

        /*double frecuencia(double nota, double octava)
        {
            return (440.0 * Math.Exp(((octava - 4) + (nota - 10) / 12.0) * Math.Log(2)));
            //Do=1, Do#=2, Re=3, Re#=4, Mi=5, Fa=6, Fa#=7, Sol=8, Sol#=9, La=10, La#=11, Si=12
        }*/

        private string getPathFile()
        {
            return txtArchivoSeleccionado.Text;     
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            byte[] myWaveData;

            // Sample rate -> Cantidad de muestras por segundo
            uint SAMPLE_FREQUENCY = Convert.ToUInt16(txtCantidadMuestrasPorSeg.Text);//3100;
            
            //Duración del audio en segundos
            int AUDIO_LENGTH_IN_SECONDS = Convert.ToInt16(txtTiempoEnSeg.Text);//22;

            List<Byte> tempBytes = new List<byte>();

            WaveHeader header = new WaveHeader();
            
            FormatChunk format = new FormatChunk(SAMPLE_FREQUENCY);
            DataChunk data = new DataChunk();

            SineGenerator leftData = new SineGenerator(/*697.0f, */SAMPLE_FREQUENCY, AUDIO_LENGTH_IN_SECONDS,0, getPathFile());

            data.AddMonoSampleData(leftData.Data);

            header.FileLength += format.Length() + data.Length();

            tempBytes.AddRange(header.GetBytes());
            tempBytes.AddRange(format.GetBytes());
            tempBytes.AddRange(data.GetBytes());

            myWaveData = tempBytes.ToArray();


            // Creación del archivo
            FileStream fileStream = new FileStream("1.wav", FileMode.Create);

            // Use BinaryWriter to write the bytes to the file
            BinaryWriter writer = new BinaryWriter(fileStream);

            // Write the all
            writer.Write(myWaveData);

            writer.Seek(4, SeekOrigin.Begin);
            uint filesize = (uint)writer.BaseStream.Length;
            writer.Write(filesize - 8);

            // Clean up
            writer.Close();
            fileStream.Close();


            SoundPlayer simpleSound = new SoundPlayer("1.wav");
            simpleSound.Play();

        }
    }

    public class WaveHeader
    {
        private const string FILE_TYPE_ID = "RIFF";
        private const string MEDIA_TYPE_ID = "WAVE";

        public string FileTypeId { get; private set; }
        public UInt32 FileLength { get; set; }
        public string MediaTypeId { get; private set; }

        public WaveHeader()
        {
            FileTypeId = FILE_TYPE_ID;
            MediaTypeId = MEDIA_TYPE_ID;
            // Minimum size is always 4 bytes
            FileLength = 4;
        }

        public byte[] GetBytes()
        {
            List<Byte> chunkData = new List<byte>();
            chunkData.AddRange(Encoding.ASCII.GetBytes(FileTypeId));
            chunkData.AddRange(BitConverter.GetBytes(FileLength));
            chunkData.AddRange(Encoding.ASCII.GetBytes(MediaTypeId));
            return chunkData.ToArray();
        }

    }

    public class FormatChunk
    {
        private ushort _bitsPerSample;
        private ushort _channels;
        private uint _frequency;
        private const string CHUNK_ID = "fmt ";

        public string ChunkId { get; private set; }
        public UInt32 ChunkSize { get; private set; }
        

        public UInt16 FormatTag { get; private set; }

        public UInt16 Channels
        {
            get { return _channels; }
            set { _channels = value; RecalcBlockSizes(); }
        }

        public UInt32 Frequency
        {
            get { return _frequency; }
            set { _frequency = value; RecalcBlockSizes(); }
        }

        public UInt32 AverageBytesPerSec { get; private set; }
        public UInt16 BlockAlign { get; private set; }

        public UInt16 BitsPerSample
        {
            get { return _bitsPerSample; }
            set { _bitsPerSample = value; RecalcBlockSizes(); }
        }

        public FormatChunk(uint SAMPLE_FREQUENCY)
        {
            ChunkId = CHUNK_ID;             //No se toca
            ChunkSize = 16;
            FormatTag = 1;                  // MS PCM -> sin compresión
            Channels = 1;                   // Un canal solo
            Frequency = SAMPLE_FREQUENCY;   // Default to 44100hz? -> Le ponemos la cantidad de muestras que procesa la placa por segundo
            BitsPerSample = 16;             // Default to 16bits  -> Deberíamos poner de 32bits ya que el micrófono retorna muestras más grandes
            RecalcBlockSizes();
        }

        private void RecalcBlockSizes()
        {
            BlockAlign = (UInt16)(_channels * (_bitsPerSample / 8));//Es la unidad de datos por muestra -> nuestro caso es 1 canal* 32bits / 8 = 4bytes
            AverageBytesPerSec = _frequency * BlockAlign;//Cantidad de bytes por segundo que procesa. Ej: si procesamos 5000 muestras de 4bytes = 1250bytes por segundo
        }

        public byte[] GetBytes()
        {
            List<Byte> chunkBytes = new List<byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            chunkBytes.AddRange(BitConverter.GetBytes(FormatTag));
            chunkBytes.AddRange(BitConverter.GetBytes(Channels));
            chunkBytes.AddRange(BitConverter.GetBytes(Frequency));
            chunkBytes.AddRange(BitConverter.GetBytes(AverageBytesPerSec));
            chunkBytes.AddRange(BitConverter.GetBytes(BlockAlign));
            chunkBytes.AddRange(BitConverter.GetBytes(BitsPerSample));

            return chunkBytes.ToArray();
        }

        public UInt32 Length()
        {
            return (UInt32)GetBytes().Length;
        }

    }

    public class DataChunk
    {
        private const string CHUNK_ID = "data";

        public string ChunkId { get; private set; }
        public UInt32 ChunkSize { get; set; }
        public short[] WaveData { get; private set; }

        public DataChunk()
        {
            ChunkId = CHUNK_ID;
            ChunkSize = 0;  // Until we add some data
        }

        public UInt32 Length()
        {
            return (UInt32)GetBytes().Length;
        }

        public byte[] GetBytes()
        {
            List<Byte> chunkBytes = new List<Byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));
            byte[] bufferBytes = new byte[WaveData.Length * 2];
            Buffer.BlockCopy(WaveData, 0, bufferBytes, 0,
               bufferBytes.Length);
            chunkBytes.AddRange(bufferBytes.ToList());

            return chunkBytes.ToArray();
        }

        public void AddMonoSampleData(short[] Buffer)
        {
            WaveData = new short[Buffer.Length];
            int bufferOffset = 0;
            for (int index = 0; index < WaveData.Length; index += 1)
            {
                WaveData[index] = Buffer[bufferOffset];
                bufferOffset++;
            }
            ChunkSize = (UInt32)WaveData.Length * 2; //sigo multiplicando * 2 ?
        }
    }

    public class DataChunkN
    {
        private const string CHUNK_ID = "data";

        public string ChunkId { get; private set; }
        public UInt32 ChunkSize { get; private set; }
        //public short[] WaveData { get; private set; }
        List<short> WaveData = new List<short>();

        public DataChunkN()
        {
            ChunkId = CHUNK_ID;
            ChunkSize = 0;  // Until we add some data            
        }

        public UInt32 Length()
        {
            return (UInt32)GetBytes().Length;
        }

        public byte[] GetBytes()
        {
            List<Byte> chunkBytes = new List<Byte>();

            chunkBytes.AddRange(Encoding.ASCII.GetBytes(ChunkId));
            chunkBytes.AddRange(BitConverter.GetBytes(ChunkSize));

            //for (int i = 0; i < WaveData.Count; i++)
            {
                byte[] bufferBytes = new byte[WaveData.Count* 2];
                
                Buffer.BlockCopy(WaveData.ToArray(), 0, bufferBytes, 0,
                   bufferBytes.Length);
                
                chunkBytes.AddRange(bufferBytes.ToList());
            }
            return chunkBytes.ToArray();
        }
    }

    public class SineGenerator
    {
        //private readonly double _frequency;
        private readonly UInt32 _sampleRate;
        private readonly double _secondsInLength;
        private short[] _dataBuffer;
        private string _path;

        public short[] Data { get { return _dataBuffer; } }
        
   

        public SineGenerator(/*double frequency, */UInt32 sampleRate, double audioLengthInseconds, int bits, string path)
        {
            //_frequency = frequency;
            _sampleRate = sampleRate;
            _secondsInLength = audioLengthInseconds;
            _path = path;
          
             GenerateDataFromFile(path);
        }
     
        private void GenerateDataFromFile(String path)
        {
            uint bufferSize = (uint)(_sampleRate * _secondsInLength);
            _dataBuffer = new short[bufferSize];
            
            //leo un archivo
            int counter = 0;  
            string line;
            String[] ss;
            System.IO.StreamReader file =   new System.IO.StreamReader(path);  
            while((line = file.ReadLine()) != null)  
            {  
                ss =line.Split('|');
                uint tope= (uint)(counter * _sampleRate);
                for (uint index = 0; index < _sampleRate - 1; index++)
                {
                    short data = (short)(Convert.ToInt16(ss[index]) * 50);
                    _dataBuffer[tope+index] = data; //lectura de linea de archivo
                }

                //-----------------
                    //iterpolacion lineal entre 2 puntos para generar uno mas
                    //y= ya +  (x-xa)* (yb-ya) / (xb-xa)
                        
                //------------------
                counter++;
                file.ReadLine();//salteo la línea que tiene la información en texto sin las muestras
            }

            file.Close();
            
        }

    }

}
