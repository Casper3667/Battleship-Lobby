using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLobby.Settings
{
    public class AppSettings
    {
        public string Token { get; set; } = "";
        public bool UseBackupPlan { get; set; }
        public string BackupIP { get; set; }
        public int BackupPort { get; set; }
        public int BackupHealthcheckPort { get; set; }
    }
}
