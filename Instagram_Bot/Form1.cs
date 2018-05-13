using System;
using System.Deployment.Application;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Instagram_Bot
{
    public partial class Form1 : Form
    {
        // To install just run https://github.com/DorsetDevStudio/Instagram_Bot/raw/master/Instagram_Bot/publish/setup.exe
        // or go to http://tinyurl.com/dorsetdevbot

        /*    
         *    
         *    ---- DEPLOYING UPDATES TO END USERS ----
        Build ALL (in RELEASE mode)
        Publish (publishes to /publish in project source)
        commit & push
        That's it!
         *
         *
         */

        // SIGNING must be OFF as WebDriver.dll is not signed which results in click ones deployment errors if the main app is signed.

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Application.DoEvents();
            var botCore = new c_bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim(), checkBoxStealthMode.Checked, !checkBoxDisableVoices.Checked);         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // fix TLS file download issues on win 7 machines, if win 7 users get an SSL/TLS error tell them to install windows updates, it WILL fix the problem.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // This file must be in the working directory but does not like to be deployed with app
            if (!System.IO.File.Exists("chromedriver.exe"))
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://github.com/DorsetDevStudio/Instagram_Bot/raw/master/Instagram_Bot/publish/chromedriver.exe", @"chromedriver.exe");
            }

        }

        private void textBoxPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                buttonStartBot.PerformClick();
        }

    }

}
