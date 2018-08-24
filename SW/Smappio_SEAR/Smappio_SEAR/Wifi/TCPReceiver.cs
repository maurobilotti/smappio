using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace Smappio_SEAR.Wifi
{
    public class TcpReceiver : WifiReceiver
    {
        protected Thread _threadReceive;
        public TcpReceiver()
        {
            this.TransmissionMethod = TransmissionMethod.Tcp;
            this.ClientReceiver = new TcpClient();
            if (CanConnect())
            {
                this.Connected = true;
                this.ClientReceiver = new TcpClient(ipAddress, port);
            }
            
        }

        public override void Close()
        {
            if (Connected)
            {
                ((TcpClient)ClientReceiver).Close();
                ClientReceiver.Dispose();
            }
        }

        public override void Receive()
        {          
            if(Connected)
            {
                ((TcpClient)ClientReceiver).Client.NoDelay = false;
                _threadReceive = new Thread(ReceiveData);

                _threadReceive.Start();
                Play();
            }                      
        }

        private void ReceiveData()
        {
            NetworkStream netStream = ((TcpClient)ClientReceiver).GetStream();

            bufferAux = new byte[_playingLength];

            while (((TcpClient)ClientReceiver).Connected)
            {
                if (netStream.CanRead)
                {
                    if (((TcpClient)ClientReceiver).Client.Available < _playingLength)
                        continue;

                    readedAux = netStream.Read(bufferAux, 0, _playingLength);
                    byte[] errorFreeBuffer = ControlAlgorithm();
                    ReceivedBytes.AddRange(errorFreeBuffer.Take(errorFreeReaded).ToList());    // Con checkeo de errores
                    //_receivedBytes.AddRange(bufferAux.Take(readedAux).ToList());              // Sin checkeo de errores                    

                    if (ReceivedBytes.Count < _playingLength * 4)
                        continue;

                    AddSamples();
                }
            }
        }
    }
}
