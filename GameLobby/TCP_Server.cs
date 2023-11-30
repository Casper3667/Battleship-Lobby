using GameLobby.Kube;
using k8s;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Sockets;

namespace GameLobby
{
    internal class TCP_Server
    {
        private readonly TcpListener server;
        private readonly int IP_Port;
        //private readonly Assign_Server serv_Finder = new();
        private readonly FindServers serv_Finder;
        public TCP_Server(int Port = 12000)
        {
            IP_Port = Port;
            server = new(IPAddress.Any, IP_Port);

            // Load Kubernetes configuration from the default location or a specified file
            KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildDefaultConfig();

            Kubernetes client = new(config);

            serv_Finder = new(client, "default");
        }

        public void StartServer()
        {
            server.Start();
            Console.WriteLine($"Server started... listening on port {IP_Port}");
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Thread clientThread = new(() => HandleClient(client));
                clientThread.Start();
            }
        }

        public void HandleClient(TcpClient client)
        {
            Guid clientId = Guid.NewGuid();
            Console.WriteLine($"Client {clientId} connected.");
            // Send client name to the client
            StreamWriter writer = new(client.GetStream());
            writer.Flush();
            // Receive and send messages
            StreamReader reader = new(client.GetStream());

            string message = reader.ReadLine() ?? "";
            if (!message.IsNullOrEmpty())
            {
                bool valid = Validate_Token.Validate(message); // Ensure their JWT is valid
                if (!valid)
                {
                    writer.WriteLine("The authorization token was invalid.");
                }
                else
                {
                    writer.WriteLine("Success");
                    writer.Flush();
                    string Server = serv_Finder.locateIP(); // Fetch a server
                    writer.WriteLine(Server);
                }
            }
            else
            {
                writer.WriteLine("The authorization token was invalid.");
            }
            writer.Flush();
            client.Close(); // Disconnect the client once done
        }
    }
}
