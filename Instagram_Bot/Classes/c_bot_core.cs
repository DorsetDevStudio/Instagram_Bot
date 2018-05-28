using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Instagram_Bot.Classes;
using System.Windows.Forms;

namespace Instagram_Bot
{
    public class C_bot_core : IC_bot_core, IDisposable
    {
        IWebDriver IwebDriver;
        string user = Environment.UserName.Replace(".", " ").Replace(@"\", "").Split(' ')[0];
        int secondsBetweenActions_min = 1;
        int secondsBetweenActions_max = 1;
        int minutesBetweenBulkActions_min = 0;
        int minutesBetweenBulkActions_max = 0;
        int maxPostsPerSearch = 1000;
        bool _sleeping = false;
        public enum bot_mode { search_follow_comment_like, follow, comment, like, unfollow, post, direct_message }

        public C_bot_core(int bot_id, bot_mode mode, string username, string password, bool stealthMode = false, bool enableVoices = true, List<timeSpans> sleepTimes = null, int banLength = 5)
        {

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
            // not reuqired and cause some login issues
            // var profiledir = $@"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data\Default";
            //options.AddArgument($@"user-data-dir={profiledir}");
            IwebDriver = new ChromeDriver(options);

            if (bot_id == 1)
                IwebDriver.Manage().Window.FullScreen();


            if (bot_id == 1 && mode == bot_mode.unfollow)
            {
                IwebDriver.Manage().Window.Maximize();
            }
            var core = new C_Bot_Common(IwebDriver);
            var thingsToSearch = new List<string>()
            {
                "UK","london","dorset","bournemouth","britain","greatbritain","sun","tan","poole","luton","birmingham","liverpool","manchenter","scotland","england","ireland"
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
                "I likes it! #FreeTommyRobinson",
                "#nice :) #FreeTommyRobinson",
                "#interesting, where is that? ",
                "Perfection, you should be a #photographer!",
                "#Perfection, you've missed your calling!",
                "#Perfection, almost looks professional! #FreeTommyRobinson",
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

            Thread.Sleep(2 * 1000);


            //core.CreateInstagramPost(enableVoices);
            /* MAIN LOOP */
            // record stats before we start so we can monitor performance for every session the bot is runing
            core.GetStats(username, enableVoices);
            if (mode == bot_mode.unfollow)
            {
                while (true)
                {
                    core.BulkUnfollow(username, enableVoices, banLength);
                }
            }
            if (mode == bot_mode.search_follow_comment_like)
            {

                Thread.Sleep(2 * 1000);

                // loop forever, performing a new search and then following, liking and spamming the hell out of everyone.
                while (true)
                {

                    C_DataLayer.TestDatabase(enableVoices);

                    DateTime commentingBannedUntil = DateTime.Now;
                    DateTime followingBannedUntil = DateTime.Now;
                    DateTime unfollowingBannedUntil = DateTime.Now;
                    DateTime likingBannedUntil = DateTime.Now;

                    //core.FollowSuggected(enableVoices, banLength, followingBannedUntil);
                    var mySearch = thingsToSearch[new Random().Next(0, thingsToSearch.Count - 1)];
                    if (enableVoices) C_voice_core.speak($"Ok, let's get some followers");
                    // just navigate to search
                    IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/explore/tags/{mySearch}");
                    Thread.Sleep(4 * 1000);                                                                                                              // save results
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

                        //we may need to sleep, need to check if we should be bwtewwn each post
                        //handle `don't run between` times       
                        if (!_sleeping) // check if we should be
                        {
                            foreach (timeSpans timeSpan in sleepTimes)
                            {
                                if (DateTime.Now.TimeOfDay > timeSpan.from.TimeOfDay
                                    && DateTime.Now.TimeOfDay < timeSpan.to.TimeOfDay)
                                {
                                    _sleeping = true;
                                    if (enableVoices) C_voice_core.speak($"I'm tired, yawn, sleeping until {timeSpan.to.ToShortTimeString()}");
                                    break;
                                }
                            }
                        }
                        while (_sleeping) // check if we shouldnt be
                        {
                            bool _sleep = false;
                            foreach (timeSpans timeSpan in sleepTimes)
                            {
                                if (DateTime.Now.TimeOfDay > timeSpan.from.TimeOfDay
                                    && DateTime.Now.TimeOfDay < timeSpan.to.TimeOfDay)
                                {
                                    _sleep = true;
                                    break;
                                }
                            }
                            if (!_sleep) // just woke up
                            {
                                if (enableVoices) C_voice_core.speak($"Nap over, damn it");
                                _sleeping = false;
                            }
                            Thread.Sleep(1 * 1000);// sleep 1 second
                            Application.DoEvents();
                        }
                        postCounter++;
                        if (link.Contains("https://www.instagram.com/"))
                        {
                            IwebDriver.Navigate().GoToUrl(link);
                        }
                        else
                        {
                            IwebDriver.Navigate().GoToUrl("https://www.instagram.com/" + link);
                        }
                        Thread.Sleep(2 * 1000);

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
                        followingBannedUntil = core.FollowPostUser(enableVoices, banLength, followingBannedUntil, instagram_post_user);
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

                           likingBannedUntil  = core.LikePost(enableVoices, banLength, likingBannedUntil, instagram_post_user);
                        }


                        // todo: test and perfect intercation with activity page
                        // core.BulkFollowBack(enableVoices, banLength, followingBannedUntil);

                    }
                    // end unfollow people that dont follow back
                    core.GetStats(username, enableVoices);
                    if (enableVoices) C_voice_core.speak($"Let's take a short break.");
                    Thread.Sleep(new Random().Next(minutesBetweenBulkActions_min, minutesBetweenBulkActions_max) * 60000);// wait between each bulk action
                }
                /* end of MAIN LOOP */
            }
            else
            {
                throw new Exception("bot mode not implemented");
            }


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
