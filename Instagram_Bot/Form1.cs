using System;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;
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

    public class bot_core
    {
        ChromeDriver driver = new ChromeDriver();
        public bot_core(string username, string password)
        {

            #region Login to Instagram
            driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            driver.FindElementByName("username").SendKeys(username);
            driver.FindElementByName("password").SendKeys(password);
            driver.FindElementByTagName("form").Submit();
            System.Threading.Thread.Sleep(3 * 1000);// wait 5 seconds for page to change
            #endregion

            #region perform a random search from the list of search terms
            List<string> thingsToSearch = new List<string>()
            {
                "summer", "web developer", "weekend", "friday", "netflix",
                "chill", "bournemouth", "poole", "dorset", "vsafety"
            };

            var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];

            // just navigate to search
            driver.Navigate().GoToUrl($"https://www.instagram.com/explore/tags/{mySearch}");
            System.Threading.Thread.Sleep(3 * 1000);// wait for page to change

            // save results
            List<string> postsToLike = new List<string>();
            foreach (var link in driver.FindElementsByTagName("a"))
            {
                if (link.GetAttribute("href").ToUpper().Contains($"TAGGED={mySearch.ToUpper()}"))
                {
                    postsToLike.Add(link.GetAttribute("href"));
                }
            }

            // load results in turn and like/follow them
            foreach (string glink in postsToLike)
            {

                if (glink.Contains("https://www.instagram.com/"))
                {
                    driver.Navigate().GoToUrl(glink);
                }
                else
                {
                    driver.Navigate().GoToUrl("https://www.instagram.com/" + glink);
                }

                System.Threading.Thread.Sleep(3 * 1000);// wait for page to change

                foreach (var button in driver.FindElementsByTagName("button"))
                {
                    if (button.Text.ToUpper().Contains("FOLLOW"))
                    {
                        button.Click();
                        System.Threading.Thread.Sleep(3 * 1000);// wait for page to change
                        continue;
                    }
                }
            }
            #endregion
        }
    }

}
