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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void btnSaveHours_Click(object sender, RoutedEventArgs e)
        {
            /*
             * This event handler binds the form information to variables and calls the database to add new working hours to the current user's profile
             */
            double mondayHours = Convert.ToDouble(txtMonday.Text.ToString());
            double tuesdayHours = Convert.ToDouble(txtTuesday.Text.ToString());
            double wednesdayHours = Convert.ToDouble(txtWednesday.Text.ToString());
            double thursdayHours = Convert.ToDouble(txtThursday.Text.ToString());
            double fridayHours = Convert.ToDouble(txtFriday.Text.ToString());
            double saturdayHours = Convert.ToDouble(txtSaturday.Text.ToString());
            double sundayHours = Convert.ToDouble(txtSunday.Text.ToString());

            DatabaseHandler.addHoursToProfile(mondayHours, tuesdayHours, wednesdayHours, thursdayHours, fridayHours, saturdayHours, sundayHours, LoginHandler.username);

        }
    }
}
