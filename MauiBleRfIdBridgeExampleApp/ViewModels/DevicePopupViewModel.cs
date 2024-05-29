using Plugin.BLE.Abstractions.Contracts;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiBleRfIdBridgeExampleApp.Services.Interfaces;

namespace MauiBleRfIdBridgeExampleApp.ViewModels
{
    public class DevicePopupViewModel : BaseViewModel
    {
        public Action? CloseAction { get; set; }
        private IDevice? _itemSelected;
        private IDevice? _foundDevice;
        public ICommand DeviceSelectCommand { private set; get; }
        public IDevice SelectedDevice { get => _itemSelected; set { _itemSelected = value; OnPropertyChanged(); } }
        public IDevice FoundDevice { get => _foundDevice; set { _foundDevice = value; OnPropertyChanged(); } }
        private IAdapter _bleAdapter;
        private IRfidSensorBle _rfidBleService;
        private ObservableCollection<IDevice>? _deviceList;
        private List<IDevice> _scannedDevices;

        private bool _isScanning;

        public ObservableCollection<IDevice> DeviceList
        {
            get => _deviceList;
            set
            {
                _deviceList = value;
                OnPropertyChanged();
            }
        }
        public bool IsScanning { get => _isScanning; set { _isScanning = value; OnPropertyChanged(); } }

        public DevicePopupViewModel(IRfidSensorBle rfidBleService)
        {
            DeviceList = new ObservableCollection<IDevice>();
            DeviceSelectCommand = new Command(async () => await DeviceSelected(null));


            _scannedDevices = new List<IDevice>();
            _rfidBleService = rfidBleService;
            _bleAdapter = _rfidBleService.BleAdapter;

            _bleAdapter.ScanMode = ScanMode.LowPower;
            _bleAdapter.DeviceDiscovered += _bleAdapter_DeviceDiscovered;
            _bleAdapter.ScanTimeoutElapsed += _bleAdapter_ScanTimeoutElapsed;
        }

        private void _bleAdapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            IsScanning = false;

            var device = _scannedDevices.Where(x => x != null && x.Name != null && x.Name.Contains("MedicalPatient")).FirstOrDefault();

            _scannedDevices.Clear();

            if (device != null)
            {
                FoundDevice = device;
            }
        }

        private void _bleAdapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            _scannedDevices.Add(e.Device);
        }

        public async Task BeginScan()
        {
            IsScanning = true;
            await _bleAdapter.StartScanningForDevicesAsync();
        }

        private async Task DeviceSelected(object item)
        {
            SelectedDevice = FoundDevice;
            _rfidBleService.ConnectedDevice = null;
            await _bleAdapter.ConnectToDeviceAsync(SelectedDevice);


            if (_rfidBleService.IsConnected)
            {
                _rfidBleService.ConnectedDevice = SelectedDevice;
                CloseAction?.Invoke();
            }
        }
    }
}
