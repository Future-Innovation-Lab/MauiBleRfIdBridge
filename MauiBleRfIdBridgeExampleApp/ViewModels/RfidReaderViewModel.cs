using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MauiBleRfIdBridgeExampleApp.Messages;
using MauiBleRfIdBridgeExampleApp.Models;
using MauiBleRfIdBridgeExampleApp.Services.Interfaces;
using MauiBleRfIdBridgeExampleApp.Views.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiBleRfIdBridgeExampleApp.ViewModels
{
    public class RfidReaderViewModel : BaseViewModel
    {
        private IRfidSensorBle _rfidBleService;

        private string _deviceName;

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                _deviceName = value;
                OnPropertyChanged();
            }
        }

        private string _lastestRfIdReading;

        public string LatestRfIdReading
        {
            get { return _lastestRfIdReading; }
            set
            {
                _lastestRfIdReading = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RfIdTagDetection> _tagList;

        public ObservableCollection<RfIdTagDetection> TagList
        {
            get { return _tagList; }
            set { _tagList = value;

                OnPropertyChanged();
            }
        }

        public ICommand DeviceSelectCommand { private set; get; }

        public ICommand ReadRfIdCommand { private set; get; }

        public RfidReaderViewModel(IRfidSensorBle rfidSensorBle)
        {
            _rfidBleService = rfidSensorBle;

            DeviceSelectCommand = new Command(OnDeviceSelect);
            ReadRfIdCommand = new Command(OnReadRfId);

            LatestRfIdReading = "N/A";
            DeviceName = "N/A";

            TagList = new ObservableCollection<RfIdTagDetection>();   

            WeakReferenceMessenger.Default.Register<RfIdValueReceivedMessage>(this, RfIdMessageReceived);
        }

        public void RfIdMessageReceived(object sender, RfIdValueReceivedMessage message)
        {
            var rfIdTagData = message.Value;
            LatestRfIdReading = rfIdTagData.RfIdTagText;

            var existingTag = TagList.FirstOrDefault(x => x.Tag == _lastestRfIdReading);    

            if (existingTag != null)
            {
                existingTag.Count++;
            }
            else
            {
                TagList.Add(new RfIdTagDetection { Tag = _lastestRfIdReading, Count = 1 });
            }
        }

        private async void OnReadRfId(object obj)
        {
            if (_rfidBleService.IsConnected)
            {
                var reading = await _rfidBleService.GetRfIdReading();

                LatestRfIdReading = reading;
            }
        }

        private async void OnDeviceSelect(object obj)
        {
            var popup = new DevicePopup(new DevicePopupViewModel(_rfidBleService));

            await Application.Current.MainPage.ShowPopupAsync(popup);

            if (_rfidBleService.ConnectedDevice != null && _rfidBleService.IsConnected)
            {
                await _rfidBleService.InitializeAsync();

                DeviceName = _rfidBleService.ConnectedDevice.Name;
            }


        }

        public async Task InitializeAsync()
        {
            _rfidBleService.IsBleSupported = await GetBLEPermission();

            if (_rfidBleService.IsBleSupported && !_rfidBleService.IsConnected)
                DeviceSelectCommand.Execute(null);
        }

        private async Task<bool> GetBLEPermission()
        {
            var locationPermission = false;
            var bluetoothPermission = false;

            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status == PermissionStatus.Granted)
                {
                    locationPermission = true;
                }
            }
            else locationPermission = true;

            status = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Bluetooth>();
                if (status == PermissionStatus.Granted)
                {
                    bluetoothPermission = true;
                }
            }
            else bluetoothPermission = true;

            return bluetoothPermission && locationPermission;
        }
    }
}
