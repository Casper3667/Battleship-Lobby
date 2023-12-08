using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameLobby.Settings
{
    public static class Settings
    {
        public static bool IsSettingsLoaded { get; private set; } = false;
        
        public static AppSettings AppSettings { get; private set; }
        
        public static void LoadSettings()
        {
            Console.WriteLine("Game_Lobby: Load Settings Called");
            if(IsSettingsLoaded==false) {
                Console.WriteLine("Loading Game Lobby Settings");
                LoadAppSettings();
                IsSettingsLoaded = true;
                Console.WriteLine("Settings Are Loaded" +
                    "\nToken: " + AppSettings.Token);
            }
        }
        public static AppSettings LoadAppSettings()
        {
            //return new AppSettings() { Token= "Thesecretstomakeatokenkeyistodothis" };
            var settings = LoadJSON<AppSettings>("AppSettings.JSON");
            AppSettings = settings[0];
            return settings[0];
        
        }


        public static string GetPathToSettingsFile(string FileName)
        {
            var newFileName = "/Settings/" + FileName;
            

#if DEBUG
            newFileName = "Settings\\" + FileName;
            //Testing.Print("Is In Debug Mode");
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //Testing.Print("Path: " + System.IO.Path.GetDirectoryName(path));

            string Identifier = "\\GameLobby\\GameLobby\\";
            string TestIdentifier = "\\GameLobby\\LobbyTest\\"; // Indicates that it is a test and splits where the Test Begins to get the part of the Path that should vary form computer to Computer
            string forginPath = "";
            if (path.Contains(Identifier))
            {
                forginPath = path.Split(Identifier)[0];
            }
            else if (path.Contains(TestIdentifier))
            {
                forginPath = path.Split(TestIdentifier)[0];
            }

           // Testing.Print("forgin Path: " + forginPath);

            string newPath = forginPath + Identifier;
           // Testing.Print("new Path: " + newPath);
            newFileName = newPath /*+ "Settings\\"*/ + newFileName;
            // Testing.Print("views Path: " + FileName);

           
#endif



            return newFileName;
        }
        private static List<T> LoadJSON<T>(string FileName)
        {
            FileName = GetPathToSettingsFile(FileName);

            //#if DEBUG
            //            Testing.Print("Is In Debug Mode");
            //            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //            Testing.Print("Path: " + System.IO.Path.GetDirectoryName(path));

            //            string Identifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService\\";
            //            string TestIdentifier = "\\BattleShip_ClientService_Repo\\BattleShip_ClientService_UnitTests\\"; // Indicates that it is a test and splits where the Test Begins to get the part of the Path that should vary form computer to Computer
            //            string forginPath="";
            //            if (path.Contains(Identifier))
            //            {
            //                forginPath= path.Split(Identifier)[0];
            //            }
            //            else if(path.Contains(TestIdentifier))
            //            {
            //                forginPath = path.Split(TestIdentifier)[0];
            //            }

            //            Testing.Print("forgin Path: " + forginPath);

            //            string newPath = forginPath + Identifier;
            //            Testing.Print("new Path: " + newPath);
            //            FileName = newPath + "Settings\\" + FileName;
            //            Testing.Print("views Path: " + FileName);

            //#endif

            //FileName = "Settings\\" + FileName;

            List<T> items = new List<T>();
            using (StreamReader r = new StreamReader(FileName))
            {
                string json = r.ReadToEnd();
                items = JsonSerializer.Deserialize<List<T>>(json);

            }


            return items;
        }
    }
   
}
