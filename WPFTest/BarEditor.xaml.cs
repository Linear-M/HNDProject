using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for BarEditor.xaml
    /// </summary>
    public partial class BarEditor : Window
    {
        public BarEditor()
        {
            InitializeComponent();
        }

        private void cmbProject_MouseEnter(object sender, MouseEventArgs e)
        {
            //When the control is entered possible reload tasks and the changing information
            ModelView.projectListComboBoxHandler(cmbProject);
        }

        private void cmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //If the user selects a different project, load the correct information
            cmbProject.Text = cmbProject.SelectedItem.ToString();
            //When the control is entered possible reload tasks and the changing information
            ModelView.taskListComboBoxHandler(cmbTask, cmbProject.Text);
            //Take selected item from list and return the task object for information
            Project SelectedProject = ModelView.projectFromName(cmbProject.Text);
            try
            {
                txtProjectName.Text = SelectedProject.x;
                txtProjectDescription.Text = SelectedProject.projectDescription;
                txtProjectStartDate.Text = SelectedProject.dateStarted.ToShortDateString();
            }
            catch (Exception)
            {
                //If this is the first time SelectedProject will be null
                Trace.WriteLine("Error loading selected project");
            }
        }

        private void cmbTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //If the user selects a different task, load the correct information
            try
            {
                cmbTask.Text = cmbTask.SelectedItem.ToString();
                //Take selected item from list and return the task object for information
                Task SelectedTask = ModelView.taskFromName(cmbTask.Text);
                try
                {
                    txtTaskName.Text = SelectedTask.TaskName;
                    txtTaskDescription.Text = SelectedTask.TaskDescription;
                    txtTaskLength.Text = SelectedTask.TaskLength.ToString();
                }
                catch (Exception)
                {
                    //If this is the first time SelectedTask will be null
                    Trace.WriteLine("Error loading selected task");
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("Error loading combo box items for cmbTask");
            }
        }

        private void btnSaveProjectChanges_Click(object sender, RoutedEventArgs e)
        {
            //If user clicks save, edit project accordingly
            DatabaseHandler.editProject(txtProjectName.Text, txtProjectDescription.Text, Convert.ToDateTime(txtProjectStartDate.Text), ModelView.projectFromName(cmbProject.Text));
        }

        private void btnSaveTaskChanges_Click(object sender, RoutedEventArgs e)
        {
            //If user clicks save, edit task accordingly
            DatabaseHandler.editTask(txtTaskName.Text, txtTaskDescription.Text, Convert.ToInt32(txtTaskLength.Text), ModelView.taskFromName(cmbTask.Text));
        }
    }
}
