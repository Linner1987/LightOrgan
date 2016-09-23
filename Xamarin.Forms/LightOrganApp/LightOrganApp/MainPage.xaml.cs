﻿using LightOrganApp.Model;
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

        private LightsData lightsData;

        public MainPage()
        {
            InitializeComponent();

            lightsData = new LightsData { BassColor = Color.FromHex("#d50000"), MidColor = Color.FromHex("#ffab00"), TrebleColor = Color.FromHex("#6200ea") };
            lights.BindingContext = lightsData;        

            if (Device.OS == TargetPlatform.Android)
            {
                var toolbarItem = new ToolbarItem(AppResources.ActionSettings, null, () => { }, ToolbarItemOrder.Secondary, 0);
                ToolbarItems.Add(toolbarItem);
            }

            Title.Text = "Gang Albanii - Napad na bank";
            Artist.Text = "<unknown>";

            //test
            //OnLightOrganDataUpdated(0.1f, 1, 0.1f);
        }

        private void OnLightOrganDataUpdated(float bassLevel, float midLevel, float trebleLevel)
        {
            lightsData.BassColor = GetColorWithAlpha(lightsData.BassColor, bassLevel);
            lightsData.MidColor = GetColorWithAlpha(lightsData.MidColor, midLevel);
            lightsData.TrebleColor = GetColorWithAlpha(lightsData.TrebleColor, trebleLevel);
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
                    lights.ContentTemplate = Resources["HorizontalLights"] as DataTemplate;
                }
                else
                {
                    lights.ContentTemplate = Resources["VerticalLights"] as DataTemplate;                                 
                }
            }
        }

        async void OnMediaFilesClicked(object sender, EventArgs e)
        {
            var fileListPage = new FileListPage();
                 
            await Navigation.PushAsync(fileListPage);
        }

        private static Color GetColorWithAlpha(Color color, float ratio)
        {            
            var newColor = new Color(color.R, color.G, color.B, ratio);

            return newColor;
        }
    }
}
