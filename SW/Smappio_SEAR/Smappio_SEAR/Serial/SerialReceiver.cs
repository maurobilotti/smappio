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

        #region Properties
        protected override int AvailableBytes => _serialPort.BytesToRead;

        public override string PortName => "Serial";
        #endregion

        public SerialReceiver(ref SerialPort serialPort)
        {
            _serialPort = serialPort;
            _serialPort.PortName = "COM13";//BluetoothHelper.GetBluetoothPort("Silicon Labs CP210x USB to UART Bridge");
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
            Notify();
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

        public override void Receive()
        {
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        protected override void ReadExtraBytes(int size)
        {
            while (AvailableBytes < size)
            {
                // do nothing
            }
            readedAux += _serialPort.Read(bufferAux, readedAux, size);
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (AvailableBytes >= _playingLength)
            {
                readedAux = _serialPort.Read(bufferAux, 0, _playingLength);
                byte[] errorFreeBuffer = ControlAlgorithm();

                ReceivedBytes.AddRange(errorFreeBuffer.Take(errorFreeReaded).ToList());

                if (ReceivedBytes.Count < _playingLength * 4)
                    return;

                AddSamples();
            }
        }
    }
}
