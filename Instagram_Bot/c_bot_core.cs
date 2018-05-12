using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Instagram_Bot
{

    public class c_bot_core
    {
        IWebDriver IwebDriver = new ChromeDriver();
        public c_bot_core(string username, string password)
        {

            string user = Environment.UserName.Replace(".", " ").Replace(@"\", "");

            // Go full screen
            IwebDriver.Manage().Window.Maximize();
            
            c_voice_core.speak($"hi {user}, just getting warmed up");

            /* CONFIG  */

            // Instagram throttling & bot detection avoidance - we randomise the time between actions (clicks) to `look` more `human`)
            
            int secondsBetweenActions_min   = 2;    // rand min, e.g Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000)
            int secondsBetweenActions_max   = 5;    // rand max

            int minutesBetweenBulkActions_min = 1;  // rand min, e.g Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000)
            int minutesBetweenBulkActions_max = 3;  // rand max

            int maxLikesIn24Hours           = 700;  // Not yet Implemented
            int maxFollowsIn24Hours         = 100;  // Not yet Implemented
            int maxCommentsIn24Hours        = 24;   // Not yet Implemented

            int maxPostsPerSearch           = 10;    // Any value will do, keep low for more variety in 


            // General interests to target, values from hashtags.txt also loaded at startup.
            var thingsToSearch = new List<string>()
            {
                "summer", "weekend", "friday", "netflix","chill", "hangover",
                "bournemouth", "poole", "dorset",
                "learningtocode", "neverstoplearning",
                "dirtyfridayuk", "justgloves",
                "mandelaeffect", "thegreatawakening",
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
                "👌","❤️","🙆","🍟","👁","💙","🌌","🛰","✔️","👩‍","♟","♾","👁‍🗨" /* people love emojis! */
            };

            /* END CONFIG */

            c_voice_core.speak($"ok, let's connect to Instagram as {username}");

            // Log in to Instagram
            IwebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            Thread.Sleep(3 * 1000); // wait for page to change
            IwebDriver.FindElement(By.Name("username")).SendKeys(username);
            IwebDriver.FindElement(By.Name("password")).SendKeys(password);
            IwebDriver.FindElement(By.TagName("form")).Submit();
            Thread.Sleep(3 * 1000); // wait for page to change
            // end Log in to Instagram


            // check we are logged in, if not return to main form UI
            if (IwebDriver.PageSource.Contains("your password was incorrect"))
            {
                c_voice_core.speak($"It didn't work, either the username {username} or password you provided were incorrect, please enter the correct login details and try again. Take your time {user}, no rush");
                IwebDriver.Close();
                IwebDriver.Quit();
                System.Windows.Forms.MessageBox.Show(
                    "Invalid username or password, please try again.",
                    "Login Failed",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return; // exit this instance of bot
            }

            c_voice_core.speak($"We are in, awesome, let's get you some new followers");

            /* MAIN LOOP */

            // loop forever, performing a new search and then following, liking and spamming the hell out of everyone.
            while (true)
            {

                Application.DoEvents(); // Prevent warnings during debugging.
                var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];
                c_voice_core.speak($"Ok, time to find some more followers. Searching for {mySearch}");

                // just navigate to search
                IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/explore/tags/{mySearch}");
                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change

                // save results
                var postsToLike = new List<string>();
                foreach (var link in IwebDriver.FindElements(By.TagName("a")))
                {
                    if (link.GetAttribute("href").ToUpper().Contains($"TAGGED={mySearch}".ToUpper()))
                    {
                        postsToLike.Add(link.GetAttribute("href"));
                    }
                    if (postsToLike.Count >= maxPostsPerSearch) // limit per search
                        break;
                }
                c_voice_core.speak($"Ok we have {postsToLike.Count} posts to work with");

                int postCounter = 0;

                // load results in turn and like/follow them
                foreach (var link in postsToLike)
                {
                    postCounter ++;
                    
                    if (link.Contains("https://www.instagram.com/"))
                    {
                        IwebDriver.Navigate().GoToUrl(link);
                    }
                    else
                    {
                        IwebDriver.Navigate().GoToUrl("https://www.instagram.com/" + link);
                    }

                    Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change

                    // get the username of the owner of the current post
                    string instagram_post_user = "";
                    foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                    {
                        if (obj.GetAttribute("title").ToUpper() == obj.Text.ToUpper() && obj.Text.Length > 5)
                        {
                            instagram_post_user = obj.Text;
                            break;
                        }
                    }

                    c_voice_core.speak($"This is post {postCounter} out of {postsToLike.Count} by instagram user {instagram_post_user}");

                    bool alreadyFollowing = false;
                    // FOLLOW
                    foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                    {

                        if (obj.Text.ToUpper().Contains("FOLLOWING".ToUpper()))
                        {
                            c_voice_core.speak($"you are already following this user, skipping");
                            alreadyFollowing = true;
                            break;
                        }
                        else
                        {
                            if (obj.Text.ToUpper().Contains("FOLLOW".ToUpper()))
                            {
                                c_voice_core.speak($"following");
                                obj.Click();
                                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                                break;
                            }
                        }
                    }
                    // end FOLLOW

                    if (phrasesToComment.Count > 0 && !alreadyFollowing) // don't try and comment if we have nothing to say, this may happen when commenting starts failing everytime and we've removed all coments from our comments list
                    {

                        // COMMENT - this is usually the first thing to be blocked if you reduce time delays, you will see "posting fialed" at bottom of screen.
                        // pick a random comment
                        var myComment = phrasesToComment[new Random().Next(0, phrasesToComment.Count - 1)];

                        // click the comment icon so the comment textarea will work (REQUIRED)
                        foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                        {
                            if (obj.Text.ToUpper().Contains("COMMENT".ToUpper()))
                            {
                                obj.Click(); // click comment icon
                                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                                break;
                            }
                        }
                        // make the comment
                        foreach (var obj in IwebDriver.FindElements(By.TagName("textarea")))
                        {
                            if (obj.GetAttribute("placeholder").ToUpper().Contains("COMMENT".ToUpper()))
                            {
                                c_voice_core.speak($"commenting");
                                bool sendKeysFailed = true;// must start as true
                                while (sendKeysFailed)
                                {
                                    try
                                    {
                                        obj.SendKeys(myComment); // put comment in textarea
                                        break;
                                    }
                                    catch
                                    {
                                        sendKeysFailed = true; // some characters are not supported by chrome driver (some emojis for example)
                                        phrasesToComment.Remove(myComment); // remove offending comment
                                        if (phrasesToComment.Count == 0)
                                        {
                                            break;
                                        }
                                        myComment = phrasesToComment[new Random().Next(0, phrasesToComment.Count - 1)]; // select another comments and try again
                                    }
                                }

                                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                                IwebDriver.FindElement(By.TagName("form")).Submit(); // Only one form on page, so submit it to comment.
                                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                                break;
                            }
                        }

                        // check if comment failed, if yes remove that comment from our comments list
                        if (IwebDriver.PageSource.ToUpper().Contains("couldn't post comment".ToUpper()))
                        {
                            c_voice_core.speak($"Oh dear {user}, that comment was rejected, I will remove it from the list so we don't try to use is again.");
                            phrasesToComment.Remove(myComment);
                            Thread.Sleep(30 * 1000); // wait after a rejection
                        }

                        // end COMMENT

                        // LIKE (do last as it opens a popup that stops us seeing the commenting in action)
                        foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                        {
                            if (obj.Text.ToUpper().Contains("LIKE")) 
                            {

                                obj.Click();
                                c_voice_core.speak($"done, loading next post in a moment");
                                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                                break;
                            }
                        }

                        // end LIKE


                    }// end already following or no comments left

                    Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                }

                c_voice_core.speak($"all done {user}, let's load your profile page and check your stats before doing a new search");

                // Return to users profile page so they can see their stats while we wait for next search to start
                IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");


                Thread.Sleep(3 * 1000); // wait a amount of time for page to change

                string followers = "";
                foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                {
                    if (obj.GetAttribute("href").Contains("followers") 
                        && obj.GetAttribute("href").Contains(username))
                    {
                        followers = obj.FindElement(By.TagName("span")).Text.Replace(",", "").Replace(" ", "").Replace("followers", "");
                        break;
                    }
                }


                string following = "";
                foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                {
                    if (obj.GetAttribute("href").Contains("following")
                        && obj.GetAttribute("href").Contains(username))
                    {
                        following = obj.FindElement(By.TagName("span")).Text.Replace(",", "").Replace(" ", "").Replace("following", "");
                        break;
                    }
                }

                c_voice_core.speak($"You have {followers} followers and are following {following}. Well done, but I take all the credit.");

                c_voice_core.speak($"Let's take a short break.");

                Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000);// wait between each bulk action
            }


            /* end of MAIN LOOP */

        }
    }

}
