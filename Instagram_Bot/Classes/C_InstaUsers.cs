using System;

namespace Instagram_Bot.Classes
{

    public class InstaUser
    {
        /// <summary>
        /// always null unless there was an error getting user from database
        /// </summary>
        public string error = null;
        public string username;
        public DateTime date_created;
        public DateTime date_followed_them;
        public DateTime date_followed_back_detected;
        public DateTime date_last_commented;
        public DateTime date_last_liked;
        public DateTime date_unfollowed;
    }
    
}
