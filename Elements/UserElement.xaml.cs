using Kazakov_KP_01._01.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kazakov_KP_01._01.Elements
{
    public partial class UserElement : UserControl
    {
        private Users _user;
        public event Action<Users> OnBanToggle;
        public event Action<Users> OnDelete;

        public UserElement(Users user)
        {
            InitializeComponent();
            _user = user;
            Render();
        }

        private void Render()
        {
            TxtId.Text = _user.UserId.ToString();
            TxtName.Text = _user.UserName;
            TxtAvatar.Text = _user.UserName.Length > 0
                ? _user.UserName[0].ToString().ToUpper() : "?";

            switch (_user.Level?.ToLower())
            {
                case "admin":
                    RoleBadge.Background = new SolidColorBrush(Color.FromArgb(40, 0, 255, 194));
                    TxtRole.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                    TxtRole.Text = "Admin";
                    break;
                case "moderator":
                    RoleBadge.Background = new SolidColorBrush(Color.FromArgb(40, 255, 170, 0));
                    TxtRole.Foreground = new SolidColorBrush(Color.FromRgb(255, 170, 0));
                    TxtRole.Text = "User";
                    break;
                default:
                    RoleBadge.Background = new SolidColorBrush(Color.FromArgb(40, 110, 116, 133));
                    TxtRole.Foreground = new SolidColorBrush(Color.FromRgb(110, 116, 133));
                    TxtRole.Text = "User";
                    break;
            }

            if (_user.IsBanned)
            {
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                TxtStatus.Text = "Заблокирован";
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 77, 77));
                BtnBan.Content = "✅";
                BtnBan.ToolTip = "Разблокировать";
            }
            else
            {
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                TxtStatus.Text = "Активен";
                TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 194));
                BtnBan.Content = "⛔";
                BtnBan.ToolTip = "Заблокировать";
            }
        }

        private void BtnBan_Click(object sender, RoutedEventArgs e)
            => OnBanToggle?.Invoke(_user);

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
            => OnDelete?.Invoke(_user);
    }
}