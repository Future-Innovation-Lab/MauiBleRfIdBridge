using Plugin.BLE.Abstractions.Contracts;

namespace MauiBleRfIdBridgeExampleApp.Services.Interfaces
{
    public interface IRfidSensorBle
    {
        public IAdapter BleAdapter { get; }
        bool IsBleSupported { get; set; }
        bool IsConnected { get; set; }
        IDevice? ConnectedDevice { get; set; }

        Task<string> GetRfIdReading();
        Task InitializeAsync();
    }
}
