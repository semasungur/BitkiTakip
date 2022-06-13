
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;

namespace PlantSensor
{
    //bmp280 ayarlaması
    public class BMP280_CalibrationData
    {
        public UInt16 dig_T1 { get; set; }
        public Int16 dig_T2 { get; set; }
        public Int16 dig_T3 { get; set; }

        public UInt16 dig_P1 { get; set; }
        public Int16 dig_P2 { get; set; }
        public Int16 dig_P3 { get; set; }
        public Int16 dig_P4 { get; set; }
        public Int16 dig_P5 { get; set; }
        public Int16 dig_P6 { get; set; }
        public Int16 dig_P7 { get; set; }
        public Int16 dig_P8 { get; set; }
        public Int16 dig_P9 { get; set; }
    }
    public class BMP280
    {
      
        const byte BMP280_Address = 0x77;
        const byte BMP280_Signature = 0x58;

        enum eRegisters : byte
        {
            BMP280_REGISTER_DIG_T1 = 0x88,
            BMP280_REGISTER_DIG_T2 = 0x8A,
            BMP280_REGISTER_DIG_T3 = 0x8C,

            BMP280_REGISTER_DIG_P1 = 0x8E,
            BMP280_REGISTER_DIG_P2 = 0x90,
            BMP280_REGISTER_DIG_P3 = 0x92,
            BMP280_REGISTER_DIG_P4 = 0x94,
            BMP280_REGISTER_DIG_P5 = 0x96,
            BMP280_REGISTER_DIG_P6 = 0x98,
            BMP280_REGISTER_DIG_P7 = 0x9A,
            BMP280_REGISTER_DIG_P8 = 0x9C,
            BMP280_REGISTER_DIG_P9 = 0x9E,

            BMP280_REGISTER_CHIPID = 0xD0,
            BMP280_REGISTER_VERSION = 0xD1,
            BMP280_REGISTER_SOFTRESET = 0xE0,

            BMP280_REGISTER_CAL26 = 0xE1,  

            BMP280_REGISTER_CONTROLHUMID = 0xF2,
            BMP280_REGISTER_CONTROL = 0xF4,
            BMP280_REGISTER_CONFIG = 0xF5,

            BMP280_REGISTER_PRESSUREDATA_MSB = 0xF7,
            BMP280_REGISTER_PRESSUREDATA_LSB = 0xF8,
            BMP280_REGISTER_PRESSUREDATA_XLSB = 0xF9, 

            BMP280_REGISTER_TEMPDATA_MSB = 0xFA,
            BMP280_REGISTER_TEMPDATA_LSB = 0xFB,
            BMP280_REGISTER_TEMPDATA_XLSB = 0xFC,

            BMP280_REGISTER_HUMIDDATA_MSB = 0xFD,
            BMP280_REGISTER_HUMIDDATA_LSB = 0xFE,
        };

        const string I2CControllerName = "I2C1";

        private I2cDevice bmp280 = null;

        BMP280_CalibrationData CalibrationData;

        bool init = false;

        public async Task Initialize()
        {
            Debug.WriteLine("BMP280::Initialize");

            try
            {
                I2cConnectionSettings settings = new I2cConnectionSettings(BMP280_Address);

                settings.BusSpeed = I2cBusSpeed.FastMode;

                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);

                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);

                bmp280 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                if (bmp280 == null)
                {
                    Debug.WriteLine("Device not found");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }

        private async Task Begin()
        {
            Debug.WriteLine("BMP280::Begin");
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CHIPID };
            byte[] ReadBuffer = new byte[] { 0xFF };

            bmp280.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("BMP280 Signature: " + ReadBuffer[0].ToString());

            if (ReadBuffer[0] != BMP280_Signature)
            {
                Debug.WriteLine("BMP280::Begin Signature Mismatch.");
                return;
            }

            init = true;

            CalibrationData = await ReadCoefficeints();

            await WriteControlRegister();

            await WriteControlRegisterHumidity();
        }

        private async Task WriteControlRegisterHumidity()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CONTROLHUMID, 0x03 };
            bmp280.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        private async Task WriteControlRegister()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CONTROL, 0x3F };
            bmp280.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }
        
        private UInt16 ReadUInt16_LittleEndian(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bmp280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);
            return value;
        }

        private byte ReadByte(byte register)
        {
            byte value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00 };

            writeBuffer[0] = register;

            bmp280.WriteRead(writeBuffer, readBuffer);
            value = readBuffer[0];
            return value;
        }

        private async Task<BMP280_CalibrationData> ReadCoefficeints()
        {
            CalibrationData = new BMP280_CalibrationData();

            CalibrationData.dig_T1 = ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T1);
            CalibrationData.dig_T2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T2);
            CalibrationData.dig_T3 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T3);

            CalibrationData.dig_P1 = ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P1);
            CalibrationData.dig_P2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P2);
            CalibrationData.dig_P3 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P3);
            CalibrationData.dig_P4 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P4);
            CalibrationData.dig_P5 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P5);
            CalibrationData.dig_P6 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P6);
            CalibrationData.dig_P7 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P7);
            CalibrationData.dig_P8 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P8);
            CalibrationData.dig_P9 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P9);

            await Task.Delay(1);
            return CalibrationData;
        }

        Int32 t_fine = Int32.MinValue;

        private double BMP280_compensate_T_double(Int32 adc_T)
        {
            double var1, var2, T;

            var1 = ((adc_T / 16384.0) - (CalibrationData.dig_T1 / 1024.0)) * CalibrationData.dig_T2;
            var2 = ((adc_T / 131072.0) - (CalibrationData.dig_T1 / 8192.0)) * CalibrationData.dig_T3;

            t_fine = (Int32)(var1 + var2);

            T = (var1 + var2) / 5120.0;
            return T;
        }

        private Int64 BMP280_compensate_P_Int64(Int32 adc_P)
        {
            Int64 var1, var2, p;

            var1 = t_fine - 128000;
            var2 = var1 * var1 * (Int64)CalibrationData.dig_P6;
            var2 = var2 + ((var1 * (Int64)CalibrationData.dig_P5) << 17);
            var2 = var2 + ((Int64)CalibrationData.dig_P4 << 35);
            var1 = ((var1 * var1 * (Int64)CalibrationData.dig_P3) >> 8) + ((var1 * (Int64)CalibrationData.dig_P2) << 12);
            var1 = (((((Int64)1 << 47) + var1)) * (Int64)CalibrationData.dig_P1) >> 33;
            if (var1 == 0)
            {
                Debug.WriteLine("BMP280_compensate_P_Int64 Jump out to avoid / 0");
                return 0; 
            }

            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((Int64)CalibrationData.dig_P9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((Int64)CalibrationData.dig_P8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((Int64)CalibrationData.dig_P7 << 4);
            return p;
        }


        public async Task<float> ReadTemperature()
        {

            if (!init) await Begin();

            byte tmsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_XLSB); 
            
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            double temp = BMP280_compensate_T_double(t);

            return (float)temp;
        }

        public async Task<float> ReadPreasure()
        {

            if (!init) await Begin();
            
            if (t_fine == Int32.MinValue)
            {
                await ReadTemperature();
            }

            byte tmsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_XLSB); 

            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            Int64 pres = BMP280_compensate_P_Int64(t);

            return ((float)pres) / 256;
        }
        
        public async Task<float> ReadAltitude(float seaLevel)
        {
            if (!init) await Begin();

            float pressure = await ReadPreasure();

            pressure /= 100;

            return 44330.0f * (1.0f - (float)Math.Pow((pressure / seaLevel), 0.1903f));
        }
    }
}
