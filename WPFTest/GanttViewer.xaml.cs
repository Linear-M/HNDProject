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

            host.Child = Gantt.returnGantt(project);

            // Add the chart to the grid so it can be displayed.
            wfhGantt.Child = host.Child;

            //Bind the MouseMove and MouseClick handler
            wfhGantt.Child.MouseMove += Child_MouseMove;
            wfhGantt.Child.MouseClick += Child_MouseClick;

            Background = UI.leftBorderColor;
            this.Show();
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
            GanttChart.GanttChart gantt = control as GanttChart.GanttChart;
            SaveFileDialog savefile = new SaveFileDialog();
            // set a default file name
            savefile.FileName = "GanttChart-" + DateTime.Now.Day + "." + DateTime.Now.Month + "." + DateTime.Now.Year + "-" + ModelView.currentProject.x + ".jpg";
            // set filters - this can be done in properties as well
            savefile.Filter = "Image files (*.jpg)|*.jpg|All files (*.*)|*.*";

            if (savefile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                gantt.SaveImage(savefile.FileName);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(wfhGantt.Child);
        }
    }
}
