package com.apps.kruszyn.lightorganapp;

import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.os.IBinder;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;

public class MainActivity extends AppCompatActivity implements View.OnClickListener {

    Button startButton;
    Button stopButton;
    Button pauseButton;

    Intent serviceIntent;
    String filePath;
    private BackgroundAudioService baService;

    private ServiceConnection serviceConnection = new ServiceConnection() {
        public void onServiceConnected(ComponentName className, IBinder baBinder) {
            baService = ((BackgroundAudioService.BackgroundAudioServiceBinder)baBinder).getService();
        }

        public void onServiceDisconnected(ComponentName className) {
            baService = null;
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        startButton = (Button) this.findViewById(R.id.StartButton);
        stopButton = (Button) this.findViewById(R.id.StopButton);
        pauseButton = (Button) this.findViewById(R.id.PauseButton);
        startButton.setOnClickListener(this);
        stopButton.setOnClickListener(this);
        pauseButton.setOnClickListener(this);

        serviceIntent = new Intent(this, BackgroundAudioService.class);

        Intent intent = getIntent();
        filePath = intent.getStringExtra(MusicHelper.MEDIA_FILE_PATH);

        if (filePath != null)
            serviceIntent.putExtra(MusicHelper.MEDIA_FILE_PATH, filePath);
    }

    @Override
     protected void onStart() {

        super.onStart();

        //only for demo
        if (filePath != null) {
            startService(serviceIntent);
            bindService(serviceIntent, serviceConnection, Context.BIND_AUTO_CREATE);
        }
    }

    @Override
    protected void onDestroy() {

        super.onDestroy();

        if (baService != null)
            unbindService(serviceConnection);
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

    @Override
    public void onClick(View v) {
        if (v == startButton) {
            startService(serviceIntent);
            bindService(serviceIntent, serviceConnection, Context.BIND_AUTO_CREATE);
        } else if (v == stopButton) {
            stopService(serviceIntent);
            unbindService(serviceConnection);
        }
        else if (v == pauseButton) {
            baService.pause();
        }
    }

}
