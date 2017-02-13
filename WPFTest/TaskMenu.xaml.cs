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
    /// Interaction logic for TaskMenu.xaml
    /// </summary>
    public partial class TaskMenu : Window
    {
        public TaskMenu()
        {
            InitializeComponent();
        }

        private void cmbProjectSelector_MouseEnter(object sender, MouseEventArgs e)
        {
            //If a user hovers over the combo box handle this in the modelview
            ModelView.taskListComboBoxHandler(cmbProjectSelector);
        }

        private void btnSaveTask_Click(object sender, RoutedEventArgs e)
        {
            /*
             * This method identifies the project being used as well as other task data, and calls the database handler to add them to the database
             */
            int projectID = 0;
            int priority = 0;
            //Identify what project is being used and update task priorities
            foreach (Project project in ModelView.projectList)
            {
                if (project.x == cmbProjectSelector.SelectedValue.ToString())
                {
                    projectID = project.ID;
                    priority = ModelView.nextTaskPriority(project);
                }
            }
            string taskName = txtTaskName.Text.ToString();
            string taskDescription = txtTaskDescription.Text.ToString();
            double taskLength = Convert.ToDouble(txtTaskLength.Text.ToString());
            //Is this the first task for this project? If so add the start date
            if (priority == 1)
            {
                DatabaseHandler.addNewTask(taskName, taskDescription, taskLength, projectID, priority, DateTime.Now);
            }
            else
            {
                DatabaseHandler.addNewTask(taskName, taskDescription, taskLength, projectID, priority);
            }
        }
    }
}
