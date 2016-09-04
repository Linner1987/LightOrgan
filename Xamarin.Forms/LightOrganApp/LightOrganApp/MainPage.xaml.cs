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
        }

        async void OnMediaFilesClicked(object sender, EventArgs e)
        {
            var fileListPage = new FileListPage();
                 
            await Navigation.PushAsync(fileListPage);
        }
    }
}
