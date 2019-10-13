using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace BotWpfClient.ViewModels
{
    public abstract class BaseViewModel<TViewModel> : INotifyPropertyChanged
        where TViewModel: BaseViewModel<TViewModel>
    {
        protected void Notify([CallerMemberName]string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        protected void SetProperty<T>(ref T text, T value, [CallerMemberName] string propertyName = null)
        {
            text = value;
            Notify(propertyName);
        }

        public void Dispatch(Action<TViewModel> a)
        {
            Application.Current?.Dispatcher?.Invoke(a, this);
        }
    }
}
