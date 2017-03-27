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
            get
            {
               return new MySqlConnection(string.Format("Server=86.157.83.39; database=betterproject; UID=Linear; password=password", "betterproject"));
            }
            set
            {
                _Connection = value;
            }
        }

        public bool IsConnected()
        {
            bool result = true;
            if (Connection == null)
            {
                    result = false;
                    Connection.Open();
                    result = true;
            }
            return result;
        }

    }
}
