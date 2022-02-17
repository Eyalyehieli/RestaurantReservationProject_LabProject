using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_project
{
    class Worker
    {
        
        [BsonId]
        public Guid Id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string accessPriority { get; set; }

        public Worker(string first_name, string last_name,string accessPriority)
        {
            this.first_name = first_name;
            this.last_name = last_name;
            this.accessPriority = accessPriority;
        }

        public override bool Equals(object obj)
        {
            return obj is Worker worker &&
                   first_name.Equals(worker.first_name) &&
                   last_name.Equals(worker.last_name)&&accessPriority.Equals(worker.accessPriority);
        }

        public override string ToString()
        {
            return this.first_name+" "+this.last_name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1796747323;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(first_name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(last_name);
            hashCode = hashCode * -1521134295 + accessPriority.GetHashCode();
            return hashCode;
        }
    }
}
