using System;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using Forms = System.Windows.Forms;
using System.Windows.Input;

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
            UI.initialiseCustomFonts();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Open the file menu, show is handled in constructor
            FileMenu fmenu = new FileMenu();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (LoginHandler.loggedIn)
            {
                //Show the please wait screen
                PleaseWait plsw = new PleaseWait();
                plsw.Show();

                Forms.Integration.WindowsFormsHost host = new Forms.Integration.WindowsFormsHost();

                // Add the chart to the Windows Form Host.
                ModelView.populateProjects(LoginHandler.username);
                host.Child = ModelView.projectChartGenerator("Project List", "Time Left (Hours)");

                // Add the chart to the grid so it can be displayed.
                wfhTest.Child = host.Child;

                //Bind the MouseMove and MouseClick handler
                wfhTest.Child.MouseMove += Child_MouseMove;
                wfhTest.Child.MouseClick += Child_MouseClick;

                //Generate tasks for the projectlist
                ModelView.generateTaskCharts("Task Name", "Hours Left");

                plsw.Close();
            }
            else
            {
                Login frmLogin = new Login();
                frmLogin.Show();
            }
        }

        private void Child_MouseClick(object sender, Forms.MouseEventArgs e)
        {
            //Remove any old event handlers to save a lot of memory
            wfhTest.Child.MouseMove -= Child_MouseMove;
            wfhTest.Child.MouseClick -= Child_MouseClick;

            //Cast chart object to the child of windows forms host to generate an object for the current chart
            Chart currChart = (Chart)wfhTest.Child;

            Forms.Integration.WindowsFormsHost host = new Forms.Integration.WindowsFormsHost();

            //Generate the new chart (do we have to generate the task chart or project chart (or stay the same)
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

            //Add new event handlers (this is why it was important to remove the old ones, they still take up space - a lot of it - and do not work when the child is modified)
            wfhTest.Child.MouseMove += Child_MouseMove;
            wfhTest.Child.MouseClick += Child_MouseClick;
        }
            

        private void Child_MouseMove(object sender, Forms.MouseEventArgs e)
        {
            //Whenever the mouse moves whilst on the chart control
            ModelView.manageBarHighlighting(((Chart)wfhTest.Child).HitTest(e.X, e.Y), (Chart)wfhTest.Child);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Send email TODO change
            EMail em = new EMail();
            em.sendEmail(ModelView.emailBody());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //If the form is closing, cancel the form close and instead move to taskbar
            e.Cancel = true;
            this.Hide();
            taskbarIcon.Visibility = Visibility.Visible;
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //If the 'show' menu item was selected from the taskbar show the form and hide the taskbar icon
            this.Show();
            taskbarIcon.Visibility = Visibility.Hidden;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //If the 'exit' menu was selected from the taskbar force shutdown the app
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //When the program is launched greate a dispatchertimer to generate a notification every two hours
            createTimerEngine();
        }

        private void createTimerEngine()
        {
            //Create the dispatcher timer and give it the tick event handler, time span for the tick and start it
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(2, 0, 0);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            //On the two hour tick, show a notification on the taskbar with a procedurally generated message
            taskbarIcon.ShowBalloonTip("Better Project Notification", ModelView.generateBaloonMessage(), Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            /*
             * Open task menu, less clutter than going through file/add new project menu, constructor handles show/hide
             */
                TaskMenu tskMenu = new TaskMenu();
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //Show profile/hours menu
            Window1 profile = new Window1();
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            //Show login menu
            Login frmLogin = new Login();
            frmLogin.Show();
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            //Show the login form first
            LoginHandler.loggedIn = true;
            LoginHandler.username = "Ben";
            LoginHandler.password = "password";
            LoginHandler.email = "benpople@outlook.com";
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            //Check database connection
            DatabaseConnection dbConn = new DatabaseConnection();
            if (dbConn.IsConnected())
            {
                MessageBox.Show("Connection Success");
            }
            else
            {
                MessageBox.Show("Cannot connect");
            }
        
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            //Gantt controller
            GanttViewer gvw = new GanttViewer(ModelView.currentProject);
        }
    }
}
