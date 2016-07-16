using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using LightOrganApp.Droid.UI;

namespace LightOrganApp.Droid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme="@style/AppTheme.NoActionBar")]
    public class MainActivity : BaseActivity
    {   

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);            
            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);            
            SetSupportActionBar(toolbar);
        }
        
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

            return true;
        }
       
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.

            switch (item.ItemId)
            {
                case Resource.Id.action_media_files:

                    //var intent = new Intent(this, FileListActivity.class);
                    //StartActivity(intent);

                    return true;

                case Resource.Id.action_settings:

                    //Intent intent2 = new Intent(this, SettingsActivity.class);
                    //startActivity(intent2);

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}

