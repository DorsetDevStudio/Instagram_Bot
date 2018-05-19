using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Instagram_Bot.Classes
{

    public struct InstaUser
    {
        public string username;
        public DateTime date_created;
        public DateTime date_followed;
        public DateTime date_followed_back_detected;
        public DateTime date_last_commented;
        public DateTime date_last_liked;
      
    }

    class C_InstaUsers : IC_InstaUsers
    {
        
        public C_InstaUsers(InstaUser instaUser)
        {
            _instaUser = instaUser;
        }

        public InstaUser _instaUser;

        public string ProfilePageURL()
        {
            return $"https://www.instagram.com/{_instaUser.username.ToLower().Trim()}";
        }

    }


}
