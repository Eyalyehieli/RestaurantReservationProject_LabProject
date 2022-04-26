using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Restaurant_reservation_project
{
    public class Reservation
    {
        [BsonId]
        public Guid Id { get; set; }
        public int table_number { get; set; }
        public Worker worker { get; set; }
        public List<dishOfReservation> allDishes { get; set; }
        public bool isFinished { get; set; }
        public DateTime dateTime { get; set; }

        public Reservation(int table_number, Worker worker, bool isFinished, List<dishOfReservation> allDishes)
        {
            this.table_number = table_number;
            this.worker = worker;
            this.isFinished = isFinished;
            this.allDishes = allDishes;
        }
        public Reservation(int table_number, Worker worker, bool isFinished, List<dishOfReservation> allDishes, DateTime dateTime)
        {
            this.table_number = table_number;
            this.worker = worker;
            this.isFinished = isFinished;
            this.allDishes = allDishes;
            this.dateTime = dateTime;
        }
    }
}
