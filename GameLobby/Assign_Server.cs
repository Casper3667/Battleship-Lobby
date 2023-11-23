using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace GameLobby
{
    static internal class Assign_Server
    {
        static Kubernetes? client;
        static internal string Find_Server()
        {
            string IP = Kubernetes_Connection();
            return IP;
        }
        static internal string Kubernetes_Connection()
        {
            if (client == null)
                client = MakeKubernetesClient();

            // Namespace where your GameServer service is deployed
            var namespaceName = "default";

            // Service name of the GameServer
            var gameServerServiceName = "gameserver-service";

            // Get the service
            var service = client.ReadNamespacedService(gameServerServiceName, namespaceName);

            // Extract GameServer IP
            var gameServerIP = service.Spec.ClusterIP;

            Console.WriteLine($"GameServer IP: {gameServerIP}");

            return gameServerIP;
        }

        static private Kubernetes MakeKubernetesClient()
        {
            // Load Kubernetes configuration from the default location or a specified file
            var config = KubernetesClientConfiguration.BuildDefaultConfig();

            // Create Kubernetes client
            var client = new Kubernetes(config);

            return client;
        }
    }
}
