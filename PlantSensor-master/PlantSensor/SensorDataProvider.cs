using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace PlantSensor
{
    public delegate void DataReceivedEventHandler(object sender, SensorDataEventArgs e);
    public class SensorDataProvider
    {
        const byte SoilMoistureChannel = 3;
        const float ReferenceVoltage = 5.0F;
        public event DataReceivedEventHandler DataReceived;
        public MCP3008 mcp3008;
        public BMP280 BMP280;
        
        const byte LowPotentiometerADCChannel = 0;
        const byte HighPotentiometerADCChannel = 1;
        const byte CDSADCChannel = 2;

        private Timer timer;
        private Timer writeToFile;
        Random rand = new Random();

        public SensorDataProvider()
        {
            mcp3008 = new MCP3008(ReferenceVoltage);
            BMP280 = new BMP280();
        }

        public void StartTimer()
        {
            timer = new Timer(timerCallback, this, 0, 3000);
            writeToFile = new Timer(writeToFileTimerCallback, this, 20000, 9000);
        }

        private async void writeToFileTimerCallback(object state)
        {
            for (int ii = 0; ii < App.BrightnessList.Count; ii++)
            {
                App.Brightnessresult.Add(App.BrightnessList[ii]);
                await Windows.Storage.FileIO.AppendTextAsync(App.BrightnessFile, App.BrightnessList[ii]);
                Debug.WriteLine("Brightness File" + App.BrightnessList[ii]);
            }

            for (int ii = 0; ii < App.TemperatureList.Count; ii++)
            {
                App.Temperatureresult.Add(App.TemperatureList[ii]);
                await Windows.Storage.FileIO.AppendTextAsync(App.TemperatureFile, App.TemperatureList[ii]);
            }

            for (int ii = 0; ii < App.SoilMoistureList.Count; ii++)
            {
                App.SoilMoistureresult.Add(App.SoilMoistureList[ii]);
                await Windows.Storage.FileIO.AppendTextAsync(App.SoilMoistureFile, App.SoilMoistureList[ii]);
            }

            App.BrightnessList.Clear();
            App.TemperatureList.Clear();
            App.SoilMoistureList.Clear();
        }

        private async void timerCallback(object state)
        {

            if (BMP280 == null)
            {
                Debug.WriteLine("BMP280 is null");
            }
            else
            {

                float currentTemperature = await BMP280.ReadTemperature();
                var tempArgs = new SensorDataEventArgs()
                {
                    SensorName = "Temperature",
                    SensorValue = currentTemperature,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(tempArgs);
            }

            if (mcp3008 == null)
            {
                Debug.WriteLine("mcp3008 is null");
            }
            else
            {

                int cdsReadVal = mcp3008.ReadADC(CDSADCChannel);
                float cdsVoltage = mcp3008.ADCToVoltage(cdsReadVal);

                float currentBrightness = cdsVoltage;
                var brightnessArgs = new SensorDataEventArgs()
                {
                    SensorName = "Brightness",
                    SensorValue = currentBrightness,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(brightnessArgs);

                float currentSoilMoisture = mcp3008.ReadADC(SoilMoistureChannel);
                Debug.WriteLine(currentSoilMoisture);
                var soilmoistureArgs = new SensorDataEventArgs()
                {
                    SensorName = "SoilMoisture",
                    SensorValue = currentSoilMoisture,
                    Timestamp = DateTime.Now
                };
                OnDataReceived(soilmoistureArgs);
            }
        }

        protected virtual void OnDataReceived(SensorDataEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, e);
            }
        }
    }
    public class SensorDataEventArgs : EventArgs
    {
        public string SensorName;
        public float SensorValue;
        public DateTime Timestamp;
    }
}
