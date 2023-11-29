using k8s;
using k8s.Models;
using System.Net.Http.Json;

namespace GameLobby.Kube
{
    public class FindServers
    {
        public Kubernetes _client;
        public readonly string _namespaceName;
        public readonly string _labelSelector;
        public readonly int maxPlayer = 2;
        public static readonly object lockObject = new();
        public Dictionary<string, string> labels = new()
        {
                {"app", "your-game-app"},
                {"env", "production"},
                {"gameserverhost", "gameserverhost"}
            };
        public List<ServerIP>? emptyPods;
        public List<ServerIP>? totalPods;
        public KubernetesClientConfiguration config =
            KubernetesClientConfiguration.BuildDefaultConfig();

        public FindServers(Kubernetes client, string namespaceName = "default",
            string labelSelector = "gameserverhost")
        {
            _client = client;
            _namespaceName = namespaceName;
            _labelSelector = labelSelector;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// // !!DO NOT USE!! This purely exists to satisfy test mock issues. 
        /// </summary>
        protected FindServers()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Fetches a game IP for the client. This should be the entry for all attempts to get one.
        /// </summary>
        /// <returns>Game IP</returns>
        public string locateIP()
        {
            // Ensures only one thread is finding a server at once, to prevent issues.
            // Inefficient if there is a lot of threads/clients.
            lock (lockObject)
            {
                Task<string> query = SortServers();
                query.GetAwaiter().GetResult();
                string? status = query.Result;
                return status;
            }

        }

        /// <summary>
        /// Sorts the servers and finds a valid gameserver IP
        /// </summary>
        /// <returns>GameServer IP</returns>
        public virtual async Task<string> SortServers()
        {
            // Get a list of pods
            V1PodList query = await ListPodsAsync();

            // Reset the lists
            emptyPods = new();
            totalPods = new();

            foreach (V1Pod pod in query.Items)
            {
                string podIP = pod.Status.PodIP;
                int connectedClients = await GetConnectedClientsCount(podIP);

                // We primarily want not-full pods.
                if (connectedClients >= 0 && connectedClients < maxPlayer)
                {
                    emptyPods.Add(new ServerIP(connectedClients, podIP));
                }
                // Primarily future-proofing in case we want all pods for one reason or another.
                totalPods.Add(new ServerIP(connectedClients, podIP));
            }
            // If there are no empty pods, we start a new one for the client.
            if (emptyPods.Count == 0)
                await ScalePods();

            // Get pods with potential players in them first.
            List<ServerIP> sortedList;
            sortedList = emptyPods.OrderByDescending(pod => pod.ActivePlayers).ToList();

            string GameServerIP = sortedList.First().IP;

            return GameServerIP;
        }

        /// <summary>
        /// Start a new pod.
        /// </summary>
        public async Task ScalePods()
        {
            V1Pod newPod = new()
            {
                ApiVersion = "v1",
                Kind = "Pod",
                Metadata = new V1ObjectMeta()
                {
                    Name = "new-pod",
                    Labels = labels
                },
                Spec = new V1PodSpec()
                {
                    Containers = new[]
                    {
                        new V1Container()
                        {
                            Name = "gameserver-container",
                            Image = "gameserver:latest",
                            Ports = new[] { new V1ContainerPort() { ContainerPort = 80 } }
                        }
                    }
                }
            };
            V1Pod createdPod = await _client.CreateNamespacedPodAsync(newPod, "default");

            Console.WriteLine($"Pod {createdPod.Metadata.Name} created.");

            if (createdPod.Status.PodIP != null && emptyPods != null)
                emptyPods.Add(new ServerIP(0, createdPod.Status.PodIP));
            else
                throw new Exception("The new pod IP is null or it attempted to add the new pod to a null list.");
        }

        /// <summary>
        /// Gets a list of all pods with the marked label.
        /// </summary>
        public virtual async Task<V1PodList> ListPodsAsync()
        {
            return await _client.ListNamespacedPodAsync(_namespaceName, labelSelector: _labelSelector);
        }

        /// <summary>
        /// Fetches the number of clients on a given pod
        /// </summary>
        /// <param name="podIP">IP of the pod to fetch client number from</param>
        /// <returns>The number of clients connected</returns>
        public virtual async Task<int> GetConnectedClientsCount(string podIP)
        {
            try
            {
                using HttpClient httpClient = new();
                HttpResponseMessage response = await httpClient.GetAsync($"http://{podIP}/status");

                if (response.IsSuccessStatusCode)
                {
                    int status = await response.Content.ReadFromJsonAsync<int>();
                    return status;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying game server status: {ex.Message}");
            }
            return -1;
        }

        /// <summary>
        /// A combination of active players on a given IP address, and the address.
        /// </summary>
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
    }
}
