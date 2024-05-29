using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiBleRfIdBridgeExampleApp.Models
{
    public class RfIdTagDetection : INotifyPropertyChanged
    {
        private string _tag;

        public string Tag
        {
            get { return _tag; }
            set { _tag = value;
                OnPropertyChanged();
            }
        }

        private int _count;

        public int Count
        {
            get { return _count; }
            set { _count = value;

                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }   
}
