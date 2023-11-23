using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace GameLobby
{
    internal class TCP_Server
    {
        TcpListener server;
        private int IP_Port;

        public TCP_Server(int Port = 12000)
        {
            IP_Port = Port;
            server = new(IPAddress.Any, IP_Port);
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
                    string Server = Assign_Server.Find_Server(); // Fetch a server
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
