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

namespace Kazakov_KP_01._01.Elements
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        public event Action<int> OnDeleteRequested;
        public event Action<int> OnEditRequested;

        private int _itemId;

        public Item()
        {
            InitializeComponent();
            this.MouseEnter += (s, e) => MainBorder.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
            this.MouseLeave += (s, e) => MainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252833"));
        }

        public void SetData(int id, string name, string price)
        {
            _itemId = id;
            TxtName.Text = name ?? "--:--:--";
            TxtPrice.Text = price ?? "(нет цены)";
        }

        private void EditItm(object sender, RoutedEventArgs e)
        {
            OnEditRequested?.Invoke(_itemId);
        }

        private void DeleteItm(object sender, RoutedEventArgs e)
        {
            OnDeleteRequested?.Invoke(_itemId);
        }
    }
}
