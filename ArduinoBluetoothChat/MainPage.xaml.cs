using System;
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

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;

            this.sendText.KeyDown += SendText_KeyDown;
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
            this.colorMap.ColorChanged += ColorMap_ColorChanged;
            await StartBluetooth();
        }

        private async void ColorMap_ColorChanged(Windows.UI.Color obj)
        {
            await SendColor("R", obj.R);
            await SendColor("G", obj.G);
            await SendColor("B", obj.B);
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

            this.bluetooth = new Bluetooth();
            await bluetooth.Find();
            await bluetooth.ConnectAsync();

            this.sendText.IsEnabled = true;
            this.status.Text = "Connected";

            while (true)
            {
                var line = await this.bluetooth.ReadLine();
                this.receiveText.Text += line + Environment.NewLine;
            }
        }
    }
}
