using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant_reservation_project
{
    class showReservation
    {
        public int table_number { get; set; }
        public  int price { get; set; }
        public  string worker { get; set; }
        public  DateTime dateTime { get; set; }
        public showReservation(int table_number,int price,string worker,DateTime dateTime)
        {
            this.table_number = table_number;
            this.price = price;
            this.worker = worker;
            this.dateTime = dateTime;
        }
        public showReservation(int table_number, int price, string worker)
        {
            this.table_number = table_number;
            this.price = price;
            this.worker = worker;
        }
    }
}
