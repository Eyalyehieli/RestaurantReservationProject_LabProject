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
using System.Net.Sockets;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for settings.xaml
    /// </summary>
    public partial class settings : Window
    {
        public const int CHANGE_PASSWORD = 2;
        NetworkStream stream;
        public settings(NetworkStream stream)
        {
            InitializeComponent();
            this.stream = stream;
        }

        /*
   private void Button_Click(object sender, RoutedEventArgs e)
   {
       ManangerCode manageCode = new ManangerCode(settings.CHANGE_PASSWORD,this.stream);
       this.Hide();
       manageCode.ShowDialog();
       NetWorking.SendRequest(stream, NetWorking.Requestes.RELEASE_SETTING_MUTEX);
       this.Close();
   }

   private void Button_Click_1(object sender, RoutedEventArgs e)
   {
       NetWorking.SendRequest(stream, NetWorking.Requestes.RELEASE_SETTING_MUTEX);
       this.Close();
   }

   private void Button_Click_2(object sender, RoutedEventArgs e)
   {
       WorkersCrud workerCrud = new WorkersCrud(stream);
       this.Hide();
       workerCrud.ShowDialog();
       this.Show();
   }

   private void Button_Click_3(object sender, RoutedEventArgs e)
   {
       DishesCrud dishCrud = new DishesCrud(stream);
       this.Hide();
       dishCrud.ShowDialog();
       this.Show();
   }
*/
    }
}
