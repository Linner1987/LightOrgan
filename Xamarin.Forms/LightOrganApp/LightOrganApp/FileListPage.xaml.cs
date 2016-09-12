using LightOrganApp.Services;

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

            var items = DependencyService.Get<IMusicService>().GetItems();         

            listView.ItemsSource = items;                          
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
    }
}
