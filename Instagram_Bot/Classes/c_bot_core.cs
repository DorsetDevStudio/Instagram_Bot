using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Instagram_Bot.Classes;


namespace Instagram_Bot
{
    public class C_bot_core : IC_bot_core, IDisposable
    {

        IWebDriver IwebDriver;
        string user = Environment.UserName.Replace(".", " ").Replace(@"\", "").Split(' ')[0];
        int secondsBetweenActions_min = 1;
        int secondsBetweenActions_max = 1;
        int minutesBetweenBulkActions_min = 1;
        int minutesBetweenBulkActions_max = 2;
        int maxPostsPerSearch = 10;


        public C_bot_core(int bot_id,string username, string password, bool stealthMode = false, bool enableVoices = true, List<timeSpans> sleepTimes = null, int banLength = 5)
        {

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");

            IwebDriver = new ChromeDriver(options);
            var core = new C_Bot_Common(IwebDriver);

            var thingsToSearch = new List<string>()
            {
                "summer", "chill","youtube","content","vlogger","losangeles","travel","interesting",
                "newcontent","moresoon","funvideo","sofun","discover","adventure","videowhat","avgeek","aviation",
                "work","life","beauty","snow","winter","michigan","fly","awesomehow","fashion","star",
                "style","film","me","swagger","photooftheday","instamood"
            };

            // add tags based of the current day
            if (DateTime.Now.ToString("dddd").ToLower() == "sunday")
            {
                thingsToSearch.AddRange(
                    new List<string> {
                        DateTime.Now.ToString("dddd") + "lunch",
                        DateTime.Now.ToString("dddd") + "roast"
                    });
            }
            if (DateTime.Now.ToString("dddd").ToLower() == "wednesday")
            {
                thingsToSearch.AddRange(
                    new List<string> {
                         "humpday",
                    });
            }
            if (DateTime.Now.ToString("dddd").ToLower() == "friday")
            {
                thingsToSearch.AddRange(
                    new List<string> {
                         "fridayfeeling",

                    });
            }
            if (DateTime.Now.ToString("dddd").ToLower() == "monday")
            {
                thingsToSearch.AddRange(
                    new List<string> {
                         "hatemondays",

                    });
            }
            // end add tags based of the current day


            if (File.Exists(@"c:\hashtags.txt"))
            {
                thingsToSearch.Clear();// just use users hashtags
                foreach (var line in File.ReadLines(@"c:\hashtags.txt"))
                    if (!thingsToSearch.Contains(line.Replace("#", "").Trim()))
                        thingsToSearch.Add(line.Replace("#", "").Trim());
                if (enableVoices) C_voice_core.speak($"{thingsToSearch.Count} hashtags loaded from hashtags file");
            }
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
                "Perfection, you've missed your calling! @" + username,
                "#Perfection, almost looks #professional! @" + username,
                "interesting approach me thinks @" + username,
                "Wish I could take #photos like yours! @" + username,
                "It's #" + DateTime.Now.ToString("dddd") + " people @" + username,
                "#Happy " + DateTime.Now.ToString("dddd") + " everybody :) from @" + username,
                "I likes it!",
                "#nice :)",
                "#interesting, where is that?",
                "Perfection, you should be a #photographer!",
                "#Perfection, you've missed your calling!",
                "#Perfection, almost looks professional!",
                "haha, interesting approach me thinks",
                "Wish I could take #photos like yours!",
                "It's #" + DateTime.Now.ToString("dddd") + " people",
                "#Happy " + DateTime.Now.ToString("dddd") + " everybody :)",
            };
            if (File.Exists(@"c:\comments.txt"))
            {
                phrasesToComment.Clear();// only use users comments if provided
                foreach (var line in File.ReadLines(@"c:\comments.txt"))
                    if (!phrasesToComment.Contains(line.Replace("#", "").Trim()))
                        phrasesToComment.Add(line.Replace("#", "").Trim());
                if (enableVoices) C_voice_core.speak($"{phrasesToComment.Count} comments loaded from comments file");
            }
            if (File.Exists(@"c:\ignore_comments.txt"))
                foreach (var line in File.ReadLines(@"c:\ignore_comments.txt"))
                    phrasesToComment.Remove(line);
            /* END CONFIG */


            core.LogInToInstagram(username, password, enableVoices);


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


            // record stats before we start so we can monitor performance for every session the bot is runing
            core.GetStats(username, enableVoices);


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
                    if (enableVoices) C_voice_core.speak($"SQLite invalid operation error {ee.InnerException.Message}", true);
                    MessageBox.Show($"SQLite invalid operation error {ee.InnerException.Message}");
                }
                catch (Exception ee)
                {
                    if (enableVoices) C_voice_core.speak($"SQLite error {ee.InnerException.Message}", true);
                    MessageBox.Show($"SQLite error {ee.InnerException.Message}");
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
                                break;
                            }
                        }
                        catch
                        {
                            if (enableVoices) C_voice_core.speak($"follow failed");
                        }
                    }
                }
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
                    // testing new database functionality
                    new Classes.C_DataLayer().AddInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_") });
                    // FOLLOW
                    foreach (var obj in IwebDriver.FindElements(By.TagName("button")))
                    {
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
                                new Classes.C_DataLayer().SaveInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_"), date_followed_them = DateTime.Now });
                            }
                            else
                            {
                                commentingBannedUntil = DateTime.Now.AddMinutes(banLength);
                                if (enableVoices) C_voice_core.speak($"following failed, I will stop following for {banLength} minutes.");
                            }
                            Thread.Sleep(2 * 1000); // wait and see it it worked, will change to following
                            break;
                        }
                    }
                    // end FOLLOW
                    commentingBannedUntil = core.CommentOnPost(username, enableVoices, banLength, secondsBetweenActions_min, secondsBetweenActions_max, phrasesToComment, commentingBannedUntil, instagram_post_user);
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
                                new Classes.C_DataLayer().SaveInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_"), date_last_liked = DateTime.Now });
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


                core.GetStats(username, enableVoices);


                if (enableVoices) C_voice_core.speak($"Let's take a short break.");

                Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000);// wait between each bulk action
            }
            /* end of MAIN LOOP */
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
