using Plugin.Permissions;
using LightOrganApp.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Permissions.Abstractions;

namespace LightOrganApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // This lookup NOT required for Windows platforms - the Culture will be automatically set
            if (Device.OS == TargetPlatform.iOS || Device.OS == TargetPlatform.Android)
            {
                // determine the correct, supported .NET culture
                var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
                Resx.AppResources.Culture = ci; // set the RESX for resource localization
                DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods
            }

            MainPage = new NavigationPage(new MainPage());
        }

        protected override async void OnStart()
        {
            await CheckPermissions();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override async void OnResume()
        {
            await CheckPermissions();
        }

        private async Task CheckPermissions()
        {
            try
            {
                if (Device.OS == TargetPlatform.Android)
                {
                    var permissionsList = new List<Permission>();

                    var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);

                    if (status != PermissionStatus.Granted)
                        permissionsList.Add(Permission.Storage);

                    status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Microphone);

                    if (status != PermissionStatus.Granted)
                        permissionsList.Add(Permission.Microphone);

                    if (permissionsList.Count > 0)
                    {
                        var results = await CrossPermissions.Current.RequestPermissionsAsync(permissionsList.ToArray());

                        if(results.Any(p => p.Value != PermissionStatus.Granted))
                            await MainPage.DisplayAlert(AppResources.Permissions, AppResources.NotAllPermissionsMsg, "OK");
                    }
                }
            }
            catch (Exception ex)
            {                
            }
        }
    }
}
