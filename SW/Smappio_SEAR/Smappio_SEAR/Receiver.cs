using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Smappio_SEAR
{
    public abstract class Receiver
    {
        public UIParams UI { get; }

        private PCMHelper pcmHelper;
        protected List<byte> ReceivedBytes { get; set; }
        private string _filePath = "../../AudioSamples/";
        private readonly int _sampleRate = 4600;// No modificar, pues modifica el audio escuchado.
        private readonly int _bytesDepth = 3;
        private readonly int _bitDepth = 24;
        private readonly BufferedWaveProvider _provider;        
        private WaveOut _waveOut = new WaveOut();
        public bool Connected = false;
        public TransmissionMethod TransmissionMethod;
        private bool _prebuffering = true;
        protected const int _prebufferingSize = 0;
        protected const int _playingLength = 345;// 3 * 20400
        protected int _offset = 0;
        protected int errorFreeReaded = 0;
        protected int _amplitudeMultiplier = 63;
        public IList<int> bufferSeqNums = new List<int>();
        public int totalDiscardedBtes = 0;
        protected byte[] bufferAux = new byte[_playingLength * 2];
        protected int readedAux = 0;
        protected WaveFormat _waveFormat;
        private Action<float> setVolumeDelegate;
        private MeteringSampleProvider _meteringSampleProvider;
        private int _channels = 1;
        #region Plotter
        private float _silenceAverage = 0.0187f;
        private float _soundMultiplier = 15;
        #endregion


        public Receiver()
        {
            this.ReceivedBytes = new List<byte>();
            this._waveFormat = new WaveFormat(_sampleRate, _bitDepth, 1);
            this._provider = new BufferedWaveProvider(this._waveFormat);
            var sampleChannel = new SampleChannel(_provider);
            //sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;
        }

        public Receiver(UIParams ui)
        {
            this.UI = ui;
            this.pcmHelper = new PCMHelper(ui.Format);
            this.ReceivedBytes = new List<byte>();
            this._waveFormat = GetWaveFormat(ui.Format);
            this._provider = new BufferedWaveProvider(this._waveFormat);
            this._provider.BufferLength = this._provider.BufferLength * 10;
            var sampleChannel = new SampleChannel(_provider);
            sampleChannel.PreVolumeMeter += OnPreVolumeMeter;
            setVolumeDelegate = vol => sampleChannel.Volume = vol;
            _meteringSampleProvider = new MeteringSampleProvider(sampleChannel);
            //indica la cantidad de samples que representan un punto en la grafica
            _meteringSampleProvider.SamplesPerNotification = 80; 
            _meteringSampleProvider.StreamVolume += OnPostVolumeMeter;            
        }

        private WaveFormat GetWaveFormat(PCMAudioFormat format)
        {
            if (format == PCMAudioFormat.PCM_32_Float)
                return WaveFormat.CreateIeeeFloatWaveFormat(_sampleRate, _channels);
            if (format == PCMAudioFormat.PCM_16)
                return new WaveFormat(_sampleRate, 16, _channels);

            return new WaveFormat(_sampleRate, _bitDepth, _channels);
        }

        private void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            //dependiendo el microfono, es necesario utilizar esta funcion para mejorar la imagen
            //var value = Math.Exp((_soundMultiplier * e.MaxSampleValues[0]) - (_soundMultiplier * _silenceAverage)) - 1;
            //if (value < 0.005f)
            //    value = 0.001f;
            //UI.WavePainter.AddMax((float)value);
            UI.WavePainter.AddMax(e.MaxSampleValues[0] * 5);
        }

        private void OnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            UI.VolumeMeter.Amplitude = e.MaxSampleValues[0];            
        }

        #region Public Methods
        protected virtual void Play()
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _waveOut.Init(_meteringSampleProvider);
                _waveOut.Play();
            }
        }

        public virtual void SaveFile()
        {
            string absolutePath = Path.GetFullPath(_filePath);
            string fileName = $"{DateTime.Now.ToString("yyyy-MM-dd_hhmmss")}.wav";
            string fullPath = Path.Combine(absolutePath, fileName);
            var bytes = pcmHelper.GetBufferForPlaying(ReceivedBytes.ToArray());            
            RawSourceWaveStream source = new RawSourceWaveStream(bytes, 0, bytes.Length, _waveFormat);
            WaveFileWriter.CreateWaveFile(fullPath, source);            
        }

        public void AddSamplesToPlayer()
        {
            var auxBuffer = ReceivedBytes.GetRange(_offset, errorFreeReaded).ToArray();

            var bufferForPlaying = pcmHelper.GetBufferForPlaying(auxBuffer);

            _offset += errorFreeReaded;            
            
            _provider.AddSamples(bufferForPlaying, 0, bufferForPlaying.Length);              
            
        }

        protected byte[] ControlAlgorithm()
        {
            //Verificar que lo que se lee cumpla con la secuencia 01, 10, 11                    
            byte[] errorFreeBuffer = new byte[_playingLength];
            errorFreeReaded = 0;
            int i = 0;
            int acumDiscardedBytes = 0;
            while (i < readedAux - 2)
            {
                int firstByteSeqNumber = bufferAux[i] >> 6;
                int secondByteSeqNumber = bufferAux[i + 1] >> 6;
                int thirdByteSeqNumber = bufferAux[i + 2] >> 6;
                int discardedBytes = 0;

                // Si algun numero no no sigue la secuencia, se descartan bytes para atras, nunca para delante
                if (firstByteSeqNumber != 1)
                {
                    if (firstByteSeqNumber == 0)
                    {
                        bufferSeqNums.Add(bufferAux[i] & 63);
                    }
                    discardedBytes += 1;
                }
                else if (secondByteSeqNumber != 2)
                {
                    if (secondByteSeqNumber == 0)
                    {
                        bufferSeqNums.Add(bufferAux[i + 1] & 63);
                        discardedBytes += 1;
                    }
                    else if (secondByteSeqNumber == 1)
                        discardedBytes += 1;
                    else if (secondByteSeqNumber == 3)
                        discardedBytes += 2;
                }
                else if (thirdByteSeqNumber != 3)
                {
                    if (thirdByteSeqNumber == 0)
                    {
                        bufferSeqNums.Add(bufferAux[i + 2] & 63);
                        discardedBytes += 1;
                    }
                    else if (thirdByteSeqNumber == 1)
                        discardedBytes += 2;
                    else if (thirdByteSeqNumber == 2)
                        discardedBytes += 3;
                }
                else
                {
                    //Vuelvo a armar las muetras originales
                    int sample = 0;
                    byte[] sampleAsByteArray = new byte[sizeof(int)];
                    int errorFreeBaseIndex = i - acumDiscardedBytes;
                    byte auxByteMSB = 0;    // Most Significant Bits

                    //Byte 1 => ultimos 6 bits del primer byte + 2 últimos bits del segundo byte
                    auxByteMSB = (byte)((bufferAux[i + 1] & 3) << 6);                           // 'XX|000000'
                    sampleAsByteArray[0] = (byte)(auxByteMSB | (bufferAux[i] & 63));            // 'XX|YYYYYY'

                    //Byte 2 => 4 bits del medio del segundo byte + 4 úlitmos bits del último byte
                    auxByteMSB = (byte)((bufferAux[i + 2] & 15) << 4);                          // 'XXXX|0000'
                    sampleAsByteArray[1] = (byte)(auxByteMSB | ((bufferAux[i + 1] >> 2) & 15)); // 'XXXX|YYYY'

                    //Byte 3 => 1 bit (el 4to de izq a derecha)
                    sampleAsByteArray[2] = (byte)((bufferAux[i + 2] >> 4) & 1);                 // '0000000|X'

                    //Byte 3 => 5 bits para el signo(depende del 3ero de izq a derecha)
                    // Si el bit mas significativo del samlpe es '1' quiere decir que el numero es negativo, entonces se
                    // agrega un padding a la izquierda de '7 + 8' unos, caso contrario, se deja el padding 0 que ya habia y se agregan '8' ceros mas
                    byte signBit = (byte)((bufferAux[i + 2] >> 5) & 1);
                    if (signBit == 1)
                    {
                        sampleAsByteArray[2] = (byte)(sampleAsByteArray[2] | 254);              // '1111111|X'
                        sampleAsByteArray[3] = 255;                                             // '11111111'
                    }
                    else
                    {
                        sampleAsByteArray[3] = 0;                                               // '00000000'
                    }

                    // Se amplifica el sonido multiplicando por la constante '_amplitudeMultiplier' si está en PCM-24
                    if(UI.Format == PCMAudioFormat.PCM_24)
                    {
                        sample = BitConverter.ToInt32(sampleAsByteArray, 0) * _amplitudeMultiplier;
                    }
                    else
                    {
                        sample = BitConverter.ToInt32(sampleAsByteArray, 0);
                    }
                    
                    sampleAsByteArray = BitConverter.GetBytes(sample);

                    errorFreeBuffer[errorFreeBaseIndex] = sampleAsByteArray[0];
                    errorFreeBuffer[errorFreeBaseIndex + 1] = sampleAsByteArray[1];
                    errorFreeBuffer[errorFreeBaseIndex + 2] = sampleAsByteArray[2];
                    errorFreeReaded += 3;
                }

                if (discardedBytes == 0)
                    i += 3;
                else
                {
                    totalDiscardedBtes += discardedBytes;
                    i += discardedBytes;
                    acumDiscardedBytes += discardedBytes;
                    ReadExtraBytes(discardedBytes);
                }
            }

            return errorFreeBuffer;
        }

        protected void AddFreeErrorSamples()
        {
            if (AvailableBytes >= _playingLength)
            {
                readedAux = ReadFromPort(bufferAux, 0, _playingLength);
                byte[] errorFreeBuffer = ControlAlgorithm();

                ReceivedBytes.AddRange(errorFreeBuffer);
                if (ReceivedBytes.Count > _playingLength * _prebufferingSize)
                {
                    if(_prebuffering)
                    {
                        _prebuffering = false;
                        for (int i = 0; i < _prebufferingSize - 1; i++)
                        {
                            AddSamplesToPlayer();
                        }
                        this.Play();
                    }
                    AddSamplesToPlayer();
                }
            }
        }

        public void ClearAndClose()
        {
            _waveOut.Dispose();
            ReceivedBytes.Clear();            
            Close();
        }

        public int GetReceivedBytes()
        {
            return this.ReceivedBytes.Count;
        }

        public int GetBytesDepth()
        {
            return _bytesDepth;
        }
        #endregion

        #region Abstract Methods and Properties
        protected abstract void ReadExtraBytes(int size);
        public abstract void Receive();
        public abstract void Close();
        protected abstract int ReadFromPort(byte[] buffer, int offset, int count);
        protected abstract int AvailableBytes { get; }
        public abstract string PortName { get; }
        #endregion
    }
}
