using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
namespace AutoUpdateServer
{
    public partial class MainForm : Form
    {
        public const string CONFIG_FILE_PATH = "server.config";
        /// <summary>
        /// 缓存的上次的配置
        /// </summary>
        ServerConfig config = null;


        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            config = LoadConfig();
            tb_ver.Text = config.Version;
            lb_output.Text = config.OutPut;
            cb_update.Checked = config.IsUpdate;
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public ServerConfig LoadConfig()
        {
            ServerConfig sc = new ServerConfig();
            try
            {
                if (File.Exists(CONFIG_FILE_PATH))
                {
                    sc = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText(CONFIG_FILE_PATH));
                }
            }
            catch { }
            return sc;
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="Sc"></param>
        /// <returns></returns>
        public void SaveConfig(ServerConfig Sc)
        {
            try
            {
                File.WriteAllText(CONFIG_FILE_PATH, JsonConvert.SerializeObject(Sc));
            }
            catch { }
        }

        private void Btn_output_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog save = new FolderBrowserDialog();
            if (save.ShowDialog() == DialogResult.OK)
            {
                lb_output.Text = save.SelectedPath;
            }
        }

        private void Btn_add_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            if (open.ShowDialog() == DialogResult.OK)
            {
                lsb_files.Items.AddRange(open.FileNames);
            }
        }

        private void Btn_delete_Click(object sender, EventArgs e)
        {
            if (lsb_files.SelectedIndex >= 0)
                lsb_files.Items.RemoveAt(lsb_files.SelectedIndex);
        }

        private void 删除文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Btn_delete_Click(null, null);
        }

        private void Btn_save_Click(object sender, EventArgs e)
        {
            string ver = tb_ver.Text.Trim();
            bool isupdate = cb_update.Checked;
            string updatelog = textBox2.Text.Trim();
            string output = lb_output.Text;
            string start = lb_start.Text;
            if (string.IsNullOrEmpty(ver))
            {
                MessageBox.Show("The version number can't be null", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(output))
            {
                MessageBox.Show("Did not choose the output directory, please choose the directory first", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(updatelog))
            {
                MessageBox.Show("Update log can't be null", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (lsb_files.Items.Count == 0)
            {
                MessageBox.Show("Update file list is empty, there is at least one file", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(start))
            {
                MessageBox.Show("Please select a start the program", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            config.IsUpdate = isupdate;
            config.OutPut = output;
            config.Version = ver;
            SaveConfig(config);

            ServerOutPut sop = new ServerOutPut();
            sop.Config = config;
            sop.UpdateLog = updatelog;
            sop.Files = lsb_files.Items.OfType<object>().Select(i => i.ToString()).ToList();
            sop.StartPath = lb_start.Text;
            new OutPutForm(sop).ShowDialog();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                lb_start.Text = open.FileName;
            }
        }
    }
}
