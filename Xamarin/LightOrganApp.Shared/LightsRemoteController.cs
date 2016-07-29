using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LightOrganApp.Shared
{
    public class LightsRemoteController
    {
        TcpClient tcpClient;
        NetworkStream writeStream;        

        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                tcpClient = new TcpClient();

                await tcpClient.ConnectAsync(host, port);
                writeStream = tcpClient.GetStream();
            }
            catch(Exception ex)
            {
                Debug.Write(ex.StackTrace);
            }           
        }

        public async Task SendCommandAsync(byte[] bytes)
        {
            try
            {
                if (writeStream != null)
                {
                    await writeStream.WriteAsync(bytes, 0, bytes.Length);
                    await writeStream.FlushAsync();
                }
            }
            catch(Exception ex)
            {
                Debug.Write(ex.StackTrace);
            }
        }        

        public async Task CloseAsync()
        {
            try
            {
                var bytes = new byte[3] { 13, 13, 13 };
                await SendCommandAsync(bytes);

                if (writeStream != null)
                    writeStream.Close();

                if (tcpClient != null)
                    tcpClient.Close();
            }
            catch(Exception ex)
            {
                Debug.Write(ex.StackTrace);
            }

            writeStream = null;
            tcpClient = null;
        }
    }
}
