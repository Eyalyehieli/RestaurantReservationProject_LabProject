﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Data;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for WorkersCrud.xaml
    /// </summary>
    public partial class WorkersCrud : Window
    {
        public enum DB_EVENTS_WORKER { EDIT_WORKER, INSERT_WORKER };
        NetworkStream stream;
        Worker prevWorker;
        DB_EVENTS_WORKER WorkerDBEvent;
        public WorkersCrud(NetworkStream stream)
        {
            InitializeComponent();
            this.stream = stream;
            create_data_grid_columns(workers_data_grid);
            loadWorkers();
            firstName_lbl.Visibility = Visibility.Hidden;
            lastName_lbl.Visibility = Visibility.Hidden;
            priority_lbl.Visibility = Visibility.Hidden;
            firstName_txb.Visibility = Visibility.Hidden;
            lastName_txb.Visibility = Visibility.Hidden;
            priority_txb.Visibility = Visibility.Hidden;
            done_btn.Visibility = Visibility.Hidden;
        }

        private void create_data_grid_columns(DataGrid dataGrid)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            DataGridTextColumn col2 = new DataGridTextColumn();
            DataGridTextColumn col3 = new DataGridTextColumn();
            dataGrid.Columns.Add(col1);
            dataGrid.Columns.Add(col2);
            dataGrid.Columns.Add(col3);
            col1.Binding = new Binding("first_name");
            col2.Binding = new Binding("last_name");
            col3.Binding = new Binding("accessPriority");
            col1.Header = "First Name";
            col2.Header = "Last Name";
            col3.Header = "Priority";
        }

        public void loadWorkers()
        {
            string worker_name,priority;
            string[] worker_name_splited;
            Worker w;
            workers_data_grid.Items.Clear();
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_ALL_WORKERS);
            do
            {
                worker_name = NetWorking.getStringOverNetStream(stream);
                priority = NetWorking.getStringOverNetStream(stream);
                worker_name_splited = worker_name.Split(' ');
                w = new Worker(worker_name_splited[0], worker_name_splited[1], priority);
                int suc= workers_data_grid.Items.Add(w);
                Thread.Sleep(100);//becase its check if there is data but the server didnt make to send the data
            }
            while (stream.DataAvailable);//race condition
           // workers_data_grid.ItemsSource = items;
        }

        private void add_btn_Click(object sender, RoutedEventArgs e)
        {
            add_btn.Visibility = Visibility.Hidden;
            edit_btn.Visibility = Visibility.Hidden;
            delete_btn.Visibility = Visibility.Hidden;
            workers_data_grid.Visibility = Visibility.Hidden;
            firstName_lbl.Visibility = Visibility.Visible;
            lastName_lbl.Visibility = Visibility.Visible;
            priority_lbl.Visibility = Visibility.Visible;
            firstName_txb.Visibility = Visibility.Visible;
            lastName_txb.Visibility = Visibility.Visible;
            priority_txb.Visibility = Visibility.Visible;
            done_btn.Visibility = Visibility.Visible;
            WorkerDBEvent = DB_EVENTS_WORKER.INSERT_WORKER;
        }

        private void done_btn_Click(object sender, RoutedEventArgs e)
        {
            //get the prev properties from datagrid
            string newFirstName = firstName_txb.Text;
            string newLastName = lastName_txb.Text;
            string newPriority = priority_txb.Text;
            if (WorkerDBEvent == DB_EVENTS_WORKER.INSERT_WORKER)
            {
                NetWorking.SendRequest(stream, NetWorking.Requestes.INSERT_WORKER);
                NetWorking.sentStringOverNetStream(stream, newFirstName);
                NetWorking.sentStringOverNetStream(stream, newLastName);
                NetWorking.sentStringOverNetStream(stream, newPriority);
            }
            else
            {
                NetWorking.SendRequest(stream, NetWorking.Requestes.UPDATE_WORKER);
                NetWorking.sentStringOverNetStream(stream, prevWorker.first_name);
                NetWorking.sentStringOverNetStream(stream, prevWorker.last_name);
                NetWorking.sentStringOverNetStream(stream, prevWorker.accessPriority);
                NetWorking.sentStringOverNetStream(stream, newFirstName);
                NetWorking.sentStringOverNetStream(stream, newLastName);
                NetWorking.sentStringOverNetStream(stream, newPriority);
            }
            add_btn.Visibility = Visibility.Visible;
            edit_btn.Visibility = Visibility.Visible;
            delete_btn.Visibility = Visibility.Visible;
            workers_data_grid.Visibility = Visibility.Visible;
            firstName_lbl.Visibility = Visibility.Hidden;
            lastName_lbl.Visibility = Visibility.Hidden;
            priority_lbl.Visibility = Visibility.Hidden;
            firstName_txb.Visibility = Visibility.Hidden;
            lastName_txb.Visibility = Visibility.Hidden;
            priority_txb.Visibility = Visibility.Hidden;
            done_btn.Visibility = Visibility.Hidden;
            loadWorkers();
         }

        private void edit_btn_Click(object sender, RoutedEventArgs e)
        {
            add_btn.Visibility = Visibility.Hidden;
            edit_btn.Visibility = Visibility.Hidden;
            delete_btn.Visibility = Visibility.Hidden;
            workers_data_grid.Visibility = Visibility.Hidden;
            firstName_lbl.Visibility = Visibility.Visible;
            lastName_lbl.Visibility = Visibility.Visible;
            priority_lbl.Visibility = Visibility.Visible;
            firstName_txb.Visibility = Visibility.Visible;
            lastName_txb.Visibility = Visibility.Visible;
            priority_txb.Visibility = Visibility.Visible;
            done_btn.Visibility = Visibility.Visible;
            if (workers_data_grid != null && workers_data_grid.SelectedItems != null && workers_data_grid.SelectedItems.Count == 1)
            {
                prevWorker = (Worker)workers_data_grid.SelectedItem;
                firstName_txb.Text = prevWorker.first_name;
                lastName_txb.Text = prevWorker.last_name;
                priority_txb.Text = prevWorker.accessPriority;
            }
            WorkerDBEvent = DB_EVENTS_WORKER.EDIT_WORKER;
        }

        private void delete_btn_Click(object sender, RoutedEventArgs e)
        {
            Worker worker=null;
            if (workers_data_grid != null && workers_data_grid.SelectedItems != null && workers_data_grid.SelectedItems.Count == 1)
            {
                worker = (Worker)workers_data_grid.SelectedItem;
                firstName_txb.Text = worker.first_name;
                lastName_txb.Text = worker.last_name;
                priority_txb.Text = worker.accessPriority;
            }
            NetWorking.SendRequest(stream, NetWorking.Requestes.DELETE_WORKER);
            NetWorking.sentStringOverNetStream(stream, worker.first_name);
            NetWorking.sentStringOverNetStream(stream, worker.last_name);
            NetWorking.sentStringOverNetStream(stream, worker.accessPriority);
            add_btn.Visibility = Visibility.Visible;
            edit_btn.Visibility = Visibility.Visible;
            delete_btn.Visibility = Visibility.Visible;
            workers_data_grid.Visibility = Visibility.Visible;
            firstName_lbl.Visibility = Visibility.Hidden;
            lastName_lbl.Visibility = Visibility.Hidden;
            priority_lbl.Visibility = Visibility.Hidden;
            firstName_txb.Visibility = Visibility.Hidden;
            lastName_txb.Visibility = Visibility.Hidden;
            priority_txb.Visibility = Visibility.Hidden;
            done_btn.Visibility = Visibility.Hidden;
            loadWorkers();
        }
    }
}