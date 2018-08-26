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

        protected UdpClient UdpClientReceiver
        {
            get => (UdpClient)ClientReceiver;
        }

        public override string PortName => "UDP";
        #endregion

        public UdpReceiver()
        {
            this.TransmissionMethod = TransmissionMethod.Udp;
            this.ClientReceiver = new UdpClient();
            if (CanConnect())
            {
                this.Connected = true;
                this.ClientReceiver = new UdpClient(new IPEndPoint(IPAddress.Parse(ipAddress), 80));
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
            await Task.Run(async () =>
            {
                while (UdpClientReceiver.Client.Connected)
                {                    
                    if (AvailableBytes == 0)
                        continue;

                    var receivedResults = await UdpClientReceiver.ReceiveAsync();
                    byte[] buffer = receivedResults.Buffer;

                    byte[] errorFreeBuffer = ControlAlgorithm();
                    ReceivedBytes.AddRange(errorFreeBuffer.Take(errorFreeReaded).ToList());    // Con checkeo de errores
                                                                                                //_receivedBytes.AddRange(bufferAux.Take(readedAux).ToList());              // Sin checkeo de errores                  
                    if (ReceivedBytes.Count < _playingLength * 4)
                        continue;

                    AddSamples();
                }
            });
        }

        protected override void ReadExtraBytes(int size)
        {
            throw new System.NotImplementedException();
        }
    }
}
