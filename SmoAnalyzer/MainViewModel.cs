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
    public enum SmoType
    {
        SingleRejection,
        SingleLimited,
        SingleInfinite,
        MultiRejection,
        MultiLimited,
        MultiInfinite
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private SmoType _selectedType = SmoType.SingleRejection;
        public SmoType SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType == value) return;
                _selectedType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSingleChannel));
                OnPropertyChanged(nameof(IsMultiChannel));
                OnPropertyChanged(nameof(HasFiniteQueue));
                OnPropertyChanged(nameof(VisibilityChannels));
                OnPropertyChanged(nameof(VisibilityQueueLimit));
                ClearResult();
                ErrorText = "";
            }
        }
        public bool IsSingleChannel => _selectedType is SmoType.SingleRejection or SmoType.SingleLimited or SmoType.SingleInfinite;
        public bool IsMultiChannel => !IsSingleChannel;
        public bool HasFiniteQueue => _selectedType is SmoType.SingleLimited or SmoType.MultiLimited;
        public Visibility VisibilityChannels => IsMultiChannel ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityQueueLimit => HasFiniteQueue ? Visibility.Visible : Visibility.Collapsed;
        private double _lambda;
        public double Lambda
        {
            get => _lambda;
            set { _lambda = value; OnPropertyChanged(); }
        }
        private int _lambdaUnitIndex;
        public int LambdaUnitIndex
        {
            get => _lambdaUnitIndex;
            set { _lambdaUnitIndex = value; OnPropertyChanged(); }
        }
        private bool _useMu = true;
        public bool UseMu
        {
            get => _useMu;
            set
            {
                if (_useMu == value) return;
                _useMu = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UseT));
                OnPropertyChanged(nameof(VisibilityMuPanel));
                OnPropertyChanged(nameof(VisibilityTPanel));
            }
        }
        public bool UseT
        {
            get => !_useMu;
            set { if (value) UseMu = false; }
        }
        public Visibility VisibilityMuPanel => _useMu ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilityTPanel => _useMu ? Visibility.Collapsed : Visibility.Visible;
        private double _mu;
        public double Mu
        {
            get => _mu;
            set { _mu = value; OnPropertyChanged(); }
        }
        private int _muUnitIndex;
        public int MuUnitIndex
        {
            get => _muUnitIndex;
            set { _muUnitIndex = value; OnPropertyChanged(); }
        }
        private double _serviceTime;
        public double ServiceTime
        {
            get => _serviceTime;
            set { _serviceTime = value; OnPropertyChanged(); }
        }
        private int _timeUnitIndex;
        public int TimeUnitIndex
        {
            get => _timeUnitIndex;
            set { _timeUnitIndex = value; OnPropertyChanged(); }
        }
        private int _channels = 2;
        public int Channels
        {
            get => _channels;
            set { _channels = value; OnPropertyChanged(); }
        }
        private int _queueLimit = 3;
        public int QueueLimit
        {
            get => _queueLimit;
            set { _queueLimit = value; OnPropertyChanged(); }
        }
        public ObservableCollection<KeyValuePair<string, string>> ResultRows { get; } = new();
        private string _resultTitle = "";
        public string ResultTitle
        {
            get => _resultTitle;
            set { _resultTitle = value; OnPropertyChanged(); }
        }
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
        public ICommand ClearCommand { get; }
        public MainViewModel()
        {
            CalculateCommand = new RelayCommand(_ => Calculate());
            ClearCommand = new RelayCommand(_ => ClearAll());
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
            if (hours <= 0)
                throw new ArgumentException("Время обслуживания должно быть > 0");
            return 1.0 / hours;
        }
        private void Calculate()
        {
            try
            {
                ErrorText = "";
                ClearResult();

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

                if (IsMultiChannel && Channels < 1)
                    throw new ArgumentException("Число каналов c должно быть ≥ 1");

                if (HasFiniteQueue && QueueLimit < 0)
                    throw new ArgumentException("Длина очереди m должна быть ≥ 0");

                SmoResult res = SelectedType switch
                {
                    SmoType.SingleRejection => SmoCalculator.Rejection(lambdaInHour, mu),
                    SmoType.SingleLimited => SmoCalculator.LimitedQueue(lambdaInHour, mu, QueueLimit),
                    SmoType.SingleInfinite => SmoCalculator.SingleInfiniteQueue(lambdaInHour, mu),
                    SmoType.MultiRejection => SmoCalculator.MultiRejection(lambdaInHour, mu, Channels),
                    SmoType.MultiLimited => SmoCalculator.MultiLimitedQueue(lambdaInHour, mu, Channels, QueueLimit),
                    SmoType.MultiInfinite => SmoCalculator.InfiniteQueue(lambdaInHour, mu, Channels),
                    _ => throw new InvalidOperationException("Неизвестный тип СМО")
                };

                ResultTitle = res.Title;
                foreach (var row in res.Rows)
                    ResultRows.Add(row);
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }
        private void ClearResult()
        {
            ResultTitle = "";
            ResultRows.Clear();
        }
        private void ClearAll()
        {
            Lambda = 0;
            LambdaUnitIndex = 0;
            UseMu = true;
            Mu = 0;
            MuUnitIndex = 0;
            ServiceTime = 0;
            TimeUnitIndex = 0;
            Channels = 0;
            QueueLimit = 0;
            ClearResult();
            ErrorText = "";
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}