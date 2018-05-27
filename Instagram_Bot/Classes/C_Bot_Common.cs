using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;

namespace Instagram_Bot.Classes
{

    class C_Bot_Common
    {

        private IWebDriver _IwebDriver = null;
        string user = Environment.UserName.Replace(".", " ").Replace(@"\", "");

        public C_Bot_Common(IWebDriver iwebDriver)
        {
            _IwebDriver = iwebDriver;
        }
        public C_Bot_Common()
        {
            throw new Exception("You can't do that, use C_Bot_Common(IWebDriver iwebDriver)");
        }
        public void LogInToInstagram(string username, string password, bool enableVoices)
        {

            _IwebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/logout/");
            _IwebDriver.Manage().Cookies.DeleteAllCookies(); //logout
            Thread.Sleep(1 * 1000); // wait for page to change
            _IwebDriver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            Thread.Sleep(1 * 1000); // wait for page to change


            foreach (var button in _IwebDriver.FindElements( By.ClassName("coreSpriteDismissLarge") ) ) // dismiss cookie policy
            {
                button.Click();
                Thread.Sleep(1 * 1000); // wait for page to change          
            }


            Thread.Sleep(3 * 1000); // wait for page to change
            foreach (var link in _IwebDriver.FindElements(By.Name("a"))) // click switch accounts link
            {
                if (link.GetAttribute("href").Contains("javascript:;"))
                {

                    C_voice_core.speak($"clicking switch user");


                    link.Click();
                    Thread.Sleep(1 * 1000); // wait for page to change
                }
            }

            // if (enableVoices) c_voice_core.speak($"let's connect to Instagram");
            if (password.Length < 4)
            {
                if (enableVoices) C_voice_core.speak($"You have one minute to complete login");
                Thread.Sleep(60 * 1000); // wait for page to change
            }
            else
            {



                Thread.Sleep(3 * 1000); // wait for page to change
                foreach (var link in _IwebDriver.FindElements(By.Name("a"))) // click switch accounts link
                {
                    if (link.GetAttribute("href") == @"javascript:;")
                    {

                        C_voice_core.speak($"clicking switch user");

                        link.Click();
                        Thread.Sleep(1 * 1000); // wait for page to change
                    }
                }


                _IwebDriver.FindElement(By.Name("username")).SendKeys(username);
                _IwebDriver.FindElement(By.Name("password")).SendKeys(password);
                _IwebDriver.FindElement(By.TagName("form")).Submit();
                Thread.Sleep(4 * 1000); // wait for page to change
                                        // end Log in to Instagram
            }


            if (_IwebDriver.PageSource.Contains("your password was incorrect"))
            {
                if (enableVoices) C_voice_core.speak($"You have one minute to complete login");
                Thread.Sleep(60 * 1000); // wait for page to change
            }
            else if (_IwebDriver.PageSource.Contains("security") || _IwebDriver.PageSource.Contains("Unusual"))
            {
                if (enableVoices) C_voice_core.speak($"You have one minute to complete login");
                Thread.Sleep(60 * 1000); // wait for page to change
            }
            else
            {
                if (enableVoices) C_voice_core.speak($"We are in, awesome");
            }
        }

        public void GetStats(string username, bool enableVoices)
        {
            // start get stats
            if (enableVoices) C_voice_core.speak($"ok {user}, let's check your stats");
            // Return to users profile page so they can see their stats while we wait for next search to start
            _IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");
            //TODO: when testing on a new account with no profile image (may be unrelated) the stats below are not found, need to figure out why. Have increased wait to from 3 to 4 seconds to see if that helps.
            Thread.Sleep(4 * 1000); // wait a amount of time for page to change
            string followers = "";
            foreach (var obj in _IwebDriver.FindElements(By.TagName("a")))
            {
                if (obj.GetAttribute("href").Contains("followers")
                    && obj.GetAttribute("href").ToLower().Contains(username))
                {
                    followers = obj.FindElement(By.TagName("span")).Text.Replace(",", "").Replace(" ", "").Replace("followers", "");
                    break;
                }
            }
            string following = "";
            foreach (var obj in _IwebDriver.FindElements(By.TagName("a")))
            {
                if (obj.GetAttribute("href").Contains("following")
                    && obj.GetAttribute("href").ToLower().Contains(username))
                {
                    following = obj.FindElement(By.TagName("span")).Text.Replace(",", "").Replace(" ", "").Replace("following", "");
                    break;
                }
            }

            string posts = "";
            foreach (var obj in _IwebDriver.FindElements(By.TagName("li")))
            {
                if (obj.Text.Contains(" posts"))
                {
                    posts = obj.Text.Replace(",", "").Replace(" ", "").Replace("posts", "");
                    break;
                }
            }


            // check scraped stat/followers/following data is valid
            if (int.TryParse(followers, out int _followers)
                && int.TryParse(following, out int _following)
                && int.TryParse(posts, out int _posts)
                )
            {
                // testing new database functionality
                new Classes.C_DataLayer().SaveCurrentStats(followers: _followers, following: _following, posts: _posts);
            }

            if (enableVoices) C_voice_core.speak($"You have a total of {posts} posts, {followers} followers and are following {following}. Well done, but I take all the credit.");
            // end get stats
        }

        public DateTime CommentOnPost(string username, bool enableVoices, int banLength, int secondsBetweenActions_min, int secondsBetweenActions_max, List<string> phrasesToComment, DateTime commentingBannedUntil, string instagram_post_user)
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
                foreach (var obj in _IwebDriver.FindElements(By.TagName("a")))
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
                foreach (var obj in _IwebDriver.FindElements(By.TagName("textarea")))
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
                                obj.SendKeys(myComment + " #FreeTommyRobinson"); // put comment in textarea
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
                        _IwebDriver.FindElement(By.TagName("form")).Submit(); // Only one form on page, so submit it to comment.
                        Thread.Sleep(3 * 1000); // wait a short(random) amount of time for page to change
                        //TODO: posts with comments disabled cause the bot to stall, moving this here should fix it
                        // check if comment failed, if yes remove that comment from our comments list
                        if (_IwebDriver.PageSource.ToUpper().Contains("couldn't post comment".ToUpper()))
                        {
                            if (enableVoices) C_voice_core.speak($"comment failed, I will stop commenting for {banLength} minutes.");
                            commentingBannedUntil = DateTime.Now.AddMinutes(banLength);
                        }
                        else
                        {
                            // commenting worked
                            // testing new database functionality
                            new Classes.C_DataLayer().SaveInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_"), date_last_commented = DateTime.Now });
                        }
                        break;
                    }
                }
            }
            // END COMMENTING
            return commentingBannedUntil;
        }

        public void CreateInstagramPost(bool enableVoices)
        {
            foreach (var obj1 in _IwebDriver.FindElements(By.TagName("input")))
            {
                if (obj1.GetAttribute("type") == "file")
                {
                    if (enableVoices) C_voice_core.speak($"setting image");
                    obj1.SendKeys(@"C:\test.png");
                    Thread.Sleep(5 * 1000); // wait for page to change
                    if (enableVoices) C_voice_core.speak($"uploading");
                    _IwebDriver.FindElement(By.TagName("form")).Submit();  // seems to submit but image does not apear on page, maybe it's the wrong form.
                    Thread.Sleep(5 * 1000); // wait for page to change
                    foreach (var obj2 in _IwebDriver.FindElements(By.TagName("button")))
                    {
                        if (obj2.Text.ToUpper().Contains("next"))
                        {
                            if (enableVoices) C_voice_core.speak($"next");
                            obj2.Click();
                            Thread.Sleep(5 * 1000); // wait for page to change
                        }
                        else if (obj2.Text.ToUpper().Contains("share"))
                        {
                            if (enableVoices) C_voice_core.speak($"share");
                            obj2.Click();
                            Thread.Sleep(5 * 1000); // wait for page to change
                            break;
                        }
                    }
                    break;
                }
            }
            if (enableVoices) C_voice_core.speak($"done");
            Thread.Sleep(60 * 1000); // wait for page to change     
        }

        public void BulkUnfollow(string username, bool enableVoices, int banLength)
        {     
            while (true)
            {
                _IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/{username}");
                Thread.Sleep(2 * 1000); // wait a amount of time for page to change
                foreach (var obj in _IwebDriver.FindElements(By.TagName("a")))
                {
                    if (obj.GetAttribute("href").Contains("following") && obj.GetAttribute("href").ToLower().Contains(username)) // find the `following` link
                    {
                        obj.Click(); // bring up follow list
                        Thread.Sleep(1 * 1000); // wait a amount of time for page to change
                        foreach (var _li in _IwebDriver.FindElements(By.TagName("li")))
                        {
                            //if (enableVoices) C_voice_core.speak($"l i found");
                            try
                            {
                                if (!_li.FindElement(By.TagName("button")).Text.ToLower().Trim().Contains("following"))
                                {
                                    //if (enableVoices) C_voice_core.speak($"no following button");
                                    continue;
                                }
                                else
                                {
                                    //if (enableVoices) C_voice_core.speak($"li found");
                                    var _unfollow_button = _li.FindElement(By.TagName("button"));
                                    if (_unfollow_button.Text.ToLower().Trim().Contains("following"))
                                    {
                                        //if (enableVoices) C_voice_core.speak($"following button found");
                                        try
                                        {
                                            // get username
                                            var _insta_user_name = _li.FindElement(By.TagName("a")).GetAttribute("href").Replace("https://www.instagram.com", "").ToLower().Replace("/", "").ToLower().Trim();
                                            if (enableVoices) C_voice_core.speak($"unfollowing {_insta_user_name}");
                                            _unfollow_button.Click();
                                            Thread.Sleep(1 * 1000); // wait a short amount of time between clicks
                                            if (_unfollow_button.Text.ToLower().Trim().Contains("following"))
                                            {
                                                if (enableVoices) C_voice_core.speak($"unfollow failed");
                                                break;
                                            }
                                            new C_DataLayer().SaveInstaUser(IU: new InstaUser() { username = _insta_user_name, date_unfollowed = DateTime.Now });
                                        }
                                        catch
                                        {
                                            if (enableVoices) C_voice_core.speak($"unfollow failed error");
                                        }
                                    }                                    
                                }
                            }
                            catch (NoSuchElementException ne)
                            {
                                Debug.Print(ne.Message);
                            }
                        }
                        break;
                    }
                }
            }
        }

        public void FollowSuggested(bool enableVoices, int banLength, DateTime followingBannedUntil)
        {
            foreach (var obj in _IwebDriver.FindElements(By.TagName("button")))
            {
                if (obj.Text.ToLower().Trim().Contains("follow"))
                {
                    try
                    {
                        obj.Click();
                        Thread.Sleep(2 * 1000);
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
        }

        public void LikePost(bool enableVoices, int banLength, DateTime likingBannedUntil, string instagram_post_user)
        {
            foreach (var obj in _IwebDriver.FindElements(By.TagName("a")))
            {
                if (obj.Text.ToUpper().Contains("LIKE") && !obj.Text.ToUpper().Contains("UNLIKE"))
                {
                    if (enableVoices) C_voice_core.speak($"liking");
                    obj.Click();
                    if (enableVoices) C_voice_core.speak($"done");
                    new Classes.C_DataLayer().SaveInstaUser(IU: new Classes.InstaUser() { username = instagram_post_user.Replace(" ", "_"), date_last_liked = DateTime.Now });
                    Thread.Sleep(1 * 1000);
                    if (obj.Text.ToUpper().Contains("LIKE") && !obj.Text.ToUpper().Contains("UNLIKE"))
                    {
                        // like failed, or another window popped up
                        if (enableVoices) C_voice_core.speak($"like failed, I will stop liking for {banLength} minutes.");
                        likingBannedUntil = DateTime.Now.AddMinutes(banLength);
                    }
                    break;
                }
            }
        }

        public void BulkFollowBack(bool enableVoices, int banLength, DateTime followingBannedUntil)
        {
            // go to activity page and follow back anyone that followed us
            var minutesLeft = (followingBannedUntil - DateTime.Now).Minutes;
            var secondsLeft = (followingBannedUntil - DateTime.Now).Seconds;
            if (secondsLeft > 0)
            {
                if (minutesLeft == 0) // must be a few seconds left 
                {
                    if (enableVoices) C_voice_core.speak($"follow ban in place for {secondsLeft} more seconds");
                }
                else
                {
                    if (enableVoices) C_voice_core.speak($"follow ban in place for {minutesLeft} more minute{(minutesLeft > 1 ? "s" : "")}");
                }
            }
            else
            {
                _IwebDriver.Navigate().GoToUrl($"https://www.instagram.com/accounts/activity/");
                Thread.Sleep(2 * 1000);
                foreach (var obj in _IwebDriver.FindElements(By.TagName("button")))
                {
                    if (obj.Text.ToLower().Trim().Contains("follow"))
                    {
                        try
                        {
                            obj.Click();
                            Thread.Sleep(2 * 1000);
                            // if following failed dont keep trying
                            if (!obj.Text.ToLower().Trim().Contains("following") && !obj.Text.ToLower().Trim().Contains("requested"))
                            {
                                if (enableVoices) C_voice_core.speak($"following failed");
                                break;
                            }
                        }
                        catch
                        {
                            if (enableVoices) C_voice_core.speak($"follow failed error");
                        }
                    }
                }
            }
        }

        public void FollowPostUser(bool enableVoices, int banLength, DateTime commentingBannedUntil, DateTime followingBannedUntil, string instagram_post_user)
        {
            // FOLLOW
            foreach (var obj in _IwebDriver.FindElements(By.TagName("button")))
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
        }

    }
}
