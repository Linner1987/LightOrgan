using Android.Content;
using Android.OS;
using Android.Preferences;
using LightOrganApp.Droid.Utils;
using Android.Text;

namespace LightOrganApp.Droid.UI
{
    public class SettingsFragment: PreferenceFragment, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            AddPreferencesFromResource(Resource.Xml.preferences);

            var sharedPreferences = PreferenceManager.SharedPreferences;
            OnSharedPreferenceChanged(sharedPreferences, PreferencesHelper.KeyPrefRemoteDeviceHost);
            OnSharedPreferenceChanged(sharedPreferences, PreferencesHelper.KeyPrefRemoteDevicePort);
        }

        public override void OnResume()
        {
            base.OnResume();
            PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);
        }

        public override void OnPause()
        {
            PreferenceManager.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
            base.OnPause();
        }

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key == PreferencesHelper.KeyPrefRemoteDeviceHost)
            {
                var hostPref = FindPreference(key);

                var value = sharedPreferences.GetString(key, "");

                if (TextUtils.IsEmpty(value))
                    hostPref.SetSummary(Resource.String.pref_no_data);
                else
                    hostPref.Summary = value;
            }
            else if (key == PreferencesHelper.KeyPrefRemoteDevicePort)
            {
                var portPref = FindPreference(key);

                //int value = sharedPreferences.GetInt(key, 0);
                var value = sharedPreferences.GetString(key, "0");

                portPref.Summary = value; //.ToString();
            }
        }
    }
}