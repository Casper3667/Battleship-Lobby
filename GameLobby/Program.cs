namespace GameLobby
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TCP_Server Server = new();

            Server.StartServer();
        }
    }
}
