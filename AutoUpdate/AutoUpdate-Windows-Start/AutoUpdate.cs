using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AutoUpdate_Windows_Start
{
    public class AutoUpdate
    {
        /// <summary>
        ///  开始检查更新
        /// </summary>
        /// <param name="uri">服务器对应url</param>
        public static void Update(Uri uri)
        {
            if (File.Exists("AutoUpdate-Windows.exe"))
            {
                string myver = Application.ProductVersion;
                Console.WriteLine(myver);
                XDocument xd = ReadXml(uri);
                string serserver = xd.Root.Element("Version").Value;
                if (IsNendUpdate(myver, serserver))
                {
                    string update = xd.Root.Element("IsUpdate").Value.ToLower();
                    if (update == "true")
                    {
                        StartUpdate(uri);
                    }
                    else
                    {
                        if (MessageBox.Show("检查到有新版本可用是否更新？", "升级提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            StartUpdate(uri);
                        }
                    }
                }
            }
        }

        private static void StartUpdate(Uri uri)
        {
            Process.Start("AutoUpdate-Windows.exe", uri.ToString());
            Application.Exit();
            Environment.Exit(0);
        }
        private static XDocument ReadXml(Uri url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Timeout = 3000;//单位毫秒

                WebResponse wr = req.GetResponse();
                Stream rs = wr.GetResponseStream();
                XDocument xd= XDocument.Load(rs);
                rs.Close();
                return xd;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ":" + ex.StackTrace);
                return null;
            }
        }
        private static bool IsNendUpdate(string myver, string server)
        {
            int[] myversion = myver.Split('.').ToList().Select(i => int.Parse(i)).ToArray();
            int[] serverversion = server.Split('.').ToList().Select(i => int.Parse(i)).ToArray();
            if (myversion.Length != serverversion.Length)
                return true;
            for (int i = 0; i < myversion.Length; i++)
            {
                if (myversion[i] > serverversion[i])
                    return false;
                else if (myversion[i] < serverversion[i])
                    return true;
            }
            return false;
        }
    }
}
