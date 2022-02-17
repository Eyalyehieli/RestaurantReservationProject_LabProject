using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Server_project
{
    class TableMutex
    {
       public static int NONE_TABLE = -1;
        Semaphore mutex;
        int clientNumber;

        public TableMutex(Semaphore mutex)
        {
            this.mutex = mutex;
            this.clientNumber = NONE_TABLE;
        }

        public void setMutex(Semaphore mutex)
        {
            this.mutex = mutex;
        }
        public void setClientNumber(int clientNumber)
        {
            this.clientNumber = clientNumber;
        }
        public Semaphore getMutex()
        {
            return this.mutex;
        }
        public int getClientNumber()
        {
            return this.clientNumber;
        }

    }
}
