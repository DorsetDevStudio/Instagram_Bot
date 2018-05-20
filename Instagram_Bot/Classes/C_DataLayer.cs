using System;
using System.Data.SQLite;
using System.Deployment.Application;

namespace Instagram_Bot.Classes
{
    internal class C_DataLayer : IDisposable
    {

        public C_DataLayer()
        {
            MakeConnection();
            InitiateDatabase();// todo, only do this once
        }

        // db file in end users working directory, will be created if does not exist
        private string SQLiteFile = "Data.db";

        private SQLiteConnection conn = new SQLiteConnection();

        private void MakeConnection()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                // When debugging use the local debugging database
                if (System.IO.File.Exists(@"c:\DebuggingData.db"))
                {
                    SQLiteFile = @"c:\DebuggingData.db";
                }
            }
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.ConnectionString = $"Data Source={SQLiteFile};Version=3;UseUTF16Encoding=True;";
                conn.Open();
            }
        }


        // add new instagram user just by username
        public bool AddInstaUser(InstaUser IU)
        {
            // only add if not already there
            SQLiteCommand SQLcommand = new SQLiteCommand("" +
                "insert into insta_users (username, date_created) select @username, @datetime " +
                "WHERE NOT EXISTS(SELECT 1 FROM insta_users WHERE username = @username);", conn);
            SQLcommand.Parameters.AddWithValue("username", IU.username);
            SQLcommand.Parameters.AddWithValue("datetime", DateTime.Now.ToString("yyyy-MM-dd"));
            SQLcommand.ExecuteNonQuery();
            return true;
        }


        public bool SaveInstaUser(InstaUser IU)
        {
            // start by calling AddInstaUser,which will create it if not exists
            AddInstaUser(IU);
            // now we know they exist, we can update all other fields
            SQLiteCommand SQLcommand = new SQLiteCommand("" +
                "update insta_users set " +
                "date_followed=@date_followed," +
                "date_followed_back_detected=@date_followed_back_detected," +
                "date_last_commented=@date_last_commented," +
                "date_last_liked=@date_last_liked" +
                "where username=@username" +
                "", conn);
            SQLcommand.Parameters.AddWithValue("username", IU.username);
            SQLcommand.Parameters.AddWithValue("date_followed", IU.date_followed != null ? IU.date_followed.ToString("yyyy-MM-dd") : "");
            SQLcommand.Parameters.AddWithValue("date_followed_back_detected", IU.date_followed_back_detected != null ? IU.date_followed_back_detected.ToString("yyyy-MM-dd") : "");
            SQLcommand.Parameters.AddWithValue("date_last_commented", IU.date_last_commented != null ? IU.date_last_commented.ToString("yyyy-MM-dd") : "");
            SQLcommand.Parameters.AddWithValue("date_last_liked", IU.date_last_liked != null ? IU.date_last_liked.ToString("yyyy-MM-dd") : "");
            SQLcommand.ExecuteNonQuery();
            return true;
        }


        public InstaUser GetInstaUser(InstaUser IU)
        {
            // IU passed in is just a username, populate all other fields then return
            using (SQLiteCommand SQLcommand = new SQLiteCommand("select * from insta_users WHERE username = @username);", conn))
            {
                SQLcommand.Parameters.AddWithValue("username", IU.username);
                using (SQLiteDataReader rdr = SQLcommand.ExecuteReader())
                {
                    if (rdr.Read()) // there can only ever be 1 row
                    {
                        if (DateTime.TryParse(rdr["date_created"].ToString(), out DateTime _date_created))
                            IU.date_created = _date_created;
                        if (DateTime.TryParse(rdr["date_followed"].ToString(), out DateTime _date_followed))
                            IU.date_followed = _date_followed;
                        if (DateTime.TryParse(rdr["date_followed_back_detected"].ToString(), out DateTime _date_followed_back_detected))
                            IU.date_followed_back_detected = _date_followed_back_detected;
                        if (DateTime.TryParse(rdr["date_last_commented"].ToString(), out DateTime _date_last_commented))
                            IU.date_last_commented = _date_last_commented;
                        if (DateTime.TryParse(rdr["date_last_liked"].ToString(), out DateTime _date_last_liked))
                            IU.date_last_liked = _date_last_liked;
                    }
                    else {
                        IU.error = "no record for user";
                    }
                }
            }
            return IU;
        }

        //TODO: create database schema
        private void InitiateDatabase()
        {

            // DO NOT ALTER TABLES

            // create insta_users table
            SQLiteCommand SQLcommand = new SQLiteCommand("" +
                "CREATE TABLE IF NOT EXISTS " +
                "insta_users" +
                "(" +
                    "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "username varchar(255) not null," +
                    "date_created TEXT not null," +
                    "date_followed_them TEXT null," +
                    "date_followed_back_detected TEXT null," +
                    "date_last_commented TEXT null," +
                    "date_last_liked TEXT null" +
                ");", conn);
            SQLcommand.ExecuteNonQuery();


            // crate config table (name / values)
            SQLcommand = new SQLiteCommand("" +
               "CREATE TABLE IF NOT EXISTS " +
               "config" +
               "(" +
                   "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                   "name varchar(255) not null," +
                   "value varchar(255) not null," +
                   "date_created TEXT not null," +
                   "date_changed TEXT null" +
               ");", conn);
            SQLcommand.ExecuteNonQuery();

        }

        public void Dispose()
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    conn.Close();
                    conn.Dispose();
                }
                catch { }
            }
        }

    }
}
