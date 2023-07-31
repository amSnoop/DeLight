//using System;
//using LibUsbDotNet;
//using LibUsbDotNet.Main;


//namespace DeLightWPF
//{
//    public class UsbHandler
//    {
//        public static UsbDeviceFinder MyUsbFinder { get; private set; } = new UsbDeviceFinder(1234, 5678); // replace with your VID and PID

//        public static void Main2(string[] args)
//        {
//            // Find and open the USB device.
//            UsbDevice usbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
//            if (!usbDevice.IsOpen)
//            {
//                Console.WriteLine("Device not found.");
//                return;
//            }

//            // Use the device.
//            IUsbDevice? wholeUsbDevice = usbDevice as IUsbDevice;
//            if (wholeUsbDevice is not null)
//            {
//                // Select config #1
//                wholeUsbDevice.SetConfiguration(1);

//                // Claim interface #0.
//                wholeUsbDevice.ClaimInterface(0);
//            }

//            // Create the writer and reader using the correct endpoint.
//            UsbEndpointWriter writer = usbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
//            UsbEndpointReader reader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep02);

//            // Write data
//            byte[] writeBuffer = new byte[1024];
//            ErrorCode ec = writer.Write(writeBuffer, 2000, out int _);
//            if (ec != ErrorCode.None) {
//                Console.WriteLine("Error writing data: {0}", ec);
//            }

//            // Read data
//            byte[] readBuffer = new byte[1024];
//            ec = reader.Read(readBuffer, 2000, out int _);
//            if (ec != ErrorCode.None) {
//                Console.WriteLine("Error reading data: {0}", ec);
//            }

//            // Close the device.
//            usbDevice.Close();
//        }
//    }
//}
