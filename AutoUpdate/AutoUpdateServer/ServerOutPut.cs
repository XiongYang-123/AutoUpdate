using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateServer
{
    public class ServerOutPut
    {
        public ServerConfig Config { get; set; }

        public List<string> Files { get; set; }

        public string UpdateLog { get; set; }

        public string StartPath { get; set;}
    }
}
