using System;
using System.IO;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Instagram_Bot.Classes
{
    internal class C_DataLayer
    {
        public C_DataLayer()
        {
            CreateAppFolder();
            //InitiateDatabase();
        }
        //Environment.SpecialFolder.ApplicationData will put data in a location that persists after clicckone updates
        private static string SQLiteFile = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Dorset Dev Studio\Databases\Data.db3";
        private static string SQLiteConnString = $@"Data Source={SQLiteFile};Version=3;UTF8Encoding=True;";
        public readonly static string SQLiteDateTimeFormat = "yyyy-MM-dd HH:mm:ss";// DO NOT CHANGE
        private string SQLiteNullDateString = "0001-01-01 00:00:00";// DO NOT CHANGE
        private void CreateAppFolder()
        {
            if (!File.Exists(SQLiteFile))
            {
                if (!Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Dorset Dev Studio"))
                {
                    Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Dorset Dev Studio");
                }
                if (!Directory.Exists($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Dorset Dev Studio\Databases"))
                {
                    Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Dorset Dev Studio\Databases");
                }
                InitiateDatabase();
            }
        }
        // add new instagram user just by username
        public bool AddInstaUser(InstaUser IU)
        {
            try
            {
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    // C_voice_core.speak("db AddInstaUser");
                    // only add if not already there
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("insert into insta_users (username, date_created,date_last_updated) select @username, @datetime, @date_last_updated " +
                    "WHERE NOT EXISTS(SELECT 1 FROM insta_users WHERE username = @username);", _conn))
                    {
                        SQLcommand.Parameters.AddWithValue("username", IU.username);
                        SQLcommand.Parameters.AddWithValue("datetime", DateTime.Now.ToString(SQLiteDateTimeFormat));
                        SQLcommand.Parameters.AddWithValue("date_last_updated", DateTime.Now.ToString(SQLiteDateTimeFormat));
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }
                }
            }
            catch (InvalidOperationException se)
            {
                C_voice_core.speak($"SQL Error: {se.Message}");
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
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    // C_voice_core.speak("db SaveInstaUser");
                    // now we know they exist, we can update all other fields
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("update insta_users set " +
                    "date_followed_them             = (case when @date_followed_them = @SQLiteNullDateString then date_followed_them else @date_followed_them end), " +
                    "date_unfollowed_them           = (case when @date_unfollowed_them = @SQLiteNullDateString then date_unfollowed_them else @date_unfollowed_them end), " +
                    "date_followed_back_detected    = (case when @date_followed_back_detected = @SQLiteNullDateString then date_followed_back_detected else @date_followed_back_detected end), " +
                    "date_last_commented            = (case when @date_last_commented = @SQLiteNullDateString then date_last_commented else @date_last_commented end), " +
                    "date_last_liked                = (case when @date_last_liked = @SQLiteNullDateString then date_last_liked else @date_last_liked end), " +
                    "date_last_updated              = @date_last_updated " +
                    "where username=@username " +
                    "", _conn))
                    {
                        SQLcommand.Parameters.AddWithValue("username", IU.username);
                        SQLcommand.Parameters.AddWithValue("date_followed_them", IU.date_followed_them != null ? IU.date_followed_them.ToString(SQLiteDateTimeFormat) : "");
                        SQLcommand.Parameters.AddWithValue("date_unfollowed_them", IU.date_unfollowed != null ? IU.date_unfollowed.ToString(SQLiteDateTimeFormat) : "");
                        SQLcommand.Parameters.AddWithValue("date_followed_back_detected", IU.date_followed_back_detected != null ? IU.date_followed_back_detected.ToString(SQLiteDateTimeFormat) : "");
                        SQLcommand.Parameters.AddWithValue("date_last_commented", IU.date_last_commented != null ? IU.date_last_commented.ToString(SQLiteDateTimeFormat) : "");
                        SQLcommand.Parameters.AddWithValue("date_last_liked", IU.date_last_liked != null ? IU.date_last_liked.ToString(SQLiteDateTimeFormat) : "");
                        SQLcommand.Parameters.AddWithValue("date_last_updated", DateTime.Now.ToString(SQLiteDateTimeFormat));
                        SQLcommand.Parameters.AddWithValue("SQLiteNullDateString", SQLiteNullDateString);
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }
                }
            }
            catch (InvalidOperationException se)
            {
                C_voice_core.speak($"SQL Error: {se.Message}");
                IU.error = se.Message;
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
            return true;
        }
        public InstaUser GetInstaUser(InstaUser IU)
        {
            C_voice_core.speak("database GetInstaUser");
            try
            {
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    // IU passed in is just a username, populate all other fields then return
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("select * from insta_users WHERE username = @username);", _conn))
                    {
                        SQLcommand.Parameters.AddWithValue("username", IU.username);
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
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
                            else
                            {
                                IU.error = "no record for user";
                                C_voice_core.speak(IU.error);
                            }
                            _conn.Close();
                        }
                    }
                }
            }
            catch (InvalidOperationException se)
            {
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
            return IU;
        }
        public string GetConfigValueFor(string name)
        {
            C_voice_core.speak("database GetConfigValueFor");
            try
            {
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("select value from config WHERE name=@name limit 1;", _conn))
                    {
                        SQLcommand.Parameters.AddWithValue("name", name);
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        using (SQLiteDataReader rdr = SQLcommand.ExecuteReader())
                        {
                            if (rdr.Read()) // there can only ever be 1 row
                            {
                                return rdr["value"].ToString();
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException se)
            {
                C_voice_core.speak($"SQL Error: {se.Message}");
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
            return null;
        }
        //upserts a config value
        public void SetConfigValueFor(string name, string value)
        {
            C_voice_core.speak("database SetConfigValueFor");
            try
            {
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("insert into config (name, value, date_created) select @name, @value, @date " +
                "where not exists (select 1 from config where name=@name); " +
                "update config set value = @value, date_changed = @date where name=@name and value != @value; ", _conn))
                    {
                        SQLcommand.Parameters.AddWithValue("name", name);
                        SQLcommand.Parameters.AddWithValue("date", DateTime.Now.ToString(SQLiteDateTimeFormat));
                        if (string.IsNullOrEmpty(value))
                            SQLcommand.Parameters.AddWithValue("value", DBNull.Value);
                        else
                            SQLcommand.Parameters.AddWithValue("value", value);
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }
                }
            }
            catch (InvalidOperationException se)
            {
                C_voice_core.speak($"SQL Error: {se.Message}");
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
        }
        //TODO: create database schema
        private void InitiateDatabase()
        {       
            // C_voice_core.speak("db create table insta_users");
            try
            {
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    // DO NOT ALTER TABLES
                    // create insta_users table
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " +
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
                ");", _conn))
                    {
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }


                    //  C_voice_core.speak("db create table config");
                    // create config table (name / values)
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " +
                       "config" +
                       "(" +
                           "name varchar(50) PRIMARY KEY not null," +
                           "value varchar(255) not null," +
                           "date_created TEXT not null," +
                           "date_changed TEXT null" +
                       ");", _conn))
                    {
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }
                    //  C_voice_core.speak("db create table stat_log");
                    // create stat_log
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("CREATE TABLE IF NOT EXISTS " +
                       "stat_log" +
                       "(" +
                           "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                           "followers INTEGER null," +
                           "following INTEGER null," +
                           "posts INTEGER null," +
                           "datetime TEXT not null" +
                       ");", _conn))
                    {
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }
                }
            }
            catch (InvalidOperationException se)
            {
                C_voice_core.speak($"SQL Error: {se.Message}");
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
        }
        internal void SaveCurrentStats(int followers, int following, int posts)
        {
            try
            {
                
                using (SQLiteConnection _conn = new SQLiteConnection(SQLiteConnString, true))
                {
                    using (SQLiteCommand SQLcommand = new SQLiteCommand("insert into stat_log (followers, following, posts, datetime) " + "select @followers, @following, @posts, @datetime ", _conn))
                    {
                        SQLcommand.Parameters.AddWithValue("followers", followers);
                        SQLcommand.Parameters.AddWithValue("following", following);
                        SQLcommand.Parameters.AddWithValue("posts", posts);
                        SQLcommand.Parameters.AddWithValue("datetime", DateTime.Now.ToString(SQLiteDateTimeFormat));
                        if (_conn.State != System.Data.ConnectionState.Open) _conn.Open();
                        SQLcommand.ExecuteNonQuery();
                        _conn.Close();
                    }
                }
                //C_voice_core.speak("saved stats to database");
            }
            catch (InvalidOperationException se)
            {
                C_voice_core.speak($"SQL Error: {se.Message}");
                System.Windows.Forms.MessageBox.Show($"SQL Error: {se.Message}");
            }
        }
        public static void TestDatabase(bool enableVoices)
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
        }
    }
}
