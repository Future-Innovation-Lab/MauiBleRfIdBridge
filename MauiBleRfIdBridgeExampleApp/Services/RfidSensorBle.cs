using CommunityToolkit.Mvvm.Messaging;
using MauiBleRfIdBridgeExampleApp.Messages;
using MauiBleRfIdBridgeExampleApp.Models;
using MauiBleRfIdBridgeExampleApp.Services.Interfaces;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Diagnostics;

namespace MauiBleRfIdBridgeExampleApp.Services
{
    public class RfidSensorBle : IRfidSensorBle
    {
        private IBluetoothLE _ble;
        private IAdapter _adapter;
        private bool _isBleSupported = false;
        private bool _isConnected = false;

        private IDevice _connectedDevice;

        private DateTime _lastUpdated;

        private IService ReceiveRfidTelemetryService;
        private ICharacteristic ReceiveRfidCharacteristic;

        public static readonly Guid ReceiveRfidTelemetryServiceId = new Guid("A7EEDF2C-DA87-4CB5-A9C5-5151C78B0057");
        public static readonly Guid ReceiveRfidCharacteristicId = new Guid("A7EEDF2C-DA88-4CB5-A9C5-5151C78B0057");

        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set { _lastUpdated = value; }
        }


        public RfidSensorBle()
        {
            _ble = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            _adapter.DeviceConnected += DeviceConnected;
            _adapter.DeviceDisconnected += DeviceDisconnected;
        }

        private void DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            IsConnected = false;
        }

        private void DeviceConnected(object sender, DeviceEventArgs e)
        {
            IsConnected = true;
        }

        public async Task InitializeAsync()
        {
            Debug.WriteLine("LEStream: Looking for service " + ReceiveRfidTelemetryServiceId + "...");
            ReceiveRfidTelemetryService = await _connectedDevice.GetServiceAsync(ReceiveRfidTelemetryServiceId);

            Debug.WriteLine("LEStream: Getting characteristics...");
            ReceiveRfidCharacteristic = await ReceiveRfidTelemetryService.GetCharacteristicAsync(ReceiveRfidCharacteristicId);
            Debug.WriteLine("LEStream: Got characteristics");

            ReceiveRfidCharacteristic.ValueUpdated += ReceiveRfidCharacteristic_ValueUpdated;

            await ReceiveRfidCharacteristic.StartUpdatesAsync();
        }

        private void ReceiveRfidCharacteristic_ValueUpdated(object? sender, CharacteristicUpdatedEventArgs e)
        {
            var data = BitConverter.ToString(e.Characteristic.Value);
            var rfIdTagData = new RfIdTagData() { RfIdTagText = data, RfIdTagUid = e.Characteristic.Value, Timestamp = DateTime.Now };
            WeakReferenceMessenger.Default.Send(new RfIdValueReceivedMessage(rfIdTagData));
        }

        public IAdapter BleAdapter => _adapter;

        public bool IsBleSupported { get => _isBleSupported; set => _isBleSupported = value; }
        public IDevice ConnectedDevice { get => _connectedDevice; set => _connectedDevice = value; }
        public bool IsConnected { get => _isConnected; set => _isConnected = value; }

        public async Task<string> GetRfIdReading()
        {
            if (IsConnected)
            {
                var result = await ReceiveRfidCharacteristic.ReadAsync();
                var tempString = System.Text.Encoding.UTF8.GetString(result.data);

                if (!string.IsNullOrEmpty(tempString))
                    return tempString;
            }

            return "N/A";
        }
    }
}
