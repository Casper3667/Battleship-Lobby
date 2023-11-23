using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using System.Net.Http;
using System.Net.Http.Json;

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

            string GameServerIP;

            // GameServer's namespace
            var namespaceName = "default";

            List<ServerIP> podListEven = new();
            List<ServerIP> podListUneven = new();
            var labelSelector = "game-server-pod";
            var pods = client.ListNamespacedPod(namespaceName, labelSelector: labelSelector);
            foreach(var pod in pods.Items)
            {
                var podIP = pod.Status.PodIP;

                var query = QueryGameServerStatus(podIP);
                query.GetAwaiter().GetResult();
                GameServerStatus? status = query.Result;
                if(status != null)
                {
                    if (IsOdd(status.ActivePlayers))
                        podListUneven.Add(new(status.ActivePlayers, pod.Status.PodIP));
                    else
                        podListEven.Add(new(status.ActivePlayers, pod.Status.PodIP));
                }
            }
            List<ServerIP> sortedList;
            if(podListUneven.Count > 0)
                sortedList = podListUneven.OrderBy(pod => pod.ActivePlayers).ToList();
            else
                sortedList = podListEven.OrderBy(pod => pod.ActivePlayers).ToList();

            GameServerIP = sortedList.First().IP;

            Console.WriteLine($"GameServer IP: {GameServerIP}");

            return GameServerIP;
        }

        private static bool IsOdd (int value)
        {
            return value % 2 != 0;
        }

        public class ServerIP
        {
            public int ActivePlayers { get; set; }
            public string IP { get; set; }

            public ServerIP(int players, string ip)
            {
                ActivePlayers = players;
                IP = ip;
            }
        }

        static async Task<GameServerStatus?> QueryGameServerStatus(string IP)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync($"http://{IP}/status");

                    if (response.IsSuccessStatusCode)
                    {
                        var status = await response.Content.ReadFromJsonAsync<GameServerStatus>();
                        return status;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying game server status: {ex.Message}");
            }
            return null;
        }

        public class GameServerStatus
        {
            public int ActivePlayers { get; set; }
        }

        static private Kubernetes MakeKubernetesClient()
        {
            // Load Kubernetes configuration from the default location or a specified file
            var config = KubernetesClientConfiguration.BuildDefaultConfig();

            var client = new Kubernetes(config);

            return client;
        }
    }
}
