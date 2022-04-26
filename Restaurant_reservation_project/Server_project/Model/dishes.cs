using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace Server_project
{
     class dishes
    {
        [BsonId]
        public Guid Id { get; set; }
        public string name { get; set; }
        public int price { get; set; }
        public string category { get; set; }

        public dishes(string name, int price, string category)
        {
            this.name = name;
            this.price = price;
            this.category = category;
        }

        public override bool Equals(object obj)
        {
            return obj is dishes dishes &&
                   name.Equals(dishes.name) &&
                   category.Equals(dishes.category)&&
                   price == dishes.price;
        }

        public override string ToString()
        {
            return this.name + " " + this.price + " " + this.category;
        }
        public override int GetHashCode()
        {
            int hashCode = -298416015;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + price.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(category);
            return hashCode;
        }
    }
}
