using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AutoUpdate_Windows
{
    public partial class MainForm : Form
    {
        string url = null;
        bool beginMove = false;//初始化鼠标位置
        int currentXPosition;
        int currentYPosition;
        public MainForm(string url)
        {
            InitializeComponent();
            this.url = url;
        }
        private void Button1_Paint(object sender, PaintEventArgs e)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, button1.Width, button1.Height);
            button1.Region = new Region(path);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("正在更新下载程序，确认是否退出？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                Environment.Exit(0);
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                beginMove = true;
                currentXPosition = MousePosition.X;//鼠标的x坐标为当前窗体左上角x坐标
                currentYPosition = MousePosition.Y;//鼠标的y坐标为当前窗体左上角y坐标
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                currentXPosition = 0; //设置初始状态
                currentYPosition = 0;
                beginMove = false;
            }
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (beginMove)
            {
                this.Left += MousePosition.X - currentXPosition;//根据鼠标x坐标确定窗体的左边坐标x
                this.Top += MousePosition.Y - currentYPosition;//根据鼠标的y坐标窗体的顶部，即Y坐标
                currentXPosition = MousePosition.X;
                currentYPosition = MousePosition.Y;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ReadXmlSync(new Uri(url)).ContinueWith(i =>
            {
                if (i.Result != null)
                {
                    XDocument xd = i.Result;
                    Dictionary<string, string> dss = new Dictionary<string, string>(); //需要更新的文件
                    var files = xd.Root.Elements("File");
                    this.Invoke((Action)delegate
                    {
                        progressBar1.Maximum = files.Count();
                        label1.Text = $"正在读取文件差异...";
                    });
                    int index = 0;
                    files.ToList().ForEach(x =>
                    {
                        string md5 = x.Element("MD5").Value;
                        string fileName = x.Element("FileName").Value;
                        string path = x.Element("Path").Value;
                        if (File.Exists(path))
                        {
                            if (GetMD5HashFromFile(path) != md5) { dss.Add(fileName, path); }
                        }
                        else dss.Add(fileName, path);
                        this.Invoke((Action)delegate
                        {
                            index++;
                            progressBar1.Value = index;
                            label1.Text = $"正在读取文件差异({index}/{files.Count()})";
                        });
                    });

                    if (Directory.Exists("UpdateCacheFiles"))
                    {
                        Directory.Delete("UpdateCacheFiles", true);
                        Thread.Sleep(10);
                    }
                    Directory.CreateDirectory("UpdateCacheFiles");
                    string[] urls = url.Split('/');
                    string urlbase = url.Replace(urls[urls.Length - 1], "");
                    this.Invoke((Action)delegate
                    {
                        progressBar1.Maximum = dss.Count;
                        progressBar1.Value = 0;
                        label1.Text = $"正在下载差异文件...";
                    });
                    index = 0;
                    if (dss.Count == 0)
                    {
                        this.Invoke((Action)delegate
                        {
                            progressBar1.Value = progressBar1.Maximum;
                            label1.Text = $"更新完成!";
                        });
                        Process.Start(xd.Root.Element("Start").Value);
                        this.Hide();
                        new UpdateLogForm(xd.Root.Element("UpdateLog").Value).ShowDialog();
                    }
                    else
                        dss.ToList().ForEach(x =>
                        {
                            DownFileSync(urlbase + x.Key + ".zip").ContinueWith(s =>
                              {
                                  if (s.Result != null)
                                  {
                                      index++;
                                      this.Invoke((Action)delegate
                                      {
                                          progressBar1.Value = index;
                                          label1.Text = $"正在下载差异文件({index}/{dss.Count})";
                                      });
                                      if (index == dss.Count)
                                      {
                                          try
                                          {
                                              index = 0;
                                              this.Invoke((Action)delegate
                                              {
                                                  progressBar1.Maximum = dss.Count;
                                                  progressBar1.Value = 0;
                                                  label1.Text = $"正在安装文件...";
                                              });
                                              dss.ToList().ForEach(xx =>
                                              {
                                                  string filename = "UpdateCacheFiles/" + xx.Key + ".zip";
                                                  string file = filename.Substring(0, filename.Length - 4);
                                                  UnZip(filename, Path.GetDirectoryName(file));
                                                  CreatePath(xx.Value);
                                                  File.Copy(file, xx.Value, true);
                                                  this.Invoke((Action)delegate
                                                  {
                                                      index++;
                                                      progressBar1.Value = index;
                                                      label1.Text = $"正在安装文件{index}/{dss.Count}";
                                                  });
                                              });
                                              Directory.Delete("UpdateCacheFiles", true);
                                              this.Invoke((Action)delegate
                                              {
                                                  progressBar1.Value = progressBar1.Maximum;
                                                  label1.Text = $"更新完成!";
                                              });

                                              Process.Start(xd.Root.Element("Start").Value);
                                              this.Hide();
                                              var f = new UpdateLogForm(xd.Root.Element("UpdateLog").Value);
                                              f.TopMost = true;
                                              f.BringToFront();
                                              f.ShowDialog();

                                          }
                                          catch (Exception ex)
                                          {
                                              MessageBox.Show(ex.Message + ex.StackTrace, "更新出现错误", buttons: MessageBoxButtons.OK, MessageBoxIcon.Error);
                                          }
                                      }
                                  }
                              });
                        });
                }
                else
                {
                    MessageBox.Show("更新失败,无法拉取更新信息!", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            });
        }

        public async Task<XDocument> ReadXmlSync(Uri url)
        {
            XDocument xDocument = null;

            await Task.Run(() =>
             {
                 try
                 {
                     HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                     req.Method = "GET";
                     req.ContentType = "application/x-www-form-urlencoded";
                     req.Timeout = 3000;//单位毫秒
                     WebResponse wr = req.GetResponse();
                     Stream rs = wr.GetResponseStream();
                     xDocument = XDocument.Load(rs);
                     rs.Close();
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.Message + ":" + ex.StackTrace);
                 }
             });
            return xDocument;
        }

        public string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
        }

        public static void UnZip(string zipFilePath, string unZipDir)
        {
            if (zipFilePath == string.Empty)
            {
                throw new Exception("压缩文件不能为空！");
            }
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("压缩文件不存在！");
            }
            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("/"))
                unZipDir += "/";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            using (var s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                    }
                    if (directoryName != null && !directoryName.EndsWith("/"))
                    {
                    }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {

                            int size;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task<string> DownFileSync(string url)
        {
            string fileName = null;
            await Task.Run(() =>
            {
                try
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.Method = "GET";
                    req.Timeout = 60 * 1000 * 60;
                    WebResponse wr = req.GetResponse();
                    Stream rs = wr.GetResponseStream();
                    fileName = Path.GetFileName(url);
                    FileStream fs = new FileStream("UpdateCacheFiles\\" + fileName, FileMode.Create);
                    byte[] data = new byte[1024];
                    int datalenght = 1;

                    while (datalenght > 0)
                    {
                        datalenght = rs.Read(data, 0, data.Length);
                        fs.Write(data, 0, datalenght);
                    }
                    fs.Close();
                    rs.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ":" + ex.StackTrace);
                }
            });
            return fileName;
        }

        public void CreatePath(string path)
        {
            var p = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(p)) return;
            string[] strs = p.Split('\\');

            string pa = "";
            strs.ToList().ForEach(i =>
            {
                if (pa == "")
                    pa += i;
                else pa += "\\" + i;
                if (!Directory.Exists(pa))
                    Directory.CreateDirectory(pa);
            });

        }
    }
}
