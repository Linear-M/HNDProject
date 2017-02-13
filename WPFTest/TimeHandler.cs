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
        //Encapsulated variables
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
            /*
             * Return the sum of every day's hour
             */
            get
            {
                return (Monday + Tuesday + Wednesday + Thursday + Friday + Saturday + Sunday);
            }
        }
        public static int ZeroHourDays
        {
            /*
             * Check each day to see if it is a 'non-working' day, and if so increment the 'zero day' counter
             */
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
            /*
             * This method takes a project object and creates a date/time estimation as to when it will be finished
             * NOTE: Project length is now determined by the length of the combined tasks to make it more accurate
             */
            if (!areTimesLoaded)
            {
                getTimes();
            }

            double projectLength = 0;

            //For each task in the project add its length to the project length
            foreach (Task task in project.taskList)
            {
                projectLength += task.TaskLength;
            }

            //(If the project has tasks) calculate the estimated finishing date
            if (projectLength != 0)
            {
                /*
                 *Take the project length and divide it by the hours per week to get an estimation as to how many working weeks/days we need, then split it into the integer (week) and double (decimal day)
                 *parts and cast them both to integers - take this, add the amount of non-working days and add the total 'new days' to the estimated finishing date (start date + needed days + zero hour days)
                */
                double totalTimeNeeded = (project.projectLength / HoursPerWeek);
                project.estimatedFinishingDate = project.dateStarted.AddDays((((double)((int)Math.Truncate(totalTimeNeeded))) * 7) + ((int)((totalTimeNeeded - ((int)Math.Truncate(totalTimeNeeded))) * Math.Pow(10, 1))));
                //Add more accurate no-working dates
                project.estimatedFinishingDate.AddDays(ZeroHourDaysBetweenDates(project.dateStarted, project.estimatedFinishingDate));
            }
            else
            {
                //DateTime.MinValue is used a constant to identify that the project has no length, or a task is not priority
                project.estimatedFinishingDate = DateTime.MinValue;
            }

            
        }

        public static void estimatedTaskFinishingDate(Task task)
        {
            /*
             * This method identifies the finishing date of a seperate task
             */
            if (!areTimesLoaded)
            {
                getTimes();
            }
            /*
            *Take the project length and divide it by the hours per week to get an estimation as to how many working weeks/days we need, then split it into the integer (week) and double (decimal day)
            *parts and cast them both to integers - take this, add the amount of non-working days and add the total 'new days' to the estimated finishing date (start date + needed days + zero hour days)
            */
            double totalTimeNeeded = (task.TaskLength / HoursPerWeek);
            task.EstimatedFinishingDate = task.StartDate.AddDays((((double)((int)Math.Truncate(totalTimeNeeded))) * 7) + ((int)((totalTimeNeeded - ((int)Math.Truncate(totalTimeNeeded))) * Math.Pow(10, 1))));
            //Add more accurate no-working dates
            task.EstimatedFinishingDate.AddDays(ZeroHourDaysBetweenDates(task.StartDate, task.EstimatedFinishingDate));
        }

        public static bool areTimesLoaded
        {
            /*
             * This method returns true/false if a database connection has already been made to load working hours
             */
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
            /*
             *Attempts to load a user's working times from the database
             */
            if (LoginHandler.username == null)
            {
                MessageBox.Show("Cannot get working times as not logged in"); 
            } else
            {
                DatabaseHandler.getWorkingHours(LoginHandler.username);
            }
        }

       public static void checkTaskComplete(Task task)
        {
            /*
             * This method takes a task and identifies if its estimated finishing date is today or has passed, and if so removes it from the task list and prompts the user to update
             */
            //If the task's finishing date is today or previously (and the task was not started today)
            if ((task.EstimatedFinishingDate >= DateTime.Now) && (task.EstimatedFinishingDate.ToShortDateString() != task.StartDate.ToShortDateString()))
            {
                foreach (Project project in ModelView.projectList)
                {
                    if (project.ID == task.ProjectID)
                    {
                        Trace.WriteLine("Removed task: " + task.TaskName + " from " + project.x.ToString());
                        project.taskList.Remove(task);
                        increaseTaskPriorities(project);
                        MessageBox.Show("Tasks have been automatically removed and cleaned - please update");
                    }
                }
            }
        }
        
        private static void increaseTaskPriorities(Project project)
        {
            /*
             * Called if a task is removed their 'friends' must be updated to have higher priority locally and externally
             */
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
