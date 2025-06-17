using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CGEOnlineTools
{
    /// <summary>
    /// information send from Client to Server. Expand as needed
    /// </summary>
    public class ClientInfo
    {
        public string Username { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public DateTime LastUpdate { get; set; }
    }


    public class CGEServer
    {
        private readonly int port;
        private UdpClient udpServer;
        private Thread receiveThread;
        private Thread broadcastThread;
        private Thread cleanupThread;
        private bool isRunning;
        private readonly Dictionary<string, ClientInfo> clients = new Dictionary<string, ClientInfo>();

        private readonly TimeSpan clientTimeout = TimeSpan.FromSeconds(5);

        public CGEServer(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            if (isRunning) return;

            udpServer = new UdpClient(port);
            isRunning = true;

            receiveThread = new Thread(ReceiveLoop);
            broadcastThread = new Thread(BroadcastLoop);
            cleanupThread = new Thread(CleanupLoop);

            receiveThread.Start();
            broadcastThread.Start();
            cleanupThread.Start();

            Console.WriteLine($"Server started on port {port}.");
        }

        public void Stop()
        {
            isRunning = false;
            udpServer?.Close();
            receiveThread?.Join();
            broadcastThread?.Join();
            cleanupThread?.Join();

            Console.WriteLine("Server stopped.");
        }

        private void ReceiveLoop()
        {
            while (isRunning)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpServer.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);

                    HandleMessage(message, remoteEP);
                }
                catch (SocketException)
                {
                    // Likely caused by udpServer.Close()
                    if (isRunning)
                        Console.WriteLine("Socket error during receive.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                }
            }
        }

        private void HandleMessage(string message, IPEndPoint sender)
        {
            string[] parts = message.Split(';');
            if (parts.Length == 3)
            {
                string username = parts[0];
                if (float.TryParse(parts[1], out float x) && float.TryParse(parts[2], out float y))
                {
                    lock (clients)
                    {
                        if (clients.ContainsKey(username))
                        {
                            clients[username].X = x;
                            clients[username].Y = y;
                            clients[username].EndPoint = sender;
                            clients[username].LastUpdate = DateTime.UtcNow;
                        }
                        else
                        {
                            clients[username] = new ClientInfo
                            {
                                Username = username,
                                X = x,
                                Y = y,
                                EndPoint = sender,
                                LastUpdate = DateTime.UtcNow
                            };
                        }
                    }

                    Console.WriteLine($"Updated {username}: X={x}, Y={y}");
                }
            }
            else
            {
                Console.WriteLine($"Invalid message from {sender}: {message}");
            }
        }

        private void BroadcastLoop()
        {
            while (isRunning)
            {
                try
                {
                    Thread.Sleep(200);

                    string state = BuildStateMessage();
                    byte[] data = Encoding.UTF8.GetBytes(state);

                    lock (clients)
                    {
                        foreach (var client in clients.Values)
                        {
                            udpServer.Send(data, data.Length, client.EndPoint);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Broadcast error: {ex.Message}");
                }
            }
        }

        private void CleanupLoop()
        {
            while (isRunning)
            {
                try
                {
                    Thread.Sleep(1000);

                    List<string> toRemove = new List<string>();

                    lock (clients)
                    {
                        foreach (var kvp in clients)
                        {
                            if (DateTime.UtcNow - kvp.Value.LastUpdate > clientTimeout)
                            {
                                toRemove.Add(kvp.Key);
                            }
                        }

                        foreach (var username in toRemove)
                        {
                            clients.Remove(username);
                            Console.WriteLine($"Removed client {username} due to timeout.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cleanup error: {ex.Message}");
                }
            }
        }

        private string BuildStateMessage()
        {
            StringBuilder sb = new StringBuilder();

            lock (clients)
            {
                foreach (var client in clients.Values)
                {
                    sb.AppendLine($"{client.Username};{client.X};{client.Y}");
                }
            }

            return sb.ToString();
        }
    }
}

