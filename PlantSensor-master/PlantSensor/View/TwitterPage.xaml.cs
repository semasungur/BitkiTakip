
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Tweetinvi;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PlantSensor
{
    public class twitterListClass
    {
        public string Tweet { get; set;}
        public string TweetDate { get; set; }
    }

    public sealed partial class TwitterPage : Page
    {
        Boolean hasAccount;
        Timer twitterTimerLive;
        ObservableCollection<twitterListClass> listOfTweets;

        public TwitterPage()
        {
            this.InitializeComponent();
            ConsumerKeyTextBlock.Text = App.TwitterSettings.ConsumerKeySetting;
            ConsumerSecretTextBlock.Text = App.TwitterSettings.ConsumerSecretSetting;
            AccessSecretTextBlock.Text = App.TwitterSettings.AccessKeySetting;
            AccessTokenTextBlock.Text = App.TwitterSettings.AccessTokenSetting;
            TwitterStatusText.Text = "If you have made changes: unsaved";

            try
            {
                setUpUser();
            }

            catch
            {
                TwitterStatusText.Text = "These credentials do not match any twitter account";
                hasAccount = false;
            }
        }

        private void setUpUser()
        {
            Auth.SetUserCredentials(App.TwitterSettings.ConsumerKeySetting, App.TwitterSettings.ConsumerSecretSetting, App.TwitterSettings.AccessTokenSetting, App.TwitterSettings.AccessKeySetting);
            var user = User.GetAuthenticatedUser();
            var tweets = Timeline.GetUserTimeline(user);
            listOfTweets = new ObservableCollection<twitterListClass>();
            foreach (Tweetinvi.Models.ITweet twitterInUI in tweets)
            {
                String twitterDateStringInUI = twitterInUI.CreatedAt.ToString();
                String twitterStringInUI = twitterInUI.FullText.ToString();
                listOfTweets.Add(new twitterListClass { Tweet = twitterStringInUI, TweetDate = twitterDateStringInUI });
            }
            HistoryTweetList.ItemsSource = listOfTweets;
            hasAccount = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("Twitter Page reached");
        }

        private async void twitterTimerLiveMethod(object state)
        {
            String TweetInTimer = determineTweet();

            var firstTweet = Tweet.PublishTweet(TweetInTimer);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {

                listOfTweets.Add(new twitterListClass { Tweet = TweetInTimer, TweetDate = DateTime.Now.ToString() });
            });
        }

        private string determineTweet()
        {
            return "Brightness: "+ MainPage.currentBrightness + Environment.NewLine + "Temperature: " + MainPage.currentTemperature + Environment.NewLine + "Soil Moisture: " + MainPage.currentSoilMoisture;
        }

        private void TwitterCalendarAppBar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HistoryPage));
        }

        private void TwitterSettingsAppBar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void TwitterHomeAppBar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void TwitterTwitterAppBar_Click(object sender, RoutedEventArgs e)
        {

        }


        private void toggleSwitchTwitter_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if(toggleSwitch.IsOn == true)
                {
                    if (hasAccount)
                    {
                        twitterTimerLive = new Timer(twitterTimerLiveMethod, this, 0, 3000);
                    }
                }
                else
                {
                    twitterTimerLive.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        private void TwitterSaveButton_Click(object sender, RoutedEventArgs e)
        {
            App.TwitterSettings.ConsumerKeySetting = ConsumerKeyTextBlock.Text;
            App.TwitterSettings.ConsumerSecretSetting = ConsumerSecretTextBlock.Text;
            App.TwitterSettings.AccessKeySetting = AccessSecretTextBlock.Text;
            App.TwitterSettings.AccessTokenSetting = AccessTokenTextBlock.Text;
            App.TwitterSettings.Save();
            try
            {
                setUpUser();
                TwitterStatusText.Text = "Account Successfully made";
            }
            catch
            {
                TwitterStatusText.Text = "These credentials do not match any twitter account";
            }
        }
    }
}
