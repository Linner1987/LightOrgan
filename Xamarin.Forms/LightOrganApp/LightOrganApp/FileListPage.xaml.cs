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
    }
}
