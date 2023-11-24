namespace GameLobby
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TCP_Server Server = new();

            Server.StartServer();
        }
    }
}
