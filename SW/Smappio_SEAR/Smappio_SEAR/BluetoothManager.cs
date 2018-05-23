using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace Smappio_SEAR
{
    public class BluetoothManager
    {
        private BluetoothClient _bluetooth;
        TextBox _txt;

        public BluetoothManager(string deviceName = null)
        {
            _bluetooth = new BluetoothClient();
        }        

        public void SearchDevice(string name)
        {
            try
            {                
                var btDevice = _bluetooth.DiscoverDevices()
                                               .Where(d => d.DeviceName == name).FirstOrDefault();
                if (btDevice != null)
                {
                    //_bluetooth.SetPin("1234");
                    _bluetooth.Connect(btDevice.DeviceAddress, BluetoothService.SerialPort);                                       
                }               
            }
            catch (Exception ex)
            {
            }
        }        

        public int GetBaudRate()
        {
            int baudRate = 115200;
            return baudRate;
        }
    }
}
