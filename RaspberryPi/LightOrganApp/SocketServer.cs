using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace LightOrganApp
{
    public class SocketServer: IDisposable
    {
        private StreamSocketListener tcpListener;
        private const string port = "8181";

        public event EventHandler<MessageSentEventArgs> NewMessageReady;

        public async void StartListener()
        {
            tcpListener = new StreamSocketListener();
            tcpListener.ConnectionReceived += TcpListener_ConnectionReceived;
            await tcpListener.BindServiceNameAsync(port);
        }

        private async void TcpListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var streamSocket = args.Socket;
            var reader = new DataReader(streamSocket.InputStream);
            try
            {
                while (true)
                {
                    uint bytesLoaded = await reader.LoadAsync(3);
                    if (bytesLoaded != 3)
                    {
                        return;
                    }

                    var bytes = new byte[3];
                    reader.ReadBytes(bytes);

                    NewMessageReady?.Invoke(this, new MessageSentEventArgs { Bytes = bytes });
                }
            }
            catch
            {
                
            }
        }        

        public void Dispose()
        {
            if (tcpListener != null)
                tcpListener.Dispose();
        }
    }

    public class MessageSentEventArgs : EventArgs
    {        
        public byte[] Bytes;
    }

}
