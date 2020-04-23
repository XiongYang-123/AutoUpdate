using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateServer
{
    public class ServerConfig
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutPut { get; set; }
        /// <summary>
        /// 是否必须更新
        /// </summary>
        public bool IsUpdate { get; set; }
    }
}
