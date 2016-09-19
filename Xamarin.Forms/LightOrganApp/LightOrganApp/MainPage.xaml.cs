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
        private double width = 0;
        private double height = 0;

        public MainPage()
        {
            InitializeComponent();            

            if (Device.OS == TargetPlatform.Android)
            {
                var toolbarItem = new ToolbarItem(AppResources.ActionSettings, null, () => { }, ToolbarItemOrder.Secondary, 0);
                ToolbarItems.Add(toolbarItem);
            }

            Title.Text = "Gang Albanii - Napad na bank";
            Artist.Text = "<unknown>";      
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
                this.width = width;
                this.height = height;
                if (width > height)
                {
                    lightsStack.Orientation = StackOrientation.Horizontal;
                }
                else
                {
                    lightsStack.Orientation = StackOrientation.Vertical;
                }
            }
        }

        async void OnMediaFilesClicked(object sender, EventArgs e)
        {
            var fileListPage = new FileListPage();
                 
            await Navigation.PushAsync(fileListPage);
        }
    }
}
