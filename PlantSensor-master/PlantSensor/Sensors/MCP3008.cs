
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PlantSensor
{
    public class MCP3008
    {
        public SpiDevice mcp3008;
        const int SPI_CHIP_SELECT_LINE = 0; 
        
        const byte MCP3008_SingleEnded = 0x08;
        const byte MCP3008_Differential = 0x00;

        float ReferenceVoltage;
        public const uint Min = 0;
        public const uint Max = 1023;


        public MCP3008(float referenceVolgate)
        {
            Debug.WriteLine("MCP3008::New MCP3008");

            ReferenceVoltage = referenceVolgate;
        }
        
        public async Task Initialize()
        {
            Debug.WriteLine("MCP3008::Initialize");
            try
            {
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                settings.ClockFrequency = 3600000;
                settings.Mode = SpiMode.Mode0;

                string aqs = SpiDevice.GetDeviceSelector();
      
                var dis = await DeviceInformation.FindAllAsync(aqs);
         
                mcp3008 = await SpiDevice.FromIdAsync(dis[0].Id, settings);

                if (mcp3008 == null)
                {
                    Debug.WriteLine(
                        "SPI Controller {0} is currently in use by another application. Please ensure that no other applications are using SPI.",
                        dis[0].Id);
                    return;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }

        public int ReadADC(byte whichChannel)
        {
            byte command = whichChannel;
            command |= MCP3008_SingleEnded;
            command <<= 4;

            byte[] commandBuf = new byte[] { 0x01, command, 0x00 };

            byte[] readBuf = new byte[] { 0x00, 0x00, 0x00 };

            mcp3008.TransferFullDuplex(commandBuf, readBuf);

            int sample = readBuf[2] + ((readBuf[1] & 0x03) << 8);
            int s2 = sample & 0x3FF;
            Debug.Assert(sample == s2);

            return sample;
        }

        public float ADCToVoltage(int adc)
        {
            return (float)adc * ReferenceVoltage / (float)Max;
        }
    }

}
