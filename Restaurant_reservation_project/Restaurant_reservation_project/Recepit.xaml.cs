using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
using System.Drawing;
using Brushes = System.Drawing.Brushes;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for Recepit.xaml
    /// </summary>
    public partial class Recepit : Window
    {
        private Reservation reservation;
        Worker w;
        dishes d;
        Random rnd = new Random();
        int payment;
        public Recepit(Reservation reservation)
        {
            InitializeComponent();
            this.reservation = reservation;
            payment = 0;
            PrintRecepit();
        }

        private void PrintRecepit()
        {
            PrintHeader();
            PrintLongLine();
            PrintStart();
            PrintShortLine();
            PrintDishes();
            PrintShortLine();
            PrintTotal();
            PrintMidLine();
            PrintEnd();
        }
        private void PrintLongLine()
        {
            string str = Environment.NewLine+"------------------------------------------------------";
            TextRange line = new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            line.Text = str;
            line.ApplyPropertyValue(TextElement.FontSizeProperty, 20.0);
            line.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }

        private void PrintMidLine()
        {
            string str = Environment.NewLine + "################################" ;
            TextRange line = new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            line.Text = str;
            line.ApplyPropertyValue(TextElement.FontSizeProperty, 20.0);
            line.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

        }
        private void PrintShortLine()
        {
            string str = Environment.NewLine+"                **********************          ";
            TextRange line = new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            line.Text = str;
            line.ApplyPropertyValue(TextElement.FontSizeProperty, 20.0);
            line.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }
        private void PrintHeader()
        {
            string str = "             Receipt";
            TextRange headerTextRange = new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            headerTextRange.Text = str;
            headerTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, 30.0);
            headerTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            headerTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Arial");
        }
        private void PrintStart()
        {
            string startStr = 
            Environment.NewLine + "                                                                  " +  rnd.Next(0, 999999) + "-" + rnd.Next(0, 10) +
            Environment.NewLine + "                                                     Mark Shagal 113, Ashdod" +
            Environment.NewLine + "                                                                7766300      " +
            Environment.NewLine + "                                                            #077-7005-037" +
            Environment.NewLine + "                                                         " + reservation.dateTime +
            Environment.NewLine + "Worker Name: " + reservation.worker.ToString() +
            Environment.NewLine + "Table Number: " + reservation.table_number.ToString();
            TextRange startTextRange = new TextRange(rich_txb_recepit.Document.ContentEnd,rich_txb_recepit.Document.ContentEnd);
            startTextRange.Text = startStr;
            startTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, 8.0);
            startTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            startTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Arial");
        }
        private void PrintDishes()
        {
            string str = Environment.NewLine;
            TextRange dishesTextRange=new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            foreach (dishOfReservation dish in reservation.allDishes)
            {
                str += dish.name + " x " + dish.amount;
                for (int i = 0; i < 50 - dish.name.Length; i++)
                {
                    str += "-";
                }
                str +=  dish.amount * dish.price + " NIS";
                str += Environment.NewLine;
                payment += dish.amount * dish.price;
            }
            str += Environment.NewLine;
            dishesTextRange.Text = str;
            dishesTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, 14.0);
            dishesTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            dishesTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Arial");
        }
        private void PrintTotal()
        {
            string str = "Total:                          " + payment.ToString() + " NIS";
            TextRange paymentTextRange = new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            str += Environment.NewLine;
            paymentTextRange.Text = str;
            paymentTextRange.ApplyPropertyValue(TextElement.FontSizeProperty,25.0);
            paymentTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            paymentTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Arial");
        }
        private void PrintEnd()
        {
            string str = Environment.NewLine;
            str += "  Thank You Very Much, Hope You Enjoyed :)";
            TextRange endTextRange = new TextRange(rich_txb_recepit.Document.ContentEnd, rich_txb_recepit.Document.ContentEnd);
            endTextRange.Text = str;
            endTextRange.ApplyPropertyValue(TextElement.FontSizeProperty, 16.5);
            endTextRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            endTextRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Arial");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
