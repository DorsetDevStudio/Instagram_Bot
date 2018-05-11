using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;

namespace Instagram_Bot
{

    public class bot_core
    {
        ChromeDriver driver = new ChromeDriver();
        public bot_core(string username, string password)
        {
           
            
            /* CONFIG  */

            // Instagram throttling
            int secondsBetweenActions       = 5;    // suggest minimum = 5 maximum = 30. Reduce below 5 and follows, comments or likes just wont work.
            int minutesBetweenBulkActions   = 15;   // suggest minimum = 15, maginus = 60. Make it too long and the session may timeout.
            int maxLikesIn24Hours           = 700;  // Not yet Implemented
            int maxFollowsIn24Hours         = 100;  // Not yet Implemented
            int maxCommentsIn24Hours        = 24;   // Not yet Implemented


            // General interests to target, values from hashtags.txt also loaded at startup.
            List<string> thingsToSearch = new List<string>()
            {
                "summer", "web developer", "weekend", "friday", "netflix",
                "chill", "bournemouth", "poole", "dorset", "vsafety"
            };
            thingsToSearch.AddRange(File.ReadLines("hashtags.txt"));


            // Random generic comments to posts, we need hundreds of these so not to be spammy, or hook up to a random comment / phrase generator API
            List<string> phrasesToComment = new List<string>()
            {
                "I like it!",
                "nice :)",
                "interesting, where is it taken?",
                "Perfection, you should be a photographer!",
                "Wish I could take photos like yours!",
                "haha",
                "hubba bubba",
                "ooh matron",
                "The A team"
            };
            
            /* END CONFIG */


            // Log in to Instagram
            driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            driver.FindElementByName("username").SendKeys(username);
            driver.FindElementByName("password").SendKeys(password);
            driver.FindElementByTagName("form").Submit();
            System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change
            // end Log in to Instagram


            // check we are logged in, if not return to main form UI
            if (driver.PageSource.Contains("your password was incorrect"))
            {
                driver.Close();
                driver.Quit();
                System.Windows.Forms.MessageBox.Show(
                    "Invalid username or password, please try again.",
                    "Login Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return; // exit this instance of bot
            }

            /* MAIN LOOP */

            // loop forever, performing a new search and them following everyone.
            while (true)
            {

                var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];

                // just navigate to search
                driver.Navigate().GoToUrl($"https://www.instagram.com/explore/tags/{mySearch}");
                System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change

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

                    System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change


                    // FOLLOW
                    foreach (var obj in driver.FindElementsByTagName("button"))
                    {
                        if (obj.Text.ToUpper().Contains("FOLLOW") && !obj.Text.ToUpper().Contains("FOLLOWING")) // don't unfollow if already following
                        {
                            obj.Click();
                            System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change
                            break;
                        }
                    }
                    // end FOLLOW


                    // COMMENT
                    // pick a random comment
                    var myComment = phrasesToComment[new Random().Next(0, phrasesToComment.Count - 1)];

                    // click the comment icon so the comment textarea will work (REQUIRED)
                    foreach (var obj in driver.FindElementsByTagName("a"))
                    {
                        if (obj.Text.ToUpper().Contains("COMMENT"))
                        {
                            obj.Click(); // click comment icon
                            System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change
                            break;
                        }
                    }
                    // make the comment
                    foreach (var obj in driver.FindElementsByTagName("textarea"))
                    {
                        if (obj.GetAttribute("placeholder").ToUpper().Contains("COMMENT"))
                        {
                            obj.SendKeys(myComment); // put comment in textarea
                            System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change
                            driver.FindElementByTagName("form").Submit(); // Only one form on page, so submit it to comment.
                            System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change
                            break;
                        }
                    }
                    // end COMMENT



                    // LIKE (do last as it opens a popup that stops us seeing the commenting in action)
                    foreach (var obj in driver.FindElementsByTagName("a"))
                    {
                        if (obj.Text.ToUpper().Contains("LIKE"))
                        {
                            obj.Click();
                            System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait for page to change
                            break;
                        }
                    }
                    // end LIKE

                    System.Threading.Thread.Sleep(secondsBetweenActions * 1000);// wait a while
                }


                // Return to users profile page so they can see their stats while we wait for next search to start
                driver.Navigate().GoToUrl($"https://www.instagram.com/{username}");

                System.Threading.Thread.Sleep(minutesBetweenBulkActions * 60000);// wait between each bulk action
            }


            /* end of MAIN LOOP */

        }
    }

}
