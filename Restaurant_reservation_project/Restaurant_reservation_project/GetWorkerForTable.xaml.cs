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
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for GetWorkerForTable.xaml
    /// </summary>
    public partial class GetWorkerForTable : Window
    {
        NetworkStream stream;
        private string selected_worker;
        private int table_number;
        public GetWorkerForTable(NetworkStream stream,int table_number)
        {
            InitializeComponent();
            this.stream = stream;
            this.table_number = table_number;
            loadWorkersToComboBox();
        }
        public void loadWorkersToComboBox()
        {
            string worker_name;
            String priority;
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_ALL_WORKERS);
            do
            {
                worker_name = NetWorking.getStringOverNetStream(stream);
                priority = NetWorking.getStringOverNetStream(stream);
                if (priority.Equals("Waiter") || priority.Equals("Owner") || priority.Equals("Manager"))
                {
                    workers_combo_box.Items.Add(worker_name);
                }
                Thread.Sleep(100);//wait for ther server to send the worker,for synchronized
            }
            while (stream.DataAvailable);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //open new reservarion in this.table_number
            NetWorking.SendRequest(stream, NetWorking.Requestes.UPSERT_RESERVATION);
            NetWorking.sentStringOverNetStream(stream, selected_worker);
            NetWorking.sentIntOverNetStream(stream, this.table_number);
            NetWorking.sentBoolOverNetStream(stream, false);
            NetWorking.sentStringOverNetStream(stream, "done");
            this.Close();
        }

        private void workers_combo_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox workersComboBox = sender as ComboBox;
            selected_worker = workers_combo_box.SelectedItem.ToString();
        }
    }
}