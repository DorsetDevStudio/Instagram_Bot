using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Instagram_Bot
{

    public class C_bot_core : IC_bot_core, IDisposable
    {

        IWebDriver IwebDriver;
        string user = Environment.UserName.Replace(".", " ").Replace(@"\", "");
        public C_bot_core(string username, string password, bool stealthMode = false, bool enableVoices = true, List<timeSpans> sleepTimes = null, int banLength = 5)
        {

            // pretend to be an android mobile app so we can upload image/create posts
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
            options.UnhandledPromptBehavior = UnhandledPromptBehavior.Dismiss;
            IwebDriver = new ChromeDriver(options);

            IwebDriver.Manage().Window.Maximize();

            if (user.Contains(""))
            { // use just the first name of pc username to be more personable
                user = user.Split(' ')[0];
            }

            /* CONFIG  */

            // Instagram throttling & bot detection avoidance - we randomise the time between actions (clicks) to `look` more `human`)
            int secondsBetweenActions_min = 1;
            int secondsBetweenActions_max = 1; // must be > secondsBetweenActions_min

            int minutesBetweenBulkActions_min = 1;
            int minutesBetweenBulkActions_max = 2; // must be > minutesBetweenBulkActions_min

            // limits based on minimal research
            int maxFollowsIn24Hours = 100;// int.TryParse(//new Classes.C_DataLayer().GetConfigValueFor("dailyFollowLimit"), out int _a) ? _a : 500;
            int maxCommentsIn24Hours = 100;// int.TryParse(//new Classes.C_DataLayer().GetConfigValueFor("dailyCommentLimit"), out int _a2) ? _a2 : 500;
            int maxLikesIn24Hours = 100;// (int)(maxFollowsIn24Hours * 1.2);

            // Any value will work, trial and error
            int maxPostsPerSearch = 10;

            // General interests to target:
            // values from c:\hashtags.txt also loaded at startup. 
            // Values from c:\ignore_hashtags.txt will be ignored.
            var thingsToSearch = new List<string>()
            {
                "summer", "chill", "hangover", "followme", "follow4follow", "followforfollow", "followback", "follow4Like", "like4follow",
                 DateTime.Now.ToString("dddd"), // today
                 DateTime.Now.ToString("dddd") + "lunch",
                 DateTime.Now.ToString("dddd") + "roast",
                 DateTime.Now.AddDays(-1).ToString("dddd"), // yesterday
                 "hate"+DateTime.Now.ToString("dddd")+"s",
                 "love"+DateTime.Now.ToString("dddd")+"s",

            };

            //if (File.Exists(@"c:\hashtags.txt"))
            //{
            //    thingsToSearch.Clear();// just use users hashtags
            //    foreach (var line in File.ReadLines(@"c:\hashtags.txt"))
            //        if (!thingsToSearch.Contains(line.Replace("#", "").Trim()))
            //            thingsToSearch.Add(line.Replace("#", "").Trim());
            //    if (enableVoices) c_voice_core.speak($"{thingsToSearch.Count} hashtags loaded from hashtags file");
            //}


            //if (File.Exists(@"c:\ignore_hashtags.txt"))
            //    foreach (var line in File.ReadLines(@"c:\ignore_hashtags.txt"))
            //        thingsToSearch.Remove(line.Replace("#", "").Trim());


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
                "#haha, interesting approach me thinks @" + username,
                "Wish I could take #photos like yours! @" + username,
                // "#Perfection, that put a #smile on my face and made my " + DateTime.Now.ToString("dddd") + " :) @" + username,
                "It's #" + DateTime.Now.ToString("dddd") + " people @" + username,
                "#Happy " + DateTime.Now.ToString("dddd") + " everybody :) from @" + username,
                //"Just what I needed to see this fine " + DateTime.Now.ToString("dddd")+ " " + (DateTime.Now.Hour >= 12 ? "afternoon" : "morning")   + " :) @" + username,
            };

            //if (File.Exists(@"c:\comments.txt"))
            //{
            //    phrasesToComment.Clear();// only use users comments if provided
            //    foreach (var line in File.ReadLines(@"c:\comments.txt"))
            //        if (!phrasesToComment.Contains(line.Replace("#", "").Trim()))
            //            phrasesToComment.Add(line.Replace("#", "").Trim());
            //    if (enableVoices) c_voice_core.speak($"{phrasesToComment.Count} comments loaded from comments file");
            //}

            //if (File.Exists(@"c:\ignore_comments.txt"))
            //    foreach (var line in File.ReadLines(@"c:\ignore_comments.txt"))
            //        phrasesToComment.Remove(line);


            /* END CONFIG */

            IwebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");

            // if (enableVoices) c_voice_core.speak($"let's connect to Instagram");

            if (password.Length < 4)
            {
                if (enableVoices) C_voice_core.speak($"Please login now {user}");
            }
            else
            {
                // Log in to Instagram               
                Thread.Sleep(1 * 1000); // wait for page to change
                IwebDriver.FindElement(By.Name("username")).SendKeys(username);
                IwebDriver.FindElement(By.Name("password")).SendKeys(password);
                IwebDriver.FindElement(By.TagName("form")).Submit();
                Thread.Sleep(4 * 1000); // wait for page to change
                                        // end Log in to Instagram
            }

            if (IwebDriver.PageSource.Contains("your password was incorrect"))
            {
                if (enableVoices) C_voice_core.speak($"You have one minute to complete login");
                Thread.Sleep(60 * 1000); // wait for page to change

            }
            else if (IwebDriver.PageSource.Contains("security") || IwebDriver.PageSource.Contains("Unusual"))
            {
                if (enableVoices) C_voice_core.speak($"You have one minute to complete login");
                Thread.Sleep(60 * 1000); // wait for page to change
            }
            else
            {
                if (enableVoices) C_voice_core.speak($"We are in, awesome");
            }

            if (stealthMode)
            {
                if (enableVoices) C_voice_core.speak($"Entering stealth mode");
                IwebDriver.Manage().Window.Minimize();
            }

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

            bool chromeIsMinimised = false;
            bool _sleeping = false;
            /* MAIN LOOP */
            // loop forever, performing a new search and then following, liking and spamming the hell out of everyone.
            while (true)
            {

                if (enableVoices) C_voice_core.speak($"testing db");

                try
                {
                    new Classes.C_DataLayer().AddInstaUser(new Classes.InstaUser() { username = "test_user" });
                    if (enableVoices) C_voice_core.speak($"db test passed");
                }
                catch (InvalidOperationException ee)
                {                  
                    if (enableVoices) C_voice_core.speak($"SQLite invalid operation error {ee.InnerException.ToString()}",true);
                    MessageBox.Show($"SQLite invalid operation error {ee.InnerException.ToString()}");
                }
                catch (Exception ee)
                {
                    if (enableVoices) C_voice_core.speak($"SQLite error {ee.InnerException.ToString()}",true);
                    MessageBox.Show($"SQLite error {ee.InnerException.ToString()}");
                }
              
                DateTime commentingBannedUntil = DateTime.Now;
                DateTime followingBannedUntil = DateTime.Now;
                DateTime unfollowingBannedUntil = DateTime.Now;
                DateTime likingBannedUntil = DateTime.Now;

                // maximise window after sleep period
                if (chromeIsMinimised && !stealthMode)
                {
                    IwebDriver.Manage().Window.Maximize();
                    chromeIsMinimised = false;
                }

                // before jumping into the search for posts by tags loop
                // let's follow all the suggect profiles that are suggected on the initial login page (if any), this list may only be visible for new accounts
                foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                {
                    if (obj.Text.ToLower().Trim().Contains("follow"))
                    {
                        try
                        {
                            obj.Click();
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time between clicks

                            // if following failed dont keep trying
                            if (!obj.Text.ToLower().Trim().Contains("following"))
                            {
                                if (enableVoices) C_voice_core.speak($"following failed, I will stop following for {banLength} minutes.");
                                followingBannedUntil = DateTime.Now.AddMinutes(banLength);
                                //new Classes.C_DataLayer().SetConfigValueFor("stopFolowingUntilDate", DateTime.Now.AddMinutes(banLength).ToString(Classes.C_DataLayer.SQLiteDateTimeFormat));
                                break;
                            }

                        }
                        catch
                        {
                            if (enableVoices) C_voice_core.speak($"follow failed");
                        }
                    }
                }

                //repeat to get more
                foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                {
                    if (obj.Text.ToLower().Trim().Contains("follow"))
                    {
                        try
                        {
                            obj.Click();
                            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time between clicks

                            // if following failed dont keep trying
                            if (!obj.Text.ToLower().Trim().Contains("following"))
                            {
                                if (enableVoices) C_voice_core.speak($"following failed, I will stop following for {banLength} minutes.");
                                followingBannedUntil = DateTime.Now.AddMinutes(banLength);
                                //new Classes.C_DataLayer().SetConfigValueFor("stopFolowingUntilDate", DateTime.Now.AddMinutes(banLength).ToString(Classes.C_DataLayer.SQLiteDateTimeFormat));
                                break;
                            }

                        }
                        catch
                        {
                            if (enableVoices) C_voice_core.speak($"follow failed");
                        }
                    }
                }



                //  if (enableVoices) C_voice_core.speak($"debugging");


                // Application.DoEvents(); // Prevent warnings during debugging.
                var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];
                if (enableVoices) C_voice_core.speak($"Ok, let's get some followers");

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
                if (enableVoices) C_voice_core.speak($"{postsToLike.Count} posts found");

                int postCounter = 0;



                

                // load results in turn and like/follow them
                foreach (var link in postsToLike)
                {


                    // we may need to sleep, need to check if we should be bwtewwn each post
                    // handle `don't run between` times       

                    //if (!_sleeping) // check if we should be
                    //{
                    //    foreach (timeSpans timeSpan in sleepTimes)
                    //    {
                    //        if (DateTime.Now.TimeOfDay > timeSpan.from.TimeOfDay
                    //            && DateTime.Now.TimeOfDay < timeSpan.to.TimeOfDay)
                    //        {
                    //            _sleeping = true;
                    //            if (enableVoices) C_voice_core.speak($"I'm tired, yawn, sleeping until {timeSpan.to.ToShortTimeString()}");
                    //            break;
                    //        }
                    //    }

                    //}


                    //while (_sleeping) // check if we shouldnt be
                    //{
                    //    bool _sleep = false;
                    //    foreach (timeSpans timeSpan in sleepTimes)
                    //    {
                    //        if (DateTime.Now.TimeOfDay > timeSpan.from.TimeOfDay
                    //            && DateTime.Now.TimeOfDay < timeSpan.to.TimeOfDay)
                    //        {
                    //            _sleep = true;
                    //            break;
                    //        }
                    //    }
                    //    if (!_sleep) // just woke up
                    //    {
                    //        if (enableVoices) C_voice_core.speak($"Nap over, damn it");
                    //        _sleeping = false;
                    //    }
                    //    Thread.Sleep(1 * 1000);// sleep 1 second
                    //    Application.DoEvents();
                    //}

                    postCounter++;

                    if (link.Contains("https://www.instagram.com/"))
                    {
                        IwebDriver.Navigate().GoToUrl(link);
                    }
                    else
                    {
                        IwebDriver.Navigate().GoToUrl("https://www.instagram.com/" + link);
                    }

                    Thread.Sleep(2 * 1000); // wait a short amount of time for page to change



                    // get the username of the owner of the current post
                    string instagram_post_user = "";
                    foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                    {
                        if (obj.GetAttribute("title").ToUpper() == obj.Text.ToUpper() && obj.Text.Length > 5)
                        {
                            instagram_post_user = obj.Text.Replace("_", " ").ToLower().Trim();
                            break;
                        }
                    }


                    // if (enableVoices) C_voice_core.speak($"user {instagram_post_user}");


                    // testing new database functionality
                    //  //new Classes.C_DataLayer().AddInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_") });


                    // if (enableVoices) c_voice_core.speak($"post {postCounter} of {postsToLike.Count} by user {instagram_post_user}");


                    // FOLLOW
                    foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                    {


                        //if (enableVoices) C_voice_core.speak($"button  {obj.Text}");


                        if (obj.Text.ToUpper().Contains("FOLLOWING".ToUpper()))
                        {
                            // if (enableVoices) C_voice_core.speak($"already following");
                            break;
                        }
                        else if (obj.Text.ToUpper().Contains("FOLLOW".ToUpper()) && followingBannedUntil > DateTime.Now)
                        {

                            var _minutesLeft = (followingBannedUntil - DateTime.Now).Minutes;
                            var _secondsLeft = (followingBannedUntil - DateTime.Now).Seconds;

                            if (_minutesLeft == 0) // must be a few seconds left 
                            {
                                if (enableVoices) C_voice_core.speak($"follow ban in place for {_secondsLeft} more seconds");
                            }
                            else
                            {
                                if (enableVoices) C_voice_core.speak($"follow ban in place for {_minutesLeft} more minute{(_minutesLeft > 1 ? "s" : "")}");
                            }
                            break;
                        }
                        else if (obj.Text.ToUpper().Contains("FOLLOW".ToUpper()))
                        {
                            if (enableVoices) C_voice_core.speak($"following");
                            obj.Click();
                            Thread.Sleep(2 * 1000); // wait and see it it worked, will change to following
                            if (obj.Text.ToUpper().Contains("FOLLOWING".ToUpper()))
                            {
                                // testing new database functionality
                                //new Classes.C_DataLayer().SaveInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_"), date_followed_them = DateTime.Now });
                            }
                            else
                            {

                                commentingBannedUntil = DateTime.Now.AddMinutes(banLength);
                                if (enableVoices) C_voice_core.speak($"following failed, I will stop following for {banLength} minutes.");
                                //new Classes.C_DataLayer().SetConfigValueFor("stopFolowingUntilDate", DateTime.Now.AddMinutes(banLength).ToString(Classes.C_DataLayer.SQLiteDateTimeFormat));

                            }
                            Thread.Sleep(2 * 1000); // wait and see it it worked, will change to following
                            break;
                        }



                    }
                    // end FOLLOW

                    //if (enableVoices) c_voice_core.speak($"{phrasesToComment.Count} comments to pick from");

                    commentingBannedUntil = CommentOnPost(username, enableVoices, banLength, secondsBetweenActions_min, secondsBetweenActions_max, phrasesToComment, commentingBannedUntil);

                    var _likeBanminutesLeft = (likingBannedUntil - DateTime.Now).Minutes;
                    var _likeBanSecondsLeft = (likingBannedUntil - DateTime.Now).Seconds;


                    if (_likeBanSecondsLeft > 0)
                    {


                        if (_likeBanSecondsLeft == 0) // must be a few seconds left 
                        {
                            if (enableVoices) C_voice_core.speak($"like ban in place for {_likeBanSecondsLeft} more seconds");
                        }
                        else
                        {
                            if (enableVoices) C_voice_core.speak($"like ban in place for {_likeBanminutesLeft} more minute{(_likeBanminutesLeft > 1 ? "s" : "")}");
                        }

                    }
                    else
                    {

                        // LIKE (do last as it opens a popup that stops us seeing the commenting in action)
                        foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                        {
                            if (obj.Text.ToUpper().Contains("LIKE") && !obj.Text.ToUpper().Contains("UNLIKE"))
                            {
                                if (enableVoices) C_voice_core.speak($"liking");
                                obj.Click();
                                if (enableVoices) C_voice_core.speak($"done");
                                // testing new database functionality
                                //new Classes.C_DataLayer().SaveInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_"), date_last_liked = DateTime.Now });
                                Thread.Sleep(1 * 1000); // wait a amount of time for page to change
                                if (obj.Text.ToUpper().Contains("LIKE") && !obj.Text.ToUpper().Contains("UNLIKE"))
                                {
                                    // like failed
                                    if (enableVoices) C_voice_core.speak($"like failed, I will stop liking for {banLength} minutes.");
                                    likingBannedUntil = DateTime.Now.AddMinutes(banLength);
                                }
                                break;
                            }
                        }
                    }


                    //// go to activity page and follow back anyone that followed us
                    //var minutesLeft = (Properties.Settings.Default.stopFolowingUntilDate - DateTime.Now).Minutes;
                    //var secondsLeft = (Properties.Settings.Default.stopFolowingUntilDate - DateTime.Now).Seconds;
                    //if (secondsLeft > 0)
                    //{
                    //    if (minutesLeft == 0) // must be a few seconds left 
                    //    {
                    //        if (enableVoices) C_voice_core.speak($"follow ban in place for {secondsLeft} more seconds");
                    //    }
                    //    else
                    //    {
                    //        if (enableVoices) C_voice_core.speak($"follow ban in place for {minutesLeft} more minute{(minutesLeft > 1 ? "s" : "")}");
                    //    }
                    //}
                    //else
                    //{
                    //IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/accounts/activity/");
                    //Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time for page to change

                    //foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                    //{
                    //    if (obj.Text.ToLower().Trim().Contains("follow"))
                    //    {
                    //        try
                    //        {
                    //            obj.Click();
                    //            Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time between clicks

                    //            // if following failed dont keep trying
                    //            if (!obj.Text.ToLower().Trim().Contains("following") && !obj.Text.ToLower().Trim().Contains("requested"))
                    //            {

                    //                if (enableVoices) C_voice_core.speak($"following failed, I will stop following for {banLength} minutes.");

                    //                //new Classes.C_DataLayer().SetConfigValueFor("stopFolowingUntilDate", DateTime.Now.AddMinutes(banLength).ToString(Classes.C_DataLayer.SQLiteDateTimeFormat));

                    //                break;
                    //            }
                    //        }
                    //        catch
                    //        {
                    //            if (enableVoices) C_voice_core.speak($"follow failed");
                    //        }
                    //    }
                    //}
                    // }

                    // end go to activity page and follow back anyone that followed us



                    //// go to activity page and follow back anyone that followed us
                    //var _unfollowBanMinutesLeft = (Properties.Settings.Default.stopUnFollowingUntilDate - DateTime.Now).Minutes;
                    //var _unfollowBansecondsLeft = (Properties.Settings.Default.stopUnFollowingUntilDate - DateTime.Now).Seconds;
                    //if (secondsLeft > 0)
                    //{
                    //    if (_unfollowBanMinutesLeft == 0) // must be a few seconds left 
                    //    {
                    //        if (enableVoices) C_voice_core.speak($"unfollow ban in place for {_unfollowBansecondsLeft} more seconds");
                    //    }
                    //    else
                    //    {
                    //        if (enableVoices) C_voice_core.speak($"unfollow ban in place for {_unfollowBanMinutesLeft} more minute{(_unfollowBanMinutesLeft > 1 ? "s" : "")}");
                    //    }
                    //}
                    //else
                    //{

                    // unfollow people , we don't care if they follow back or not
                    // go to https://www.instagram.com/g.stuart/
                    // find link with followers in text and click it
                    // scan page for a tags with following in the text and click to unfollow

                    //IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");
                    //Thread.Sleep(4 * 1000); // wait a amount of time for page to change

                    //foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                    //{
                    //    if (obj.GetAttribute("href").Contains("following")
                    //        && obj.GetAttribute("href").ToLower().Contains(username))
                    //    {
                    //        obj.Click(); // bring up follow list
                    //        Thread.Sleep(2 * 1000); // wait a amount of time for page to change

                    //        foreach (var obj2 in IwebDriver.FindElements(By.TagName("button")))
                    //        {
                    //            if (obj2.Text.ToLower().Trim().Contains("following"))
                    //            {
                    //                try
                    //                {
                    //                    obj2.Click();
                    //                    Thread.Sleep(new Random().Next(secondsBetweenActions_min, secondsBetweenActions_max) * 1000); // wait a short(random) amount of time between clicks

                    //                    // if unfollow failed dont keep trying
                    //                    if (obj2.Text.ToLower().Trim().Contains("following"))
                    //                    {

                    //                        if (enableVoices) C_voice_core.speak($"unfollow failed, I will stop unfollowing for {banLength} minutes.");



                    //                        //new Classes.C_DataLayer().SetConfigValueFor("stopUnFollowingUntilDate", DateTime.Now.AddMinutes(banLength).ToString(Classes.C_DataLayer.SQLiteDateTimeFormat));

                    //                        break;
                    //                    }
                    //                }
                    //                catch
                    //                {
                    //                    if (enableVoices) C_voice_core.speak($"unfollow failed");
                    //                }
                    //            }
                    //        }


                    //        break;
                    //    }
                    //}



                }

                // end unfollow people that dont follow back


                if (enableVoices) C_voice_core.speak($"all done {user}, let's check your stats");

                // Return to users profile page so they can see their stats while we wait for next search to start
                IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");


                //TODO: when testing on a new account with no profile image (may be unrelated) the stats below are not found, need to figure out why. Have increased wait to from 3 to 4 seconds to see if that helps.
                Thread.Sleep(4 * 1000); // wait a amount of time for page to change

                string followers = "";
                foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                {
                    if (obj.GetAttribute("href").Contains("followers")
                        && obj.GetAttribute("href").ToLower().Contains(username))
                    {
                        followers = obj.FindElement(By.TagName("span")).Text.Replace(",", "").Replace(" ", "").Replace("followers", "");
                        break;
                    }
                }


                string following = "";
                foreach (var obj in IwebDriver.FindElements(By.TagName("a")))
                {
                    if (obj.GetAttribute("href").Contains("following")
                        && obj.GetAttribute("href").ToLower().Contains(username))
                    {
                        following = obj.FindElement(By.TagName("span")).Text.Replace(",", "").Replace(" ", "").Replace("following", "");
                        break;
                    }
                }

                if (enableVoices) C_voice_core.speak($"You have {followers} followers and are following {following}. Well done, but I take all the credit.");

                if (enableVoices) C_voice_core.speak($"Let's take a short break.");

                Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000);// wait between each bulk action
            }


            /* end of MAIN LOOP */

        }

















        private DateTime CommentOnPost(string username, bool enableVoices, int banLength, int secondsBetweenActions_min, int secondsBetweenActions_max, List<string> phrasesToComment, DateTime commentingBannedUntil)
        {
            // START COMMENTING
            // check if we are banned from commenting
            var _commentBanminutesLeft = (commentingBannedUntil - DateTime.Now).Minutes;
            var _commentBanSecondsLeft = (commentingBannedUntil - DateTime.Now).Seconds;
            if (_commentBanSecondsLeft > 0)
            {

                if (_commentBanSecondsLeft == 0) // must be a few seconds left 
                {
                    if (enableVoices) C_voice_core.speak($"comment ban in place for {_commentBanSecondsLeft} more seconds");
                }
                else
                {
                    if (enableVoices) C_voice_core.speak($"comment ban in place for {_commentBanminutesLeft} more minute{(_commentBanminutesLeft > 1 ? "s" : "")}");
                }
            }
            else
            {

                // COMMENT - this is usually the first thing to be blocked if you reduce time delays, you will see "posting fialed" at bottom of screen.
                // pick a random comment
                // {USERNAME} get's replaced with @USERNAME
                // {DAY} get's replaced with today's day .g: MONDAY, TUESDAY etc..
                var myComment = phrasesToComment[new Random().Next(0, phrasesToComment.Count - 1)].Replace("{USERNAME}", "@" + username.Replace("{DAY}", "@" + DateTime.Now.ToString("dddd")));
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
                //TODO: posts with comments disabled cause the bot to stall
                // make the comment
                foreach (var obj in IwebDriver.FindElements(By.TagName("textarea")))
                {
                    if (obj.GetAttribute("placeholder").ToUpper().Contains("COMMENT".ToUpper()))
                    {
                        if (enableVoices) C_voice_core.speak($"commenting");
                        bool sendKeysFailed = true;// must start as true

                        int attempsToComment = 0;
                        while (sendKeysFailed && attempsToComment < 3)
                        {
                            attempsToComment++;
                            try
                            {
                                obj.SendKeys(myComment); // put comment in textarea
                                break;
                            }
                            catch (Exception e)
                            {

                                if (e.Message.Contains("element not visible"))
                                { // comments disbaled on post, nothing to wory about

                                }
                                else if (e.Message.Contains("character"))
                                {
                                    if (enableVoices) C_voice_core.speak($"The comment {myComment} contains an unsupported character, i'll remove it from the list.");
                                    sendKeysFailed = true; // some characters are not supported by chrome driver (some emojis for example)
                                    phrasesToComment.Remove(myComment); // remove offending comment
                                }
                                else
                                {   // other unknown error, relay full error message but dont remove comment from list as it may be perfectly fine.
                                    if (enableVoices) C_voice_core.speak($"error with a comment, the error was {e.Message}. The comment {myComment} will be removed from the list.");
                                    sendKeysFailed = true; // some characters are not supported by chrome driver (some emojis for example)
                                }

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

                        //TODO: posts with comments disabled cause the bot to stall, moving this here should fix it
                        // check if comment failed, if yes remove that comment from our comments list
                        if (IwebDriver.PageSource.ToUpper().Contains("couldn't post comment".ToUpper()))
                        {
                            if (enableVoices) C_voice_core.speak($"comment failed, I will stop commenting for {banLength} minutes.");
                            commentingBannedUntil = DateTime.Now.AddMinutes(banLength);
                            //new Classes.C_DataLayer().SetConfigValueFor("stopCommentingUntilDate", DateTime.Now.AddMinutes(banLength).ToString(Classes.C_DataLayer.SQLiteDateTimeFormat));
                        }
                        break;
                    }
                }
            }
            // END COMMENTING
            return commentingBannedUntil;
        }













        public void TerminateBot()
        {
            try { IwebDriver.Close(); } catch { }
            try { IwebDriver.Quit(); } catch { }
        }

        public void Dispose()
        {
            
        }
    }
}
