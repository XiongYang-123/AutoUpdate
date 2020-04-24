using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdate_Windows
{
    public partial class UpdateLogForm : Form
    {
        bool beginMove = false;//初始化鼠标位置
        int currentXPosition;
        int currentYPosition;
        public UpdateLogForm(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
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

        private void Button2_Click(object sender, EventArgs e)
        {
            Button1_Click(null, null);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Activate();
        }
    }
}
