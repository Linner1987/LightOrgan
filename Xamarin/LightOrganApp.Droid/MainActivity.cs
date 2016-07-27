using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using LightOrganApp.Droid.UI;
using System;

namespace LightOrganApp.Droid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme="@style/AppTheme.NoActionBar")]
    public class MainActivity : BaseActivity
    {
        readonly BroadcastReceiver mLightOrganReceiver = new BroadcastReceiver();

        class BroadcastReceiver : Android.Content.BroadcastReceiver
        {
            public Action<Context, Intent> OnReceiveImpl { get; set; }

            public override void OnReceive(Context context, Intent intent)
            {
                OnReceiveImpl(context, intent);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);            
            SetContentView(Resource.Layout.activity_main);
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);            
            SetSupportActionBar(toolbar);

            mLightOrganReceiver.OnReceiveImpl = (context, intent) =>
            {
                float b = intent.GetFloatExtra(MusicService.BassLevel, 0);
                float m = intent.GetFloatExtra(MusicService.MidLevel, 0);
                float t = intent.GetFloatExtra(MusicService.TrebleLevel, 0);

                SetLight(Resource.Id.bass_light, b);
                SetLight(Resource.Id.mid_light, m);
                SetLight(Resource.Id.treble_light, t);

                //SetTitle("b=" + b + " m=" + m + " t=" + t);
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            IntentFilter intentFilter = new IntentFilter(MusicService.ActionLightOrganDataChanged);
            LocalBroadcastManager.GetInstance(this).RegisterReceiver(mLightOrganReceiver, intentFilter);
        }

        protected override void OnPause()
        {
            base.OnPause();

            LocalBroadcastManager.GetInstance(this).UnregisterReceiver(mLightOrganReceiver);
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

                    var intent = new Intent(this, typeof(FileListActivity));
                    StartActivity(intent);

                    return true;

                case Resource.Id.action_settings:

                    //Intent intent2 = new Intent(this, SettingsActivity.class);
                    //startActivity(intent2);

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void SetLight(int id, float ratio)
        {
            var light = FindViewById<CircleView>(id);
            light.CircleColor = GetColorWithAlpha(light.CircleColor, ratio);
        }

        private static Color GetColorWithAlpha(Color color, float ratio)
        {            
            int alpha = (int) Math.Round(255 * ratio);           
            var newColor = Color.Argb(alpha, color.R, color.G, color.B);

            return newColor;
        }
    }
}

