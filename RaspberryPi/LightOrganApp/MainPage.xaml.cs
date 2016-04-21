using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LightOrganApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LightsController _controller;
        private SocketServer _socketServer;

        private bool _isIoT;

        public MainPage()
        {
            this.InitializeComponent();

            _isIoT = IsIoT();
        }

        private bool IsIoT()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT";
        }        

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_isIoT)
            {
                _controller = new LightsController();
                await _controller.InitAsync();

                _socketServer = new SocketServer();
                _socketServer.StartListener();
                _socketServer.NewMessageReady += SendCommand;
            }           
        }        

        private void SendCommand(object sender, MessageSentEventArgs e)
        {
            var message = e.Message;

            if (!string.IsNullOrEmpty(message))
            {
                var parsedCommand = e.Message.Split('|');
                var bassValue = Convert.ToDouble(parsedCommand[0], CultureInfo.InvariantCulture.NumberFormat);
                var midValue = Convert.ToDouble(parsedCommand[1], CultureInfo.InvariantCulture.NumberFormat);
                var trebleValue = Convert.ToDouble(parsedCommand[2], CultureInfo.InvariantCulture.NumberFormat);

                _controller.SetValues(bassValue, midValue, trebleValue);
            }           
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_isIoT)
            {
                if (_socketServer != null)
                {
                    _socketServer.NewMessageReady -= SendCommand;
                    _socketServer.Dispose();
                    _socketServer = null;
                }

                if (_controller != null)
                {
                    _controller.Dispose();
                    _controller = null;
                }
            }

            base.OnNavigatingFrom(e);
        }
    }
}
