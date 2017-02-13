using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTest
{
    class LoginHandler
    {
        //Many encapsulated variables
        private static string _username, _password, _email;
        private static bool _loggedIn = false;

        public static string username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }
        public static string password
        {
            get{
                return _password;
            }
            set
            {
                _password = value;
            }
        }
        public static string email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
            }
        }
        public static bool loggedIn
        {
            get
            {
                return _loggedIn;
            }
            set
            {
                _loggedIn = value;
            }
        }

        public static void logout()
        {
            /*
             * Clear the stored username, password and email as well as setting the program to 'not-logged-in' mode
             */
            username = "";
            password = "";
            email = "";
            loggedIn = false;
        }

    }
}
