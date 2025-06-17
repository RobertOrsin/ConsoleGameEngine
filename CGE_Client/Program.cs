using System;
using CGEOnlineTools;

namespace UdtLikeClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter your username: ");
            string username = Console.ReadLine();

            CGEClient client = new CGEClient(username, "127.0.0.1", 12345);

            client.OnServerMessage += (msg) => {
               // Handle server message here
            };

            client.Start();

            Console.WriteLine("Press ENTER to stop the client...");
            Console.ReadLine();

            client.Stop();
        }
    }
}