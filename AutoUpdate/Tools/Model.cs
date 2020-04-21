using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public class UpdateConfig
    {
        /// <summary>
        /// 主版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 当前版本是否需要强制更新
        /// </summary>
        public bool IsForcedUpdate { get; set; }
        /// <summary>
        /// 需要更新的文件
        /// </summary>
        public List<FileConfig> Files { get; set; }

    }
    public class FileConfig
    {
        /// <summary>
        /// 子文件版本号
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 文件目录 存放在服务器上的地址
        /// </summary>
        public string Path { get; set; }
    }
}
