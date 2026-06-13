using Kazakov_KP_01._01.Services;
using Kazakov_KP_01._01.Models;
using Kazakov_KP_01._01.Elements;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Kazakov_KP_01._01.Pages
{
    public partial class AdminPage : Page
    {
        private ApiService _api = new ApiService();
        private List<Users> _allUsers = new List<Users>();

        public AdminPage()
        {
            InitializeComponent();
        }

        private async void AdminPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        public async Task LoadUsers()
        {
            _allUsers = await _api.GetAllUsersAsync() ?? new List<Users>();
            UpdateStats();
            RenderUsers(_allUsers);
        }

        private void UpdateStats()
        {
            txTotalUsers.Text = _allUsers.Count.ToString();
            txActiveUsers.Text = _allUsers.Count(u => !u.IsBanned).ToString();
            txBannedUsers.Text = _allUsers.Count(u => u.IsBanned).ToString();
        }

        private void RenderUsers(List<Users> users)
        {
            UsersContainer.Children.Clear();
            foreach (var user in users)
            {
                var el = new UserElement(user);
                el.OnBanToggle += async (u) =>
                {
                    await _api.ToggleBanAsync(u.UserId, !u.IsBanned);
                    await LoadUsers();
                };
                el.OnDelete += async (u) =>
                {
                    var res = MessageBox.Show($"Удалить пользователя {u.UserName}?",
                        "Подтверждение", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        await _api.DeleteUserAsync(u.UserId);
                        await LoadUsers();
                    }
                };
                UsersContainer.Children.Add(el);
            }
        }

        private void txSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = txSearch.Text.ToLower();
            txSearchHint.Visibility = string.IsNullOrEmpty(query)
                ? Visibility.Visible : Visibility.Collapsed;

            var filtered = string.IsNullOrEmpty(query)
                ? _allUsers
                : _allUsers.Where(u => u.UserName.ToLower().Contains(query)).ToList();

            RenderUsers(filtered);
        }

        private void txSearch_GotFocus(object sender, RoutedEventArgs e)
            => txSearchHint.Visibility = Visibility.Collapsed;

        private void txSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txSearch.Text))
                txSearchHint.Visibility = Visibility.Visible;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddUserWindow();
            if (win.ShowDialog() == true)
                _ = LoadUsers();
        }
    }
}