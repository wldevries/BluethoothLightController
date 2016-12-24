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
                this.sentText.Text = text + Environment.NewLine + this.sentText.Text;
                await this.bluetooth.WriteLine(text);
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await StartBluetooth();
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
