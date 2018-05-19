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

        //TODO: create database schema
        private void InitiateDatabase()
        {
            SQLiteCommand SQLcommand = new SQLiteCommand("" +
                "CREATE TABLE IF NOT EXISTS insta_users (" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "username varchar(255) not null," +
                "date_created TEXT not null," +
                "date_followed_them TEXT null," +
                "date_followed_back_detected TEXT null," +
                "date_last_commented TEXT null," +
                "date_last_liked TEXT null)" +
                ";", conn);
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
