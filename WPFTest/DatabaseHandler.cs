﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WPFTest
{
    class DatabaseHandler
    {
        //Generate the connection and command objects
        private static DatabaseConnection DbConnection = new DatabaseConnection();
        private static MySqlConnection Conn = DbConnection.Connection;
        private static MySqlCommand command;

        public static void addNewProject(string Username, string ProjectName, string ProjectDescription, DateTime DateStarted, DateTime prjDueDate)
        {
            //Generate SQL (with params) and populate the commmand object
            string SQL = "INSERT INTO tblproject(Username,ProjectName,ProjectDescription,ProjectLength,DateStarted,DateDue) VALUES(@UN,@PN,@PD,@PL,@DS,@DD)";
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add the paramater values to the command objects
                command.Parameters.AddWithValue("@UN", Username);
                command.Parameters.AddWithValue("@PN", ProjectName);
                command.Parameters.AddWithValue("@PD", ProjectDescription);
                command.Parameters.AddWithValue("@PL", 1);
                command.Parameters.AddWithValue("@DS", TimeHandler.ConvertToAccessDateTime(DateStarted));
                command.Parameters.AddWithValue("@DD", TimeHandler.ConvertToAccessDateTime(prjDueDate));
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
                MessageBox.Show("Project Added Successfuly");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("New project failed - check that all project criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }

        public static void addNewUser(string Username, string Password, string FirstName, string Surname, DateTime DateOfBirth, string EMail)
        {
            //Generate SQL (with params) and populate the commmand object
            string SQL = "INSERT INTO tbluser(Username, Password, FirstName, Surname, DateOfBirth, EMail) VALUES(@UN, @PW, @FN, @SN, @DOB, @EM)";
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add the paramater values to the command objects
                command.Parameters.AddWithValue("@UN", Username);
                command.Parameters.AddWithValue("@PW", Password);
                command.Parameters.AddWithValue("@FN", FirstName);
                command.Parameters.AddWithValue("@SN", Surname);
                command.Parameters.AddWithValue("@DOB", TimeHandler.ConvertToAccessDateTime(DateOfBirth));
                command.Parameters.AddWithValue("@EM", EMail);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to bind user information to SQL variables");
            }

            /*Open the connection and execute the insert-into command. This method will return the number of rows that have been affected (in this case, added)
            therefore, if affectedRows>0, insertion has been completed successfully*/
            Conn.Open();
            if (command.ExecuteNonQuery() > 0)
            {
                MessageBox.Show("User Registered Successfuly");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("User registration failed - check that all project criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }

        public static void loadProjects(string username)
        {
            //This SQL/connection retruns all relevant project information(s) for the currently logged-in user
            string SQL = "SELECT ProjectID, ProjectName, ProjectDescription, DateStarted, DateDue, ProjectLength FROM tblProject WHERE Username='" + username + "'";
            MySqlCommand command = new MySqlCommand(SQL, Conn);

            //If a connection is already opened (error check) close it first
            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }
            Conn.Open();

            //Create reader object that 'scans' the SQL return
            MySqlDataReader newReader = command.ExecuteReader();

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

            //Once all projects have been loaded update their lengths and generate their tasks
            foreach (Project project in ModelView.projectList)
            {
                int projectID = project.ID;
                loadNewTasks(project);
                Trace.WriteLine("Updating project length: " + projectID.ToString());
                updateProjectLength(projectID);
                //Check if any of the tasks in the current iteration project have been completed
                foreach (Task task in project.taskList)
                {
                    Trace.WriteLine("Checking task '" + task.TaskName + "' for completion...");
                    TimeHandler.checkTaskComplete(task);
                }
            }

        }

        public static void login(string username, string password)
        {
            /*
             * This method checks to see if a user has a profile in the database, and saves this data within code in order to generate proper task/project lists
             * The email is also saved locally
             */
            if (LoginHandler.loggedIn)
            {
                MessageBox.Show("Already logged in - please logout first");
            }
            else
            {
                //Return number of records with the username and password fields (1 for profile exist/correct combination, 0 for erroneous input or no profile saved)
                string sql = "SELECT COUNT(*) FROM tblUser WHERE Username='" + username + "' and Password='" + password + "'";
                command = new MySqlCommand(sql, Conn);

                Conn.Open();

                int result = Convert.ToInt32(command.ExecuteScalar());

                if (result > 0)
                {
                    //Save the profile information locally
                    MessageBox.Show("Login Successful");
                    LoginHandler.username = username;
                    LoginHandler.password = password;
                    LoginHandler.loggedIn = true;
                    LoginHandler.shouldSendEMail = TimeHandler.shouldSendEmail();
                    
                    //Close database connection
                    Conn.Close();

                    //Select their email (based on username) and save it      
                    sql = "SELECT EMail FROM tblUser WHERE Username='" + username + "'";
                    command = new MySqlCommand(sql, Conn);

                    Conn.Open();

                    MySqlDataReader newReader = command.ExecuteReader();

                    //If there is information stored in the connection buffer, save the email
                    while (newReader.Read())
                    {
                        LoginHandler.email = newReader["EMail"].ToString();
                    }

                    Conn.Close();
                }
                else
                {
                    //Output incorrect entry
                    MessageBox.Show("Incorrect Credentials - Try Again");
                    LoginHandler.loggedIn = false;
                    Conn.Close();
                }
            }
        }

        public static void addHoursToProfile(double monday, double tuesday, double wednesday, double thursday, double friday, double saturday, double sunday, string username)
        {
            /*
             * Takes hours-input monday-->friday and saves them into the database (data is stored in the profile table)
             */ 
            string SQL = "UPDATE tblUser SET MondayHours = @MH, TuesdayHours = @TH, WednesdayHours = @WH, ThursdayHours = @THH, FridayHours = @FH, SaturdayHours = @SS, SundayHours = @SUH WHERE Username = @UNAME";
            //Create command
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add param values
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

        public static void editProject(string ProjectName, string ProjectDescription, DateTime StartDate, Project ProjectToEdit)
        {
            /*
             * Takes new project information and syncs these changes to the database
             */
            string SQL = "UPDATE tblProject SET ProjectName = @PN, ProjectDescription = @PDN, DateStarted = @SD WHERE ProjectID = @PID";
            //Create command
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add param values
                command.Parameters.AddWithValue("@PN", ProjectName);
                command.Parameters.AddWithValue("@PDN", ProjectDescription);
                command.Parameters.AddWithValue("@SD", TimeHandler.ConvertToAccessDateTime(StartDate));
                command.Parameters.AddWithValue("@PID", ProjectToEdit.ID);
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
                MessageBox.Show("Project information updated");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("Project edit failed - check that all criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }

        public static void editTask(string TaskName, string TaskDescription, int TaskLength, Task TaskToEdit)
        {
            /*
             * Takes new task information and syncs these changes to the database
             */
            string SQL = "UPDATE tblTask SET TaskName = @TN, TaskDescription = @TDN, TaskLength = @TL WHERE TaskID = @TID";
            //Create command
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add param values
                command.Parameters.AddWithValue("@TN", TaskName);
                command.Parameters.AddWithValue("@TDN", TaskDescription);
                command.Parameters.AddWithValue("@TL", TaskLength);
                command.Parameters.AddWithValue("@TID", TaskToEdit.TaskID);
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
                MessageBox.Show("Task information updated");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("Task edit failed - check that all criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }

        public static void getWorkingHours(string username)
        {
            //This SQL/connection retruns all relevant project information(s) for the currently logged-in user
            string SQL = "SELECT MondayHours,TuesdayHours,WednesdayHours,ThursdayHours,FridayHours,SaturdayHours,SundayHours FROM tblUser WHERE Username='" + username + "'";
            MySqlCommand command = new MySqlCommand(SQL, Conn);

            try
            {
                //Close connection if one is already established
                if (Conn.State == System.Data.ConnectionState.Open)
                {
                    Conn.Close();
                }
                Conn.Open();
                MySqlDataReader newReader = command.ExecuteReader();

                //While there is information stored in the connection buffer, add the time data to the timehandler class
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
            catch (Exception e)
            {
                Trace.WriteLine("Error when saving time data, stack: " + e.ToString());
            }
        }

        public static void loadNewTasks(Project project)
        {
            //SQL grabs the needed information to display task information, based on the clicked project
            string SQL = "SELECT TaskID, ProjectID, TaskName, TaskDescription, TaskLength, Priority, StartDate FROM tblTask WHERE ProjectID=" + project.ID + "";
            MySqlCommand command = new MySqlCommand(SQL, Conn);
            //If a connection is open close it
            if (Conn.State == System.Data.ConnectionState.Open)
            {
                Conn.Close();
            }
            Conn.Open();
            MySqlDataReader newReader = command.ExecuteReader();
            
            //if the project has tasks
            if (newReader.HasRows)
            {
                while (newReader.Read())
                {
                    //[While there are results left in the command buffer] save the relevant info
                    string xPoint = newReader["TaskName"].ToString();
                    double yPoint = Convert.ToDouble(newReader["TaskLength"].ToString());
                    string description = newReader["TaskDescription"].ToString();
                    int priority = Convert.ToInt32(newReader["Priority"].ToString());
                    int taskID = Convert.ToInt32(newReader["TaskID"].ToString());
                    int projectID = Convert.ToInt32(newReader["ProjectID"].ToString());
                    DateTime dateStarted;
                    try
                    {
                        //Attempt to convert the Access date/time data to the .NET standard
                        dateStarted = Convert.ToDateTime(newReader["StartDate"]);
                        Trace.WriteLine("Task DateStarted: " + dateStarted.ToShortDateString());
                    }
                    catch (Exception)
                    {
                        //If there is no date started entry this means the task is not a priority and will have a dateStarted value of 01/01/0001
                        Trace.WriteLine("No Date Started - Providing MinDate");
                        dateStarted = DateTime.MinValue;
                    }
                    //Add a new task object to the projec iteraion's task list
                    project.taskList.Add(new Task(xPoint, yPoint, description, priority, project.ID, taskID, dateStarted, projectID));
                }
            }
            Conn.Close();
        }

        public static void addNewTask(string TaskName, string TaskDescription, double TaskLength, int ProjectID, int Priority)
        {
            /*
             * This method will add a new task to the database
             */
            string SQL = "INSERT INTO tblTask(TaskName, TaskDescription, TaskLength, ProjectID, Priority) VALUES(@TN,@TD,@TL,@PID,@PR)";
            //Fill the command object
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add SQL param values
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
                MessageBox.Show("Task addition failed - check that all task criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();

            //Update project length
            updateProjectLength(ProjectID);
        }

        public static void addNewTask(string TaskName, string TaskDescription, double TaskLength, int ProjectID, int Priority, DateTime StartDate)
        {
            /*
             * This method acts similarly to the one above, however is used if a task with priotiy 1 needs to be added (with a start date)
             */ 
            string SQL = "INSERT INTO tblTask(TaskName, TaskDescription, TaskLength, ProjectID, Priority, StartDate) VALUES(@TN,@TD,@TL,@PID,@PR,@SD)";
            //Populate command object
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add SQL param values
                command.Parameters.AddWithValue("@TN", TaskName);
                command.Parameters.AddWithValue("@TD", TaskDescription);
                command.Parameters.AddWithValue("@TL", TaskLength);
                command.Parameters.AddWithValue("@PID", ProjectID);
                command.Parameters.AddWithValue("@PR", Priority);
                command.Parameters.AddWithValue("@SD", TimeHandler.ConvertToAccessDateTime(StartDate));

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
                MessageBox.Show("Task addition failed - check that all task criteria have a correct entry");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();

            //Update project length
            updateProjectLength(ProjectID);
        }

        public static void updateProjectLength(int projectID)
        {   /*
             *This method takes a project's ID and updates that project's length in the database             
             */
            double projectLength = 0;
            Project currProject = null;

            //Return the project object based on its unique ID
            foreach (Project project in ModelView.projectList)
            {
                if (project.ID == projectID)
                {
                    currProject = project;
                    projectLength = TimeHandler.getProjectHoursFromTasks(project);
                }
            }

            //If the projects new length is greater than zero (it exists) update the database with the 'new' length
            if (projectLength > 0)
            {
                string SQL = "UPDATE tblProject SET ProjectLength = @LENGTH WHERE ProjectID = @PID";
                //Populate command object
                command = new MySqlCommand(SQL, Conn);
                try
                {
                    //Add SQL param values
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
                    Trace.WriteLine("Updated project hours and length successfully");
                }
                else
                {
                    //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                    MessageBox.Show("Project Length Updating failied - check that all criteria have a correct entry");
                }
                //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
                Conn.Close();
            } else
            {
                //Is this just a new empty project
                if (currProject.taskList.Count != 0)
                {
                    MessageBox.Show("Project Length Updating failed - likely project does not exist");
                    Trace.WriteLine("ERROR UPDATING PROJECT HOURS");
                }
            }

        }

        public static void updateTaskPriorities(Project project)
        {
            /*
             * If a task is completed, for example, this method will update all of the tasks in a project to accept new priorities
             */
            if (project.projectLength > 0)
            {
                foreach (Task task in project.taskList)
                {
                    string SQL = "UPDATE tblTask SET Priority = @PRI WHERE ProjectID = @PID AND TaskID = @TSKID";
                    //Populate command object
                    command = new MySqlCommand(SQL, Conn);
                    try
                    {
                        //Add SQL param values
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
                        Trace.WriteLine("Task priorities updated");
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

        public static void updateTaskStartTime(Task task, DateTime newStartTime)
        {
            /*
             * Calculates and updates a tasks's start date
             */
            string SQL = "UPDATE tblTask SET StartDate = @SD WHERE TaskID = @TID";
            //Create command
            command = new MySqlCommand(SQL, Conn);
            try
            {
                //Add param values
                command.Parameters.AddWithValue("@SD", newStartTime);
                command.Parameters.AddWithValue("@TID", task.TaskID);
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
                Trace.WriteLine("Updated task start time");
            }
            else
            {
                //If the code gets this far, it means that there has not been an in-code syntax error, instead a data entry error (v.likely to be user-related)
                MessageBox.Show("Task start time updating failed");
            }
            //Close the connection to reduce resource usage and prevent other changes to database from other users (and this software)
            Conn.Close();
        }
    }
}
