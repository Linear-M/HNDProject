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
            DatabaseHandler.login(txtUsername.Text.ToString(), txtPassword.Password.ToString());
            if (LoginHandler.loggedIn)
            {
                btnLogout.Opacity = 1;
                btnLogin.Opacity = 0.5;
                Close();
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginHandler.logout();
            btnLogin.Opacity = 1;
            btnLogout.Opacity = 0.5;
        }
    }
}
