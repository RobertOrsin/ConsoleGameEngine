using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CGEOnlineTools
{
    public class CGEClient
    {
        private readonly IPEndPoint serverEndPoint;
        private readonly UdpClient udpClient;
        private Thread sendThread;
        private Thread receiveThread;
        private bool isRunning;

        public ClientInfo clientInfo {  get; private set; }


        public event Action<string> OnServerMessage;  // Optional: raise events for received data

        public CGEClient(string username, string serverIp, int serverPort)
        {
            clientInfo = new ClientInfo();
            clientInfo.Username = username;
            this.serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            this.udpClient = new UdpClient();
        }

        public void SetClientInfos(ClientInfo clientInfo)
        {
            this.clientInfo = clientInfo;
        }

        public bool Start()
        {
            if (isRunning) return true;

            try
            {
                isRunning = true;
                sendThread = new Thread(SendLoop);
                receiveThread = new Thread(ReceiveLoop);

                sendThread.Start();
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool Stop()
        {
            isRunning = false;
            udpClient.Close();
            sendThread?.Join();
            receiveThread?.Join();

            return true;

        }

        private void SendLoop()
        {
            while (isRunning)
            {
                try
                {
                    string message = $"{clientInfo.Username};{clientInfo.X};{clientInfo.Y}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udpClient.Send(data, data.Length, serverEndPoint);

                    Thread.Sleep(50); // Send update every 500 ms
                }
                catch (Exception ex){}
            }
        }

        private void ReceiveLoop()
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    OnServerMessage?.Invoke(message);
                }
                catch (SocketException)
                {
                    if (isRunning)
                        Console.WriteLine("Socket closed unexpectedly.");
                }
                catch (Exception ex){}
            }
        }
    }
}
