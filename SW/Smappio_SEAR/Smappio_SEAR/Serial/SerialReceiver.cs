using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smappio_SEAR.Serial
{
    public class SerialReceiver : Receiver
    {
        private SerialPort _serialPort;
        private float _baudRate = 2000000;
        public SerialReceiver(ref SerialPort serialPort)
        {
            _serialPort = serialPort;
            _serialPort.PortName = "COM4";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
            _serialPort.BaudRate = Convert.ToInt32(_baudRate);
            _serialPort.Handshake = Handshake.None;

            //ojo con estos flags! sin esto, no recibe!!
            _serialPort.DtrEnable = true;
            _serialPort.RtsEnable = true;

            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                Connected = true;
            }                
        }

        public override void Close()
        {
            if(Connected)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
        }

        public void Notify()
        {
            _serialPort.Write("s");
        }

        public override byte[] ControlAlgorithm()
        {
            throw new NotImplementedException();
        }        

        public override void Receive()
        {
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var bufferSize = _serialPort.BytesToRead;

            byte[] data = new byte[bufferSize];
            errorFreeReaded = _serialPort.Read(data, 0, bufferSize);

            ReceivedBytes.AddRange(data);

            bool releaseCondition = ReceivedBytes.Count >= _offset + _playingLength;

            if (ReceivedBytes.Count < _offset + _playingLength)
                return;

            AddSamples();
        }
    }
}
