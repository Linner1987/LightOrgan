
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using LightOrganApp.Droid.UI;

namespace LightOrganApp.Droid
{
    [Activity(Label = "@string/settings_activity_name")]
    public class SettingsActivity: AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetupActionBar();

            FragmentManager.BeginTransaction()
                    .Replace(Android.Resource.Id.Content, new SettingsFragment())
                    .Commit();
        }

        private void SetupActionBar()
        {
            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);            
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Android.Resource.Id.Home)
            {
                StartActivity(new Intent(this, typeof(MainActivity)));
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }    
}