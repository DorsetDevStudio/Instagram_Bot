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


        IWebDriver IwebDriver;
        string user = Environment.UserName.Replace(".", " ").Replace(@"\", "");
        public c_bot_core(string username, string password, bool stealthMode = false, bool enableVoices = true)
        {

            // pretend to be an android mobile app so we can upload image/create posts
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
            IwebDriver = new ChromeDriver(options);

            if (user.Contains(""))
            { // use just the first name of pc username to be more personable
                user = user.Split(' ')[0];
            }

            if (stealthMode)
            {
                IwebDriver.Manage().Window.Minimize();
            }
            else
            {
                IwebDriver.Manage().Window.Maximize();
            }

            /* CONFIG  */

            // Instagram throttling & bot detection avoidance - we randomise the time between actions (clicks) to `look` more `human`)
            int secondsBetweenActions_min = 2;
            int secondsBetweenActions_max = 3; // must be > secondsBetweenActions_min

            int minutesBetweenBulkActions_min = 1;  
            int minutesBetweenBulkActions_max = 2; // must be > minutesBetweenBulkActions_min

            // limits based on minimal research
            int maxFollowsIn24Hours = Properties.Settings.Default.dailyFollowLimit;
            int maxCommentsIn24Hours = Properties.Settings.Default.dailyCommentLimit;
            int maxLikesIn24Hours = Properties.Settings.Default.dailyLikeLimit;// no more than follow * 1.2
           
            // Any value will work, trial and error
            int maxPostsPerSearch = 15;

            // General interests to target:
            // values from c:\hashtags.txt also loaded at startup. 
            // Values from c:\ignore_hashtags.txt will be ignored.
            var thingsToSearch = new List<string>()
            {
                "summer", "chill", "hangover", "followme", "follow4follow", "followforfollow", "followback", "follow4Like", "like4follow",
                 DateTime.Now.ToString("dddd"), // today
                 DateTime.Now.AddDays(-1).ToString("dddd"), // yesterday
                 "hate"+DateTime.Now.ToString("dddd")+"s",
                 "love"+DateTime.Now.ToString("dddd")+"s",

            };

            if (File.Exists(@"c:\hashtags.txt"))
                foreach (var line in File.ReadLines(@"c:\hashtags.txt"))
                    if(!thingsToSearch.Contains(line.Replace("#", "").Trim()))
                        thingsToSearch.Add(line.Replace("#", "").Trim());


            if (File.Exists(@"c:\ignore_hashtags.txt"))
                foreach (var line in File.ReadLines(@"c:\ignore_hashtags.txt"))
                    thingsToSearch.Remove(line.Replace("#", "").Trim());


            // Generic comments to post
            // values from c:\comments.txt also loaded at startup. 
            // Values from c:\ignore_comments.txt will be ignored.
            var phrasesToComment = new List<string>()
            {
                "I #like it! @" + username,
                "#nice :) @" + username,
                "#interesting, where is that? @" + username,
                "#Perfection, you should be a #photographer! @" + username,
                "#Perfection, you've missed your calling! @" + username,
                "#Perfection, almost looks professional! @" + username,
                "#haha, interesting approach me thinks 👌 @" + username,
                "Wish I could take #photos like yours! @" + username,
                "#Perfection, that put a #smile on face and made my " + DateTime.Now.ToString("dddd") + " :) @" + username,
                "It's #" + DateTime.Now.ToString("dddd") + " people @" + username,
                "#Happy " + DateTime.Now.ToString("dddd") + " everybody :) from @" + username,
                "✔️👌✔️ @" + username,
                "❤️✔️✔️ @" + username,
                "✔️🙆 @" + username,
                "🍟✔️ @" + username,
                "💙💙👌 @" + username,
                "✔️ @" + username,
                "✔️👩‍✔️ @" + username,
                "Just what I needed to see this fine " + DateTime.Now.ToString("dddd")+ " " + (DateTime.Now.Hour >= 12 ? "afternoon" : "morning")   + " :) @" + username,
            };

            if (File.Exists(@"c:\comments.txt"))
                foreach (var line in File.ReadLines(@"c:\comments.txt"))
                    if (!phrasesToComment.Contains(line.Replace("#", "").Trim()))
                        phrasesToComment.Add(line.Replace("#", "").Trim());

            if (File.Exists(@"c:\ignore_comments.txt"))
                foreach (var line in File.ReadLines(@"c:\ignore_comments.txt"))
                    phrasesToComment.Remove(line);
               
            
            /* END CONFIG */

            IwebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");

            if (enableVoices) c_voice_core.speak($"let's connect to Instagram");

            if (password.Length < 4)
            {
                if (enableVoices) c_voice_core.speak($"Please login now {user}");
            }
            else
            {
                // Log in to Instagram               
                Thread.Sleep(3 * 1000); // wait for page to change
                IwebDriver.FindElement(By.Name("username")).SendKeys(username);
                IwebDriver.FindElement(By.Name("password")).SendKeys(password);
                IwebDriver.FindElement(By.TagName("form")).Submit();
                Thread.Sleep(3 * 1000); // wait for page to change
                                        // end Log in to Instagram
            }

            //// check we are logged in, if not return to main form UI
            //if (IwebDriver.PageSource.Contains("your password was incorrect"))
            //{
            //    if (enableVoices) c_voice_core.speak($"It didn't work, either the username {username} or password you provided were incorrect, please enter the correct login details and try again. Take your time {user}, no rush");
            //    IwebDriver.Close();
            //    IwebDriver.Quit();
            //    MessageBox.Show(
            //        "Invalid username or password, please try again.",
            //        "Login Failed",
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Error);
            //    return; // exit this instance of bot
            //}

            if (enableVoices) c_voice_core.speak($"You have one minute to complete login");

            Thread.Sleep(60 * 1000); // wait for page to change

            //// Thread.Sleep(5 * 1000); // wait for page to change

            // // has the user been asked to enter a passcode, if yes wait 2 minutes then assume we are in.
            // if (IwebDriver.PageSource.Contains("We Detected An Unusual Login Attempt"))
            // {
            //     if (enableVoices) c_voice_core.speak($"Instagram needs to validate your identity, please follow instructions and then wait up to 2 minutes.");
            //     Thread.Sleep(2 * 60000);
            // }

            // if (enableVoices) c_voice_core.speak($"We are in, awesome, let's get you some new followers");



            // upload image (work in progress) does NOT work, way too many forms in DOM with `file` input type and have not found one that works.
            /*
            foreach (var obj1 in IwebDriver.FindElements(By.TagName("input")))
            {
                if (obj1.GetAttribute("type") == "file")
                {

                    if (enableVoices) c_voice_core.speak($"setting image");
                    obj1.SendKeys(@"C:\test.png");

                    Thread.Sleep(5 * 1000); // wait for page to change

                    if (enableVoices) c_voice_core.speak($"uploading");
                    IwebDriver.FindElement(By.TagName("form")).Submit();  // seems to submit but image does not apear on page, maybe it's the wrong form.

                    Thread.Sleep(5 * 1000); // wait for page to change


                    // we dont even get this far yet
                    //foreach (var obj2 in IwebDriver.FindElements(By.TagName("button")))
                    //{
                    //    if (obj2.Text.ToUpper().Contains("next"))
                    //    {
                    //        if (enableVoices) c_voice_core.speak($"next");
                    //        obj2.Click();
                    //        Thread.Sleep(5 * 1000); // wait for page to change
                    //    }
                    //    else if (obj2.Text.ToUpper().Contains("share"))
                    //    {
                    //        if (enableVoices) c_voice_core.speak($"share");
                    //        obj2.Click();
                    //        Thread.Sleep(5 * 1000); // wait for page to change
                    //        break;
                    //    }
                    //}

                    break;

                }
            }
            if (enableVoices) c_voice_core.speak($"done");
            // click button with next in text
            // click button with share in text
            Thread.Sleep(60 * 1000); // wait for page to change
            // end test code tocreate a new post
            // stay here for debugging.
            while (true) { };
            */




            /* MAIN LOOP */

            // loop forever, performing a new search and then following, liking and spamming the hell out of everyone.
            while (true)
            {

                if (!DateTime.TryParse(Properties.Settings.Default.countersStarted.ToString(), out DateTime o) )
                {   // first run or rinning in debug mode
                    Properties.Settings.Default.countersStarted = DateTime.Now;
                    Properties.Settings.Default.Save();
                    if (enableVoices) c_voice_core.speak($"Instagram limiters configured");
                }

                // dont worry about limits if not installed (debugging)
                if (DateTime.TryParse(Properties.Settings.Default.countersStarted.ToString(), out DateTime _o))
                {
                    // get hours since counters started
                    var hours = (_o - DateTime.Now).TotalHours;
                    if (Properties.Settings.Default.totalFollowsSinceCountersStarted / hours > Properties.Settings.Default.dailyFollowLimit / 24)
                    {
                        if (enableVoices) c_voice_core.speak($"Daily follow limit exceeded");
                        Thread.Sleep(3 * 60000); // wait a amount of time before trying again
                        continue;// go to next post
                    }
                    else
                    {
                        //TODO: followLeftThisHour if not calculating correctly (it was rushed)
                        var followLeftThisHour = (int)(Properties.Settings.Default.dailyFollowLimit / 24) - (int)(Properties.Settings.Default.totalFollowsSinceCountersStarted / hours) ;
                        if (enableVoices) c_voice_core.speak($"{followLeftThisHour} follows left this hour");
                    }
                }
   
                Application.DoEvents(); // Prevent warnings during debugging.
                var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];
                if (enableVoices) c_voice_core.speak($"Ok, let's get some followers");

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
                if (enableVoices) c_voice_core.speak($"{postsToLike.Count} posts found");

                int postCounter = 0;

                // load results in turn and like/follow them
                foreach (var link in postsToLike)
                {
                    postCounter++;

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
                            instagram_post_user = obj.Text.Replace("_"," ");
                            break;
                        }
                    }

                    if (enableVoices) c_voice_core.speak($"post {postCounter} of {postsToLike.Count} by user {instagram_post_user}");

                    bool alreadyFollowing = false;
                    // FOLLOW
                    foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                    {
                        if (obj.Text.ToUpper().Contains("FOLLOWING".ToUpper()))
                        {
                            if (enableVoices) c_voice_core.speak($"already following");
                            alreadyFollowing = true;
                            break;
                        }
                        else if (obj.Text.ToUpper().Contains("FOLLOW".ToUpper()) && Properties.Settings.Default.stopFolowingUntilDate > DateTime.Now)
                        {

                            var minutesLeft = (Properties.Settings.Default.stopFolowingUntilDate - DateTime.Now).Minutes;
                            var secondsLeft = (Properties.Settings.Default.stopFolowingUntilDate - DateTime.Now).Seconds;

                            if (minutesLeft == 0) // must be a few seconds left 
                            {
                                if (enableVoices) c_voice_core.speak($"follow ban in place for {secondsLeft} more seconds");
                            }
                            else
                            {
                                if (enableVoices) c_voice_core.speak($"follow ban in place for {minutesLeft} more minute{(minutesLeft > 1 ? "s" : "")}");
                            }
                        }
                        else if (obj.Text.ToUpper().Contains("FOLLOW".ToUpper()))
                        {
                            if (enableVoices) c_voice_core.speak($"following");
                            obj.Click();
                            Thread.Sleep(2 * 1000); // wait and see it it worked, will change to following
                            if (obj.Text.ToUpper().Contains("FOLLOWING".ToUpper()))
                            {
                                Properties.Settings.Default.totalFollowsSinceCountersStarted += 1;
                                Properties.Settings.Default.Save();
                            }
                            else
                            {
                                if (enableVoices) c_voice_core.speak($"following failed, I will stop following for 15 minutes.");
                                Properties.Settings.Default.stopFolowingUntilDate = DateTime.Now.AddMinutes(15);
                                Properties.Settings.Default.Save();
                            }
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                            break;
                        }

                    }
                    // end FOLLOW

                    if (phrasesToComment.Count > 0 && !alreadyFollowing) // don't try and comment if we have nothing to say, this may happen when commenting starts failing everytime and we've removed all coments from our comments list
                    {

                        // COMMENT - this is usually the first thing to be blocked if you reduce time delays, you will see "posting fialed" at bottom of screen.
                        // pick a random comment
                        // {USERNAME} get's replaced with @USERNAME
                        // {DAY} get's replaced with today's day .g: MONDAY, TUESDAY etc..

                        var myComment = phrasesToComment[new Random().Next(0, phrasesToComment.Count - 1)].Replace("{USERNAME}","@" + username.Replace("{DAY}", "@" + DateTime.Now.ToString("dddd")));

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
                                if (enableVoices) c_voice_core.speak($"commenting");
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
                                Thread.Sleep(1 * 1000);// wait for comment to type
                                IwebDriver.FindElement(By.TagName("form")).Submit(); // Only one form on page, so submit it to comment.
                                Thread.Sleep(3 * 1000); // wait a short(random) amount of time for page to change
                                break;
                            }
                        }

                        // check if comment failed, if yes remove that comment from our comments list
                        if (IwebDriver.PageSource.ToUpper().Contains("couldn't post comment".ToUpper()))
                        {
                            if (enableVoices) c_voice_core.speak($"comment rejected");
                            //TODO: try commenting another comment, perhaps a comment with no #hashtags or @users will work
                        }

                        // end COMMENT

                        // LIKE (do last as it opens a popup that stops us seeing the commenting in action)
                        foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                        {
                            if (obj.Text.ToUpper().Contains("LIKE"))
                            {
                                obj.Click();
                                if (enableVoices) c_voice_core.speak($"done, loading next post");
                                Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                                break;
                            }
                        }

                        // end LIKE


                    }// end already following or no comments left
                    if (!alreadyFollowing)
                        Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change
                }

                if (enableVoices) c_voice_core.speak($"all done {user}, let's check your stats");

                // Return to users profile page so they can see their stats while we wait for next search to start
                IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");


                //TODO: when testing on a new account with no profile image (may be unrelated) the stats below are not found, need to figure out why. Have increased wait to from 3 to 4 seconds to see if that helps.
                Thread.Sleep(4 * 1000); // wait a amount of time for page to change

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

                if (enableVoices) c_voice_core.speak($"You have {followers} followers and are following {following}. Well done, but I take all the credit.");

                if (enableVoices) c_voice_core.speak($"Let's take a short break.");

                Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000);// wait between each bulk action
            }


            /* end of MAIN LOOP */

        }
        public void terminateBot()
        {
            try { IwebDriver.Close();} catch { }
            try { IwebDriver.Quit(); } catch { }
        }
    }
}

