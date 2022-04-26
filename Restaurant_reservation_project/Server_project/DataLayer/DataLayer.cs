using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson;

namespace Server_project
{
    /// <summary>
    /// //////////////////////////////////////// adding lock on the whole functions
    /// 
    /// </summary>
    class DataLayer
    {
        private static DataLayer db = null;
        private static readonly object locker = new object();
        IMongoDatabase database=null;
        public DataLayer(string db_name)
        {
            var client = new MongoClient();
            database = client.GetDatabase(db_name);
           // InitializeDB();
        }
        public static DataLayer GetInstance()
        { 
            lock (locker)
            {
                if (db == null)//singletone
                {
                    db = new DataLayer("Reservation_Lab_Project");
                }
                return db;
            }
        }
        
        public void InitializeDB()
        {
            List<Worker> workers = new List<Worker>();
            workers.Add(new Worker("Eyal", "Yehieli","Manager"));
            workers.Add(new Worker("Ronie", "Shir","Waiter"));
            workers.Add(new Worker("Lidor", "Elmakayes","waiter"));
            workers.Add(new Worker("Reut", "Maman","waiter"));
            InsertListOfRecords("workers", workers);
            List<dishes> dishes = new List<dishes>();
            dishes.Add(new dishes("cigar", 45,"firsts"));
            dishes.Add(new dishes("mushrooms", 40,"firsts"));
            dishes.Add(new dishes("meat-bruschettas", 60,"firsts"));
            dishes.Add(new dishes("steak", 90,"main-dishes"));
            dishes.Add(new dishes("hamburger", 80, "main-dishes"));
            dishes.Add(new dishes("fish&chips", 90, "main-dishes"));
            dishes.Add(new dishes("ice-cream", 20,"deserts"));
            dishes.Add(new dishes("chocolate-cake", 40, "deserts"));
            dishes.Add(new dishes("cheese-cake", 45, "deserts"));
            dishes.Add(new dishes("coke", 12,"drinks"));
            dishes.Add(new dishes("water", 10, "drinks"));
            dishes.Add(new dishes("beer", 15, "drinks"));
            InsertListOfRecords("dishes", dishes);
        }
        
        public  void InsertRecord<T>(string table,T record)
        {
            var collection = database.GetCollection<T>(table);
            collection.InsertOne(record);
        }

        public void InsertListOfRecords<T>(string table,List<T> list)
        {
            List<T> existRecords = LoadRecords<T>(table);
            bool contains;
            foreach (T rec in list)
            {
                contains=existRecords.Any(item => item.Equals(rec));
                if(!contains)
                {
                    InsertRecord(table, rec);
                    existRecords = LoadRecords<T>(table);
                }
            }
        }

        public void DeleteRecord<T>(string table, Guid id)
        {
            var collection = database.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("_id", id);
            collection.DeleteOne(filter);
        }

        public List<T> LoadRecords<T>(string table)
        {
            var collection = database.GetCollection<T>(table);
            return collection.Find(new BsonDocument()).ToList();
        }

        /*public T LoadRecordById<T>(string table ,Guid id)
        {
            var collection = database.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            return collection.Find(filter).First();
        }*/


        public void UpdateRecord<T>(string table,Guid id,T record)
        {
            DeleteRecord<T>(table, id);
            InsertRecord<T>(table, record);
        }

        public  List<dishOfReservation> get_reservationByTableNumber(int table_number,bool isFinished)
        {
            List<Reservation> reservations = LoadRecords<Reservation>("reservation");
            foreach(Reservation res in reservations)
            {
                if(res.table_number==table_number&&res.isFinished==isFinished)
                {
                    if (res.allDishes.Count == 0) { return null; }
                    return res.allDishes;
                }
            }
            return null;
        }
        public  List<Worker> get_all_workers()
        {
           return LoadRecords<Worker>("workers");
        }

        public string get_worker_name_by_reservation(int table_number,bool isFinished)
        {
            List<Reservation> reservations = LoadRecords<Reservation>("reservation");
            foreach (Reservation res in reservations)
            {
                if (res.table_number == table_number && res.isFinished == isFinished)
                {
                    return res.worker.first_name + " " + res.worker.last_name;
                }
            }
            return null;
        }
        public List<dishes> get_all_dishes_by_category(string category)
        {   
            List<dishes> allDishes = LoadRecords<dishes>("dishes");
            List<dishes> dishesByCategory=new List<dishes>();
            foreach(dishes dish in allDishes)
            {
                if(dish.category.Equals(category))
                {
                    dishesByCategory.Add(dish);
                }
            }
            return dishesByCategory;
        }

       
        public string GetPriorityByWorkerName(string workerName)
        {
            List<Worker> workers = LoadRecords<Worker>("workers");
            string[] workerNameSplited = workerName.Split(' ');
            foreach(Worker w in workers)
            {
                if(w.first_name.Equals(workerNameSplited[0])&&w.last_name.Equals(workerNameSplited[1]))
                {
                    return w.accessPriority;
                }
            }
            return null;
        }
        
        public Guid GetIdByReservation(int table_number)
        {
            List<Reservation> collection = LoadRecords<Reservation>("reservation");
            foreach(Reservation res in collection)
            {
                if(res.table_number==table_number&&res.isFinished==false)
                {
                    return res.Id;
                }
            }
            return Guid.Empty;
        }

        public Guid GetIdByDish(dishes dish)
        {
            List<dishes> collection = LoadRecords<dishes>("dishes");
            foreach(dishes d in collection)
            {
                if(d.category.Equals(dish.category)&&d.name.Equals(dish.name))
                {
                    return d.Id;
                }
            }
            return Guid.Empty;
        }
        public Guid GetIdByWorker(Worker worker)
        {
            List<Worker> collection = LoadRecords<Worker>("workers");
            foreach(Worker w in collection)
            {
                if(w.first_name.Equals(worker.first_name)&&w.last_name.Equals(worker.last_name)&&w.accessPriority.Equals(worker.accessPriority))
                {
                    return w.Id;
                }
            }
            return Guid.Empty;
        }

        public void DeleteDish(dishes dish)
        {
            Guid idDish = GetIdByDish(dish);
            DeleteRecord<dishes>("dishes", idDish);
        }
        public void DeleteWorker(Worker worker)
        {
            Guid workerId = GetIdByWorker(worker);
            DeleteRecord<Reservation>("workers", workerId);
        }

        public void upsert_reservation(int table_number, string worker_name, bool is_finished, List<dishOfReservation> dishes)
        {
            Guid idReservation = GetIdByReservation(table_number);//if its for update or insert
            string[] workerName = worker_name.Split(' ');
            Worker worker = new Worker(workerName[0], workerName[1], GetPriorityByWorkerName(worker_name));
            DateTime date;
            Reservation res;
            if (is_finished == true)
            {
                date = DateTime.Now;
                res = new Reservation(table_number, worker, is_finished, dishes, date);
            }
            else
            {
                res = new Reservation(table_number, worker, is_finished, dishes);
            }
            if (idReservation != Guid.Empty)
            {
                DeleteRecord<Reservation>("reservation", idReservation);
            }
            InsertRecord<Reservation>("reservation", res);
        }

        public void InsertWorker(Worker worker)
        {
            InsertRecord<Worker>("workers", worker);
        }

        public void UpdateWorker(Worker prevWorker,Worker newWorker)
        {
            Guid prevWorkerId = GetIdByWorker(prevWorker);
            UpdateRecord<Worker>("workers", prevWorkerId, newWorker);
        }

        public void InsertDish(dishes dish)
        {
            InsertRecord<dishes>("dishes", dish);
        }

        public void UpdateDish(dishes prevDish , dishes newDish)
        {
            Guid prevDishId = GetIdByDish(prevDish);
            UpdateRecord<dishes>("dishes", prevDishId, newDish);
        }

        public void UpdateWorkerByReservation(int tableNumber,string workerName)
        {
            var collection = database.GetCollection<Reservation>("reservation");
            Guid reservationId = GetIdByReservation(tableNumber);
            string[] splitedWorkerName = workerName.Split(' ');
            Worker w = new Worker(splitedWorkerName[0], splitedWorkerName[1], GetPriorityByWorkerName(workerName));
            var filter = Builders<Reservation>.Filter.Eq("_id", reservationId);
            var updateDefinition = Builders<Reservation>.Update.Set<Worker>(p => p.worker,w);
            collection.FindOneAndUpdate(filter, updateDefinition);
        }
       public void UpdateTableNumberByReservation(int prevTableNumber,int newTableNumber)
        {
            var collection = database.GetCollection<Reservation>("reservation");
            Guid reservatinoId = GetIdByReservation(prevTableNumber);
            var filter = Builders<Reservation>.Filter.Eq("_id", reservatinoId);
            var updateDefinition = Builders<Reservation>.Update.Set<int>(p => p.table_number, newTableNumber);
            collection.FindOneAndUpdate(filter, updateDefinition);
        }

        public IEnumerable<Reservation> GetOcuppiedTables()
        {
            List<Reservation> reservations = LoadRecords<Reservation>("reservation");
            foreach(Reservation res in reservations)
            {
                if(res.isFinished==false)
                {
                    yield return res;
                }
            }
        }
        public List<Reservation> getOpenReservation()
        {
           return LoadRecords<Reservation>("reservation").FindAll(x => x.isFinished == false);
        }
        public List<Reservation> getClosedReservation()
        {
            return LoadRecords<Reservation>("reservation").FindAll(x => x.isFinished == true &&isToday(x.dateTime));
        }

        public bool isToday(DateTime day)
        {
            return day.Day == DateTime.Now.Day && day.Month == DateTime.Now.Month && day.Year == DateTime.Now.Year;
        }
    }
}
