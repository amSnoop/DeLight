using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.Management;

namespace DeLight.Utilities.LightingOutput
{
    public static class LightingController
    {
        private static readonly int VID = 0x1069;
        private static readonly int PID = 0x1040;

        private static UsbDevice? UsbDevice;
        private static readonly System.Timers.Timer SendDataTimer = new(GlobalSettings.TickRate);

        public static byte[] LastSentData { get; private set; } = new byte[512];


        public static void SendData(byte?[] data)
        {
            if (UsbDevice == null)
            {
                Console.WriteLine("No USB Device connected");
                return;
            }
            var dataToSend = new byte[512];
            if (data.Length != 512)
            {
                throw new ArgumentException("DMX Data must be 512 bytes long");
            }
            for (int i = 0; i < 512; i++)
            {
                dataToSend[i] = data[i] == null ? LastSentData[i] : data[i]!.Value;
            }
            LastSentData = dataToSend;//The next time the timer elapses, this will be sent.
        }

        private static void SendData()
        {
            if(UsbDevice == null)
            {
                return;
            }
            var writer = UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
            ErrorCode e = writer.Write(LastSentData, 1000, out int _);//idc how many bytes were actually written
            if (e != ErrorCode.Success)
            {
                Console.WriteLine("Failed to communicate with USB Device: " + e);
            }
        }
        //Attempts to connect to the first available Lighting Controller. null if no device is found.
        private static void AttemptConnection()
        {
            UsbDevice = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(VID, PID));
        }

        private static void StartMonitoringDeviceChanges()
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
            using var watcher = new ManagementEventWatcher(query);

            watcher.EventArrived += (sender, args) =>
            {
                AttemptConnection();
            };
            watcher.Start();
        }

        public static void Start()
        {
            StartMonitoringDeviceChanges();
            SendDataTimer.Start();
            SendDataTimer.Elapsed += (s, e) =>
            {
                SendData();
            };
        }
    }
}
