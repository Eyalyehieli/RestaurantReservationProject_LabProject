using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_project
{
    class dishOfReservation
    {
        public string name { get; set; }
        public int price { get; set; }
        public string category { get; set; }
        public int amount { get; set; }

        public dishOfReservation(string name, int price, string category, int amount)
        {
            this.name = name;
            this.price = price;
            this.category = category;
            this.amount = amount;
        }

        public override string ToString()
        {
            return name + " " + price + " " + category + " " + amount;
        }

        public override bool Equals(object obj)
        {
            return obj is dishOfReservation reservation &&
                   name.Equals(reservation.name) &&
                   price == reservation.price &&
                   category.Equals(reservation.category) &&
                   amount == reservation.amount;
        }
    }
}
