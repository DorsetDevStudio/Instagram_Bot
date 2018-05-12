using System;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            //var t = new Task(() => {
                var botCore = new bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim());
            //});
            //t.Start();
            //t.Wait();
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
