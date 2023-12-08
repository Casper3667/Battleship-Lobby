using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameLobby
{


    public static class BackupFindServer
    {
        //static Mutex getServerMutex = new Mutex();

        public static readonly object lockObject = new();
        
        public static string? GetServerAdress()
        {
            lock (lockObject)
            {
                Thread.Sleep(1000); // TO Give THe Last one in here time to connect to GameServer
                Task<string?> query = FindServer();
                query.GetAwaiter().GetResult();
                string? status = query.Result;
                return status;
            }
        }
        /// <summary>
        /// Either you get a Port or You get Nothing
        /// </summary>
        /// <returns></returns>
        private static async Task<string?> FindServer()
        {
            var settings = Settings.Settings.AppSettings;
            string IP = settings.BackupIP + ":" + settings.BackupHealthcheckPort;
            int count=await GetConnectedClientsCount(IP);
            string? result;
            if(count<2)
            {
                var adress = new BackupServerAdress(settings.BackupIP, settings.BackupPort);
                 result=JsonSerializer.Serialize(adress);
               

            }
            else
            {
                result= null;
            }

            return result;
        }
        /// <summary>
        /// Fetches the number of clients on a given pod
        /// </summary>
        /// <param name="podIP">IP of the pod to fetch client number from</param>
        /// <returns>The number of clients connected</returns>
        public static async Task<int> GetConnectedClientsCount(string podIP)
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

    }

    public class BackupServerAdress
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public BackupServerAdress(string ip, int port)
        {
            IP = ip;
            Port = port;
        }
    }

   
}
