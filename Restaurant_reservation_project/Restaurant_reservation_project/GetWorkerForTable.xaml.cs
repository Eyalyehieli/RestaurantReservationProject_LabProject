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
        Worker worker;
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
            String job;
            int workersCount = 0;
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_ALL_WORKERS);
            workersCount = NetWorking.getIntOverNetStream(stream);
            for (int i = 0; i < workersCount; i++)
            {
                worker_name = NetWorking.getStringOverNetStream(stream);
                job = NetWorking.getStringOverNetStream(stream);
                if (job.Equals("Waiter") || job.Equals("Owner") || job.Equals("Manager"))
                {
                    workers_combo_box.Items.Add(worker_name);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //open new reservarion in this.table_number
            NetWorking.SendRequest(stream, NetWorking.Requestes.UPSERT_RESERVATION);
            NetWorking.sentStringOverNetStream(stream, selected_worker);
            NetWorking.sentIntOverNetStream(stream, this.table_number);
            NetWorking.sentBoolOverNetStream(stream, false);
            NetWorking.sentIntOverNetStream(stream, 0);
            this.Close();
        }

        private void workers_combo_box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox workersComboBox = sender as ComboBox;
            selected_worker = workers_combo_box.SelectedItem.ToString();
        }
    }
}