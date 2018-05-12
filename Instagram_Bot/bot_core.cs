using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Instagram_Bot
{

    public class bot_core
    {
        IWebDriver IwebDriver = new ChromeDriver();
        public bot_core(string username, string password)
        {

            // Go full screen
            IwebDriver.Manage().Window.Maximize();

            /* CONFIG  */

            // Instagram throttling & bot detection avoidance - we randomise the time between actions (clicks) to `look` more `human`)
            
            int secondsBetweenActions_min   = 1;    // rand min, e.g Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000)
            int secondsBetweenActions_max   = 10;   // rand max

            int minutesBetweenBulkActions_min = 1;  // rand min, e.g Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000)
            int minutesBetweenBulkActions_max = 30; // rand max

            int maxLikesIn24Hours           = 700;  // Not yet Implemented
            int maxFollowsIn24Hours         = 100;  // Not yet Implemented
            int maxCommentsIn24Hours        = 24;   // Not yet Implemented

            // General interests to target, values from hashtags.txt also loaded at startup.
            var thingsToSearch = new List<string>()
            {
                "summer", "coding", "weekend", "friday", "netflix","chill", "hangover",
                "bournemouth", "poole", "dorset", "vsafety",
                "learningtocode", "bebetter", "neverstoplearning", "everydayisaschoolday",
                "dirtyfridayuk", "jamban_uk", "justgloves_uk", "vpoutlet",
                "mandelaeffect", "thegreatawakening",
                "flishtschool", "pilottraining",
                "followme", "follow4follow", "followforfollow", "followback", "follow4Like", "like4follow",
                "footballsucks", "formula1", "f1", "lewishamilton", "redbullracing", "ferrari", 
            };
            thingsToSearch.AddRange(File.ReadLines("hashtags.txt"));


            // Random generic comments to posts, we need hundreds of these so not to be spammy, or hook up to a random comment / phrase generator API
            var phrasesToComment = new List<string>()
            {
                "I like it!",
                "nice :)",
                "interesting, where is it taken?",
                "Perfection, you should be a photographer!",
                "Wish I could take photos like yours!",
                "haha",
                "hubba bubba",
                "ooh matron",
                "The A team",
                "👌","❤️","🙆","🍟","👁","💙","🌌","🛰","✔️","👩‍","♟","♾","👁‍🗨" /* people love emojis! */
            };
            
            /* END CONFIG */


            // Log in to Instagram
            IwebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            IwebDriver.FindElement(By.Name("username")).SendKeys(username);
            IwebDriver.FindElement(By.Name("password")).SendKeys(password);
            IwebDriver.FindElement(By.TagName("form")).Submit();
            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
            // end Log in to Instagram


            // check we are logged in, if not return to main form UI
            if (IwebDriver.PageSource.Contains("your password was incorrect"))
            {
                IwebDriver.Close();
                IwebDriver.Quit();
                System.Windows.Forms.MessageBox.Show(
                    "Invalid username or password, please try again.",
                    "Login Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return; // exit this instance of bot
            }

            /* MAIN LOOP */

            // loop forever, performing a new search and then following, liking and spamming the hell out of everyone.
            while (true)
            {

                var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];

                // just navigate to search
                IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/explore/tags/{mySearch}");
                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change

                // save results
                var postsToLike = new List<string>();
                foreach (var link in IwebDriver.FindElements(By.TagName("a")))
                {
                    if (link.GetAttribute("href").ToUpper().Contains($"TAGGED={mySearch.ToUpper()}"))
                    {
                        postsToLike.Add(link.GetAttribute("href"));
                    }
                }

                // load results in turn and like/follow them
                foreach (var link in postsToLike)
                {

                    if (link.Contains("https://www.instagram.com/"))
                    {
                        IwebDriver.Navigate().GoToUrl(link);
                    }
                    else
                    {
                        IwebDriver.Navigate().GoToUrl("https://www.instagram.com/" + link);
                    }

                    Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change


                    // FOLLOW
                    foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                    {
                        if (obj.Text.ToUpper().Contains("FOLLOW") && !obj.Text.ToUpper().Contains("FOLLOWING")) // don't unfollow if already following
                        {
                            obj.Click();
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                            break;
                        }
                    }
                    // end FOLLOW


                    // COMMENT
                    // pick a random comment
                    var myComment = phrasesToComment[new Random().Next(0, phrasesToComment.Count - 1)];

                    // click the comment icon so the comment textarea will work (REQUIRED)
                    foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                    {
                        if (obj.Text.ToUpper().Contains("COMMENT"))
                        {
                            obj.Click(); // click comment icon
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                            break;
                        }
                    }
                    // make the comment
                    foreach (var obj in IwebDriver.FindElements(By.TagName("textarea")))
                    {
                        if (obj.GetAttribute("placeholder").ToUpper().Contains("COMMENT"))
                        {
                            obj.SendKeys(myComment); // put comment in textarea
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                            IwebDriver.FindElement(By.TagName("form")).Submit(); // Only one form on page, so submit it to comment.
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                            break;
                        }
                    }
                    // end COMMENT


                    // LIKE (do last as it opens a popup that stops us seeing the commenting in action)
                    foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                    {
                        if (obj.Text.ToUpper().Contains("LIKE")) // This does allow for `unliking`, we are cool with that as it makes us look more human.
                        {
                            obj.Click();
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                            break;
                        }
                    }
                    // end LIKE

                    Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                }


                // Return to users profile page so they can see their stats while we wait for next search to start
                IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");

                Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000);// wait between each bulk action
            }


            /* end of MAIN LOOP */

        }
    }

}
