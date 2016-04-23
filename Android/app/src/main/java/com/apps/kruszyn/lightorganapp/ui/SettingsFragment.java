package com.apps.kruszyn.lightorganapp.ui;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.Preference;
import android.preference.PreferenceFragment;
import android.preference.PreferenceManager;
import android.text.TextUtils;

import com.apps.kruszyn.lightorganapp.R;

/**
 * Created by nazyw on 4/23/2016.
 */

public class SettingsFragment extends PreferenceFragment implements SharedPreferences.OnSharedPreferenceChangeListener {

    public static final String KEY_PREF_REMOTE_DEVICE_HOST = "pref_remote_device_host";
    public static final String KEY_PREF_REMOTE_DEVICE_PORT = "pref_remote_device_port";

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        addPreferencesFromResource(R.xml.preferences);

        SharedPreferences sharedPreferences = getPreferenceManager().getSharedPreferences();
        onSharedPreferenceChanged(sharedPreferences, KEY_PREF_REMOTE_DEVICE_HOST);
        onSharedPreferenceChanged(sharedPreferences, KEY_PREF_REMOTE_DEVICE_PORT);
    }

    @Override
    public void onResume() {
        super.onResume();
        getPreferenceManager().getSharedPreferences().registerOnSharedPreferenceChangeListener(this);
    }

    @Override
    public void onPause() {
        getPreferenceManager().getSharedPreferences().unregisterOnSharedPreferenceChangeListener(this);
        super.onPause();
    }

    @Override
    public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {

        if (key.equals(KEY_PREF_REMOTE_DEVICE_HOST)) {
            Preference hostPref = findPreference(key);

            String value = sharedPreferences.getString(key, "");

            if (TextUtils.isEmpty(value))
                hostPref.setSummary(R.string.pref_no_data);
            else
                hostPref.setSummary(value);
        }
        else if (key.equals(KEY_PREF_REMOTE_DEVICE_PORT)) {
            Preference portPref = findPreference(key);

            int value = sharedPreferences.getInt(key, 0);

            portPref.setSummary(Integer.toString(value));
        }
    }
}
