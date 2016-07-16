using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using LightOrganApp.Droid.UI;

namespace LightOrganApp.Droid
{
    [Activity(Label = "@string/file_list_activity_name", Theme = "@style/AppTheme.NoActionBar")]
    public class FileListActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_file_list);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            MenuInflater.Inflate(Resource.Menu.menu_file_list, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.

            switch (item.ItemId)
            {
                case Resource.Id.action_search:                    

                    return true;

                case Resource.Id.action_settings:                   

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}