using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.Collections;
using System.Threading;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int check = 0;
        const int NUMBER_OF_TABLES = 15;
        TcpClient socketOutput = new TcpClient();
        TcpClient socketInput = new TcpClient();
        int[] isTableCreatedCounter = new int[NUMBER_OF_TABLES + 1];//put it in the server
        tableReservation[] tbl_reservations = new tableReservation[NUMBER_OF_TABLES + 1];
        List<Button> listButtons = new List<Button>();
        Semaphore mouseEnterMutex;
        public MainWindow()
        {
            InitializeComponent();
            GetLogicalChildCollection(this, listButtons);
            showIternalTables();
            socketOutput.Connect(IPAddress.Parse("127.0.0.1"), 8000);
            socketInput.Connect(IPAddress.Parse("127.0.0.1"), 8001);
            mouseEnterMutex = new Semaphore(1,1);
        }

        public bool isTableButton(Button b)
        {
            return (b.Content.ToString() != "Settings" && b.Content.ToString() != "Change To External Tables" && b.Content.ToString() != "Change To Iternal Tables");
        }
        private void showIternalTables()
        {
            switcher.Content = "Change To External Tables";
            foreach (Button b in listButtons)
            {
                if (isTableButton(b))
                {
                    if (Convert.ToInt32(b.Content.ToString()) > 15)//number of iternal tables=15
                    {
                        b.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        b.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void showExternalTables()
        {
            switcher.Content = "Change To Iternal Tables";
            foreach (Button b in listButtons)
            {
                if (isTableButton(b))
                {
                    if (Convert.ToInt32(b.Content.ToString()) <= 15)
                    {
                        b.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        b.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button table_pressed_button = sender as Button;
            int table_number = Int32.Parse(table_pressed_button.Content.ToString());
            tbl_reservations[table_number] = new tableReservation(socketOutput, socketInput, table_number);
            this.Hide();
            tbl_reservations[table_number].ShowDialog();
            this.Show();
        }

        private void switcher_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if(b.Content.ToString()== "Change To External Tables")
            {
                showExternalTables();
            }
            else
            {
                showIternalTables();
            }
        }
        private void MenuItem_Click_ManagerCode(object sender, RoutedEventArgs e)
        {
            NetworkStream stream = socketOutput.GetStream();
            NetWorking.SendRequest(stream, NetWorking.Requestes.WAIT_ONE_MANAGER_CODE);
            bool status = NetWorking.getBoolOverNetStream(stream);
            if (status == false)
            {
                MessageBox.Show("Another Manager Is Changing The Code, Please Check","Manager Warning",MessageBoxButton.OK,MessageBoxImage.Stop);
            }
            else
            {
                ManangerCode managerCode = new ManangerCode(tableReservation.GET_ACCESS, stream);
                this.Hide();
                managerCode.ShowDialog();
                ManangerCode manageCode = new ManangerCode(ManangerCode.CHANGE_PASSWORD,stream);
                this.Hide();
                manageCode.ShowDialog();
                this.Show();
                NetWorking.SendRequest(stream, NetWorking.Requestes.RELEASE_MANAGER_CODE_MUTEX);
            }
        }

        private void MenuItem_Click_WorkersCrud(object sender, RoutedEventArgs e)
        {
            NetworkStream stream = socketOutput.GetStream();
            NetWorking.SendRequest(stream, NetWorking.Requestes.WAIT_ONE_WORKERS_CRUD);
            bool status = NetWorking.getBoolOverNetStream(stream);
            if (status == false)
            {
                MessageBox.Show("Another Manager Is In Workers CRUD ,Please Check","Manager Warning",MessageBoxButton.OK,MessageBoxImage.Stop);
            }
            else
            {
                ManangerCode managerCode = new ManangerCode(tableReservation.GET_ACCESS, stream);
                this.Hide();
                managerCode.ShowDialog();
                WorkersCrud workerCrud = new WorkersCrud(stream);
                this.Hide();
                workerCrud.ShowDialog();
                this.Show();
                NetWorking.SendRequest(stream, NetWorking.Requestes.RELEASE_WORKERS_CRUD_MUTEX);
            }
        }

        private void MenuItem_Click_DishesCrud(object sender, RoutedEventArgs e)
        {
            NetworkStream stream = socketOutput.GetStream();
            NetWorking.SendRequest(stream, NetWorking.Requestes.WAIT_ONE_DISHES_CRUD);
            bool status = NetWorking.getBoolOverNetStream(stream);
            if (status == false)
            {
                MessageBox.Show("Another Manager Is In Dishes CRUD, Please Check","Manager Warning",MessageBoxButton.OK,MessageBoxImage.Stop);
            }
            else
            {
                ManangerCode managerCode = new ManangerCode(tableReservation.GET_ACCESS, stream);
                this.Hide();
                managerCode.ShowDialog();
                DishesCrud dishCrud = new DishesCrud(stream);
                this.Hide();
                dishCrud.ShowDialog();
                this.Show();
                NetWorking.SendRequest(stream, NetWorking.Requestes.RELEASE_DISHES_CRUD_MUTEX);
            }
        }

        private void table1_btn_MouseEnter(object sender, MouseEventArgs e)
        {
            bool status;
            do
            {
                status = mouseEnterMutex.WaitOne(0);//because the transfer between the buttons
                                                    //can be very fast is starts to go crazy, so i put mutex
                                                    //to let the whole code to execute because network are os 
                                                    //heavy code
            } while (!status);

            //ask for reservation
            check++;
            int payment = 0;
            int table_number = Convert.ToInt32(((Button)sender).Content);
            NetworkStream stream = socketOutput.GetStream();
            string dish_string,worker_name;
            string[] seperated_dish;
            dishOfReservation dishOfReservation;
            int dishesCount;
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_RESERVATION);
            NetWorking.sentIntOverNetStream(stream, table_number);
            NetWorking.sentBoolOverNetStream(stream, false);
            dishesCount = NetWorking.getIntOverNetStream(stream);
            for (int i = 0; i < dishesCount; i++)
            {
                dish_string = NetWorking.getStringOverNetStream(stream);
                seperated_dish = dish_string.Split(' ');
                dishOfReservation = new dishOfReservation(seperated_dish[0], Convert.ToInt32(seperated_dish[1]), seperated_dish[2], Convert.ToInt32(seperated_dish[3]));
                payment += dishOfReservation.price * dishOfReservation.amount;
                showTableGrid.Items.Add(dishOfReservation);
            }

            if (showTableGrid.Items.Count > 0)
            {
                NetWorking.SendRequest(stream, NetWorking.Requestes.GET_WORKER_OF_RESERVATION);
                NetWorking.sentIntOverNetStream(stream, table_number);
                NetWorking.sentBoolOverNetStream(stream, false);
                worker_name = NetWorking.getStringOverNetStream(stream);
                lbl_payment.Content = payment.ToString() + " NIS " + ", " + worker_name;
            }
            else
            {
                lbl_payment.Content = "There Is No Open Reservation!";
            }
        }

        private void table1_btn_MouseLeave(object sender, MouseEventArgs e)
        {
            showTableGrid.Items.Clear();
            lbl_payment.Content = "";
            mouseEnterMutex.Release();
        }

        private void MenuItem_Click_openReservations(object sender, RoutedEventArgs e)
        {
            ShowReservations showReservations = new ShowReservations(socketOutput.GetStream(), "open");
            this.Hide();
            showReservations.ShowDialog();
            this.Show();
        }

        private void MenuItem_Click_closedReservations(object sender, RoutedEventArgs e)
        {
            ShowReservations showReservations = new ShowReservations(socketOutput.GetStream(), "closed");
            this.Hide();
            showReservations.ShowDialog();
            this.Show();
        }
    }
}
