using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AutoUpdateServer
{
    public partial class OutPutForm : Form
    {
        ServerOutPut SOP = null;
        List<FileConfig> lfs = new List<FileConfig>();
        int index = 0;
        public OutPutForm(ServerOutPut sp)
        {
            InitializeComponent();
            SOP = sp;
        }

        private void OutPutForm_Load(object sender, EventArgs e)
        {
            progressBar1.Maximum = SOP.Files.Count;
            StartLoad();
        }
        private void StartLoad()
        {
            LoadFileSync().ContinueWith(i =>
            {
                if (i.Exception == null)
                {
                    if (index + 1 == SOP.Files.Count)
                    {
                        WriteXML();
                        this.Invoke((Action)delegate { progressBar1.Value = index + 1; listBox1.Items.Add("打包已完成!"); listBox1.SelectedIndex = listBox1.Items.Count - 1; });
                    }
                    else
                    {
                        this.Invoke((Action)delegate { progressBar1.Value = index + 1; });
                        index++;
                        StartLoad();
                    }
                }
                else MessageBox.Show(i.Exception.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });

        }
        private async Task LoadFileSync()
        {
            await Task.Run(() =>
            {
                string file = SOP.Files[index];

                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(file);

                lfs.Add(new FileConfig()
                {
                    MD5 = MD5.GetMD5HashFromFile(file),
                    Path = GetRelativePath(SOP.StartPath, file),
                    FileName = Path.GetFileName(file)
                });
                this.Invoke((Action)delegate { label2.Text = fileVersionInfo.FileVersion; label1.Text = Path.GetFileName(file); });
                //todo 压缩文件
                ZipFloClass.ZipFile(file, SOP.Config.OutPut + "\\" + Path.GetFileName(file) + ".zip");
                this.Invoke((Action)delegate { listBox1.Items.Add(fileVersionInfo.FileName + "---100%"); listBox1.SelectedIndex = listBox1.Items.Count - 1; });
            });
        }
        private void WriteXML()
        {
            XmlDocument xmlDoc = new XmlDocument();
            //创建类型声明节点  
            XmlNode node = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "");
            xmlDoc.AppendChild(node);
            //创建根节点  
            XmlNode root = xmlDoc.CreateElement("Config");
            xmlDoc.AppendChild(root);

            XmlNode ver = xmlDoc.CreateElement("Version");
            ver.InnerText = SOP.Config.Version;
            root.AppendChild(ver);

            XmlNode update = xmlDoc.CreateElement("IsUpdate");
            update.InnerText = SOP.Config.IsUpdate.ToString();
            root.AppendChild(update);

            lfs.ForEach(i =>
            {
                XmlNode xn = xmlDoc.CreateElement("File");
                root.AppendChild(xn);
                XmlNode xn1 = xmlDoc.CreateElement("MD5");
                xn1.InnerText = i.MD5;
                XmlNode xn2 = xmlDoc.CreateElement("FileName");
                xn2.InnerText = i.FileName;
                XmlNode xn3 = xmlDoc.CreateElement("Path");
                xn3.InnerText = i.Path;
                xn.AppendChild(xn1);
                xn.AppendChild(xn2);
                xn.AppendChild(xn3);
            });
            XmlNode log = xmlDoc.CreateElement("UpdateLog");
            log.InnerText = SOP.UpdateLog;
            root.AppendChild(log);
            xmlDoc.Save(SOP.Config.OutPut + "\\" + "Config.xml");
        }
        /// <summary>
        /// path1 相对于 path2的相对位置
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        private string GetRelativePath(string path1, string path2)
        {
            try
            {
                string[] path1Array = path1.Split('\\');
                string[] path2Array = path2.Split('\\');
                //
                int s = path1Array.Length >= path2Array.Length ? path2Array.Length : path1Array.Length;
                //两个目录最底层的共用目录索引
                int closestRootIndex = -1;
                for (int i = 0; i < s; i++)
                {
                    if (path1Array[i] == path2Array[i])
                    {
                        closestRootIndex = i;
                    }
                    else
                    {
                        break;
                    }
                }
                //由path1计算 ‘../’部分
                string path1Depth = "";
                for (int i = 0; i < path1Array.Length; i++)
                {
                    if (i > closestRootIndex + 1)
                    {
                        path1Depth += "../";
                    }
                }
                //由path2计算 ‘../’后面的目录
                string path2Depth = "";
                for (int i = closestRootIndex + 1; i < path2Array.Length; i++)
                {
                    path2Depth += "/" + path2Array[i];
                }
                if (path2Depth.Length > 1)
                    path2Depth = path2Depth.Substring(1);

                return path1Depth + path2Depth;
            }
            catch (Exception ex)
            {

                return "";
            }

        }
    }
}
