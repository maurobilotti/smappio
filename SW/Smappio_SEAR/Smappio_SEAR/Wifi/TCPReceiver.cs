using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Smappio_SEAR.Wifi
{
    public class TcpReceiver : WifiReceiver
    {
        protected Thread _threadReceive;
        protected NetworkStream netStream;

        #region Properties
        protected override int AvailableBytes => TcpClientReceiver.Client.Available;

        protected TcpClient TcpClientReceiver
        {
            get => (TcpClient)ClientReceiver;
        }

        public override string PortName => "TCP";
        #endregion

        public TcpReceiver()
        {
            this.TransmissionMethod = TransmissionMethod.Tcp;
            this.ClientReceiver = new TcpClient();
            if (CanConnect())
            {
                this.Connected = true;
                this.ClientReceiver = new TcpClient(IpAddress, Port);
            }
            
        }

        public override void Close()
        {
            if (Connected)
            {
                TcpClientReceiver.Close();
                ClientReceiver.Dispose();
            }
        }

        public override void Receive()
        {          
            if(Connected)
            {
                TcpClientReceiver.Client.NoDelay = false;
                _threadReceive = new Thread(ReceiveData);

                _threadReceive.Start();
            }                      
        }

        protected override void ReadExtraBytes(int size)
        {
            while (AvailableBytes < size)
            {
                // do nothing
            }
            readedAux += netStream.Read(bufferAux, readedAux, size);
        }

        private void ReceiveData()
        {
            netStream = TcpClientReceiver.GetStream();

            while (TcpClientReceiver.Connected)
            {
                if (netStream.CanRead)
                {
                    AddFreeErrorSamples();
                }
            }
        }

        protected override int ReadFromPort(byte[] buffer, int offset, int count)
        {
            return netStream.Read(bufferAux, 0, _playingLength);
        }
    }
}
