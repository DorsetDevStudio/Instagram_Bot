using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DateTime date_followed;
        public DateTime date_followed_back_detected;
        public DateTime date_last_commented;
        public DateTime date_last_liked;     
    }
    
}
