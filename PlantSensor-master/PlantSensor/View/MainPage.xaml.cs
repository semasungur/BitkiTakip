
using System;
using System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PlantSensor
{
    public sealed partial class MainPage : Page
    {
       
        public Timer controlPanelTimer;

        public Timer altPressTimer;

        int idealTemperature;
        int idealSoilMoisture;
        int idealBrightness;

        Color colorBlue;
        Color colorlightBlue;
        Color colorWhite;
        Color colorlightRed;
        Color colorRed;
        SolidColorBrush SolidColorBrushBlue;
        SolidColorBrush SolidColorBrushLightBlue; 
        SolidColorBrush SolidColorBrushWhite; 
        SolidColorBrush SolidColorBrushLightRed; 
        SolidColorBrush SolidColorBrushRed;

        public static float currentBrightness;
        public static float currentTemperature;
        public static float currentSoilMoisture;

        public MainPage()
        {
            this.InitializeComponent();
            controlPanelTimer = new Timer(timerControlPanel, this, 0, 1000);

            idealTemperature = App.PlantSettings.IdealTemp;
            idealSoilMoisture = App.PlantSettings.IdealSoilMoist;
            idealBrightness = App.PlantSettings.IdealBright;

            colorBlue = Color.FromArgb(255, 85, 98, 112);
            colorlightBlue = Color.FromArgb(255, 78, 205, 196);
            colorWhite = Color.FromArgb(255, 199, 244, 100);
            colorlightRed = Color.FromArgb(255, 236, 107, 107);
            colorRed = Color.FromArgb(255, 196, 77, 88);
            SolidColorBrushBlue = new SolidColorBrush(colorBlue);
            SolidColorBrushLightBlue = new SolidColorBrush(colorlightBlue);
            SolidColorBrushWhite = new SolidColorBrush(colorWhite);
            SolidColorBrushLightRed = new SolidColorBrush(colorlightRed);
            SolidColorBrushRed = new SolidColorBrush(colorRed);

            DateTime Now = DateTime.Now;
            Random rand = new Random();
            TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
            TimeSpan oneHour = new TimeSpan(1, 0, 0);
            DateTime LowerBound = Now - oneDay;
            while (LowerBound < Now)
            {
                float randomValue = (float)rand.NextDouble() * 10;
                String nextValue = randomValue + "," + LowerBound + Environment.NewLine;
                App.TemperatureList.Add(nextValue);

                randomValue = (float)rand.NextDouble() * 10;
                nextValue = randomValue + "," + LowerBound + Environment.NewLine;
                App.BrightnessList.Add(nextValue);

                randomValue = (float)rand.NextDouble() * 10;
                nextValue = randomValue + "," + LowerBound + Environment.NewLine;
                App.SoilMoistureList.Add(nextValue);

                LowerBound += oneHour;
            }
        }

        private async void SensorProvider_DataReceived(object sender, SensorDataEventArgs e)
        {
            String format = formatOfSensorValue(e.SensorValue);
            String nextValue = e.SensorValue + "," + DateTime.Now + Environment.NewLine;
            SolidColorBrush ellipseFill;
            switch (e.SensorName)
            {
                case "Brightness":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentBrightness = e.SensorValue;
                        float suggestionBrightness = idealBrightness - e.SensorValue;
                        CurrentBrightnessNumber.Text = e.SensorValue.ToString(format);
                        OurSuggestionNumberBrightness.Text = suggestionBrightness.ToString(format);
                        ellipseFill = FigureOutFill(suggestionBrightness);
                        BrightnessOutsideEllipse.Fill = ellipseFill;

                        BrightnessUnitsMain.Foreground = ellipseFill;
                        IdealBrightnessText.Foreground = ellipseFill;
                        IdealBrightnessNumber.Foreground = ellipseFill;
                        CurrentBrightnessNumber.Foreground = ellipseFill;
                        CurrentBrightnessText.Foreground = ellipseFill;
                        OurSuggestionTextBrightness.Foreground = ellipseFill;
                        OurSuggestionNumberBrightness.Foreground = ellipseFill;

                        App.BrightnessList.Add(nextValue);
                    });
                    break;
                case "Temperature":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentTemperature = e.SensorValue;
                        float suggestionTemperature = idealTemperature - e.SensorValue;
                        CurrentTemperatureNumber.Text = e.SensorValue.ToString(format);
                        OurSuggestionNumberTemperature.Text = suggestionTemperature.ToString(format);
                        ellipseFill = FigureOutFill(suggestionTemperature);
                        TemperatureOutsideEllipse.Fill = ellipseFill;

                        TemperatureUnitsMain.Foreground = ellipseFill;
                        IdealTemperatureText.Foreground = ellipseFill;
                        IdealTemperatureNumber.Foreground = ellipseFill;
                        CurrentTemperatureNumber.Foreground = ellipseFill;
                        CurrentTemperatureText.Foreground = ellipseFill;
                        OurSuggestionTextTemperature.Foreground = ellipseFill;
                        OurSuggestionNumberTemperature.Foreground = ellipseFill;

                        App.TemperatureList.Add(nextValue);
                    });
                    break;
                case "SoilMoisture":
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        currentSoilMoisture = e.SensorValue;
                        float suggestionSoilMoisture = idealSoilMoisture - e.SensorValue;
                        CurrentSoilMoistureNumber.Text = e.SensorValue.ToString(format);
                        OurSuggestionNumberSoilMoisture.Text = suggestionSoilMoisture.ToString(format);
                        ellipseFill = FigureOutFill(suggestionSoilMoisture);
                        SoilMoistureOutsideEllipse.Fill = ellipseFill;

                        SoilMoistureUnitsMain.Foreground = ellipseFill;
                        IdealSoilMoistureText.Foreground = ellipseFill;
                        IdealSoilMoistureNumber.Foreground = ellipseFill;
                        CurrentSoilMoistureNumber.Foreground = ellipseFill;
                        CurrentSoilMoistureText.Foreground = ellipseFill;
                        OurSuggestionTextSoilMoisture.Foreground = ellipseFill;
                        OurSuggestionNumberSoilMoisture.Foreground = ellipseFill;

                        App.SoilMoistureList.Add(nextValue);
                    });
                    break;
            }
        }

        private String formatOfSensorValue(float value)
        {
            if(value == Math.Floor(value))
            {
                return "000";
            }
            return "####0.0";
        }

        protected override void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            App.SensorProvider.StartTimer();

            IdealTemperatureNumber.Text = idealTemperature.ToString();
            IdealBrightnessNumber.Text = idealBrightness.ToString();
            IdealSoilMoistureNumber.Text = idealSoilMoisture.ToString();

            App.SensorProvider.DataReceived += SensorProvider_DataReceived;


        }

        private async void timerControlPanel(object state)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                string formatTime = "HH:mm";
                string formatDate = "MM/dd/yy";
                ControlPanelTime.Text = DateTime.Now.ToString(formatTime);
                ControlPanelDate.Text = DateTime.Now.ToString(formatDate);
            });
        }

        public SolidColorBrush FigureOutFill(float suggestion)
        {
            if (suggestion > 9)
            {
                return SolidColorBrushBlue;
            }
            else if (suggestion > 3)
            {
                return SolidColorBrushLightBlue;
            }
            else if (suggestion > -3)
            {
                return SolidColorBrushWhite;
            }
            else if (suggestion > -9)
            {
                return SolidColorBrushLightRed;
            }
            else
            {
                return SolidColorBrushRed;
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HistoryPage));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void Twitter_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TwitterPage));
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
