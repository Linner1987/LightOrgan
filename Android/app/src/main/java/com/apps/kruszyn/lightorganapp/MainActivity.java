package com.apps.kruszyn.lightorganapp;

import android.content.Intent;
import android.graphics.Color;
import android.os.Bundle;
import android.support.annotation.IdRes;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;

import com.apps.kruszyn.lightorganapp.ui.BaseActivity;
import com.apps.kruszyn.lightorganapp.ui.CircleView;


public class MainActivity extends BaseActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
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

                //test
                setLight(R.id.bass_light, 0.5f);
                setLight(R.id.mid_light, 0.5f);
                setLight(R.id.treble_light, 0.5f);

                return true;
        }

        return super.onOptionsItemSelected(item);
    }

    private void setLight(@IdRes int id, float ratio)
    {
        CircleView bassLight = (CircleView) findViewById(id);
        bassLight.setCircleColor(getColorWithAlpha(bassLight.getCircleColor(), ratio));
    }

    private static int getColorWithAlpha(int color, float ratio) {
        int newColor = 0;
        int alpha = Math.round(Color.alpha(color) * ratio);
        int r = Color.red(color);
        int g = Color.green(color);
        int b = Color.blue(color);
        newColor = Color.argb(alpha, r, g, b);
        return newColor;
    }
}
