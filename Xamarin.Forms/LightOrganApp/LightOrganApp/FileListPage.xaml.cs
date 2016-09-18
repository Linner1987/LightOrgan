using LightOrganApp.Model;
using LightOrganApp.Services;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;

namespace LightOrganApp
{
    public partial class FileListPage : ContentPage
    {
        private string searchText = null;

        List<MediaItem> allMediaItems;  
               

        public FileListPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            allMediaItems = DependencyService.Get<IMusicService>().GetItems().ToList();
            SearchFiles();                                       
        }

        private List<MediaItem> Filter(List<MediaItem> items, string query)
        {
            if (string.IsNullOrEmpty(query))
                return items;

            query = query.Trim().ToLower();

            var filteredList = new List<MediaItem>();
            foreach (var item in items)
            {
                var text1 = item.Title.ToString().Trim().ToLower();
                var text2 = item.Artist.ToString().Trim().ToLower();
                if (text1.Contains(query) || text2.Contains(query))
                {
                    filteredList.Add(item);
                }
            }
            return filteredList;
        }

        private void SearchFiles()
        {
            var filteredList = Filter(allMediaItems, searchText);
            listView.ItemsSource = filteredList;
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var mainPage = new MainPage();

            await Navigation.PushAsync(mainPage);
        }

        private void listView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            listView.SelectedItem = null;
        }

        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            searchText = e.NewTextValue;

            SearchFiles();
        }       
    }
}
