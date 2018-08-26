using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;

namespace Smappio_SEAR
{
    public abstract class Receiver
    {
        protected List<byte> ReceivedBytes { get; set; }
        private string _filePath = "../../AudioSamples/";
        private readonly int _sampleRate = 32000;// No modificar, pues modifica el audio escuchado.
        private readonly int _bytesDepth = 3;
        private readonly int _bitDepth = 24;
        private readonly BufferedWaveProvider _provider;
        private WaveOut _waveOut = new WaveOut();
        public bool Connected = false;
        public TransmissionMethod TransmissionMethod;

        protected int _playingLength = 3000;// 3 * 20400
        protected int _offset = 0;
        protected int errorFreeReaded = 0;
        protected int _amplitudeMultiplier = 63;
        public IList<int> bufferSeqNums = new List<int>();
        public int totalDiscardedBtes = 0;

        public Receiver()
        {
            this.ReceivedBytes = new List<byte>();
            this._provider = new BufferedWaveProvider(new WaveFormat(_sampleRate, 24, 1));
        }

        #region Public Methods
        public virtual void Play()
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                _waveOut.Init(_provider);
                _waveOut.Play();
            }
        }

        public virtual void SaveFile()
        {
            string absolutePath = Path.GetFullPath(_filePath);
            string fileName = $"{DateTime.Now.ToString("yyyy-MM-dd_hhmmss")}.pcm";
            string fullPath = Path.Combine(absolutePath, fileName);            
            File.WriteAllBytes(fullPath, ReceivedBytes.ToArray());
        }

        public void AddSamples()
        {
            var bufferForPlaying = ReceivedBytes.GetRange(_offset, errorFreeReaded).ToArray();
            _offset += errorFreeReaded;
            _provider.AddSamples(bufferForPlaying, 0, bufferForPlaying.Length);
        }

        public void ClearAndClose()
        {
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

        #region Abstract Methods
        public abstract byte[] ControlAlgorithm();
        public abstract void Receive();
        public abstract void Close(); 
        #endregion
    }
}
