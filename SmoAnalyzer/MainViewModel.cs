using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SmoAnalyzer
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object? p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p) => _execute(p);
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private int _selectedTypeIndex = 0;
        public int SelectedTypeIndex
        {
            get => _selectedTypeIndex;
            set
            {
                _selectedTypeIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsType1));
                OnPropertyChanged(nameof(IsType2));
                OnPropertyChanged(nameof(IsType3));
                OnPropertyChanged(nameof(VisibilityType2));
                OnPropertyChanged(nameof(VisibilityType3));
            }
        }

        public bool IsType1 => SelectedTypeIndex == 0;
        public bool IsType2 => SelectedTypeIndex == 1;
        public bool IsType3 => SelectedTypeIndex == 2;
        public Visibility VisibilityType2 => IsType2 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityType3 => IsType3 ? Visibility.Visible : Visibility.Collapsed;
        private double _lambda = 0;
        public double Lambda { get => _lambda; set { _lambda = value; OnPropertyChanged(); } }
        private int _lambdaUnitIndex = 0;
        public int LambdaUnitIndex
        {
            get => _lambdaUnitIndex;
            set { _lambdaUnitIndex = value; OnPropertyChanged(); }
        }
        private bool _useMu = true;
        private int _muUnitIndex = 0;
        public int MuUnitIndex
        {
            get => _muUnitIndex;
            set { _muUnitIndex = value; OnPropertyChanged(); }
        }
        public bool UseMu
        {
            get => _useMu;
            set
            {
                _useMu = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseT));
                OnPropertyChanged(nameof(VisibilityMuPanel));
                OnPropertyChanged(nameof(VisibilityTPanel));
            }
        }
        public bool UseT => !UseMu;
        public Visibility VisibilityMuPanel => _useMu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityTPanel => _useMu ? Visibility.Collapsed : Visibility.Visible;
        private double _mu = 0;
        public double Mu { get => _mu; set { _mu = value; OnPropertyChanged(); } }
        private double _serviceTime = 0;
        public double ServiceTime { get => _serviceTime; set { _serviceTime = value; OnPropertyChanged(); } }
        private int _timeUnitIndex = 0;
        public int TimeUnitIndex { get => _timeUnitIndex; set { _timeUnitIndex = value; OnPropertyChanged(); } }
        private int _channels = 2;
        public int Channels { get => _channels; set { _channels = value; OnPropertyChanged(); } }
        private int _queueLimit = 3;
        public int QueueLimit { get => _queueLimit; set { _queueLimit = value; OnPropertyChanged(); } }
        public ObservableCollection<KeyValuePair<string, string>> ResultRows { get; } = new();
        private string _resultTitle = "";
        public string ResultTitle { get => _resultTitle; set { _resultTitle = value; OnPropertyChanged(); } }
        private string _errorText = "";
        public string ErrorText
        {
            get => _errorText;
            set
            {
                _errorText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(VisibilityError));
            }
        }
        public bool HasError => !string.IsNullOrEmpty(ErrorText);
        public Visibility VisibilityError => HasError ? Visibility.Visible : Visibility.Collapsed;
        public ICommand CalculateCommand { get; }

        public MainViewModel()
        {
            CalculateCommand = new RelayCommand(_ => Calculate());
        }
        private double GetMu()
        {
            if (UseMu)
            {
                return MuUnitIndex switch
                {
                    0 => Mu,
                    1 => Mu * 60.0,
                    2 => Mu / 24.0,
                    _ => Mu
                };
            }
            double hours = TimeUnitIndex switch
            {
                0 => ServiceTime / 60.0,
                1 => ServiceTime,
                2 => ServiceTime * 24.0,
                _ => ServiceTime
            };
            if (hours <= 0) throw new ArgumentException("Время обслуживания должно быть > 0");
            return 1.0 / hours;
        }
        private void Calculate()
        {
            try
            {
                ErrorText = "";
                ResultRows.Clear();
                ResultTitle = "";

                double lambdaInHour = LambdaUnitIndex switch
                {
                    0 => Lambda,
                    1 => Lambda * 60.0,
                    2 => Lambda / 24.0,
                    _ => Lambda
                };

                double mu = GetMu();

                if (lambdaInHour <= 0 || mu <= 0)
                    throw new ArgumentException("Значения λ и μ (или времени t) должны быть больше нуля");

                SmoResult res;

                switch (SelectedTypeIndex)
                {
                    case 0: res = SmoCalculator.Rejection(lambdaInHour, mu); break;
                    case 1: res = SmoCalculator.InfiniteQueue(lambdaInHour, mu, Channels); break;
                    case 2: res = SmoCalculator.LimitedQueue(lambdaInHour, mu, QueueLimit); break;
                    default: return;
                }

                ResultTitle = res.Title;
                foreach (var row in res.Rows) ResultRows.Add(row);
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}