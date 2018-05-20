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
        private string SQLiteDateTimeFormat = "yyyy-MM-dd HH:MM";// DO NOT CHALGE
        private string SQLiteNullDateString = "0001-01-01 00:01";// DO NOT CHALGE

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
            try
            {
                // only add if not already there
                SQLiteCommand SQLcommand = new SQLiteCommand("" +
                "insert into insta_users (username, date_created,date_last_updated) select @username, @datetime, @date_last_updated " +
                "WHERE NOT EXISTS(SELECT 1 FROM insta_users WHERE username = @username);", conn);
                SQLcommand.Parameters.AddWithValue("username", IU.username);
                SQLcommand.Parameters.AddWithValue("datetime", DateTime.Now.ToString(SQLiteDateTimeFormat));
                SQLcommand.Parameters.AddWithValue("date_last_updated", DateTime.Now.ToString(SQLiteDateTimeFormat));
                SQLcommand.ExecuteNonQuery();
            }
            catch (SQLiteException se)
            {
                IU.error = se.Message;
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
            return true;
        }




        // REMEBER spaces at end of each line in SQL to avoid SQL error

        public bool SaveInstaUser(InstaUser IU)
        {
            // start by calling AddInstaUser,which will create it if not exists
            AddInstaUser(IU);
            try
            {
                // now we know they exist, we can update all other fields
                SQLiteCommand SQLcommand = new SQLiteCommand("" +
                    "update insta_users set " +
                    "date_followed_them             = (case when @date_followed_them = @SQLiteNullDateString then date_followed_them else @date_followed_them end), " +
                    "date_followed_back_detected    = (case when @date_followed_back_detected = @SQLiteNullDateString then date_followed_back_detected else @date_followed_back_detected end), " +
                    "date_last_commented            = (case when @date_last_commented = @SQLiteNullDateString then date_last_commented else @date_last_commented end), " +
                    "date_last_liked                = (case when @date_last_liked = @SQLiteNullDateString then date_last_liked else @date_last_liked end), " +
                    "date_last_updated              = @date_last_updated " +
                    "where username=@username " +
                    "", conn);
                SQLcommand.Parameters.AddWithValue("username", IU.username);
                SQLcommand.Parameters.AddWithValue("date_followed_them", IU.date_followed_them != null ? IU.date_followed_them.ToString(SQLiteDateTimeFormat) : "");
                SQLcommand.Parameters.AddWithValue("date_followed_back_detected", IU.date_followed_back_detected != null ? IU.date_followed_back_detected.ToString(SQLiteDateTimeFormat) : "");
                SQLcommand.Parameters.AddWithValue("date_last_commented", IU.date_last_commented != null ? IU.date_last_commented.ToString(SQLiteDateTimeFormat) : "");
                SQLcommand.Parameters.AddWithValue("date_last_liked", IU.date_last_liked != null ? IU.date_last_liked.ToString(SQLiteDateTimeFormat) : "");
                SQLcommand.Parameters.AddWithValue("date_last_updated", DateTime.Now.ToString(SQLiteDateTimeFormat));
                SQLcommand.Parameters.AddWithValue("SQLiteNullDateString", SQLiteNullDateString);
                SQLcommand.ExecuteNonQuery();
            }
            catch (SQLiteException se)
            {
                IU.error = se.Message;
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
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
                        if (DateTime.TryParse(rdr["date_followed_them"].ToString(), out DateTime _date_followed_them))
                            IU.date_followed_them = _date_followed_them;
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

            // note: in sqlite INTEGER is not the same as INT in Sql Server, it can be and 8 byte long LONG.
            // note: in sqlite there is not date type, use TEXT or INTEGER and save are formatted datetime string or unix style timestamp, we are using .ToString("YYYY-MM-DD HH:MM:SS.SSS"), 
            // just dates. Except in log tables

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
                    "date_unfollowed_them TEXT null," +
                    "date_followed_back_detected TEXT null," +
                    "date_last_commented TEXT null," +
                    "date_last_liked TEXT null," +
                    "times_followed INTEGER DEFAULT 0," +
                    "times_unfollowed INTEGER DEFAULT 0," +
                    "date_last_updated TEXT not null" +
                ");", conn);
            SQLcommand.ExecuteNonQuery();


            // create config table (name / values)
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



            // create stat_log
            SQLcommand = new SQLiteCommand("" +
               "CREATE TABLE IF NOT EXISTS " +
               "stat_log" +
               "(" +
                   "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                   "followers INTEGER null," +
                   "following INTEGER null," +
                   "posts INTEGER null," +
                   "datetime TEXT not null" +
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
