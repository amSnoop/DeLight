using System;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace DeLight.Utilities.LightingOutput
{
    public class LightingController
    {
        public static LightingController? Instance { get; set; }
        public static readonly string VID = "1069";
        public static readonly string PID = "1040";

        public readonly string ComPort;

        public readonly int BaudRate = 9600;

        public byte[] LastSentData = new byte[512];

        private LightingController(string comPort)
        {
            ComPort = comPort;
        }
        public void SendData(byte?[] data)
        {
            var dataToSend = new byte[512];
            if (data.Length != 512)
            {
                throw new ArgumentException("DMX Data must be 512 bytes long");
            }
            for(int i = 0; i < 512; i++)
            {
                if (data[i] == null)
                {
                    dataToSend[i] = LastSentData[i];
                }
                else
                {
                    dataToSend[i] = data[i]!.Value;
                }
            }
            
            using var serialPort = new SerialPort(ComPort, BaudRate);
            serialPort.Open();
            serialPort.Write(dataToSend, 0, 512);
            serialPort.Close();
            LastSentData = dataToSend;
        }
        //Attempts to connect to the first available Lighting Controller
        public static void Connect()
        {
            

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE PNPDeviceID LIKE '%VID_" + VID + "&PID_" + PID + "%'");
            foreach (ManagementObject device in searcher.Get().Cast<ManagementObject>())
            {
                var caption = device["Caption"].ToString();
                if (!string.IsNullOrEmpty(caption) && caption.Contains("COM"))
                {
                    int startIndex = caption.IndexOf("COM");
                    int endIndex = caption.IndexOf(")", startIndex);

                    // If we don't find the closing parenthesis, take until the end of the string
                    if (endIndex == -1)
                    {
                        endIndex = caption.Length;
                    }

                    var comPort = caption[startIndex..endIndex];
                    if(Instance != null && Instance.ComPort == comPort)
                    {
                        return;
                    }
                    Instance = new LightingController(comPort);
                    return;
                }
            }
            Instance = null;
            return;
        }

        public static void MonitorDeviceChanges()
        {
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
            using var watcher = new ManagementEventWatcher(query);

            watcher.EventArrived += (sender, args) =>
            {
                Connect();
            };
            watcher.Start();
        }

    }
}
