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
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Threading;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for tableReservation.xaml
    /// </summary>
    public partial class tableReservation : Window
    {
        public const int GET_MUTEX = 0;
        public const int GET_ACCESS = 1;
        const int RESERVATION_DATA_DRID = 0;
        const int DISHES_BY_CATEGORY_DATA_GRID = 1;
        const int AMOUNT_OF_DISH_PARAMETERS = 2;
        NetworkStream streamerOutput;
        NetworkStream streamerInput;
        byte[] request = new byte[10];
        GetWorkerForTable getWorker;
        int amountOfDish;
        private Action amountOfDishButtonAction = null;
        bool is_finished = false;
        bool is_occupied;
        List<dishOfReservation> dishesToAddForReservation = new List<dishOfReservation>();
        string worker_name;
        int table_number;
        bool mutex_status;
        Thread getRequestThread;
        public tableReservation(TcpClient socketOutput,TcpClient socketInput  ,int table_number)
        {
            InitializeComponent();
            streamerOutput = socketOutput.GetStream();
            streamerInput = socketInput.GetStream();
            this.table_number = table_number;
            change_tableNumber_comboBox.Visibility = Visibility.Hidden;
            change_worker_comboBox.Visibility = Visibility.Hidden;
            getRequestThread = new Thread(getRequest);
            //get mutex

            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.WAIT_ONE_MUTEX);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            mutex_status = NetWorking.getBoolOverNetStream(streamerOutput);
            if (!mutex_status)
            {
                //TODO:add manager code access 1-im manager,2-try again,3-wait
                ManangerCode managerCode = new ManangerCode(GET_MUTEX, streamerOutput, table_number);
                managerCode.ShowDialog();
            }

            getRequestThread.Start();

            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.GET_WORKER_OF_RESERVATION);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            NetWorking.sentBoolOverNetStream(streamerOutput, is_finished);//is_finished=false
            worker_name = NetWorking.getStringOverNetStream(streamerOutput);

            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.IS_OCCUPEID_TABLE);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            is_occupied = NetWorking.getBoolOverNetStream(streamerOutput);

            if (worker_name == "empty"&&is_occupied==false)//DO NOT NEED IS_OCCUPIED
            {
                getWorker = new GetWorkerForTable(streamerOutput,this.table_number);
                getWorker.ShowDialog();
                NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.GET_WORKER_OF_RESERVATION);
                NetWorking.sentIntOverNetStream(streamerOutput, table_number);
                NetWorking.sentBoolOverNetStream(streamerOutput, is_finished);//is_finished=false
                worker_name = NetWorking.getStringOverNetStream(streamerOutput);
            }
            /*NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.WAIT_ONE_TABLE);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            */

            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.GET_RESERVATION);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            NetWorking.sentBoolOverNetStream(streamerOutput, false);
            getDishes(reservation_data_grid, RESERVATION_DATA_DRID);
            worker_lbl.Content += worker_name;

            table_num_lbl.Content = "table number " + table_number + " reservation";
            create_data_grid_columns(dishes_by_category_data_grid);
            create_data_grid_columns(reservation_data_grid);

            amountOfDishBtn.Visibility = Visibility.Hidden;
            amountOfDishLbl.Visibility = Visibility.Hidden;
            amountOfDishTxb.Visibility = Visibility.Hidden;
        }

        public void getRequest()
        {
            String mutex_status;
            while(true)
            {
                mutex_status = NetWorking.getStringOverNetStream(streamerInput);
                if (mutex_status.Equals("A Manager grabbed the MUTEX")) 
                {                
                    MessageBox.Show("Your manager is in the reservation,please wait...");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dishes_by_category_data_grid.Items.Clear();
            Button categoryButton = sender as Button;
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.GET_DISHES_BY_CATEGORY);
            switch (categoryButton.Content)
            {
                case "main-dishes":NetWorking.sentStringOverNetStream(streamerOutput, "main-dishes");break;
                case "firsts":NetWorking.sentStringOverNetStream(streamerOutput, "firsts");break;
                case "deserts":NetWorking.sentStringOverNetStream(streamerOutput, "deserts");break;
                case "drinks":NetWorking.sentStringOverNetStream(streamerOutput, "drinks");break;
            }
            getDishes(dishes_by_category_data_grid,DISHES_BY_CATEGORY_DATA_GRID);
        }

        public void getDishes(DataGrid dataGrid,int whichDataGrid)
        {
            string dish_string;
            string[] seperated_dish;
            dishes dish;
            dishOfReservation dishOfReservation;
            if (whichDataGrid == DISHES_BY_CATEGORY_DATA_GRID)
            {
                do
                {
                    dish_string = NetWorking.getStringOverNetStream(streamerOutput);
                    if (dish_string != "empty")
                    {
                        seperated_dish = dish_string.Split(' ');
                        dish = new dishes(seperated_dish[0], Convert.ToInt32(seperated_dish[1]), seperated_dish[2]);
                        dataGrid.Items.Add(dish);
                    }
                }
                while (streamerOutput.DataAvailable);
            }//TODO:seperate to 2 functions
            else if (whichDataGrid == RESERVATION_DATA_DRID)
            {
                do
                {
                    dish_string = NetWorking.getStringOverNetStream(streamerOutput);
                    if (dish_string != "empty")
                    {
                        seperated_dish = dish_string.Split(' ');
                        dishOfReservation = new dishOfReservation(seperated_dish[0], Convert.ToInt32(seperated_dish[1]), seperated_dish[2], Convert.ToInt32(seperated_dish[3]));
                        dishesToAddForReservation.Add(dishOfReservation);//adding the existing dishes to my list
                        dataGrid.Items.Add(dishOfReservation);
                    }


                } while (streamerOutput.DataAvailable);
            }
        }
        private void create_data_grid_columns(DataGrid dataGrid)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            DataGridTextColumn col2 = new DataGridTextColumn();
            DataGridTextColumn col3 = new DataGridTextColumn();
            dataGrid.Columns.Add(col1);
            dataGrid.Columns.Add(col2);
            dataGrid.Columns.Add(col3);
            col1.Binding = new Binding("name");
            col2.Binding = new Binding("price");
            col3.Binding = new Binding("category");
            col1.Header = "name";
            col2.Header = "price";
            col3.Header = "category";
            if(dataGrid.Name== "reservation_data_grid")
            {
                DataGridTextColumn col4 = new DataGridTextColumn();
                dataGrid.Columns.Add(col4);
                col4.Binding = new Binding("amount");
                col4.Header = "amount";
            }
        }

        private void dishes_by_category_data_grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            string selected_dish="";
            string[] seperated_dish;
            dishOfReservation dishToAdd;
            if (dataGrid.SelectedIndex != -1)
            {
                for (int i = 0; i <= AMOUNT_OF_DISH_PARAMETERS; i++)
                {
                    selected_dish += GetCellGridValue(dataGrid, i);
                   // if (i == 2) { break; }
                    selected_dish += " ";
                }
                seperated_dish = selected_dish.Split(' ');
                MessageBox.Show(seperated_dish[0] + " selected");
                ShowAmountOfDish(seperated_dish[0]);
                amountOfDishButtonAction = () =>
                {
                    dishToAdd = new dishOfReservation(seperated_dish[0], Convert.ToInt32(seperated_dish[1]), seperated_dish[2], amountOfDish);
                    dishesToAddForReservation.Add(dishToAdd);
                    int succ = reservation_data_grid.Items.Add(dishToAdd);
                };//action to wait the amount od dish btn will pushed,wait to not show the reservation in the datagrid
            }
        }

        public void ShowAmountOfDish(string dish)
        {
            amountOfDishBtn.Visibility = Visibility.Visible;
            amountOfDishLbl.Visibility = Visibility.Visible;
            amountOfDishTxb.Visibility = Visibility.Visible;
            amountOfDishTxb.Focus();
            amountOfDishBtn.Content = "approve amount of " + dish + " dish";
        }
        public string GetCellGridValue(DataGrid dataGrid,int selectedIndex)
        {
            string cellValue;
            DataRowView rowview = dataGrid.SelectedItem as DataRowView;
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(dataGrid.SelectedIndex);
            DataGridCell RowColumn = dataGrid.Columns[selectedIndex].GetCellContent(row).Parent as DataGridCell;
            cellValue = ((TextBlock)RowColumn.Content).Text;
            return cellValue;
        }

        private void amountOfDishBtn_Click(object sender, RoutedEventArgs e)
        {
            amountOfDish = Convert.ToInt32(amountOfDishTxb.Text);
            amountOfDishButtonAction();
            amountOfDishButtonAction = null;
            amountOfDishTxb.Text = "";
            amountOfDishBtn.Visibility = Visibility.Hidden;
            amountOfDishLbl.Visibility = Visibility.Hidden;
            amountOfDishTxb.Visibility = Visibility.Hidden;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.UPSERT_RESERVATION);
            NetWorking.sentStringOverNetStream(streamerOutput, worker_name);
            NetWorking.sentIntOverNetStream(streamerOutput, this.table_number);
            NetWorking.sentBoolOverNetStream(streamerOutput, false);
            dishesToAddForReservation = RemoveDuplicates(dishesToAddForReservation);
            foreach(dishOfReservation dish in dishesToAddForReservation)
            {
                NetWorking.sentStringOverNetStream(streamerOutput, "not done");//instead of data availble because i send data and not getting data|synchronize
                NetWorking.sentStringOverNetStream(streamerOutput, dish.name);
                NetWorking.sentIntOverNetStream(streamerOutput, dish.price);
                NetWorking.sentStringOverNetStream(streamerOutput, dish.category);
                NetWorking.sentIntOverNetStream(streamerOutput, dish.amount);
            }
            NetWorking.sentStringOverNetStream(streamerOutput, "done");
            
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.RELEASE_MUTEX);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            
            this.Close();
        }

        private List<dishOfReservation> RemoveDuplicates(List<dishOfReservation> dishesToAddForReservation)
        {
            List<dishOfReservation> dishesToAdd = new List<dishOfReservation>();
            var query = dishesToAddForReservation.GroupBy(x => x.name,(dishName,dishes)=>new 
            {
                Name= dishName,
                price=dishes.First().price,
                category=dishes.First().category,
                amount= dishes.Sum(x=>x.amount)
            });
            foreach(var result in query)
            {
                dishesToAdd.Add(new dishOfReservation(result.Name, result.price, result.category, result.amount));
            }
            return dishesToAdd;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.UPSERT_RESERVATION);
            NetWorking.sentStringOverNetStream(streamerOutput, worker_name);
            NetWorking.sentIntOverNetStream(streamerOutput, this.table_number);
            NetWorking.sentBoolOverNetStream(streamerOutput, true);//the reservation is end,is_finished=true
            dishesToAddForReservation = RemoveDuplicates(dishesToAddForReservation);
            foreach (dishOfReservation dish in dishesToAddForReservation)
            {
                NetWorking.sentStringOverNetStream(streamerOutput, "not done");//instead of data availble|synchronize because i send data
                NetWorking.sentStringOverNetStream(streamerOutput, dish.name);
                NetWorking.sentIntOverNetStream(streamerOutput, dish.price);
                NetWorking.sentStringOverNetStream(streamerOutput, dish.category);
                NetWorking.sentIntOverNetStream(streamerOutput, dish.amount);
            }
            NetWorking.sentStringOverNetStream(streamerOutput, "done");

            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.RELEASE_MUTEX);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.RELEASE_TABLE);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            string[] workerFullName = worker_name.Split(' ');
            Recepit recepit = new Recepit(new Reservation(table_number,new Worker(workerFullName[0],workerFullName[1]),true, dishesToAddForReservation,DateTime.Now));
            this.Hide();
            recepit.ShowDialog();
            this.Close(); 
        }

        private void worker_lbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            change_worker_comboBox.Visibility = Visibility.Visible;
            string worker_name;
            String priority;
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.GET_ALL_WORKERS);
            do
            {
                worker_name = NetWorking.getStringOverNetStream(streamerOutput);
                priority = NetWorking.getStringOverNetStream(streamerOutput);
                if (priority.Equals("Waiter") || priority.Equals("Owner") || priority.Equals("Manager"))
                {
                    change_worker_comboBox.Items.Add(worker_name);
                }
                Thread.Sleep(100);
            }
            while (streamerOutput.DataAvailable);
        }

        private void table_num_lbl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            change_tableNumber_comboBox.Visibility = Visibility.Visible;
            List<int> occupiedTables = new List<int>();
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.GET_OCCUPIED_TABLES);
            Thread.Sleep(50);
            while(streamerOutput.DataAvailable)
            {
                occupiedTables.Add(NetWorking.getIntOverNetStream(streamerOutput));
                Thread.Sleep(50);
            }
            for(int i=0; i<29;i++)
            {
                if(!occupiedTables.Contains(i+1))
                {
                    change_tableNumber_comboBox.Items.Add(i + 1);
                }
            }
        }

        private void change_worker_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string workerName = change_worker_comboBox.SelectedItem.ToString();
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.UPDATE_WORKER_OF_RESERVATION);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            NetWorking.sentStringOverNetStream(streamerOutput,workerName);
            change_worker_comboBox.Visibility = Visibility.Hidden;
            worker_lbl.Content = "Worker: " + workerName;
            worker_name = workerName;
        }

        private void change_tableNumber_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int tableNumber = Convert.ToInt32(change_tableNumber_comboBox.SelectedItem.ToString());
            NetWorking.SendRequest(streamerOutput, NetWorking.Requestes.UPDATE_TABLE_NUMBER_OF_RESERVATION);
            NetWorking.sentIntOverNetStream(streamerOutput, table_number);
            NetWorking.sentIntOverNetStream(streamerOutput, tableNumber);
            change_tableNumber_comboBox.Visibility = Visibility.Hidden;
            table_num_lbl.Content="table number " + tableNumber + " reservation";
            table_number = tableNumber;
        }
    }
}
