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
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        return;
                    }

                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    if (stringLength != actualStringLength)
                    {
                        return;
                    }

                    var message = reader.ReadString(actualStringLength);

                    NewMessageReady?.Invoke(this, new MessageSentEventArgs { Message = message });
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
        public string Message;
    }

}
