using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFTest
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Log the user in based on their information and change opacity
             */
            DatabaseHandler.login(txtUsername.Text.ToString(), txtPassword.Password.ToString());
            checkOpacity();
            checkEnabled();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            /*
             * Log the user out and change opacity
             */
            LoginHandler.logout();
            checkOpacity();
            checkEnabled();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Check opacity on each load of the login form
            checkOpacity();
            checkEnabled();
        }

        private void checkOpacity()
        {
            /*
             * Changes logout/login button opacity based on login status
             */
            if (LoginHandler.loggedIn)
            {
                btnLogout.Opacity = 1;
                btnLogin.Opacity = 0.5;
            }
            else
            {
                btnLogin.Opacity = 1;
                btnLogout.Opacity = 0.5;
            }
        }

        private void checkEnabled()
        {
            /*
             * Makes relevant login/logout buttons clickable or not dependant on if the user is logged in
             */
            if (LoginHandler.loggedIn)
            {
                btnLogin.IsEnabled = false;
                btnLogout.IsEnabled = true;
            } else
            {
                btnLogout.IsEnabled = false;
                btnLogin.IsEnabled = true;
            }
        }
    }
}
