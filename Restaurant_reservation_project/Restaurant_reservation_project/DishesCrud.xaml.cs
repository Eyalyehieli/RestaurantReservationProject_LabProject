using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
using System.Threading;

namespace Restaurant_reservation_project
{
    /// <summary>
    /// Interaction logic for DishesCrud.xaml
    /// </summary>
    public partial class DishesCrud : Window
    {
        NetworkStream stream;
        enum DB_EVENT_DISH { EDIT_DISH, INSERT_DISH }
        DB_EVENT_DISH dishDBEvent;
        dishes prevDish;
        public DishesCrud(NetworkStream stream)
        {
            InitializeComponent();
            this.stream = stream;
            hideProperControls();
            create_data_grid_columns(dishes_data_grid);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string dish_string;
            string[] seperated_dish;
            dishes dish;
            dishes_data_grid.Items.Clear();
            Button categoryButton = sender as Button;
            NetWorking.SendRequest(stream, NetWorking.Requestes.GET_DISHES_BY_CATEGORY);
            switch (categoryButton.Content)
            {
                case "main-dishes": NetWorking.sentStringOverNetStream(stream, "main-dishes"); break;
                case "firsts": NetWorking.sentStringOverNetStream(stream, "firsts"); break;
                case "deserts": NetWorking.sentStringOverNetStream(stream, "deserts"); break;
                case "drinks": NetWorking.sentStringOverNetStream(stream, "drinks"); break;
            }
            do
            {
                dish_string = NetWorking.getStringOverNetStream(stream);
                if (dish_string != "empty")
                {
                    seperated_dish = dish_string.Split(' ');
                    dish = new dishes(seperated_dish[0], Convert.ToInt32(seperated_dish[1]), seperated_dish[2]);
                    dishes_data_grid.Items.Add(dish);
                }
                Thread.Sleep(50);
            }
            while (stream.DataAvailable);
        }
        private void create_data_grid_columns(DataGrid dataGrid)
        {
            DataGridTextColumn col1 = new DataGridTextColumn();
            DataGridTextColumn col2 = new DataGridTextColumn();
            DataGridTextColumn col3 = new DataGridTextColumn();
            dataGrid.Columns.Add(col1);
            dataGrid.Columns.Add(col2);
            dataGrid.Columns.Add(col3);
            col1.Binding = new Binding("name");
            col2.Binding = new Binding("price");
            col3.Binding = new Binding("category");
            col1.Header = "name";
            col2.Header = "price";
            col3.Header = "category";
        }

        private void add_btn_Click(object sender, RoutedEventArgs e)
        {
            showProperControls();
            dishDBEvent = DB_EVENT_DISH.INSERT_DISH;
        }

        private void edit_btn_Click(object sender, RoutedEventArgs e)
        {
            showProperControls();
            if (dishes_data_grid != null && dishes_data_grid.SelectedItems != null && dishes_data_grid.SelectedItems.Count == 1)
            {
                prevDish = (dishes)dishes_data_grid.SelectedItem;
                category_txb.Text = prevDish.category;
                name_txb.Text = prevDish.name;
                price_txb.Text = prevDish.price.ToString();
            }
            dishDBEvent = DB_EVENT_DISH.EDIT_DISH;
        }

        private void done_btn_Click(object sender, RoutedEventArgs e)
        {
            //get the prev properties from datagrid
            string newCategory = category_txb.Text;
            int newPrice = Convert.ToInt32(price_txb.Text);
            string newName = name_txb.Text;
            if (dishDBEvent == DB_EVENT_DISH.INSERT_DISH)
            {
                NetWorking.SendRequest(stream, NetWorking.Requestes.INSERT_DISH);
                NetWorking.sentStringOverNetStream(stream, newName);
                NetWorking.sentIntOverNetStream(stream, newPrice);
                NetWorking.sentStringOverNetStream(stream, newCategory);
            }
            else
            {
                NetWorking.SendRequest(stream, NetWorking.Requestes.UPDATE_DISH);
                NetWorking.sentStringOverNetStream(stream, prevDish.name);
                NetWorking.sentIntOverNetStream(stream, prevDish.price);
                NetWorking.sentStringOverNetStream(stream, prevDish.category);
                NetWorking.sentStringOverNetStream(stream, newName);
                NetWorking.sentIntOverNetStream(stream, newPrice);
                NetWorking.sentStringOverNetStream(stream, newCategory);
            }
            hideProperControls();
            dishes_data_grid.Items.Clear();
        }

        private void delete_btn_Click(object sender, RoutedEventArgs e)
        {
            dishes dish = null;
            if (dishes_data_grid != null && dishes_data_grid.SelectedItems != null && dishes_data_grid.SelectedItems.Count == 1)
            {
                dish = (dishes)dishes_data_grid.SelectedItem;
                name_txb.Text = dish.name;
                price_txb.Text = dish.price.ToString();
                category_txb.Text = dish.category;
            }
            NetWorking.SendRequest(stream, NetWorking.Requestes.DELETE_DISH);
            NetWorking.sentStringOverNetStream(stream, dish.name);
            NetWorking.sentIntOverNetStream(stream, dish.price);
            NetWorking.sentStringOverNetStream(stream, dish.category);
            hideProperControls();
            dishes_data_grid.Items.Clear();
        }
        public void showProperControls()
        {
            add_btn.Visibility = Visibility.Hidden;
            edit_btn.Visibility = Visibility.Hidden;
            delete_btn.Visibility = Visibility.Hidden;
            dishes_data_grid.Visibility = Visibility.Hidden;
            category_lbl.Visibility = Visibility.Visible;
            price_lbl.Visibility = Visibility.Visible;
            name_lbl.Visibility = Visibility.Visible;
            category_txb.Visibility = Visibility.Visible;
            price_txb.Visibility = Visibility.Visible;
            name_txb.Visibility = Visibility.Visible;
            done_btn.Visibility = Visibility.Visible;
        }
        public void hideProperControls()
        {
            add_btn.Visibility = Visibility.Visible;
            edit_btn.Visibility = Visibility.Visible;
            delete_btn.Visibility = Visibility.Visible;
            dishes_data_grid.Visibility = Visibility.Visible;
            category_lbl.Visibility = Visibility.Hidden;
            price_lbl.Visibility = Visibility.Hidden;
            name_lbl.Visibility = Visibility.Hidden;
            category_txb.Visibility = Visibility.Hidden;
            price_txb.Visibility = Visibility.Hidden;
            name_txb.Visibility = Visibility.Hidden;
            done_btn.Visibility = Visibility.Hidden;
        }
    }
}
