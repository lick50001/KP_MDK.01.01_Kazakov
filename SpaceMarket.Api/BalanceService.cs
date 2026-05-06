using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MarketAO.Services
{
    public class BalanceService : INotifyPropertyChanged
    {
        private static BalanceService _instance;

        // Классическая реализация Singleton для C# 7.3
        public static BalanceService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BalanceService();
                }
                return _instance;
            }
        }

        private long _currentBalance = 5000000;

        public long CurrentBalance
        {
            get { return _currentBalance; }
            set
            {
                _currentBalance = value;
                OnPropertyChanged();
            }
        }

        public bool CanAfford(long amount) => _currentBalance >= amount;

        public void Add(long amount) => CurrentBalance += amount;
        public void Subtract(long amount) => CurrentBalance -= amount;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}