using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Smappio_SEAR.Wifi
{
    public class UdpReceiver : WifiReceiver
    {
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
                ((UdpClient)ClientReceiver).Close();
                ClientReceiver.Dispose();
            }
        }

        public async override void Receive()
        {
            await Task.Run(async () =>
            {
                while (((UdpClient)ClientReceiver).Client.Connected)
                {                    
                    if (((UdpClient)ClientReceiver).Available == 0)
                        continue;

                    var receivedResults = await ((UdpClient)ClientReceiver).ReceiveAsync();
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
    }
}
