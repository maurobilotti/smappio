﻿using NAudio.Gui;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Smappio_SEAR.Wifi
{
    public class TcpReceiver : WifiReceiver
    {
        protected Thread _threadReceive;
        protected NetworkStream netStream;

        #region Properties
        protected override int AvailableBytes => TcpClient.Client.Available;

        protected TcpClient TcpClient
        {
            get => (TcpClient)ClientReceiver;
        }

        public override string PortName => "TCP";
        #endregion

        public TcpReceiver(UIParams ui) : base(ui)
        {
            this.TransmissionMethod = TransmissionMethod.Tcp;
            this.ClientReceiver = new TcpClient();
            if (CanConnect())
            {
                try
                {
                    this.Connected = true;
                    this.ClientReceiver = new TcpClient(IpAddress, Port);
                }
                catch 
                {
                    return;
                }
            }            
        }

        public override void Close()
        {
            if (Connected)
            {
                TcpClient.Close();
                TcpClient.Dispose();
                ClientReceiver.Dispose();
                netStream.Close();
                netStream.Dispose();
            }
        }

        public override void Receive()
        {          
            if(Connected)
            {
                TcpClient.Client.NoDelay = true;
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
            netStream = TcpClient.GetStream();

            while (!TcpClient.Connected || !netStream.CanWrite)
            {
                // do nothing
            }
            byte[] myWriteBuffer = Encoding.ASCII.GetBytes("send");
            //byte[] myWriteBuffer = Encoding.ASCII.GetBytes("test");
            netStream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            while (TcpClient.Connected)
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
