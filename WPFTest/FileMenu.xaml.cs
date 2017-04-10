using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Interaction logic for FileMenu.xaml
    /// </summary>
    public partial class FileMenu : Window
    {
        public FileMenu()
        {
            //Check there are projects in the constructor
            if (LoginHandler.loggedIn == false)
            {
                //Due to windows weirdisms, the form has to first open before we can force-close
                MessageBox.Show("Please log in first!");
                InitializeComponent();
                this.Close();
            }
            else
            {
                InitializeComponent();
                leftBorder.Background = UI.leftBorderColor;
                leftLabel1.FontFamily = UI.xmlFont;
                leftLabel1.FontSize = 12;
                leftLabel2.FontFamily = UI.xmlFont;
                leftLabel2.FontSize = 12;
                leftLabel3.FontFamily = UI.xmlFont;
                leftLabel2.FontSize = 12;
                this.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            
            DateTime prjDueDate;
            try
            {
                //Obtain new project data
                string prjName = txtProjectName.Text;
                string prjDescription = txtProjectDescription.Text;
                prjDueDate = Convert.ToDateTime(txtDatePicker.Text);
                DateTime startDate = DateTime.Now.Date;
                //Move the data to the model
                DatabaseHandler.addNewProject(LoginHandler.username, prjName, prjDescription, startDate, prjDueDate);

            }
            catch (Exception)
            {
                MessageBox.Show("Error creating new project - incorrect data entry");
            }

        }

        private void btnTaskView_Click(object sender, RoutedEventArgs e)
        {
            //Open a new task menu window
            TaskMenu tskMenu = new TaskMenu();
            tskMenu.Show();
        }
    }
}
