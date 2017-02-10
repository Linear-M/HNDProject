using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTest
{
    class LoginHandler
    {
        public static string username;
        public static string password;
        public static bool loggedIn = false;

        public static void logout()
        {
            username = "";
            password = "";
            loggedIn = false;
        }

    }
}
