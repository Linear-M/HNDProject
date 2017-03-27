using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GanttChart;
using System.Drawing;
using System.Windows.Forms;

namespace WPFTest
{
    class Gantt
    {
       public Gantt()
        {
        }

        public static GanttChart.GanttChart returnGantt(Project project)
        {
                GanttChart.GanttChart ganttChart1 = new GanttChart.GanttChart();

                //Gantt chart settings
                ganttChart1.AllowChange = false;
                ganttChart1.FromDate = project.dateStarted.AddDays(-2);
                ganttChart1.ToDate = ModelView.taskLastFinishing(project).AddDays(2);
                ganttChart1.GridColor = Pens.Black;
                ganttChart1.ForeColor = Color.Black;
                ganttChart1.BackColor = UI.chartColour;
                int barIndex = 0;
                
                //For each task add a relevant row in the gantt chart
                foreach (Task task in project.taskList)
                {
                    string taskName = task.TaskName;
                    DateTime fromTime = task.StartDate;
                    DateTime toTime = task.EstimatedFinishingDate;
                    Color barColor = UI.dataPointColour;
                    Color barHoverColor = UI.dataPointHoverColour;
                    barIndex++;

                    BarInformation bInf = new BarInformation(taskName, fromTime, toTime, barColor, barHoverColor, barIndex);

                    ganttChart1.AddChartBar(taskName, bInf, fromTime, toTime, barColor, barHoverColor, barIndex);
                MessageBox.Show("GNC");
                }

                return ganttChart1;
        }
    }
}
