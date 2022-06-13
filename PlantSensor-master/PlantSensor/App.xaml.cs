using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PlantSensor
{    
    sealed partial class App : Application
    {
        public static SensorDataProvider SensorProvider;

        public static Windows.Storage.StorageFile BrightnessFile;
        public static Windows.Storage.StorageFile TemperatureFile;
        public static Windows.Storage.StorageFile SoilMoistureFile;
        public static Windows.Storage.StorageFile TwitterFile;

        static Windows.Storage.StorageFolder storageFolder;

        public static IList<string> Brightnessresult;
        public static IList<string> Temperatureresult;
        public static IList<string> SoilMoistureresult;
        public static IList<string> Twitterresult;

        public static List<String> BrightnessList;
        public static List<String> TemperatureList;
        public static List<String> SoilMoistureList;

        public static Settings PlantSettings;
        public static Settings TwitterSettings;

        public App()
        {
            PlantSettings = new Settings();
            TwitterSettings = new Settings();
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            SensorProvider = new SensorDataProvider();
        }

        public async Task setUpFile()
        {
            storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                BrightnessFile = await storageFolder.GetFileAsync(FileNames.BrightnessfileName);
            }
            catch (FileNotFoundException e)
            {
                BrightnessFile = await storageFolder.CreateFileAsync(FileNames.BrightnessfileName);
            }

            try
            {
                TemperatureFile = await storageFolder.GetFileAsync(FileNames.TemperaturefileName);
            }
            catch (FileNotFoundException e)
            {
                TemperatureFile = await storageFolder.CreateFileAsync(FileNames.TemperaturefileName);
            }

            try
            {
                SoilMoistureFile = await storageFolder.GetFileAsync(FileNames.SoilMoisturefileName);
                Debug.WriteLine("Old Files are used");
            }
            catch (FileNotFoundException e)
            {
                SoilMoistureFile = await storageFolder.CreateFileAsync(FileNames.SoilMoisturefileName);
                Debug.WriteLine("New Files were created");
            }

            try
            {
                TwitterFile = await storageFolder.GetFileAsync(FileNames.SettingsfileName);
                Debug.WriteLine("Old settings Files are used");
            }
            catch (FileNotFoundException e)
            {
                TwitterFile = await storageFolder.CreateFileAsync(FileNames.SettingsfileName);
                Debug.WriteLine("new settingsFiles are used");
            }

            Brightnessresult = await Windows.Storage.FileIO.ReadLinesAsync(BrightnessFile);
            Temperatureresult = await Windows.Storage.FileIO.ReadLinesAsync(TemperatureFile);
            SoilMoistureresult = await Windows.Storage.FileIO.ReadLinesAsync(SoilMoistureFile);
            Twitterresult = await Windows.Storage.FileIO.ReadLinesAsync(TwitterFile);

            BrightnessList = new List<string>();
            TemperatureList = new List<string>();
            SoilMoistureList = new List<string>();

        }

        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            await setUpFile();
            await App.SensorProvider.mcp3008.Initialize();
            await App.SensorProvider.BMP280.Initialize();
            try
            {
                PlantSettings = await Settings.Load(FileNames.SettingsfileName);
            }
            catch
            {

            }
            try
            {
                TwitterSettings = await Settings.Load(FileNames.SettingsfileName);
            }
            catch
            {

            }
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;
            
            if (rootFrame == null)
            {

                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {

                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {

                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
