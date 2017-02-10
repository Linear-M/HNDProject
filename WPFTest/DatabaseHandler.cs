using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;

namespace WPFTest
{
    class DatabaseHandler
    {

        static OleDbConnection Conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source=ScheduleDatabase.accdb;");
        static OleDbCommand command;

        public static void addNewProject(string Username, string ProjectName, string ProjectDescription, double ProjectLength, DateTime DateStarted, DateTime prjDueDate)
        {
            string SQL = "INSERT INTO tblproject(Username,ProjectName,ProjectDescription,ProjectLength,DateStarted,DateDue) VALUES(@UN,@PN,@PD,@PL,@DS,@DD)";
            command = new OleDbCommand(SQL, Conn);
            try
            {
                command.Parameters.AddWithValue("@UN", Username);
                command.Parameters.AddWithValue("@PN", ProjectName);
                command.Parameters.AddWithValue("@PD", ProjectDescription);
                command.Parameters.AddWithValue("@PL", ProjectLength);
                command.Parameters.AddWithValue("@DS", ConvertToDateFormatAccessLikes(DateStarted));
                command.Parameters.AddWithValue("@DD", ConvertToDateFormatAccessLikes(prjDueDate));
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to bind project information to SQL variables");
            }

            /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
            therefore, if affectedRows>0, insertion has been completed successfully*/
            Conn.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                MessageBox.Show("Hours updated");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("New hours failed - check that all project criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }

        public static void loadProjects(string username)
        {
            //This SQL/connection retruns all relevant project information(s) for the currently logged-in user
            string SQL = "SELECT ProjectID, ProjectName, ProjectDescription, DateStarted, DateDue, ProjectLength FROM tblProject WHERE Username='" + username + "'";
            OleDbCommand command = new OleDbCommand(SQL, Conn);

            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }
            Conn.Open();

            OleDbDataReader newReader = command.ExecuteReader();

            //While there is information stored in the connection buffer, add the projects (as data points/bars) to the chart
            while (newReader.Read())
            {
                string xPoint = newReader["ProjectName"].ToString();
                double yPoint = Convert.ToDouble(newReader["ProjectLength"].ToString());
                string projectDescription = newReader["ProjectDescription"].ToString();
                DateTime dateStarted = Convert.ToDateTime(newReader["DateStarted"].ToString());
                DateTime dateDue = Convert.ToDateTime(newReader["DateDue"].ToString());
                double projectLength = yPoint;
                int projectID = Convert.ToInt32(newReader["ProjectID"]);
                //Create a new project from the raw data output from the database, add it to the project list in the model-view
                ModelView.projectList.Add(new Project(xPoint, yPoint, projectDescription, dateStarted, dateDue, projectLength, projectID));
            }

            Conn.Close();


            foreach (Project project in ModelView.projectList)
            {
                int projectID = project.ID;
                loadNewTasks(project);
                Trace.WriteLine("Updating project length: " + projectID.ToString());
                updateProjectLength(projectID);
                foreach (Task task in project.taskList)
                {
                    Trace.WriteLine("Checking task '" + task.TaskName + "' for completion...");
                    TimeHandler.checkTaskComplete(task);
                }
            }

        }

        public static void login(string username, string password)
        {
            if (LoginHandler.loggedIn)
            {
                MessageBox.Show("Already logged in - please logout first");
            }
            else
            {
                string sql = "SELECT COUNT(*) FROM tblUser WHERE Username='" + username + "' and Password='" + password + "'";
                command = new OleDbCommand(sql, Conn);

                Conn.Open();

                int result = (int)command.ExecuteScalar();

                if (result > 0)
                {
                    MessageBox.Show("Login Successful");
                    LoginHandler.username = username;
                    LoginHandler.password = password;
                    LoginHandler.loggedIn = true;
                    Conn.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect Credentials - Try Again - " + username + " - " + password);
                    LoginHandler.loggedIn = false;
                    Conn.Close();
                }
            }
        }

        public static void addHoursToProfile(double monday, double tuesday, double wednesday, double thursday, double friday, double saturday, double sunday, string username)
        {
            string SQL = "UPDATE tblUser SET MondayHours = @MH, TuesdayHours = @TH, WednesdayHours = @WH, ThursdayHours = @THH, FridayHours = @FH, SaturdayHours = @SS, SundayHours = @SUH WHERE Username = @UNAME";
            MessageBox.Show(SQL);
            command = new OleDbCommand(SQL, Conn);
            try
            {
                command.Parameters.AddWithValue("@MH", monday);
                command.Parameters.AddWithValue("@TH", tuesday);
                command.Parameters.AddWithValue("@WH", wednesday);
                command.Parameters.AddWithValue("@THH", thursday);
                command.Parameters.AddWithValue("@FH", friday);
                command.Parameters.AddWithValue("@SS", saturday);
                command.Parameters.AddWithValue("@SUH", sunday);
                command.Parameters.AddWithValue("@UNAME", username);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to bind information to SQL variables");
            }

            /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
            therefore, if affectedRows>0, insertion has been completed successfully*/
            Conn.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                MessageBox.Show("Working hours updated");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("Hours addition failed - check that all criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }

        public static void getWorkingHours(string username)
        {
            //This SQL/connection retruns all relevant project information(s) for the currently logged-in user
            string SQL = "SELECT MondayHours,TuesdayHours,WednesdayHours,ThursdayHours,FridayHours,SaturdayHours,SundayHours FROM tblUser WHERE Username='" + username + "'";
            OleDbCommand command = new OleDbCommand(SQL, Conn);

            try
            {
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                }
                Conn.Open();
                OleDbDataReader newReader = command.ExecuteReader();

                //While there is information stored in the connection buffer, add the projects (as data points/bars) to the chart
                while (newReader.Read())
                {
                    TimeHandler.Monday = Convert.ToDouble(newReader["MondayHours"].ToString());
                    TimeHandler.Tuesday = Convert.ToDouble(newReader["TuesdayHours"].ToString());
                    TimeHandler.Wednesday = Convert.ToDouble(newReader["WednesdayHours"].ToString());
                    TimeHandler.Thursday = Convert.ToDouble(newReader["ThursdayHours"].ToString());
                    TimeHandler.Friday = Convert.ToDouble(newReader["FridayHours"].ToString());
                    TimeHandler.Saturday = Convert.ToDouble(newReader["SaturdayHours"].ToString());
                    TimeHandler.Sunday = Convert.ToDouble(newReader["SundayHours"].ToString());
                }
                Conn.Close();
            }
            catch (Exception)
            {

            }
        }

        public static void loadNewTasks(Project project)
        {
            //SQL grabs the needed information to display task information, based on the clicked project
            string SQL = "SELECT TaskID, TaskName, TaskDescription, TaskLength, Priority FROM tblTask WHERE ProjectID=" + project.ID + "";
            OleDbCommand command = new OleDbCommand(SQL, Conn);
            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }
            Conn.Open();
            OleDbDataReader newReader = command.ExecuteReader();

            //[If the project contains tasks] clear the chart and add task information data points/bars
            if (newReader.HasRows)
            {
                while (newReader.Read())
                {
                    //[While there are results left in the command buffer] plot each result on the graph
                    string xPoint = newReader["TaskName"].ToString();
                    double yPoint = Convert.ToDouble(newReader["TaskLength"].ToString());
                    string description = newReader["TaskDescription"].ToString();
                    int priority = Convert.ToInt32(newReader["Priority"].ToString());
                    int taskID = Convert.ToInt32(newReader["TaskID"].ToString());
                    project.taskList.Add(new Task(xPoint, yPoint, description, priority, project.ID, taskID));
                }
            }
            Conn.Close();
        }

        public static void addNewTask(string TaskName, string TaskDescription, double TaskLength, int ProjectID, int Priority)
        {

            string SQL = "INSERT INTO tblTask(TaskName, TaskDescription, TaskLength, ProjectID, Priority) VALUES(@TN,@TD,@TL,@PID,@PR)";
            command = new OleDbCommand(SQL, Conn);
            try
            {
                command.Parameters.AddWithValue("@TN", TaskName);
                command.Parameters.AddWithValue("@TD", TaskDescription);
                command.Parameters.AddWithValue("@TL", TaskLength);
                command.Parameters.AddWithValue("@PID", ProjectID);
                command.Parameters.AddWithValue("@PR", Priority);

            }
            catch (Exception)
            {
                MessageBox.Show("Failed to bind project information to SQL variables");
            }

            /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
            therefore, if affectedRows>0, insertion has been completed successfully*/
            Conn.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                MessageBox.Show("Task added successfully");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("New hours failed - check that all project criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();

            //Update project length
            updateProjectLength(ProjectID);
        }

        public static void addNewTask(string TaskName, string TaskDescription, double TaskLength, int ProjectID, int Priority, DateTime StartDate)
        {

            string SQL = "INSERT INTO tblTask(TaskName, TaskDescription, TaskLength, ProjectID, Priority, StartDate) VALUES(@TN,@TD,@TL,@PID,@PR,@SD)";
            command = new OleDbCommand(SQL, Conn);
            try
            {
                command.Parameters.AddWithValue("@TN", TaskName);
                command.Parameters.AddWithValue("@TD", TaskDescription);
                command.Parameters.AddWithValue("@TL", TaskLength);
                command.Parameters.AddWithValue("@PID", ProjectID);
                command.Parameters.AddWithValue("@PR", Priority);
                command.Parameters.AddWithValue("@SD", ConvertToDateFormatAccessLikes(StartDate));

            }
            catch (Exception)
            {
                MessageBox.Show("Failed to bind project information to SQL variables");
            }

            /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
            therefore, if affectedRows>0, insertion has been completed successfully*/
            Conn.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                MessageBox.Show("Task added successfully");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("New hours failed - check that all project criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();

            //Update project length
            updateProjectLength(ProjectID);
        }

        private static double getProjectHoursFromTasks(int projectID)
        {
            double projectLength = 0;
            Project currProject = null;

            foreach (Project project in ModelView.projectList)
            {
                if (project.ID == projectID)
                {
                    currProject = project;
                }
            }

            foreach (Task task in currProject.taskList)
            {
                projectLength += task.TaskLength;
            }

            Trace.WriteLine("Project: " + currProject.x + " - Length: " + projectLength.ToString());

            return projectLength;
        }

        private static void updateProjectLength(int projectID)
        {
            double projectLength = 0;
            Project currProject = null;

            foreach (Project project in ModelView.projectList)
            {
                if (project.ID == projectID)
                {
                    currProject = project;
                }
            }

            projectLength = getProjectHoursFromTasks(projectID);

            if (projectLength > 0)
            {
                string SQL = "UPDATE tblProject SET ProjectLength = @LENGTH WHERE ProjectID = @PID";
                //MessageBox.Show(SQL);
                command = new OleDbCommand(SQL, Conn);
                try
                {
                    command.Parameters.AddWithValue("@LENGTH", projectLength);
                    command.Parameters.AddWithValue("@PID", currProject.ID);
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed to bind information to SQL variables");
                }

                /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
                therefore, if affectedRows>0, insertion has been completed successfully*/
                Conn.Open();
                if (command.ExecuteNonQuery() > 0)
                {
                    //MessageBox.Show("Project length updated");
                }
                else
                {
                    //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                    MessageBox.Show("Project Length Updating failied - check that all criteria have a correct entry");
                }
                //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
                Conn.Close();
            }
        }

        public static void updateTaskPriorities(Project project)
        {
            if (project.projectLength > 0)
            {
                foreach (Task task in project.taskList)
                {
                    string SQL = "UPDATE tblTask SET Priority = @PRI WHERE ProjectID = @PID AND TaskID = @TSKID";
                    //MessageBox.Show(SQL);
                    command = new OleDbCommand(SQL, Conn);
                    try
                    {
                        command.Parameters.AddWithValue("@PRI", task.Priority);
                        command.Parameters.AddWithValue("@PID", task.ProjectID);
                        command.Parameters.AddWithValue("@TSKID", task.TaskID);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to bind information to SQL variables");
                    }

                    /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
                    therefore, if affectedRows>0, insertion has been completed successfully*/
                    Conn.Open();
                    if (command.ExecuteNonQuery() > 0)
                    {
                        //MessageBox.Show("Project length updated");
                    }
                    else
                    {
                        //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                        MessageBox.Show("Task priority update failed - check that all criteria have a correct entry");
                    }
                    //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
                    Conn.Close();
                }

            }
        }

        private static DateTime ConvertToDateFormatAccessLikes(DateTime d)
        {
            //Will return dd/mm/yyyy OR dd/mm/yyyy hh:mm:ss
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
        }


    }
}
