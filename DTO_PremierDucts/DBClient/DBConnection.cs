using System;
using MySql.Data.MySqlClient;

namespace DTO_PremierDucts.DBClient
{
    public class DBConnection
    {
        public string ConnectionString { get; set; }
        private DBConnection(string connectionstring)
        {
            ConnectionString = connectionstring;
        }

        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static DBConnection _instance = null;
        public static DBConnection Instance(string connectionstring)
        {
            if (_instance == null)
                _instance = new DBConnection(connectionstring);
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (string.IsNullOrEmpty(ConnectionString))
                    return false;

                Connection = new MySqlConnection(ConnectionString);
                DatabaseName = Connection.Database;
                Connection.Open();
            }

            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
