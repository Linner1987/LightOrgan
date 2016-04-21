using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightOrganApp.Tests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StreamSocket _streamSocket;
        private static DataWriter _writer;

        public MainPage()
        {
            this.InitializeComponent();
        }        

        private async void Connect(string serverIP, string serverPort)
        {
            try
            {
                var hostName = new HostName(serverIP);
                _streamSocket = new StreamSocket();
                await _streamSocket.ConnectAsync(hostName, serverPort);
                _writer = new DataWriter(_streamSocket.OutputStream);

                var dialog = new MessageDialog("Connection OK");
                await dialog.ShowAsync();
            }
            catch
            {
                var dialog = new MessageDialog("Connection error");
                await dialog.ShowAsync();
            }
        }

        private async void SendMessage(string message)
        {
            _writer.WriteUInt32(_writer.MeasureString(message));
            _writer.WriteString(message);
            await _writer.StoreAsync();
        }

        private void connectBtn_Click(object sender, RoutedEventArgs e)
        {
            Connect(hostTxt.Text, "8181");
        }

        private void test1Btn_Click(object sender, RoutedEventArgs e)
        {
            var bassValue = Convert.ToDouble(bassTxt.Text, CultureInfo.InvariantCulture.NumberFormat);
            var midValue = Convert.ToDouble(midTxt.Text, CultureInfo.InvariantCulture.NumberFormat);
            var trebleValue = Convert.ToDouble(trebleTxt.Text, CultureInfo.InvariantCulture.NumberFormat);

            SendMessage($"{bassValue}|{midValue}|{trebleValue}");
        }

        private void test2Btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void test3Btn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
