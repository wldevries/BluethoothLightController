using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace ArduinoBluetoothChat
{
    public class Bluetooth : IDisposable
    {
        private StreamSocket socket;
        private DataReader dataReader;
        private DataWriter dataWriter;

        private string readBuffer;
        private RfcommDeviceService device;

        public string DeviceHostName { get; private set; } = "HC-06";

        public async Task Find()
        {
            var deviceSelectors = await DeviceInformation.FindAllAsync(
                RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            foreach (var selector in deviceSelectors)
            {
                this.device = await RfcommDeviceService.FromIdAsync(selector.Id);
            }
        }

        public async Task ConnectAsync()
        {
            this.socket = new StreamSocket();
            await socket.ConnectAsync(
                device.ConnectionHostName,
                device.ConnectionServiceName);

            this.dataReader = new DataReader(socket.InputStream);
            this.dataReader.InputStreamOptions = InputStreamOptions.Partial;

            this.dataWriter = new DataWriter(socket.OutputStream);
        }

        public async Task WriteLine(string value)
        {
            this.dataWriter.WriteString(value);
            this.dataWriter.WriteString(Environment.NewLine);
            await this.dataWriter.StoreAsync();
        }

        public async Task<string> ReadLine(CancellationToken cancelToken)
        {
            const uint ReadBufferLength = 1024;
            while (!cancelToken.IsCancellationRequested)
            {
                var bytesRead = await this.dataReader.LoadAsync(ReadBufferLength);
                if (bytesRead > 0)
                {
                    var received = this.dataReader.ReadString(bytesRead);
                    this.readBuffer += received;

                    var lines = this.readBuffer.Split(new[] { "\r", "\n", "\r\n" }, StringSplitOptions.None);
                    if (lines.Length > 1)
                    {
                        this.readBuffer = string.Join("\n", lines.Skip(1));
                        return lines[0];
                    }
                }
            }

            return null;      
        }

        public void Dispose()
        {
            this.socket.Dispose();
            this.dataReader.Dispose();
            this.dataWriter.Dispose();
            this.device.Dispose();
        }
    }
}
