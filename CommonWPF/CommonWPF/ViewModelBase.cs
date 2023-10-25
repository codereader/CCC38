using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CommonWPF;

public class ViewModelBase : INotifyPropertyChanged
{
    private Dictionary<string, object> _values = new();

    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected T GetValue<T>([CallerMemberName]string propertyName = null)
    {
        if (_values.TryGetValue(propertyName, out var value))
        {
            return (T)value;
        }
        return default(T);
    }

    protected void SetValue<T>(T value, [CallerMemberName]string propertyName = null)
    {
        SetValue(value, () => { }, propertyName);
    }
    protected void SetValue<T>(T value, Action changedCallback, [CallerMemberName] string propertyName = null)
    {
        if (!_values.TryGetValue(propertyName, out var oldValue) || !Equals(oldValue, value))
        {
            _values[propertyName] = value;
            RaisePropertyChanged(propertyName);
            changedCallback();
        }
    }

}
