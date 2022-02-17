using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Restaurant_reservation_project
{
    class Orders
    {
        [BsonId]
        public Guid Id { get; set; }
        public Reservation reservation {get;set;}
        public dishes dish { get; set; }
        public int dish_amount { get; set; }

        public Orders(Reservation reservation, dishes dish, int dish_amount)
        {
            this.reservation = reservation;
            this.dish = dish;
            this.dish_amount = dish_amount;
        }
    }
}
