using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WPFTest
{
    class DatabaseConnection
    {
        //Encapsulated variables
        private string _DatabaseName = "";
        private string _Password = "";
        private MySqlConnection _Connection = null;

        public string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
            set
            {
                _DatabaseName = value;
            }
        }

        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

        public MySqlConnection Connection
        {
            //Returns the default connection strings, setting allowed for future expansion of server editing changes on admin-enabled accounts
            get
            {
                return new MySqlConnection(string.Format("Server=31.50.244.250; database=betterproject; UID=Linear; password=password", "betterproject"));
            }
            set
            {
                _Connection = value;
            }
        }

        public bool IsConnected()
        {
            //Boolean returns true if able to connect on given string, if not false is returned
            //Generate the connection and command objects
            DatabaseConnection DbConnection = new DatabaseConnection();
            MySqlConnection Conn = DbConnection.Connection;
            MySqlCommand command;
            bool result = true;

                //Attempt a simple query
                command = new MySqlCommand("SELECT username FROM tblUser", Conn);
                try
                {
                    //Close connection if one is already established
                    if (Conn.State == System.Data.ConnectionState.Open)
                    {
                        Conn.Close();
                    }
                    Conn.Open();
                    MySqlDataReader newReader = command.ExecuteReader();
                }
                catch
                {
                    result = false;
                }
                return result;
        }
    }
}
