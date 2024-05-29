using Iot.Device.Mfrc522;
using Iot.Device.Rfid;
using nanoFramework.Hardware.Esp32;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;

namespace MedicalRfIdSensor.Sensor
{
    public class RfIdReader
    {
        private GpioController _gpioController;
        private MfRc522 _mfrc522;
        public RfIdReader(GpioController gpioController)
        {
            _gpioController = gpioController;
        }

        public void Initialize()
        {
           //  GPIO23 = MOSI; GPIO25 = MISO; GPIO19 = Clock

            int pinReset = 22;

            nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(23, DeviceFunction.SPI1_MOSI);
            nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(19, DeviceFunction.SPI1_MISO);
            nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(18, DeviceFunction.SPI1_CLOCK);


            SpiConnectionSettings connection = new (1, 21);
            connection.ClockFrequency = 5_000_000;

            SpiDevice spi = SpiDevice.Create(connection);
            _mfrc522 = new(spi, pinReset, _gpioController, false);

        }

        public byte[] ReadCardNfcId()
        {
            bool res;
            Data106kbpsTypeA card;
            do
            {
                res = _mfrc522.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(2));
                Thread.Sleep(res ? 0 : 200);
            }
            while (!res);

            if (card != null)
            {
                Debug.WriteLine("Card Found");

                return card.NfcId;

            }
            else
            {
                Debug.WriteLine("No card found");

                return null;
            }
        }

        void ProcessMifare(Data106kbpsTypeA card)
        {
            var mifare = new MifareCard(_mfrc522!, 0);
            mifare.SerialNumber = card.NfcId;
            mifare.Capacity = MifareCardCapacity.Mifare1K;
            mifare.KeyA = MifareCard.DefaultKeyA.ToArray();
            mifare.KeyB = MifareCard.DefaultKeyB.ToArray();
            int ret;

            for (byte block = 0; block < 64; block++)
            {
                mifare.BlockNumber = block;
                mifare.Command = MifareCardCommand.AuthenticationB;
                ret = mifare.RunMifareCardCommand();
                if (ret < 0)
                {
                    // If you have an authentication error, you have to deselect and reselect the card again and retry
                    // Those next lines shows how to try to authenticate with other known default keys
                    mifare.ReselectCard();
                    // Try the other key
                    mifare.KeyA = MifareCard.DefaultKeyA.ToArray();
                    mifare.Command = MifareCardCommand.AuthenticationA;
                    ret = mifare.RunMifareCardCommand();
                    if (ret < 0)
                    {
                        mifare.ReselectCard();
                        mifare.KeyA = MifareCard.DefaultBlocksNdefKeyA.ToArray();
                        mifare.Command = MifareCardCommand.AuthenticationA;
                        ret = mifare.RunMifareCardCommand();
                        if (ret < 0)
                        {
                            mifare.ReselectCard();
                            mifare.KeyA = MifareCard.DefaultFirstBlockNdefKeyA.ToArray();
                            mifare.Command = MifareCardCommand.AuthenticationA;
                            ret = mifare.RunMifareCardCommand();
                            if (ret < 0)
                            {
                                mifare.ReselectCard();
                                Debug.WriteLine($"Error reading bloc: {block}");
                            }
                        }
                    }
                }

                if (ret >= 0)
                {
                    mifare.BlockNumber = block;
                    mifare.Command = MifareCardCommand.Read16Bytes;
                    ret = mifare.RunMifareCardCommand();
                    if (ret >= 0)
                    {
                        if (mifare.Data is object)
                        {
                            Debug.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifare.Data)}");
                        }
                    }
                    else
                    {
                        mifare.ReselectCard();
                        Debug.WriteLine($"Error reading bloc: {block}");
                    }

                    if (block % 4 == 3)
                    {
                        if (mifare.Data != null)
                        {
                            // Check what are the permissions
                            for (byte j = 3; j > 0; j--)
                            {
                                var access = mifare.BlockAccess((byte)(block - j), mifare.Data);
                                Debug.WriteLine($"Bloc: {block - j}, Access: {access}");
                            }

                            var sector = mifare.SectorTailerAccess(block, mifare.Data);
                            Debug.WriteLine($"Bloc: {block}, Access: {sector}");
                        }
                        else
                        {
                            Debug.WriteLine("Can't check any sector bloc");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Authentication error");
                }
            }
        }

    }
}
