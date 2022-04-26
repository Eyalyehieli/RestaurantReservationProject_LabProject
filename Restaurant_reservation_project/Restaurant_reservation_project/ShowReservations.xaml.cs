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
using System.Threading;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for ShowReservations.xaml
    /// </summary>
    public partial class ShowReservations : Window
    {
        NetworkStream stream;
        public ShowReservations(NetworkStream stream,String reservations)
        {
            InitializeComponent();
            this.stream = stream;
            if(reservations.Equals("open"))
            {
                openReservations();
            }
            else
            {
                closedReservations();
            }
        }
        public void openReservations()
        {
            reservations_lbl.Content = "Open Reservations";
            int table_number, price;
            string worker;
            NetWorking.SendRequest(stream,NetWorking.Requestes.GET_OPEN_RESERVATION);
            do
            {
                table_number = NetWorking.getIntOverNetStream(stream);
                price = NetWorking.getIntOverNetStream(stream);
                worker = NetWorking.getStringOverNetStream(stream);
                reservations_dataGrid.Items.Add(new showReservation(table_number, price, worker,DateTime.MinValue));
                Thread.Sleep(20);
            } while (stream.DataAvailable);
        }
        public void closedReservations()
        {
            reservations_lbl.Content = "Closed Reservations";
            int table_number, price;
            string worker;
            DateTime dateTime;
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_CLOSED_RESERVATION);
            do
            {
                table_number = NetWorking.getIntOverNetStream(stream);
                if (table_number == -1) { return; }
                price = NetWorking.getIntOverNetStream(stream);
                worker = NetWorking.getStringOverNetStream(stream);
                dateTime = NetWorking.getDateTimeOverNetStream(stream);
                reservations_dataGrid.Items.Add(new showReservation(table_number, price, worker, dateTime));
                Thread.Sleep(20);
            } while (stream.DataAvailable);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
