using System;
using System.Collections.Generic;
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
         * http://chromedriver.storage.googleapis.com/index.html
         */

        // SIGNING must be OFF as WebDriver.dll is not signed which results in click ones deployment errors if the main app is signed.

        public Form1()
        {
            InitializeComponent();
        }
        C_bot_core botCore;
        Thread th = null;
        private void button1_Click(object sender, EventArgs e)
        {


            // package up `don't run between times` so we can pass to bot_core as a list of class timeSpans
            var sleepTimes = new List<timeSpans>();
            if (dateTimePicker1.Value < dateTimePicker2.Value)
            {
                sleepTimes.Add(new timeSpans() { from = dateTimePicker1.Value, to =dateTimePicker2.Value});
            }
            if (dateTimePicker3.Value < dateTimePicker4.Value)
            {
                sleepTimes.Add(new timeSpans() { from = dateTimePicker3.Value, to = dateTimePicker4.Value });
            }
            // end package up `don't run between times`




            if (textBoxUsername.Text.Length < 4)
            {
                MessageBox.Show("You need to enter your Instagram user" +
                    "name. It's used to tag comments and monitoring follower numbers.");
                return;
            }

            WindowState = FormWindowState.Minimized;



            // save users' settings for next time. (only works if running fully installed via click once)
            textBoxUsername.Text = textBoxUsername.Text.Trim().ToLower();
            //Properties.Settings.Default.username = textBoxUsername.Text;
            //Properties.Settings.Default.password = textBoxPassword.Text;
            //Properties.Settings.Default.sleepTimeSpan1_From = dateTimePicker1.Value;
            //Properties.Settings.Default.sleepTimeSpan1_To = dateTimePicker2.Value;
            //Properties.Settings.Default.sleepTimeSpan2_From = dateTimePicker3.Value;
            //Properties.Settings.Default.sleepTimeSpan2_To = dateTimePicker4.Value;
            //Properties.Settings.Default.banLength = (int)numericUpDownBanLength.Value;

            //Properties.Settings.Default.Save();

            Application.DoEvents();
            try
            {
                Task.Factory.StartNew(() =>
                {
                    th = Thread.CurrentThread;
                    botCore = new C_bot_core(textBoxUsername.Text.Trim(), textBoxPassword.Text.Trim(), checkBoxStealthMode.Checked, !checkBoxDisableVoices.Checked, sleepTimes, (int)numericUpDownBanLength.Value);
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

            //textBoxUsername.Text = Properties.Settings.Default.username;
            //textBoxPassword.Text = Properties.Settings.Default.password;

            //try // load `don't run between times` from user settings , could fail on first load
            //{
            //    dateTimePicker1.Value = Properties.Settings.Default.sleepTimeSpan1_From;
            //    dateTimePicker2.Value = Properties.Settings.Default.sleepTimeSpan1_To;

            //    dateTimePicker3.Value = Properties.Settings.Default.sleepTimeSpan2_From;
            //    dateTimePicker4.Value = Properties.Settings.Default.sleepTimeSpan2_To;


            //    numericUpDownBanLength.Value = Properties.Settings.Default.banLength;

            //}
            //catch { }

            buttonStopBot.Enabled = false;

            Text += $"{(ApplicationDeployment.IsNetworkDeployed ? " - Version:" + ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : " - [NOT INSTALLED]")} ({DateTime.Now.Year})";

            // fix TLS file download issues on win 7 machines, if win 7 users get an SSL/TLS error tell them to install windows updates, it WILL fix the problem.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // This file must be in the working directory but does not like to be deployed with app
            if (!System.IO.File.Exists("chromedriver.exe"))
            {
                notifyIcon1.ShowBalloonTip(2 * 1000, "Instagram Bot", "Installing drivers...", ToolTipIcon.None);
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://github.com/DorsetDevStudio/Instagram_Bot/blob/master/Instagram_Bot/Resources/Downloads/chromedriver.exe?raw=true", @"chromedriver.exe");
            }

            if(textBoxUsername.Text.Length==0)// first time user
                notifyIcon1.ShowBalloonTip(5 * 1000, "Welcome to Instagram Bot", "To get started enter your Instagram login details and click Start.", ToolTipIcon.None);

            MessageBox.Show("Please ensure your sound is turned up.","Instagram Bot is talking to you!");


            C_voice_core.speak(
                "Enter your Instagram username and password and then click start. If prompted by Instagram you should follow the security challenge and enter the pin.", 
                async: true);
            


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

        private void Form1_Shown(object sender, EventArgs e)
        {
         
   
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker1.MaxDate = dateTimePicker2.Value;
            dateTimePicker3.MinDate = dateTimePicker2.Value;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.MinDate = dateTimePicker1.Value;
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker3.MaxDate = dateTimePicker4.Value;
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.MaxDate = dateTimePicker3.Value;
            dateTimePicker4.MinDate = dateTimePicker3.Value;
        }
    }


    //TODO Move to own file
    public class timeSpans
    {
        public DateTime from;
        public DateTime to;
    }


}
