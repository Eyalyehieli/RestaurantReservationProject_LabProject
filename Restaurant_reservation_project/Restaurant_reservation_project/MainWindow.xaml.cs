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
        const int NUMBER_OF_TABLES = 15;
        TcpClient socketOutput = new TcpClient();
        TcpClient socketInput = new TcpClient();
        int[] isTableCreatedCounter = new int[NUMBER_OF_TABLES + 1];//put it in the server
        tableReservation[] tbl_reservations = new tableReservation[NUMBER_OF_TABLES + 1];
        List<Button> listButtons = new List<Button>();
        Mutex mouseEnterMutex;
        public MainWindow()
        {
            InitializeComponent();
            GetLogicalChildCollection(this, listButtons);
            showIternalTables();
            create_data_grid_columns(showTableGrid);
            socketOutput.Connect(IPAddress.Parse("127.0.0.1"), 8000);
            socketInput.Connect(IPAddress.Parse("127.0.0.1"), 8001);
            MessageBox.Show("connected");
            mouseEnterMutex = new Mutex();
        }

        public bool isTableButton(Button b)
        {
            return (b.Content.ToString() != "Settings" && b.Content.ToString() != "change to external tables" && b.Content.ToString() != "change to iternal tables");
        }
        private void showIternalTables()
        {
            switcher.Content = "change to external tables";
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
            switcher.Content = "change to iternal tables";
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
            if(b.Content.ToString()== "change to external tables")
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
                MessageBox.Show("Another Manager is changing the code, please check");
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
                MessageBox.Show("Another Manager is in Workers CRUD ,please check");
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
                MessageBox.Show("Another Manager is in Dishes CRUD ,please check");
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

        private void create_data_grid_columns(DataGrid dataGrid)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            DataGridTextColumn col2 = new DataGridTextColumn();
            DataGridTextColumn col3 = new DataGridTextColumn();
            DataGridTextColumn col4 = new DataGridTextColumn();
            dataGrid.Columns.Add(col1);
            dataGrid.Columns.Add(col2);
            dataGrid.Columns.Add(col3);
            dataGrid.Columns.Add(col4);
            col1.Binding = new Binding("name");
            col2.Binding = new Binding("price");
            col3.Binding = new Binding("category");
            col4.Binding = new Binding("amount");
            col1.Header = "name";
            col2.Header = "price";
            col3.Header = "category";
            col4.Header = "amount";
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
            int payment = 0;
            int table_number = Convert.ToInt32(((Button)sender).Content);
            NetworkStream stream = socketOutput.GetStream();
            string dish_string;
            string[] seperated_dish;
            dishOfReservation dishOfReservation;
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_RESERVATION);
            NetWorking.sentIntOverNetStream(stream, table_number);
            NetWorking.sentBoolOverNetStream(stream, false);

            //get dishes if reservation and add to the tableGrid
            do
            {
                dish_string = NetWorking.getStringOverNetStream(stream);
                if (dish_string != "empty")
                {
                    seperated_dish = dish_string.Split(' ');
                    dishOfReservation = new dishOfReservation(seperated_dish[0], Convert.ToInt32(seperated_dish[1]), seperated_dish[2], Convert.ToInt32(seperated_dish[3]));
                    payment += dishOfReservation.price*dishOfReservation.amount;
                    showTableGrid.Items.Add(dishOfReservation);
                }
            } while (stream.DataAvailable);

            if (showTableGrid.Items.Count > 0)
            {
                lbl_payment.Content = payment.ToString() + " NIS";
            }
            else
            {
                lbl_payment.Content = "There Is No Open Reservation In This Table";
            }
            mouseEnterMutex.ReleaseMutex();
        }

        private void table1_btn_MouseLeave(object sender, MouseEventArgs e)
        {
            showTableGrid.Items.Clear();
            lbl_payment.Content = "";
        }


    }
}
