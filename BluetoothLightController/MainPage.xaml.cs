using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ArduinoBluetoothChat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Bluetooth bluetooth;
        private CancellationTokenSource cancellationSource;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
            this.colorMap.ColorChanged += ColorMap_ColorChanged;
            this.sendText.KeyDown += SendText_KeyDown;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.cancellationSource.Cancel();
            this.cancellationSource = null;
            this.bluetooth?.Dispose();
        }

        private async void SendText_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var text = this.sendText.Text;
                this.sendText.Text = "";
                this.sentText.Text = this.sentText.Text + text + Environment.NewLine;
                await this.bluetooth.WriteLine(text);
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        { 
            await StartBluetooth();
        }

        private async void ColorMap_ColorChanged(Windows.UI.Color obj)
        {
            this.colorValue.Text = $"Color: R {(int)obj.R} G {(int)obj.G} B {(int)obj.B}";
            if (this.bluetooth != null)
            {
                await SendColor("R", obj.R);
                await SendColor("G", obj.G);
                await SendColor("B", obj.B);
            }
        }

        private async Task SendColor(string name, byte r)
        {
            var msg = $"{name} {(int)r}";
            this.sentText.Text += Environment.NewLine + msg;
            await this.bluetooth.WriteLine(msg);
        }

        private async Task StartBluetooth()
        {
            this.sendText.IsEnabled = false;
            this.status.Text = "Connecting..";

            try
            {
                this.bluetooth = new Bluetooth();
                await bluetooth.Find();
                await bluetooth.ConnectAsync();
            }
            catch (Exception)
            {
                this.status.Text = "Could not connect";
                this.bluetooth?.Dispose();
                this.bluetooth = null;
                return;
            }

            this.sendText.IsEnabled = true;
            this.status.Text = "Connected";

            this.cancellationSource = new CancellationTokenSource();
            while (!this.cancellationSource.IsCancellationRequested)
            {
                var line = await this.bluetooth.ReadLine(cancellationSource.Token);
                this.receiveText.Text += line + Environment.NewLine;
            }
        }
    }
}
