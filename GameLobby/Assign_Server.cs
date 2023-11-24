using k8s;
using k8s.Models;
using System.Net.Http.Json;

namespace GameLobby
{
    public class Assign_Server
    {
        private Kubernetes? client;
        public Assign_Server() { }
        public Assign_Server(Kubernetes new_client)
        {
            client = new_client;
        }

        public string Find_Server()
        {
            string IP = Kubernetes_Connection();
            return IP;
        }
        public string Kubernetes_Connection()
        {
            if (client == null)
                client = MakeKubernetesClient();

            string GameServerIP;

            // GameServer's namespace
            var namespaceName = "default";

            List<ServerIP> podListEven = new();
            List<ServerIP> podListUneven = new();
            string labelSelector = "game-server-pod";
            V1PodList pods = ListNamespacedPod(namespaceName, labelSelector: labelSelector);
            foreach (var pod in pods.Items)
            {
                string podIP = pod.Status.PodIP;

                GameServerStatus? status = GetStatus(podIP);
                if (status != null)
                {
                    if (IsOdd(status.ActivePlayers))
                        podListUneven.Add(new(status.ActivePlayers, pod.Status.PodIP));
                    else
                        podListEven.Add(new(status.ActivePlayers, pod.Status.PodIP));
                }
            }
            List<ServerIP> sortedList;
            if (podListUneven.Count > 0)
                sortedList = podListUneven.OrderBy(pod => pod.ActivePlayers).ToList();
            else
                sortedList = podListEven.OrderBy(pod => pod.ActivePlayers).ToList();

            GameServerIP = sortedList.First().IP;

            Console.WriteLine($"GameServer IP: {GameServerIP}");

            return GameServerIP;
        }

        private static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public virtual V1PodList ListNamespacedPod(string namespaceParameter, string labelSelector)
        {
            return client.ListNamespacedPod(namespaceParameter, labelSelector: labelSelector);
        }

        public virtual GameServerStatus? GetStatus(string podIP)
        {
            Task<GameServerStatus?> query = QueryGameServerStatus(podIP);
            query.GetAwaiter().GetResult();
            GameServerStatus? status = query.Result;

            return status;
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

        public async Task<GameServerStatus?> QueryGameServerStatus(string IP)
        {
            try
            {
                using var httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync($"http://{IP}/status");

                if (response.IsSuccessStatusCode)
                {
                    GameServerStatus? status = await response.Content.ReadFromJsonAsync<GameServerStatus>();
                    return status;
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

        public Kubernetes MakeKubernetesClient()
        {
            // Load Kubernetes configuration from the default location or a specified file
            KubernetesClientConfiguration config = KubernetesClientConfiguration.BuildDefaultConfig();

            Kubernetes client = new(config);

            return client;
        }
    }
}
