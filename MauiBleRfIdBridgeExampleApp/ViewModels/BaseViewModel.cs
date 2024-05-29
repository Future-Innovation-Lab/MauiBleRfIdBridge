﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiBleRfIdBridgeExampleApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public virtual void OnAppearing()
        {

        }

    }
}
