﻿using System;
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
    public partial class MainForm : Form
    {
        string url = null;
        public MainForm(string url)
        {
            InitializeComponent();
            this.url = url;
        }
    }
}
