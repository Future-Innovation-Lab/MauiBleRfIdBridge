using Iot.Device.Mfrc522;
using Iot.Device.Rfid;
using MedicalRfIdSensor.Sensor;
using nanoFramework.Device.Bluetooth;
using nanoFramework.Device.Bluetooth.GenericAttributeProfile;
using System;
using System.Collections;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;

namespace MedicalRfIdSensor
{
    public class Program
    {
        static GattLocalCharacteristic _rfidNotifyCharacteristic;
        static RfIdReader _rfIdReader;

        static byte[] _lastReadRfIdTag;

        public static void Main()
        {
            GpioController gpioController = new GpioController();

            _rfIdReader = new RfIdReader(gpioController);

            Guid serviceUuid = new Guid("A7EEDF2C-DA87-4CB5-A9C5-5151C78B0057");
            Guid rfidNotifyUuid = new Guid("A7EEDF2C-DA88-4CB5-A9C5-5151C78B0057");
            Guid readDeviceInfoUuid = new Guid("A7EEDF2C-DA89-4CB5-A9C5-5151C78B0057");

            BluetoothLEServer server = BluetoothLEServer.Instance;
            server.DeviceName = "MedicalPatient001";

            GattServiceProviderResult result = GattServiceProvider.Create(serviceUuid);

            if (result.Error != BluetoothError.Success)
            {
                return;
            }

            GattServiceProvider serviceProvider = result.ServiceProvider;

            GattLocalService service = serviceProvider.Service;

            DataWriter applicationVersionDw = new DataWriter();
            applicationVersionDw.WriteString("Medical Rfid Device Version 1.0");

            GattLocalCharacteristicResult characteristicResult = service.CreateCharacteristic(readDeviceInfoUuid,
new GattLocalCharacteristicParameters()
{
    CharacteristicProperties = GattCharacteristicProperties.Read,
    UserDescription = "Device Information",
    StaticValue = applicationVersionDw.DetachBuffer()
});

            if (characteristicResult.Error != BluetoothError.Success)
            {
                return;
            }

            characteristicResult = service.CreateCharacteristic(rfidNotifyUuid,
    new GattLocalCharacteristicParameters()
    {
        CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
        UserDescription = "RFid Value Notification"
    });

            if (characteristicResult.Error != BluetoothError.Success)
            {
                return;
            }

            _rfidNotifyCharacteristic = characteristicResult.Characteristic;
            _rfidNotifyCharacteristic.ReadRequested += RfidReadCharacteristic_ReadRequested; ;
            _rfidNotifyCharacteristic.SubscribedClientsChanged += RfidNotifyCharacteristic_SubscribedClientsChanged;

            serviceProvider.StartAdvertising(new GattServiceProviderAdvertisingParameters()
            {
                IsConnectable = true,
                IsDiscoverable = true
            });

            _rfIdReader.Initialize();

            ThreadStart threadDelegate = new ThreadStart(RfidWork);
            Thread newThread = new Thread(threadDelegate);
            newThread.Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void RfidNotifyCharacteristic_SubscribedClientsChanged(GattLocalCharacteristic sender, object args)
        {
            if (sender.SubscribedClients.Length > 0)
            {
                Debug.WriteLine($"Client connected ");
            }
        }

        private static void UpdateNotifyValue(string newValue)
        {
            DataWriter dw = new DataWriter();
            dw.WriteString(newValue);

            _rfidNotifyCharacteristic.NotifyValue(dw.DetachBuffer());
        }

        private static void UpdateNotifyValue(byte[] newValue)
        {
            DataWriter dw = new DataWriter();
            dw.WriteBytes(newValue);

            _rfidNotifyCharacteristic.NotifyValue(dw.DetachBuffer());
        }

        private static void RfidReadCharacteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs ReadRequestEventArgs)
        {
            if (_lastReadRfIdTag != null)
            {
                DataWriter dw = new DataWriter();
                dw.WriteBytes(_lastReadRfIdTag);

                _rfidNotifyCharacteristic.NotifyValue(dw.DetachBuffer());
            }
        }


        public static void RfidWork()
        {
            while (true)
            {
                var id = _rfIdReader.ReadCardNfcId();
                _lastReadRfIdTag = id;
                UpdateNotifyValue(id);

               Thread.Sleep(1000);
            }

        }
    }
}
