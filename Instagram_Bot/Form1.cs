using System;
using System.Deployment.Application;
using System.Diagnostics;
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
        c_bot_core botCore;
        private void button1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            Application.DoEvents();

            try
            {
                botCore = new c_bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim(), checkBoxStealthMode.Checked, !checkBoxDisableVoices.Checked);
                buttonStartBot.Enabled = false;
                buttonStopBot.Enabled = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show($"It looks like the main bot window was closed by you or maybe it crashed.\n\nError message = {ee.Message}", "STOPPED!");
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            buttonStopBot.Enabled = false;

            Text += $"{(ApplicationDeployment.IsNetworkDeployed ? " - Version:" + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : " - [NOT INSTALLED]")}";

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

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/DorsetDevStudio/Instagram_Bot");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile("https://github.com/DorsetDevStudio/Instagram_Bot/raw/master/Instagram_Bot/publish/setup.exe", @"instagram_bot_updates.exe");
            try
            {
                Process.Start("instagram_bot_updates.exe");
            }
            catch
            {
                Process.Start("https://github.com/DorsetDevStudio/Instagram_Bot/raw/master/Instagram_Bot/publish/setup.exe");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                botCore.terminateBot();
                buttonStartBot.Enabled = true;
                buttonStopBot.Enabled = false;
                MessageBox.Show("bot stopped");
            }
            catch (Exception eee)
            {
                MessageBox.Show($"There was an error when trying to stop the bot! Maybe it stopped, maybe it didn't\n\nError message = {eee.Message}");
            }
        }
    }

}
