package com.apps.kruszyn.lightorganapp;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Color;
import android.os.Bundle;
import android.support.annotation.IdRes;
import android.support.v7.widget.Toolbar;
import android.support.v4.content.LocalBroadcastManager;
import android.view.Menu;
import android.view.MenuItem;

import com.apps.kruszyn.lightorganapp.ui.BaseActivity;
import com.apps.kruszyn.lightorganapp.ui.CircleView;


public class MainActivity extends BaseActivity {

    private final BroadcastReceiver mLightOrganReceiver = new BroadcastReceiver() {

        @Override
        public void onReceive(Context context, Intent intent) {

            float b = intent.getFloatExtra(MusicService.BASS_LEVEL,0);
            float m = intent.getFloatExtra(MusicService.MID_LEVEL,0);
            float t = intent.getFloatExtra(MusicService.TREBLE_LEVEL,0);

            setLight(R.id.bass_light, b);
            setLight(R.id.mid_light, m);
            setLight(R.id.treble_light, t);

            //setTitle("b=" + b + " m=" + m + " t=" + t);
        }
    };


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        //test
        setLight(R.id.bass_light, 0.05f);
        setLight(R.id.mid_light, 0.05f);
        setLight(R.id.treble_light, 0.05f);
    }

    @Override
    protected void onResume() {
        super.onResume();

        IntentFilter intentFilter = new IntentFilter(MusicService.ACTION_LIGHT_ORGAN_DATA_CHANGED);
        LocalBroadcastManager.getInstance(this).registerReceiver(mLightOrganReceiver, intentFilter);
    }

    @Override
    protected void onPause() {
        super.onPause();

        LocalBroadcastManager.getInstance(this).unregisterReceiver(mLightOrganReceiver);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);

        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.

        switch (item.getItemId()) {
            case R.id.action_media_files:

                Intent intent = new Intent(this, FileListActivity.class);
                startActivity(intent);

                return true;

            case R.id.action_settings:

                return true;
        }

        return super.onOptionsItemSelected(item);
    }

    private void setLight(@IdRes int id, float ratio)
    {
        CircleView light = (CircleView) findViewById(id);
        light.setCircleColor(getColorWithAlpha(light.getCircleColor(), ratio));
    }

    private static int getColorWithAlpha(int color, float ratio) {
        int newColor = 0;
        int alpha = Math.round(255 * ratio);
        int r = Color.red(color);
        int g = Color.green(color);
        int b = Color.blue(color);
        newColor = Color.argb(alpha, r, g, b);
        return newColor;
    }
}
