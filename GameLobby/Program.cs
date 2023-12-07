namespace GameLobby
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Settings.Settings.LoadSettings();
            TCP_Server Server = new();

            Server.StartServer();
        }
    }
}
