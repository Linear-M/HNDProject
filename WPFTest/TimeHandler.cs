using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WPFTest
{
    class TimeHandler
    {
        private static double monday;
        private static double tuesday;
        private static double wednesday;
        private static double thursday;
        private static double friday;
        private static double saturday;
        private static double sunday;
        private static int zhd;

        public static double Monday
        {
            get
            {
                
                return monday;
            }
            set
            {
                monday = value;
            }
        }
        public static double Tuesday
        {
            get
            {
                
                return tuesday;
            }
            set
            {
                tuesday = value;
            }
        }
        public static double Wednesday
        {
            get
            {
                
                return wednesday;
            }
            set
            {
                wednesday = value;
            }
        }
        public static double Thursday
        {
            get
            {
                
                return thursday;
            }
            set
            {
                thursday = value;
            }
        }
        public static double Friday
        {
            get
            {
                
                return friday;
            }
            set
            {
                friday = value;
            }
        }
        public static double Saturday
        {
            get
            {
                
                return saturday;
            }
            set
            {
                saturday = value;
            }
        }
        public static double Sunday
        {
            get
            {
                
                return sunday;
            }
            set
            {
                sunday = value;
            }
        }
        public static double HoursPerWeek
        {
            get
            {
                return (Monday + Tuesday + Wednesday + Thursday + Friday + Saturday + Sunday);
            }
        }
        public static int ZeroHourDays
        {
            get
            {
                if (Monday == 0)
                {
                    zhd++;
                }
                if (Tuesday == 0)
                {
                    zhd++;
                }
                if (Wednesday == 0)
                {
                    zhd++;
                }
                if (Thursday == 0)
                {
                    zhd++;
                }
                if (Friday == 0)
                {
                    zhd++;
                }
                if (Saturday == 0)
                {
                    zhd++;
                }
                if (Sunday == 0)
                {
                    zhd++;
                }
                return zhd;
            }
        }

        public static int ZeroHourDaysBetweenDates(DateTime start, DateTime finish)
        {
            /*days inbetween / 7 = weeks
             * weeks * zerohourdaysPerWeek = number of zero hour days
             * NOTE: If user works every day this returns zero!
             */
            return (((int)(finish - start).TotalDays) / 7) * ZeroHourDays;
        }

        public static void estimatedFinishingDate(Project project)
        {
            if (!areTimesLoaded)
            {
                getTimes();
            }

            double projectLength = 0;

            foreach (Task task in project.taskList)
            {
                projectLength += task.TaskLength;
            }

            if (projectLength != 0)
            {
                double totalTimeNeeded = (project.projectLength / HoursPerWeek);
                project.estimatedFinishingDate = project.dateStarted.AddDays((((double)((int)Math.Truncate(totalTimeNeeded))) * 7) + ((int)((totalTimeNeeded - ((int)Math.Truncate(totalTimeNeeded))) * Math.Pow(10, 1))));
                //Add more accurate no-working dates
                project.estimatedFinishingDate.AddDays(ZeroHourDaysBetweenDates(project.dateStarted, project.estimatedFinishingDate));
            }
            else
            {
                project.estimatedFinishingDate = DateTime.MinValue;
            }

            
        }

        public static void estimatedFinishingDate(Task task)
        {
            if (!areTimesLoaded)
            {
                getTimes();
            }
            double totalTimeNeeded = (task.TaskLength / HoursPerWeek);
            task.EstimatedFinishingDate = task.StartDate.AddDays((((double)((int)Math.Truncate(totalTimeNeeded))) * 7) + ((int)((totalTimeNeeded - ((int)Math.Truncate(totalTimeNeeded))) * Math.Pow(10, 1))));
            //Add more accurate no-working dates
            task.EstimatedFinishingDate.AddDays(ZeroHourDaysBetweenDates(task.StartDate, task.EstimatedFinishingDate));
        }

        public static bool areTimesLoaded
        {
            get
            {
                if (HoursPerWeek != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void getTimes()
        {
            string username = LoginHandler.username;
            if (username == null)
            {
                MessageBox.Show("Cannot get working times as not logged in"); 
            } else
            {
                DatabaseHandler.getWorkingHours(username);
            }
        }

       public static void checkTaskComplete(Task task)
        {
            bool updated = false;
            //If the task's finishing date is today or previously (and the task was not started today)
            if ((task.EstimatedFinishingDate >= DateTime.Now) && task.EstimatedFinishingDate.ToShortDateString() != task.StartDate.ToShortDateString())
            {
                foreach (Project project in ModelView.projectList)
                {
                    if (project.ID == task.ProjectID)
                    {
                        Trace.WriteLine("Removed task: " + task.TaskName + " from " + project.x.ToString());
                        project.taskList.Remove(task);
                        updated = true;
                    }
                }
                if (updated)
                {
                    MessageBox.Show("Tasks have been automatically removed and cleaned - please update");
                    updated = false;
                }
            }
        }

        private static void increaseTaskPriorities(Project project)
        {
            foreach (Task task in project.taskList)  
            {
                int newPriority = (task.Priority -= 1);
                Trace.WriteLine("Changing '" + task.TaskName + "' priority from " + task.Priority + " to " + newPriority.ToString());
                //Update task priorities client-side
                task.Priority = newPriority;
            }
            //Update task priorities server side
            Trace.WriteLine(project.x + "'s tasks have been updated - renewing database information...");
            DatabaseHandler.updateTaskPriorities(project);
            DatabaseHandler.updateProjectLength(project.ID);
            Trace.WriteLine(project.x + "'s tasks have been updated - database successfully updated with " + project.taskList.Count.ToString() + " new task priorities");
        }
    }
}
