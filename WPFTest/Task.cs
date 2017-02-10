using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFTest
{
    class Task
    {
        private string _TaskName, _TaskDescription;
        private double _TaskLength;
        private DateTime _EstimatedFinishingDate, _StartDate;
        private int _Priority, _ProjectID, _TaskID;

        public string TaskName
        {
            get
            {
                return _TaskName;
            }
            set
            {
                _TaskName = value;
            }
        }
        public string TaskDescription
        {
            get
            {
                return _TaskDescription;
            }
            set
            {
                _TaskDescription = value;
            }
        }
        public double TaskLength
        {
            get
            {
                return _TaskLength;
            }
            set
            {
                _TaskLength = value;
            }
        }
        public DateTime EstimatedFinishingDate
        {
            get
            {
                return _EstimatedFinishingDate;
            }
            set
            {
                _EstimatedFinishingDate = value;
            }
        }
        public DateTime StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                _StartDate = value;
            }
        } 
        public int Priority
        {
            get
            {
                return _Priority;
            }
            set
            {
                _Priority = value;
            }
        }

        public int ProjectID
        {
            get
            {
                return _ProjectID;
            }
            set
            {
                _ProjectID = value;
            }
        }

        public int TaskID
        {
            get
            {
                return _TaskID;
            }
            set
            {
                _TaskID = value;
            }
        }

        public Task(string name, double length, string description, int priority, int projectID, int taskID, DateTime startDate)
        {
            TaskName = name;
            TaskDescription = description;
            TaskLength = length;
            Priority = priority;
            TaskID = taskID;
            //If the project is currently ON
            if (startDate != DateTime.MinValue)
            {
                StartDate = startDate;
            }
        } 
    }
}
