using System;
using System.Deployment.Application;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Instagram_Bot
{
    public partial class Form1 : Form
    {

        // hosting click once on github so we can push out updates automatically
        // https://developers.de/2018/02/10/clickonce-on-github/

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var botCore = new c_bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim());         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void textBoxPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                buttonStartBot.PerformClick();
        }

    }

}
