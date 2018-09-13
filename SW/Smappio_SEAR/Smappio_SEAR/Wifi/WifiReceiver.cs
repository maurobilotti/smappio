using NAudio.Gui;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Smappio_SEAR.Wifi
{
    public abstract class WifiReceiver : Receiver
    {
        public WifiReceiver()
        {

        }
        public WifiReceiver(ref WaveformPainter wavePainter) : base(ref wavePainter)
        {
        }
        protected IDisposable ClientReceiver { get; set; }
        protected string IpAddress = "192.168.1.2";
        protected int Port = 80;

        public bool CanConnect()
        {
            Ping x = new Ping();
            PingReply reply = x.Send(IPAddress.Parse(IpAddress));
            return reply.Status != IPStatus.TimedOut && reply.Status != IPStatus.DestinationHostUnreachable;
        }
    }
}
