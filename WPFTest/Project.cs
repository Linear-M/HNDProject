using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WPFTest
{
    class Project
    {
        private ArrayList tasks = new ArrayList();
        private int _id;

        public ArrayList taskList
        {
            get
            {
                return tasks;
            }
        }

        public Chart taskChart
        {
            get;
            set;
        }

        public DateTime estimatedFinishingDate
        {
            get;
            set;
        }

        public string x {
            get;
            set;
        }
    
        public double y
        {
            get;
            set;
        }

        public string projectDescription
        {
            get;
            set;
        }

        public DateTime dateStarted
        {
            get;
            set;
        }

        public DateTime dateDue
        {
            get;
            set;
        }

        public double projectLength
        {
            get;
            set;
        }

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public Project(string _x_projectName, double _y_projectLength, string _projectDescription, DateTime _dateStarted, DateTime _dateDue, double _projectLength, int _id)
        {
            x = _x_projectName;
            y = _y_projectLength;
            projectDescription = _projectDescription;
            dateStarted = _dateStarted;
            dateDue = _dateDue;
            projectLength = _projectLength;
            ID = _id;

        }

    }
}
