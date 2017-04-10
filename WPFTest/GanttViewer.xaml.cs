using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualBasic;

namespace WPFTest
{
    /// <summary>
    /// Interaction logic for GanttViewer.xaml
    /// </summary>
    public partial class GanttViewer : Window
    {
        public GanttViewer(Project project)
        {
            InitializeComponent();
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();


            //Set the new WFH object equal to a new gantt chart of selected project
            host.Child = Gantt.returnGantt(project);

            if (host.Child != null)
            {
                // Add the chart to the grid so it can be displayed.
                wfhGantt.Child = host.Child;

                //Bind the MouseMove and MouseClick handler
                wfhGantt.Child.MouseMove += Child_MouseMove;
                wfhGantt.Child.MouseClick += Child_MouseClick;

                Background = UI.leftBorderColor;
                this.Show();
            } else
            {
                System.Windows.MessageBox.Show("Error loading Gantt chart - make sure you are in 'Task' view!");
            }

        }

        private void Child_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Click
        }

        private void Child_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Move
        }

        private void SaveImage(System.Windows.Forms.Control control)
        {
            //Objects
            GanttChart.GanttChart gantt = control as GanttChart.GanttChart;
            SaveFileDialog savefile = new SaveFileDialog();
            DateTime now = DateTime.Now;

            //Give a decent template name
            savefile.FileName = "GanttChart-" + now.Day + "." + now.Month + "." + now.Year + "-" + ModelView.currentProject.x + ".jpg";
            //Filters are nicer for the average user
            savefile.Filter = "Image files (*.jpg)|*.jpg|All files (*.*)|*.*";

            if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Save image to where the user wants as well as the temporary image for email
                gantt.SaveImage(savefile.FileName);
                gantt.SaveImage(ModelView.temporaryPictureString);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(wfhGantt.Child);
        }
    }
}
