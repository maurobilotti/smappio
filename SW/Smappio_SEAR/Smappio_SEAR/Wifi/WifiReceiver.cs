using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Smappio_SEAR.Wifi
{
    public abstract class WifiReceiver : Receiver
    {
        protected IDisposable ClientReceiver { get; set; }        
        protected string ipAddress = "192.168.1.2";
        protected int port = 80;

        public bool CanConnect()
        {
            Ping x = new Ping();
            PingReply reply = x.Send(IPAddress.Parse(ipAddress));
            return reply.Status != IPStatus.TimedOut && reply.Status != IPStatus.DestinationHostUnreachable;
        }
    }
}
