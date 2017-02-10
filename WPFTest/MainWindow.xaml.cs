using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;


namespace WPFTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            FileMenu fmenu = new FileMenu();
            fmenu.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (LoginHandler.loggedIn)
            {
                PleaseWait plsw = new PleaseWait();
                plsw.Show();
                Forms.Integration.WindowsFormsHost host = new Forms.Integration.WindowsFormsHost();

                // Add the chart to the Windows Form Host.
                ModelView.populateProjects(LoginHandler.username);
                host.Child = ModelView.projectChartGenerator("Project Name", "Time Left (Hours)");

                // Add the chart to the grid so it can be displayed.
                wfhTest.Child = host.Child;

                //Bind the MouseMove and MouseClick handler
                wfhTest.Child.MouseMove += Child_MouseMove;
                wfhTest.Child.MouseClick += Child_MouseClick;

                //Gen tasks
                ModelView.generateTaskCharts("Task Name", "Hours Left");
                plsw.Close();
            } else
            {
                /*
                Login frmLogin = new Login();
                frmLogin.Show();
                */

                LoginHandler.loggedIn = true;
                LoginHandler.username = "Ben";
                LoginHandler.password = "password";
            }

        }

        private void Child_MouseClick(object sender, Forms.MouseEventArgs e)
        {
            wfhTest.Child.MouseMove -= Child_MouseMove;
            wfhTest.Child.MouseClick -= Child_MouseClick;

            Chart currChart = (Chart)wfhTest.Child;
            Forms.Integration.WindowsFormsHost host = new Forms.Integration.WindowsFormsHost();

            Chart newChart = ModelView.manageBarClicking(currChart.HitTest(e.X, e.Y), currChart);

            //If no DP was clicked ModelView.manageBarClicking returns the same chart
            if (newChart != currChart)
            {
                if (newChart != null)
                {
                    // Add the chart to the Windows Form Host.
                    host.Child = newChart;
                    // Add the chart to the grid so it can be displayed.
                    wfhTest.Child = host.Child;
                    //Bind the MouseMove and MouseClick handler
                }
                else
                {
                    MessageBox.Show("No tasks found");
                }
            }
            wfhTest.Child.MouseMove += Child_MouseMove;
            wfhTest.Child.MouseClick += Child_MouseClick;
        }
            

        private void Child_MouseMove(object sender, Forms.MouseEventArgs e)
        {
            ModelView.manageBarHighlighting(((Chart)wfhTest.Child).HitTest(e.X, e.Y), (Chart)wfhTest.Child);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Login frmLogin = new Login();
            frmLogin.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Window1 profile = new Window1();
            profile.Show();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            EMail em = new EMail();
            em.sendEmail(ModelView.emailBody());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            taskbarIcon.Visibility = Visibility.Visible;
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            taskbarIcon.Visibility = Visibility.Hidden;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
