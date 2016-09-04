using LightOrganApp.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LightOrganApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            if (Device.OS == TargetPlatform.Android)
            {
                var toolbarItem = new ToolbarItem(AppResources.ActionSettings, null, () => { }, ToolbarItemOrder.Secondary, 0);
                ToolbarItems.Add(toolbarItem);
            }
        }

        async void OnMediaFilesClicked(object sender, EventArgs e)
        {
            var fileListPage = new FileListPage();
                 
            await Navigation.PushAsync(fileListPage);
        }
    }
}
