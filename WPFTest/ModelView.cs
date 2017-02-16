using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;


namespace WPFTest
{
    class ModelView
    {
        //Encapsulated variables
        private static ArrayList projList = new ArrayList();

        public static ArrayList projectList
        {
            get
            {
                return projList;
            }
            set
            {
                projList = value;
            }
        }

        public static int taskCount
        {
            /*
             * Returns the number of tasks accross each project stored for that user
             */
            get
            {
                int size = 0;
                if (projectList.Count > 0)
                {
                    //Add the size of the task list for every project and return it
                    foreach (Project project in projectList)
                    {
                        size += project.taskList.Count;
                    }
                    return size;
                }
                else return 0;
            }
        }

        public static int taskCountForProject(Project project)
        {
            //Return the number of tasks for a given project
            return project.taskList.Count;
        }


        public static void populateProjects(string Username)
        {
            /*
             * Populate the project list for a given username
             */
            //Clear any projects in the system and load more from the database
            purgeProjects();
            DatabaseHandler.loadProjects(Username);

            //Threadsafe estimation calculations for each project
            foreach (Project project in projectList)
            {
                TimeHandler.estimatedFinishingDate(project);
                foreach (Task task in project.taskList)
                {
                    Trace.WriteLine("Calculating EFT for task: " + task.TaskName);
                    TimeHandler.estimatedTaskFinishingDate(task);
                }
            }

            //Run email handler now projects are loaded
            if (LoginHandler.shouldSendEMail)
            {
                EMail email = new EMail();
                email.sendEmail(emailBody());
                LoginHandler.shouldSendEMail = false;
            }
        }

        private static void purgeProjects()
        {
            //Clear the project list
            projList.Clear();
        }

        public static void manageBarHighlighting(HitTestResult result, Chart usedChart)
        {
            /*Basic error check/optimisation to make sure that no more processing is done if there are no bars to highlight, likely to be changed to check on
            mouse move event before any processing is done at all */
            if (usedChart.Series.Count > 0)
            {
                //Reset all bars/points of the chart (to make sure that a previously highlighted chart/point does not STAY highlighted)
                foreach (DataPoint point in usedChart.Series[0].Points)
                {
                    //Colour etc changed to default
                    point.BackSecondaryColor = Color.Black;
                    point.BackHatchStyle = ChartHatchStyle.None;
                    point.BorderWidth = 1;
                    point.Color = ColorTranslator.FromHtml("#009ee8");
                }
                //If the mouse if over a data point (bar)
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    //Identify specific data point/bar that is highlighted
                    DataPoint point = getHoveredPoint(result, usedChart);

                    //Change the bar to make it seem highlighted (cross-hatch and lighter colour than the bar itself)
                    point.Color = ColorTranslator.FromHtml("#0077af");
                }

                //Draw labels
                DataPoint currPoint = getHoveredPoint(result, usedChart);

                /*
                 * If the current point exists as a datapoint and the axis' title is task name assume the clicked datapoint is in the task view
                */
                if (currPoint != null)
                {
                    if (usedChart.ChartAreas[0].AxisX.Title == "Task Name")
                    {
                        //Identify the task that has been highlighted and generate an appropriate label
                        foreach (Project project in projectList)
                        {
                            foreach (Task task in project.taskList)
                            {
                                if (task.TaskName == currPoint.AxisLabel)
                                {
                                    currPoint.Font = new Font("Arial", 8f);
                                    currPoint.LabelForeColor = Color.White;
                                    if (task.StartDate != DateTime.MinValue)
                                    {
                                        currPoint.Label = "Task Name: " + task.TaskName + Environment.NewLine + "EFT: " + task.EstimatedFinishingDate.ToShortDateString();
                                    } else
                                    {
                                        currPoint.Label = "Task Name: " + task.TaskName + Environment.NewLine + "No EFT as Not Priority";
                                    }
                                    //Add a tooltip to the datapoint
                                    currPoint.ToolTip = task.TaskDescription + Environment.NewLine + "Priority: " + task.Priority.ToString();
                                }
                            }
                        }
                    } else
                    {
                        //If a project has been highlighted, identify which one it is an add an appropriate label
                        currPoint.Font = new Font("Arial", 8f);
                        currPoint.LabelForeColor = Color.White;
                        foreach (Project project in projectList)
                        {
                            if (project.x == currPoint.AxisLabel)
                            {
                                currPoint.Label = "Project Name: " + project.x.ToString() +
                                    Environment.NewLine +
                                    "Date Project Started: " + project.dateStarted.ToShortDateString() +
                                    Environment.NewLine +
                                    "Date Project Due: " + project.dateDue.ToShortDateString();
                                if (taskCountForProject(project) <= 0)
                                {
                                    currPoint.Label += Environment.NewLine + "Estimated Finishing Date: NA (no tasks)";
                                } else
                                {
                                    currPoint.Label += Environment.NewLine + "Estimated Finishing Date: " + project.estimatedFinishingDate.ToShortDateString();
                                }
                                //Add a tooltip to the datapoint
                                currPoint.ToolTip = project.projectDescription.ToString();
                            }
                        }
                    }
                }
                else
                {
                    //Seems as we're doing the hit-test on everything we just as well remove labels non-efficiently
                    removeLabels(usedChart);
                }

            }
        }

        
        public static Chart manageBarClicking(HitTestResult result, Chart usedChart)
        {
            //Check to see if we are in task view (not project view) and if so return a new project chart
            if (usedChart.ChartAreas[0].AxisX.Title == "Task Name")
            {
                return projectChartGenerator("Project Name", "Hours Left");
            //If we're in project view (and we need to generate a task view) find the project that has been clicked and either return null (no point clicked), usedChart (same) or the new task chart
            } else if (usedChart.ChartAreas[0].AxisX.Title == "Project Name")
            {
                foreach (Project project in projectList)
                {
                    if (result.PointIndex >= 0)
                    {
                        if (project.y == usedChart.Series[0].Points[result.PointIndex].YValues[0])
                        {
                            //Check the project has tasks
                            if (project.taskList.Count <= 0)
                            {
                                return null;
                            }
                            else
                            {
                                return project.taskChart;
                            }
                        }
                    } else
                    {
                        return usedChart;
                    }
                }
                return null;
            }
            return null;
        }
        

        public static Chart projectChartGenerator(string xAxisTitle, string yAxisTitle)
        {
            /*
             * This method returns a new chart based on the projects a user has
             */
            //If we haven't got a project list generate one based on username
            if (ModelView.projectList.Count <= 0)
            {
                populateProjects(LoginHandler.username);
            }

            ArrayList projectList = ModelView.projectList;

            // Initialize the Chart object
            Chart Chart1 = new Chart();
            Chart1.BackColor = Color.LightSkyBlue;

            // Add a chart area.
            Chart1.ChartAreas.Add("Default");
            Chart1.ChartAreas["Default"].BackColor = Color.LightSkyBlue;
            Chart1.ChartAreas["Default"].AxisX.Title = xAxisTitle.ToString();
            Chart1.ChartAreas["Default"].AxisY.Title = yAxisTitle.ToString();

            // Add a series with some data points.
            Series series = new Series();

            //Sort projects by ascending estimated finishing date (we have to use an array because we can't manipulate arraylists in loop)
            Project[] sortedProjects = projectList.OfType<Project>().OrderBy(p => p.estimatedFinishingDate).ToArray();

            foreach (Project project in sortedProjects)
            {
                series.Points.AddXY(project.x, project.y);
            }

            //Add the series to the chart and format any labelling
            Chart1.Series.Add(series);
            Chart1.Series[0]["LabelStyle"] = "Bottom";

            foreach (Series s in Chart1.Series)
            {
                s.Color = ColorTranslator.FromHtml("#009ee8");
            }

            return Chart1;
        }

        private static void taskChartGenerator(string xAxisTitle, string yAxisTitle, Project project)
        {
            /*
             * this method returns a new chart based upon a project's task list
             */
            // Initialize the Chart object
            Chart Chart1 = new Chart();
            Chart1.BackColor = Color.LightSkyBlue;

            // Add a chart area.
            Chart1.ChartAreas.Add("Default");
            Chart1.ChartAreas["Default"].BackColor = Color.LightSkyBlue;
            Chart1.ChartAreas["Default"].AxisX.Title = xAxisTitle.ToString();
            Chart1.ChartAreas["Default"].AxisY.Title = yAxisTitle.ToString();

            // Add a series with some data points.
            Series series = new Series();

            //Sort tasks by priority
            Task[] descSortedTasks = project.taskList.OfType<Task>().OrderBy(t => t.Priority).ToArray();

            //Add the tasks to the series/chart
            foreach (Task task in descSortedTasks)
            {
                series.Points.AddXY(task.TaskName, task.TaskLength);
            }

            Chart1.Series.Add(series);
            Chart1.Series[0]["LabelStyle"] = "Bottom";

            foreach (Series s in Chart1.Series)
            {
                s.Color = Color.DeepSkyBlue;
            }

            project.taskChart = Chart1;
        }

        public static void generateTaskCharts(string xAxisTitle, string yAxisTitle)
        {
            /*
             * This method generates a chart for each task within a project
             * NOTE: We have to be a bit 'hacky' here and convert the arraylist to an array so we can manipulate objects at run time
             */
            Array projList = projectList.ToArray();

            foreach (Project p in projList)
            {
                taskChartGenerator("Task Name", "Hours Left", p);
            }

            //Convert the array and reinstantiate the project list
            projectList = new ArrayList(projList);
        }

        public static DataPoint getHoveredPoint(HitTestResult result, Chart usedChart)
        {
            /*
             * This method returns a datapoint that has been hovered over
             */
            //If the result is a datapoint return the datapoint itself
            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                return usedChart.Series[0].Points[result.PointIndex];
            } else
            {
                return null;
            }
        }

        public static void taskListComboBoxHandler(System.Windows.Controls.ComboBox cb)
        {
            /*
             * This method takes a combo box and organises what items it should have based on loaded projects
             */
            if (cb.Text.ToString() == "Please Select a Project")
            {
                cb.Items.Clear();
                //ProjectList is an ArrayList of Project objects where project.x is the project name
                if (projectList.Count != 0)
                {
                    foreach (Project project in projectList)
                    {
                        cb.Items.Add(project.x);
                    }
                }
                else
                {
                    cb.Items.Add("No Projects Loaded - Please Login");
                }
            }
       }

       public static int nextTaskPriority(Project project)
        {
            /*
             * This method identifies what priority a task should have and effectively applies them
             */
            //semi-const start priority of 1 to make sure that if no other taks are present the initital task has 'first' priority
            int priority = 1;

            //If there's only one task, can assume next priority = base_priority + 1
            if (project.taskList.Count == 1)
            {
                return priority += 1;
            }

            //Organise number oft asks and priority
            foreach (Task task in project.taskList)
            {
                if (task.Priority > priority)
                {
                    priority = task.Priority;
                }
            }

            //If this is the first task (no priority > base_priority) return base priority, could be handled earlier but this is safer
            if (priority == 1)
            {
                return priority;
            }

            //If more than one task and priority > base_priority return new priority
            return priority+=1;
        }

        public static string emailBody()
        {
            /*
             * This method returns a long string that constitutes the entire body of an email. It shows a welcome message, project list and a seperate table per project of current tasks, their lengths and notes
             */  
            string body = "<!DOCTYPE html><html><head><style>table,th,td{border: 1px solid black; padding: 5px;} table{border-spacing: 5px;} h1,p,td,tr,th,body{font-family: arial;} body{background-color: lightblue;}</style></head><body>";

            body += "<h1>Hi, " + LoginHandler.username + "!</h1>";

            body += "<p>You've been busy this week - you currently have " + projectList.Count + " projects you're working on totalling " + taskCount + " tasks.</p>";

            body += "<p>Here's a list of your projects:</p>";

            body += "<table><tr><th>Project Name</th><th>Project Length</th></tr>";

            foreach (Project project in projectList)
            {
                body += "<tr><td>" + project.x + "</td><td>" + project.projectLength + "</td></tr>";
            }

            body += "</table><br />";

            body += "<p>Here's a list of the tasks you should be working on this week:<p>";
            

            foreach (Project project in projectList)
            {
                body += "<table><tr><th>Project Name</th><th>Task Name</th><th>Task Length (Hours)</th><th>Notes</th></tr>";
                foreach (Task task in project.taskList)
                {
                    //If the task's estimated finishing date is within a fortnight
                    Trace.WriteLine(task.EstimatedFinishingDate);
                    if (task.EstimatedFinishingDate <= DateTime.Now.AddDays(14))
                    {
                        body += "<tr><td>" + project.x + "</td><td>" + task.TaskName + "</td><td>" + task.TaskLength + "</td>";
                        
                        if (task.StartDate != DateTime.MinValue)
                        {
                            body += "<td>You started this task on " + task.StartDate.ToShortDateString() + " and are on target to finish it by " + task.EstimatedFinishingDate.ToShortDateString() + "</td></tr>";
                        } else
                        {
                            body += "<td>While this task isn't your priority, there's no harm in starting it!</td></tr>";
                        }
                    }
                }
                body += "</table><br/>";
            }
            body += "Make sure you check the Better Project app for a more detailed look! </body></html>";
            return body;
        }

        public static string generateBaloonMessage()
        {
            //Return an informational message with the first-priority project and task, if none are available nudge the user to open the app and add projects/tasks
            string returnString = "Another hour has passed, are you still working on (Project: Task):";
            foreach (Project project in projectList)
            {
                foreach (Task task in project.taskList)
                {
                    if (task.StartDate != DateTime.MinValue)
                    {
                        returnString += Environment.NewLine + project.x + ": " + task.TaskName;
                        return returnString;
                    }
                }
            }
            return "Make sure you have enough projects and tasks to work on - open the app!";
        }

        public static void removeLabels(Chart chart)
        {
            //Remove all of the labels for every datapoint
            foreach (DataPoint dp in chart.Series[0].Points)
            {
                dp.Label = "";
            }
        }

    }
}
