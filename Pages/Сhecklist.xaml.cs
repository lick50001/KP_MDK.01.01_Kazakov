using Kazakov_KP_01._01.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kazakov_KP_01._01.Pages
{
    /// <summary>
    /// Логика взаимодействия для Сhecklist.xaml
    /// </summary>
    public partial class Сhecklist : Page
    {
        public Сhecklist()
        {
            InitializeComponent();
            this.Loaded += async (s, e) => await LoadItem();
        }

        public async Task LoadItem()
        {
            ApiService _api = new ApiService();
            var item = await _api.GetItemAsync();

            ItemsContainer.Children.Clear();
            foreach (var itm in item)
            {
                var itmRow = new Kazakov_KP_01._01.Elements.Item();
                itmRow.SetData(itm.Item_Id, itm.ItemName, itm.MaxBuyPrice.ToString());

                itmRow.OnDeleteRequested += async (id) =>
                {
                    var result = MessageBox.Show("Удалить этот предмет?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        await _api.DeleteItemAsync(id);
                        await LoadItem();
                    }
                };

                itmRow.OnEditRequested += (id) =>
                {
                    MessageBox.Show($"Редактируем предмет с ID: {id}");
                };

                ItemsContainer.Children.Add(itmRow); 
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
