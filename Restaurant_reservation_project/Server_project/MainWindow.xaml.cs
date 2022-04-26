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
using System.Threading;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Server_project
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        public const int NUMBER_OF_TABLES = 29;
        string ManagerCode;
        TcpListener serverInput = new TcpListener(IPAddress.Parse("127.0.0.1"), 8000);
        TcpListener serverOutput = new TcpListener(IPAddress.Parse("127.0.0.1"), 8001);//becase i want to send data to client but he is wait for data
        TcpClient[] clientsInput;
        TcpClient[] clientsOutput;
        NetworkStream[] streamsInput;
        NetworkStream[] streamsOutput;
        Thread[] threadsClient;
        int client_number = 0;
        DataLayer DBServer;
        TableMutex[] mutices = new TableMutex[NUMBER_OF_TABLES];
        Mutex managerCodeMutex;
        Mutex workersCrudMutex;
        Mutex dishesCrudMutex;
        bool[] is_occupied_tables = new bool[NUMBER_OF_TABLES];
        //string messagesText = "";
 

        public MainWindow()
        {
            InitializeComponent();
            DBServer = DataLayer.GetInstance();
            ManagerCode = "2580";
            for(int i=0;i<NUMBER_OF_TABLES;i++)
            {
                mutices[i] = new TableMutex(new Semaphore(1,1));
            }
            managerCodeMutex = new Mutex();
            workersCrudMutex = new Mutex();
            dishesCrudMutex = new Mutex();
            messages_lbl.Visibility = Visibility.Hidden;
            messages_txb.Visibility = Visibility.Hidden;
        }

        public void UpdateTextBlock(TextBlock txb, string text)
        {
            txb.Dispatcher.Invoke(() =>
            {
                txb.Text += text+Environment.NewLine;

            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int numberOfClients = Convert.ToInt32(number_of_clients_txb.Text);
            clientsInput = new TcpClient[numberOfClients];
            clientsOutput = new TcpClient[numberOfClients];
            streamsInput = new NetworkStream[numberOfClients];
            streamsOutput = new NetworkStream[numberOfClients];
            threadsClient = new Thread[numberOfClients];
            messages_lbl.Visibility = Visibility.Visible;
            messages_txb.Visibility = Visibility.Visible;
            start_btn.Visibility = Visibility.Hidden;
            number_of_clients_txb.Visibility = Visibility.Hidden;
            number_of_clients_lbl.Visibility = Visibility.Hidden;
            Task.Run( () =>
            {
                serverInput.Start();
                serverOutput.Start();
                UpdateTextBlock(messages_txb, "Server waiting for clients..."); 
                // MessageBox.Show("server waiting for clients...");//TODO:change to UI thread using Updae UI function
                while (client_number < numberOfClients)
                {
                    int current_client_number = client_number;//to synchronized the thread because its start after one loop ycyle is done,because Threa.start() is system call so until in execute the main thread is run
                    clientsInput[client_number] = serverInput.AcceptTcpClient();
                    clientsOutput[client_number] = serverOutput.AcceptTcpClient();
                    streamsInput[client_number] = clientsInput[client_number].GetStream();
                    streamsOutput[client_number] = clientsOutput[client_number].GetStream();
                    threadsClient[client_number] = new Thread(() => startClient(current_client_number));
                    threadsClient[client_number].Start();
                    client_number++;
                }
            });
        }
        public void startClient(int clientNumber)
        {
            Task.Run(() =>
            {
                byte[] request;
                while (true)
                {
                    request = NetWorking.GetRequest(streamsInput[clientNumber]);
                    switch ((NetWorking.Requestes)request[0])
                    {
                        case NetWorking.Requestes.GET_ALL_WORKERS: UpdateTextBlock(messages_txb, "client number "+clientNumber+ " GET_ALL_WORKERS");Get_all_workers(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.GET_DISHES_BY_CATEGORY: UpdateTextBlock(messages_txb, "client number " + clientNumber + " GET_DISHES_BY_CATEGORY");Get_dishes_by_category(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.GET_RESERVATION: UpdateTextBlock(messages_txb, "client number " + clientNumber + " GET_RESRVATION"); Get_reservation_by_table_number(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.GET_WORKER_OF_RESERVATION: UpdateTextBlock(messages_txb, "client number " + clientNumber + " GET_WORKER_OF_RESRVATION"); Get_worker_name_of_reservation(streamsInput[clientNumber]); break;
                        //case NetWorking.Requestes.INSERT_RESERVAION:Insert_reservation(streamsInput[clientNumber]);break;
                        //case NetWorking.Requestes.UPDATE_RESERVAION: Update_reservation(streamsInput[clientNumber]); break;
                        //case NetWorking.Requestes.TERMINATE_RESERVATION: Terminate_reservation(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.UPSERT_RESERVATION: UpdateTextBlock(messages_txb, "client number " + clientNumber + " UPSERT_RESERVATION"); upsert_reservation(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.WAIT_ONE_MUTEX: UpdateTextBlock(messages_txb, "client number " + clientNumber + " WAIT_ONE_MUTEX"); wait_one_mutex(streamsInput[clientNumber], clientNumber); break;
                        case NetWorking.Requestes.RELEASE_MUTEX: UpdateTextBlock(messages_txb, "client number " + clientNumber + " RELEASE_MUTEX"); release_mutex(streamsInput[clientNumber], clientNumber); break;
                        case NetWorking.Requestes.WAIT_ONE_MUTEX_MANAGER: UpdateTextBlock(messages_txb, "client number " + clientNumber + " WAIT_ONE_MUTEX_MANAGER"); wait_one_mutex_manager(streamsInput[clientNumber], clientNumber); break;
                        case NetWorking.Requestes.IS_OCCUPEID_TABLE: UpdateTextBlock(messages_txb, "client number " + clientNumber + " IS_OCCUPIED_TABLE"); is_occupied_table(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.WAIT_ONE_TABLE: UpdateTextBlock(messages_txb, "client number " + clientNumber + " WAIT_ONE_TABLE"); wait_one_table(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.RELEASE_TABLE: UpdateTextBlock(messages_txb, "client number " + clientNumber + " RELEASE_TABLE"); release_table(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.GET_MANAGER_CODE: UpdateTextBlock(messages_txb, "client number " + clientNumber + " GET_MANAGER_CODE"); Get_manager_code(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.CHANGE_MANAGER_CODE: UpdateTextBlock(messages_txb, "client number " + clientNumber + " CHANGE_MANAGER_CODE"); change_manager_code(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.WAIT_ONE_MANAGER_CODE: UpdateTextBlock(messages_txb, "client number " + clientNumber + " WAIT_ONE_MANAGER_CODE"); wait_one_manager_code_mutex(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.RELEASE_MANAGER_CODE_MUTEX: UpdateTextBlock(messages_txb, "client number " + clientNumber + " RELEASE_MANAGER_CODE_MUTEX"); release_manager_code_mutex(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.WAIT_ONE_WORKERS_CRUD: UpdateTextBlock(messages_txb, "client number " + clientNumber + " WAIT_ONE_WORKER_CRUD"); wait_one_workers_crud_mutex(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.RELEASE_WORKERS_CRUD_MUTEX: UpdateTextBlock(messages_txb, "client number " + clientNumber + " RELEASE_WORKERS_CRUD_MUTEX"); release_workers_crud_mutex(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.WAIT_ONE_DISHES_CRUD: UpdateTextBlock(messages_txb, "client number " + clientNumber + " WAIT_ONE_DISHES_CRUD"); wait_one_dishes_crud_mutex(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.RELEASE_DISHES_CRUD_MUTEX: UpdateTextBlock(messages_txb, "client number " + clientNumber + " RELEASE_DISHES_CRUD_MUTEX"); release_dishes_crud_mutex(streamsInput[clientNumber]); break;
                        // case NetWorking.Requestes.UPSERT_WORKER: upsert_worker(streamsInput[clientNumber]);break;
                        // case NetWorking.Requestes.UPSERT_DISH:upsert_dish(streamsInput[clientNumber]);break;
                        case NetWorking.Requestes.INSERT_WORKER: UpdateTextBlock(messages_txb, "client number " + clientNumber + " INSERT_WORKER"); Insert_worker(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.UPDATE_WORKER: UpdateTextBlock(messages_txb, "client number " + clientNumber + " UPDATE_WORKER"); Update_Worker(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.INSERT_DISH: UpdateTextBlock(messages_txb, "client number " + clientNumber + " INSERT_DISH"); Insert_dish(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.UPDATE_DISH: UpdateTextBlock(messages_txb, "client number " + clientNumber + " UPDATE_DISH"); Update_dish(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.DELETE_WORKER: UpdateTextBlock(messages_txb, "client number " + clientNumber + " DELETE_WORKER"); delete_worker(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.DELETE_DISH: UpdateTextBlock(messages_txb, "client number " + clientNumber + " DELETE_DISH"); delete_dish(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.UPDATE_WORKER_OF_RESERVATION: UpdateTextBlock(messages_txb, "client number " + clientNumber + " UPDATE_WORKER_OF_RESERVATION"); Update_worker_of_reservation(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.UPDATE_TABLE_NUMBER_OF_RESERVATION: UpdateTextBlock(messages_txb, "client number " + clientNumber + " UPDATE_TABLE_NUMBER_OF_RESERVATION"); Update_table_number_of_reservation(streamsInput[clientNumber], clientNumber); break;
                        case NetWorking.Requestes.GET_OCCUPIED_TABLES: UpdateTextBlock(messages_txb, "client number " + clientNumber + " GET_OCCUPIED_TABLES"); Get_occupied_tables(streamsInput[clientNumber]); break;
                        case NetWorking.Requestes.GET_OPEN_RESERVATION:UpdateTextBlock(messages_txb, "client number " + clientNumber + "GET_OPEN_RESERVATION"); get_open_reservations(streamsInput[clientNumber]);break;
                        case NetWorking.Requestes.GET_CLOSED_RESERVATION:UpdateTextBlock(messages_txb, "client number " + clientNumber + "GET_CLOSED_RESERVATION"); get_closed_reservation(streamsInput[clientNumber]);break;
                    }
                }
            });
        }

        private void get_closed_reservation(NetworkStream stream)
        {
            List<Reservation> reservations = DBServer.getClosedReservation();
            if (reservations.Count == 0) { NetWorking.sentIntOverNetStream(stream, -1);return; }
            int price;
            foreach (Reservation res in reservations)
            {
                price = 0;
                NetWorking.sentIntOverNetStream(stream, res.table_number);
                foreach (dishOfReservation dish in res.allDishes)
                {
                    price += dish.price * dish.amount;
                }
                NetWorking.sentIntOverNetStream(stream, price);
                NetWorking.sentStringOverNetStream(stream, res.worker.ToString());
                NetWorking.sendDateTimeOverNetStream(stream, res.dateTime);
            }
        }
        private void get_open_reservations(NetworkStream stream)
        {
            List<Reservation> reservations = DBServer.getOpenReservation();
            int price;
            foreach (Reservation res in reservations)
            {
                price = 0;
                NetWorking.sentIntOverNetStream(stream,res.table_number);
                foreach(dishOfReservation dish in res.allDishes)
                {
                    price += dish.price * dish.amount;
                }
                NetWorking.sentIntOverNetStream(stream, price);
                NetWorking.sentStringOverNetStream(stream, res.worker.ToString());
            }
        }

        private void release_dishes_crud_mutex(NetworkStream networkStream)
        {
            dishesCrudMutex.ReleaseMutex();
        }

        private void wait_one_dishes_crud_mutex(NetworkStream stream)
        {
            bool status = dishesCrudMutex.WaitOne(500);
            NetWorking.sentBoolOverNetStream(stream, status);
        }

        private void release_workers_crud_mutex(NetworkStream networkStream)
        {
            workersCrudMutex.ReleaseMutex();
        }

        private void wait_one_workers_crud_mutex(NetworkStream stream)
        {
            bool status = workersCrudMutex.WaitOne(500);
            NetWorking.sentBoolOverNetStream(stream, status);
        }

        private void Get_occupied_tables(NetworkStream stream)
        {
            foreach(Reservation res in DBServer.GetOcuppiedTables())
            {
                NetWorking.sentIntOverNetStream(stream, res.table_number);
            }
        }

        private void Update_table_number_of_reservation(NetworkStream stream,int clientNumber)
        {
            int prevTableNumber = NetWorking.getIntOverNetStream(stream);
            int newTableNumber = NetWorking.getIntOverNetStream(stream);
            mutices[prevTableNumber - 1].getMutex().Release();
            mutices[prevTableNumber - 1].setClientNumber(TableMutex.NONE_TABLE);
            bool status=mutices[newTableNumber - 1].getMutex().WaitOne(100);
            if (status) { mutices[newTableNumber - 1].setClientNumber(clientNumber); }
            DBServer.UpdateTableNumberByReservation(prevTableNumber, newTableNumber);
        }

        private void Update_worker_of_reservation(NetworkStream stream)
        {
            int tableNumber = NetWorking.getIntOverNetStream(stream);
            string workerName = NetWorking.getStringOverNetStream(stream);
            DBServer.UpdateWorkerByReservation(tableNumber, workerName);
        }

        private void Update_dish(NetworkStream stream)
        {
            string prevName = NetWorking.getStringOverNetStream(stream);
            int prevPrice = NetWorking.getIntOverNetStream(stream);
            string prevCategory = NetWorking.getStringOverNetStream(stream);
            dishes prevDish = new dishes(prevName, prevPrice, prevCategory);
            string newName = NetWorking.getStringOverNetStream(stream);
            int newPrice = NetWorking.getIntOverNetStream(stream);
            string newCategory = NetWorking.getStringOverNetStream(stream);
            dishes newDish = new dishes(newName, newPrice, newCategory);
            DBServer.UpdateDish(prevDish,newDish);

        }

        private void Insert_dish(NetworkStream stream)
        {
            string name = NetWorking.getStringOverNetStream(stream);
            int price = NetWorking.getIntOverNetStream(stream);
            string category = NetWorking.getStringOverNetStream(stream);
            dishes dish = new dishes(name, price, category);
            DBServer.InsertDish(dish);
        }

        private void Update_Worker(NetworkStream stream)
        {
            string prevFirstName = NetWorking.getStringOverNetStream(stream);
            string prevLastName = NetWorking.getStringOverNetStream(stream);
            string prevPriority = NetWorking.getStringOverNetStream(stream);
            Worker prevWorker = new Worker(prevFirstName, prevLastName, prevPriority);
            string newFirstName = NetWorking.getStringOverNetStream(stream);
            string newvLastName = NetWorking.getStringOverNetStream(stream);
            string newvPriority = NetWorking.getStringOverNetStream(stream);
            Worker newvWorker = new Worker(newFirstName, newvLastName, newvPriority);
            DBServer.UpdateWorker(prevWorker, newvWorker);
        }

        private void Insert_worker(NetworkStream stream)
        {
            string firstName = NetWorking.getStringOverNetStream(stream);
            string lastName = NetWorking.getStringOverNetStream(stream);
            string priority = NetWorking.getStringOverNetStream(stream);
            Worker worker = new Worker(firstName, lastName, priority);
            DBServer.InsertWorker(worker);
        }

        private void delete_dish(NetworkStream stream)
        {
            string name = NetWorking.getStringOverNetStream(stream);
            int price = NetWorking.getIntOverNetStream(stream);
            string category = NetWorking.getStringOverNetStream(stream);
            dishes dish = new dishes(name, price, category);
            DBServer.DeleteDish(dish);
        }

        private void delete_worker(NetworkStream stream)
        {

            string firstName = NetWorking.getStringOverNetStream(stream);
            string lastName = NetWorking.getStringOverNetStream(stream);
            string priority = NetWorking.getStringOverNetStream(stream);
            Worker worker = new Worker(firstName, lastName, priority);
            DBServer.DeleteWorker(worker);
        }
        
        private void release_manager_code_mutex(NetworkStream networkStream)
        {
            managerCodeMutex.ReleaseMutex();
        }

        private void wait_one_manager_code_mutex(NetworkStream stream)
        {
            bool status = managerCodeMutex.WaitOne(500);
            NetWorking.sentBoolOverNetStream(stream, status);
        }

        private void change_manager_code(NetworkStream stream)
        {
            this.ManagerCode=NetWorking.getStringOverNetStream(stream);
        }

        private void Get_manager_code(NetworkStream stream)
        {
            NetWorking.sentStringOverNetStream(stream, this.ManagerCode);
        }

        private void release_table(NetworkStream stream)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            is_occupied_tables[table_number - 1] = false;
        }

        private void wait_one_table(NetworkStream stream)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            is_occupied_tables[table_number - 1] = true;
        }

        private void is_occupied_table(NetworkStream stream)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            NetWorking.sentBoolOverNetStream(stream, is_occupied_tables[table_number - 1]);
        }

        private void wait_one_mutex_manager(NetworkStream stream,int clientNumber)
        {
            int table_number;
            bool status;
            NetworkStream previousClientStreamer;
            //1-release the mutex from the current client
            table_number = NetWorking.getIntOverNetStream(stream);
            int clientNumberBefore = mutices[table_number - 1].getClientNumber();
            mutices[table_number-1].getMutex().Release();
            previousClientStreamer = streamsOutput[mutices[table_number-1].getClientNumber()];
            mutices[table_number - 1].setClientNumber(TableMutex.NONE_TABLE);
            //TODO:2-send the current catcher client a message
            NetWorking.sentStringOverNetStream(previousClientStreamer, "A Manager grabbed the MUTEX");
            //3- catch the mutex to the manager client
            status = mutices[table_number - 1].getMutex().WaitOne(500);
            if (status)
            {
                mutices[table_number - 1].setClientNumber(clientNumber);
            }
            //4-let the manager client get in critical section
            NetWorking.sentBoolOverNetStream(stream, status);
        }
        private void release_mutex(NetworkStream stream,int clientNumber)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            mutices[table_number - 1].getMutex().Release();
            mutices[table_number - 1].setClientNumber(TableMutex.NONE_TABLE);
        }

        private void wait_one_mutex(NetworkStream stream,int clientNumber)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            bool status=mutices[table_number - 1].getMutex().WaitOne(500);
            if (status) { mutices[table_number - 1].setClientNumber(clientNumber); }
            NetWorking.sentBoolOverNetStream(stream, status);
        }

        private void upsert_reservation(NetworkStream stream)
        {  
            List<dishOfReservation> dishes = new List<dishOfReservation>();
            string worker_name = NetWorking.getStringOverNetStream(stream);
            string[] workerName = worker_name.Split(' ');
            int table_number = NetWorking.getIntOverNetStream(stream);
            bool is_finished = NetWorking.getBoolOverNetStream(stream);
         
            string is_done;
            do
            {
                is_done = NetWorking.getStringOverNetStream(stream);
                if (is_done == "done") { break;}
                string name = NetWorking.getStringOverNetStream(stream);
                int price = NetWorking.getIntOverNetStream(stream);
                string category = NetWorking.getStringOverNetStream(stream);
                int amount = NetWorking.getIntOverNetStream(stream);
                dishes.Add(new dishOfReservation(name,price,category,amount));
            } while (is_done=="not done");
            //while (stream.DataAvailable) ;//because the delay i used done\not done
            DBServer.upsert_reservation(table_number, worker_name, is_finished, dishes);
        }
       
        private void Get_worker_name_of_reservation(NetworkStream stream)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            bool isFinished = NetWorking.getBoolOverNetStream(stream);
            string worker_name = DBServer.get_worker_name_by_reservation(table_number, isFinished);
            if (worker_name != null)
            {
                NetWorking.sentStringOverNetStream(stream, worker_name);
            }
            else
            {
                worker_name = "empty";
                NetWorking.sentStringOverNetStream(stream, worker_name);
            }
        }

        public void Get_reservation_by_table_number(NetworkStream stream)
        {
            int table_number = NetWorking.getIntOverNetStream(stream);
            bool isFinished = NetWorking.getBoolOverNetStream(stream);
            string dishString;
            List<dishOfReservation> reservation_by_table_number = DBServer.get_reservationByTableNumber(table_number,isFinished);
            if(reservation_by_table_number!=null)//the problem was that it didnt send "empty" but there was no dish 
            {
                foreach(dishOfReservation dish in reservation_by_table_number)
                {
                    dishString = dish.ToString();
                    NetWorking.sentStringOverNetStream(stream, dishString);
                }
            }
            else
            {
                dishString = "empty";
                NetWorking.sentStringOverNetStream(stream, dishString);
            }
        }

        public void Get_dishes_by_category(NetworkStream stream)
        {
            string dishString;
            string category = NetWorking.getStringOverNetStream(stream);
            List<dishes> dishes = DBServer.get_all_dishes_by_category(category);
            foreach(dishes dish in dishes)
            {
                dishString = dish.ToString();
                NetWorking.sentStringOverNetStream(stream, dishString);
            }
        }
        public void Get_all_workers(NetworkStream stream)
        {
            string workerString;
            string priority;
            List<Worker> workers = DBServer.get_all_workers();
            foreach (Worker worker in workers)
            {
                workerString = worker.ToString();
                priority = worker.accessPriority;
                NetWorking.sentStringOverNetStream(stream, workerString);
                NetWorking.sentStringOverNetStream(stream, priority);
            }
        }
    }
}