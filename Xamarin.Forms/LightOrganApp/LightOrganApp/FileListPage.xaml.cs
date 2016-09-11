using LightOrganApp.Model;
using System.Collections.Generic;

using Xamarin.Forms;

namespace LightOrganApp
{
    public partial class FileListPage : ContentPage
    {
        public FileListPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


            var items = new List<MediaItem>();

            for (int i = 0; i < 15; i++)
                items.Add(new MediaItem($"Kawa {i}", $"Gang {i}", i * 1000));

            listView.ItemsSource = items;
        }       
    }
}
