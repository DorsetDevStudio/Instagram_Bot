using System;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
using System.IO;

namespace Instagram_Bot
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var botCore = new bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim());
        }

    }



}
