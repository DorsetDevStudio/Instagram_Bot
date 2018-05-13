using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Net;
using System.Threading;
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
        Thread th = null;
        private void button1_Click(object sender, EventArgs e)
        {

            if (textBoxUsername.Text.Length < 4)
            {
                MessageBox.Show("You need to enter your Instagram username. It's used to tag comments and monitoring follower numbers.");
                return;
            }

            if(checkBoxStealthMode.Checked)
                WindowState = FormWindowState.Minimized;

            Properties.Settings.Default.username = textBoxUsername.Text;
            Properties.Settings.Default.password = textBoxPassword.Text;
            Properties.Settings.Default.Save();


            Application.DoEvents();
            try
            {
                Task.Factory.StartNew(() =>
                {
                    th = Thread.CurrentThread;
                    botCore = new c_bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim(), checkBoxStealthMode.Checked, !checkBoxDisableVoices.Checked);
                });
                buttonStartBot.Enabled = false;
                buttonStopBot.Enabled = true;

                //notifyIcon1.ShowBalloonTip(3 * 1000, "Running", "", ToolTipIcon.None);


            }
            catch (Exception ee)
            {
                MessageBox.Show($"It looks like the main bot window was closed by you or maybe it crashed.\n\nError message = {ee.Message}", "STOPPED!");
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            textBoxUsername.Text = Properties.Settings.Default.username;
            textBoxPassword.Text = Properties.Settings.Default.password;

            buttonStopBot.Enabled = false;

            Text += $"{(ApplicationDeployment.IsNetworkDeployed ? " - Version:" + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : " - [NOT INSTALLED]")} ({DateTime.Now.Year})";

            // fix TLS file download issues on win 7 machines, if win 7 users get an SSL/TLS error tell them to install windows updates, it WILL fix the problem.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // This file must be in the working directory but does not like to be deployed with app
            if (!System.IO.File.Exists("chromedriver.exe"))
            {
                notifyIcon1.ShowBalloonTip(2 * 1000, "Instagram Bot", "Installing drivers...", ToolTipIcon.None);
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://github.com/DorsetDevStudio/Instagram_Bot/raw/master/Instagram_Bot/publish/chromedriver.exe", @"chromedriver.exe");
            }

            if(textBoxUsername.Text.Length==0)// first time user
                notifyIcon1.ShowBalloonTip(5 * 1000, "Welcome to Instagram Bot", "To get started enter your Instagram login details and click Start.", ToolTipIcon.None);

            MessageBox.Show("Please ensure your sound is turned up.","Instagram Bot is talking to you!");

            c_voice_core.speak("Thanks, I hope you can hear me now. You can disable speach but I suggest you leave it on for now as I have a lot to say.");

            c_voice_core.speak("Enter your Instagram username and password and then click start. Then just watch the screen without clicking or resizing the windows that appear. The bot may take a minute to start working after the login screen, if prompted by Instagram you should follow the security challenge and enter the pin.");

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
                //TODO: stopping bot does not close the instance of chrome, but at least it stops and does not lock up UI
                th.Abort();
                foreach (var process in Process.GetProcessesByName("chromedriver"))
                {
                    process.Kill();
                }
                buttonStartBot.Enabled = true;
                buttonStopBot.Enabled = false;
                MessageBox.Show("You will need to close the web browser manually", "STOPPED!");
            }
            catch (Exception eee)
            {
                MessageBox.Show($"There was an error when trying to stop the bot! Maybe it stopped, maybe it didn't\n\nError message = {eee.Message}");
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("chromedriver"))
            {
                process.Kill();
            }
            Close();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("By entering your login details here they will be saved for next time you use the bot. If you don't wan't to enter your details here just click [Start] and log in manually each time.");
        }
    }

}
