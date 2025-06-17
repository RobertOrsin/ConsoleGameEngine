using System;
using CGEOnlineTools;

namespace UdtLikeServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            CGEServer server = new CGEServer(12345);
            server.Start();

            Console.WriteLine("Press ENTER to stop the server...");
            Console.ReadLine();

            server.Stop();
        }
    }
}