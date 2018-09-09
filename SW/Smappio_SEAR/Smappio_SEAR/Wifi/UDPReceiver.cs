using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Smappio_SEAR.Wifi
{
    public class UdpReceiver : WifiReceiver
    {
        #region Properties
        protected override int AvailableBytes => UdpClientReceiver.Client.Available;
        new readonly string ipAddress = "192.168.1.2";
        protected UdpClient UdpClientReceiver
        {
            get => (UdpClient)ClientReceiver;
        }

        public override string PortName => "UDP";
        #endregion
        public TcpClient TcpClient { get; set; }
        public int UdpListenPort = 1234;

        public UdpReceiver()
        {
            this.TransmissionMethod = TransmissionMethod.Udp;
            this.ClientReceiver = new UdpClient();
            if (CanConnect())
            {
                this.Connected = true;
                this.ClientReceiver = new UdpClient(UdpListenPort);
                this.TcpClient = new TcpClient(ipAddress, Port);
                //((UdpClient)this.ClientReceiver).Connect(IPAddress.Parse("192.168.1.15"), 1234);
            }
        }

        public override void Close()
        {
            if(Connected)
            {
                UdpClientReceiver.Close();
                ClientReceiver.Dispose();
            }
        }

        public async override void Receive()
        {
            while (TcpClient.Client.Connected)
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 1444);
                var result = UdpClientReceiver.Receive(ref ipEndPoint);                

                byte[] errorFreeBuffer = ControlAlgorithm();
                ReceivedBytes.AddRange(errorFreeBuffer.Take(errorFreeReaded).ToList());    // Con checkeo de errores
                                                                                           //_receivedBytes.AddRange(bufferAux.Take(readedAux).ToList());              // Sin checkeo de errores                  
                if (ReceivedBytes.Count < _playingLength * 4)
                    continue;
                // Maurito, a UDP no le di mucha bola en el refactor, pero creo que deberia quedar parecido a TCP, fijate como esta funcando llamando al metodo
                // AddFreeErrorSamples();
                AddSamplesToPlayer();
            }
        }

        private void ReceiveUdp(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }

        protected override void ReadExtraBytes(int size)
        {
            throw new System.NotImplementedException();
        }

        protected override int ReadFromPort(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}
