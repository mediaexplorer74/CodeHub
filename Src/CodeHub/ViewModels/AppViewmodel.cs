﻿using CodeHub.Helpers;
using CodeHub.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Services.Store;
using Windows.System.Profile;
using Windows.UI.Popups;

namespace CodeHub.ViewModels
{
    public class AppViewmodel : ViewModelBase
    {
        #region properties
        public bool _isLoggedin;
        public bool isLoggedin
        {
            get
            {
                return _isLoggedin;
            }
            set
            {
                Set(() => isLoggedin, ref _isLoggedin, value);
            }
        }

        public bool _isLoading;
        public bool isLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                Set(() => isLoading, ref _isLoading, value);
            }
        }

        public User _user;
        public User User
        {
            get
            {
                return _user;
            }
            set
            {
                Set(() => User, ref _user, value);
            }
        }

        public bool _IsNotificationsUnread;
        public bool IsNotificationsUnread
        {
            get
            {
                return _IsNotificationsUnread;
            }
            set
            {
                Set(() => IsNotificationsUnread, ref _IsNotificationsUnread, value);
            }
        }

        private bool _isDesktopAdsVisible;
        public bool IsDesktopAdsVisible
        {
            get { return _isDesktopAdsVisible; }
            set
            {
                Set(() => IsDesktopAdsVisible, ref _isDesktopAdsVisible, value);
            }
        }

        private bool _isMobileAdsVisible;
        public bool IsMobileAdsVisible
        {
            get { return _isMobileAdsVisible; }
            set
            {
                Set(() => IsMobileAdsVisible, ref _isMobileAdsVisible, value);
            }
        }

        public string WhatsNewText
        {
            get
            {
                //return "Hi all! \nHere's the changelog for v2.4.9\n\n\x2022 Under the hood improvements\n\x2022 NOTE: Please upgrade to Fall Creator's Update to get latest CodeHub feature updates.";
                return "";
            }
        }
        #endregion

        private const string donateFirstAddOnId = "9pd0r1dxkt8j";
        private const string donateSecondAddOnId = "9msvqcz4pbws";
        private const string donateThirdAddOnId = "9n571g3nr2cs";
        private const string donateFourthAddOnId = "9nsmgzx3p43x";
        private const string donateFifthAddOnId = "9phrhpvhscdv";
        private const string donateSixthAddOnId = "9nnqdq0kq21j";

        public async void MarkdownTextBlock_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(e.Link));
            }
            catch(UriFormatException)
            {
                MessageDialog dialog = new MessageDialog("Incorrect URI Format");
                await dialog.ShowAsync();
            }
        }

        public void Navigate(Type pageType)
        {
            SimpleIoc.Default.GetInstance<IAsyncNavigationService>().NavigateAsync(pageType, User);
        }

        public void GoBack()
        {
            SimpleIoc.Default.GetInstance<IAsyncNavigationService>().GoBackAsync();
        }

        public void UpdateUnreadNotificationIndicator(bool IsUnread)
        {
            IsNotificationsUnread = IsUnread;
        }

        public async Task CheckForUnreadNotifications()
        {
            var unread = await NotificationsService.GetAllNotificationsForCurrentUser(false, false);
            if (unread != null)
            {
                if (unread.Count > 0)
                    UpdateUnreadNotificationIndicator(true);
                else
                    UpdateUnreadNotificationIndicator(false);
            }
        }

        public async Task<bool> HasAlreadyDonated()
        {
            try
            {
                if (SettingsService.Get<bool>(SettingsKeys.HasUserDonated))
                {
                    return true;
                }
                else
                {
                    StoreContext WindowsStore = StoreContext.GetDefault();

                    string[] productKinds = { "Durable" };
                    List<String> filterList = new List<string>(productKinds);

                    StoreProductQueryResult queryResult = await WindowsStore.GetUserCollectionAsync(filterList);

                    if (queryResult.ExtendedError != null)
                    {
                        return false;
                    }

                    foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
                    {
                        if (item.Value != null)
                        {
                            if (item.Value.IsInUserCollection)
                            {
                                SettingsService.Save(SettingsKeys.HasUserDonated, true, true);
                                return true;
                            }
                        }
                        return false;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task ConfigureAdsVisibility()
        {
            //if (await HasAlreadyDonated())
            if (true)
            {
                GlobalHelper.HasAlreadyDonated = true;
                SettingsService.Save<bool>(SettingsKeys.IsAdsEnabled, false);
                ToggleAdsVisiblity();
            }
            else
            {
                SettingsService.Save<bool>(SettingsKeys.IsAdsEnabled, true);

                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    IsMobileAdsVisible = true;
                    IsDesktopAdsVisible = false;
                }
                else
                {
                    IsDesktopAdsVisible = true;
                    IsMobileAdsVisible = false;
                }
            }
        }

        public void ToggleAdsVisiblity()
        {
            if (SettingsService.Get<bool>(SettingsKeys.IsAdsEnabled))
            {
                if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                {
                    IsMobileAdsVisible = true;
                    IsDesktopAdsVisible = false;
                }
                else
                {
                    IsDesktopAdsVisible = true;
                    IsMobileAdsVisible = false;
                }
            }
            else IsMobileAdsVisible = IsDesktopAdsVisible = false;
        }
    }
}
