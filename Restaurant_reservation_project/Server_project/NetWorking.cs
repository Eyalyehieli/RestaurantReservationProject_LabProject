using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_project
{
    class NetWorking
    {
        public enum Requestes { GET_RESERVATION, ADD_DISH, GET_ALL_WORKERS,GET_DISHES_BY_CATEGORY, GET_WORKER_OF_RESERVATION,INSERT_RESERVAION,UPDATE_RESERVAION,TERMINATE_RESERVATION, UPSERT_RESERVATION, WAIT_ONE_MUTEX,RELEASE_MUTEX, WAIT_ONE_MUTEX_MANAGER,GET_MUTEX_STATE,IS_OCCUPEID_TABLE,WAIT_ONE_TABLE,RELEASE_TABLE,GET_MANAGER_CODE,CHANGE_MANAGER_CODE,WAIT_ONE_MANAGER_CODE, RELEASE_MANAGER_CODE_MUTEX,WAIT_ONE_WORKERS_CRUD,RELEASE_WORKERS_CRUD_MUTEX, WAIT_ONE_DISHES_CRUD, RELEASE_DISHES_CRUD_MUTEX, DELETE_WORKER,DELETE_DISH,UPDATE_WORKER,INSERT_WORKER,UPDATE_DISH,INSERT_DISH,UPDATE_WORKER_OF_RESERVATION,UPDATE_TABLE_NUMBER_OF_RESERVATION,GET_OCCUPIED_TABLES};
        const int SIZE_PARAMETERS = 2;

        public static void sentBoolOverNetStream(NetworkStream stream,bool flag)
        {
            byte[] buffer = BitConverter.GetBytes(flag);
            stream.Write(buffer, 0, buffer.Length);
        }

        public static bool getBoolOverNetStream(NetworkStream stream)
        {
            byte[] buffer = new byte[sizeof(bool)];
            bool flag;
            stream.Read(buffer, 0, buffer.Length);
            flag = BitConverter.ToBoolean(buffer,0);
            return flag;
        }

        public static void sentIntOverNetStream(NetworkStream stream, int number)
        {
            byte[] buffer = BitConverter.GetBytes(number);//size of integer
            stream.Write(buffer, 0, buffer.Length);
        }
        public static int getIntOverNetStream(NetworkStream stream)
        {
            byte[] buffer = new byte[sizeof(int)];//size of integer
            int number = 0;
            stream.Read(buffer, 0, buffer.Length);
            number = BitConverter.ToInt32(buffer, 0);
            return number;
        }
        public static string getStringOverNetStream(NetworkStream stream)
        {
            byte[] size_buffer = new byte[sizeof(int)];//size of integer
            byte[] string_buffer;
            int stringSize;
            stream.Read(size_buffer, 0, size_buffer.Length);
            stringSize = BitConverter.ToInt32(size_buffer, 0);
            string_buffer = new byte[stringSize];
            stream.Read(string_buffer, 0, string_buffer.Length);
            return Encoding.UTF8.GetString(string_buffer);
        }
        public static void sentStringOverNetStream(NetworkStream stream, string str)
        {
            byte[] buffer = BitConverter.GetBytes(str.Length);//always the size of the array will be 4
            stream.Write(buffer, 0, buffer.Length);
            buffer = Encoding.UTF8.GetBytes(str);
            stream.Write(buffer, 0, buffer.Length);
        }
        public static void SendRequest(NetworkStream stream, Requestes request)
        {
            byte[] request_buffer = new byte[sizeof(NetWorking.Requestes)];
            request_buffer = BitConverter.GetBytes(Convert.ToInt32(request));
            stream.Write(request_buffer, 0, request_buffer.Length);
        }
        
        public static byte[] GetRequest(NetworkStream stream)
        {
            byte[] request_buffer = new byte[sizeof(NetWorking.Requestes)];
            stream.Read(request_buffer, 0, request_buffer.Length);
            return request_buffer;
        }

        public static void sendDateTimeOverNetStream(NetworkStream stream,DateTime dateTime)
        {
            sentIntOverNetStream(stream, dateTime.Year);
            sentIntOverNetStream(stream, dateTime.Month);
            sentIntOverNetStream(stream, dateTime.Day);
            sentIntOverNetStream(stream, dateTime.Hour);
            sentIntOverNetStream(stream, dateTime.Minute);
            sentIntOverNetStream(stream, dateTime.Second);
        }
        public static DateTime getDateTimeOverNetStream(NetworkStream stream)
        {
            int year = getIntOverNetStream(stream);
            int month = getIntOverNetStream(stream);
            int day = getIntOverNetStream(stream);
            int hour = getIntOverNetStream(stream);
            int minute = getIntOverNetStream(stream);
            int second = getIntOverNetStream(stream);
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second);
            return dateTime;
        }
    }
}
