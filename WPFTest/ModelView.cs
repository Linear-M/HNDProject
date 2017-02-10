using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Controls;
using System.Diagnostics;

namespace WPFTest
{
    class ModelView
    {
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
            get
            {
                int size = 0;
                if (projectList.Count > 0)
                {
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
            return project.taskList.Count;
        }


        public static void populateProjects(string Username)
        {
            purgeProjects();
            DatabaseHandler.loadProjects(Username);

            //Threadsafe estimation calculations
            foreach (Project project in projectList)
            {
                TimeHandler.estimatedFinishingDate(project);
                foreach (Task task in project.taskList)
                {
                    Trace.WriteLine("Calculating EFT for task: " + task.TaskName);
                    TimeHandler.estimatedTaskFinishingDate(task);
                }
            }
        }

        private static void purgeProjects()
        {
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
                    //Colour changed to default, no hatch style (highlighting) and default border size
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

                if (currPoint != null)
                {
                    if (usedChart.ChartAreas[0].AxisX.Title == "Task Name")
                    {
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
                                    currPoint.ToolTip = task.TaskDescription + Environment.NewLine + "Priority: " + task.Priority.ToString();
                                }
                            }
                        }
                    } else
                    {
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
                                currPoint.ToolTip = project.projectDescription.ToString();
                            }
                        }
                    }
                }
                else
                {
                    removeLabels(usedChart);
                }

            }
        }

        
        public static Chart manageBarClicking(HitTestResult result, Chart usedChart)
        {
            //Check to see if we are in task view (not project view)
            if (usedChart.ChartAreas[0].AxisX.Title == "Task Name")
            {
                return projectChartGenerator("Project Name", "Hours Left");
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

            //Sort projects by ascending estimated finishing date
            Project[] sortedProjects = projectList.OfType<Project>().OrderBy(p => p.estimatedFinishingDate).ToArray();

            foreach (Project project in sortedProjects)
            {
                series.Points.AddXY(project.x, project.y);
            }

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

            Task[] descSortedTasks = project.taskList.OfType<Task>().OrderBy(t => t.Priority).ToArray();

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
            //Little bit hacky, cant manipulate arraylist in non-safe collection so create array copy and then use arraylist constructor to reinstate data from the array
            Array projList = projectList.ToArray();

            foreach (Project p in projList)
            {
                taskChartGenerator("Task Name", "Hours Left", p);
            }

            projectList = new ArrayList(projList);
        }

        public static DataPoint getHoveredPoint(HitTestResult result, Chart usedChart)
        {
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
            if (cb.Text.ToString() == "Please Select a Project")
            {
                cb.Items.Clear();
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
            string body = "Hi, " + LoginHandler.username + "!" + Environment.NewLine + Environment.NewLine;
            body += "You've been busy this week, you currently have " + projectList.Count + " projects you're working on with " + taskCount + " tasks" + Environment.NewLine + Environment.NewLine;
            body += "Here's a list of your projects (Name - Length): ";

            foreach (Project project in projectList)
            {
                body += Environment.NewLine + project.x + " - " + project.projectLength;
            }

            body += Environment.NewLine + Environment.NewLine + "And here's a list of the tasks you should be working on this week" + Environment.NewLine;

            foreach (Project project in projectList)
            {
                foreach (Task task in project.taskList)
                {
                    Trace.WriteLine(task.EstimatedFinishingDate);
                    if (task.EstimatedFinishingDate <= DateTime.Now.AddDays(14))
                    {
                        body += Environment.NewLine + "Project Name: " + project.x + ", Task Name: " + task.TaskName + ", Task Length: " + task.TaskLength;
                        
                        if (task.StartDate != DateTime.MinValue)
                        {
                            body += Environment.NewLine + "You started this task on " + task.StartDate.ToShortDateString() + " and are on target to finish it by " + task.EstimatedFinishingDate.ToShortDateString();
                        } else
                        {
                            body += Environment.NewLine + "While this task isn't your priority, there's no harm in starting it!" + Environment.NewLine;
                        }
                    }
                    body += Environment.NewLine;
                }
                body += Environment.NewLine;
            }
            body += Environment.NewLine + "Make sure you check the Better Project app for a more detailed look!";
            return body;
        }

        public static void removeLabels(Chart chart)
        {
            foreach (DataPoint dp in chart.Series[0].Points)
            {
                dp.Label = "";
            }
        }

    }
}
