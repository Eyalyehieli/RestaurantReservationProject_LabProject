using System;
using System.Collections.Generic;
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
using System.Net.Sockets;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for ManangerCode.xaml
    /// </summary>
    public partial class ManangerCode : Window
    {
        string correctPassword;
        char[] password;
        const int PASSWORD_LENGTH = 4;
        const int ENTER_KEY= 10;
        const int DELETE_KEY = 127;
        int passwordIndex = 0;
        int type;
        NetworkStream streamer;
        int table_number;
        public ManangerCode(int type,NetworkStream stream,int table_number)
        {
            InitializeComponent();
            password = new char[PASSWORD_LENGTH];
            this.type = type;
            this.streamer = stream;
            this.table_number = table_number;
            NetWorking.SendRequest(streamer, NetWorking.Requestes.GET_MANAGER_CODE);
            correctPassword = NetWorking.getStringOverNetStream(streamer);
        }
        public ManangerCode(int type,NetworkStream stream)
        {
            InitializeComponent();
            password = new char[PASSWORD_LENGTH];
            this.type = type;
            this.streamer = stream;
            NetWorking.SendRequest(streamer, NetWorking.Requestes.GET_MANAGER_CODE);
            correctPassword = NetWorking.getStringOverNetStream(streamer);//the error line
        }

  
        private void IncPasswordIndex()
        {
            if (passwordIndex<PASSWORD_LENGTH)
            {
                passwordIndex++;
            }
            else
            {
                MessageBox.Show("Too many numbers");
            }
        }
        private void DecPasswordIndex()
        {
            if(passwordIndex>0)
            {
                passwordIndex--;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool isPassCorrect;
            Button b = sender as Button;

            if (b.Content.Equals("Del"))
            {
                DecPasswordIndex();
                printPassword();
            }
            else if (b.Content.Equals("Enter"))
            {
                if (this.type == tableReservation.GET_MUTEX || this.type == tableReservation.GET_ACCESS)
                {
                    isPassCorrect = true;
                    for (int i = 0; i < PASSWORD_LENGTH; i++)
                    {
                        if (password[i] != correctPassword[i])
                        {
                            MessageBox.Show("Incorrect password");
                            isPassCorrect = false;
                            break;
                        }
                    }
                    if (isPassCorrect)
                    {
                        switch (this.type)
                        {
                            case tableReservation.GET_MUTEX: getMutex(); break;
                            case tableReservation.GET_ACCESS: getAccess(); break;
                        }
                    }
                }
                else if (this.type == settings.CHANGE_PASSWORD)
                {
                    ChangeManagerPassword();
                }

            }
            else
            {
                IncPasswordIndex();
                password[passwordIndex-1] = Convert.ToChar(b.Content);
                for(int i=0;i<passwordIndex;i++)
                {
                    printPassword();
                }
            }
        }

        private void ChangeManagerPassword()
        {
            NetWorking.SendRequest(streamer, NetWorking.Requestes.CHANGE_MANAGER_CODE);
            NetWorking.sentStringOverNetStream(streamer, new string(password));
            this.Close();
        }

        private void getAccess()
        {
            this.Close();
        }

        private void getMutex()
        {
            bool mutex_status;
            NetWorking.SendRequest(streamer, NetWorking.Requestes.WAIT_ONE_MUTEX_MANAGER);
            NetWorking.sentIntOverNetStream(streamer, table_number);
            mutex_status=NetWorking.getBoolOverNetStream(streamer);
            if (mutex_status)
            {
                this.Close();
            }
        }

        private void printPassword()
        {
            password_lbl.Content = "";
            for (int i=0;i<passwordIndex;i++)
            {
                password_lbl.Content += "*";
            }
        }
    }
}