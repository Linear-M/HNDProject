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
    /// Interaction logic for FileMenu.xaml
    /// </summary>
    public partial class FileMenu : Window
    {
        public FileMenu()
        {
            InitializeComponent();
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
                double projectLength = Convert.ToDouble(txtProjectLength.Text.ToString());
                prjDueDate = dpkrProjectStartDate.SelectedDate.Value.Date;
                DateTime startDate = DateTime.Now.Date;
                //Move the data to the model
                DatabaseHandler.addNewProject("Ben", prjName, prjDescription, projectLength, startDate, prjDueDate);

            }
            catch (Exception)
            {
                MessageBox.Show("Error creating new project - incorrect data entry");
            }

        }

        private void PART_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void btnTaskView_Click(object sender, RoutedEventArgs e)
        {
            //Open a new task menu window
            TaskMenu tskMenu = new TaskMenu();
            tskMenu.Show();
        }
    }
}
