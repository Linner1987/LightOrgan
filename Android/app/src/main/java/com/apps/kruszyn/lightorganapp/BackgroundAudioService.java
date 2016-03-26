package com.apps.kruszyn.lightorganapp;

import android.app.Service;
import android.content.Intent;
import android.media.MediaPlayer;
import android.net.Uri;
import android.os.Binder;
import android.os.IBinder;
import android.support.annotation.Nullable;

/**
 * Created by nazyw on 3/25/2016.
 */
public class  BackgroundAudioService  extends Service implements MediaPlayer.OnCompletionListener {

    MediaPlayer mediaPlayer;
    String filePath;

    public class BackgroundAudioServiceBinder extends Binder {
        BackgroundAudioService getService() {
            return BackgroundAudioService.this; }
    }

    private final IBinder basBinder = new BackgroundAudioServiceBinder();


    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return basBinder;
    }

    @Override
    public void onCreate() {
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {

        if (mediaPlayer == null)
        {
            if (filePath == null && intent != null)
                filePath = intent.getStringExtra(MusicHelper.MEDIA_FILE_PATH);

            if (filePath == null)
                return START_STICKY;

            mediaPlayer = MediaPlayer.create(this, Uri.parse(filePath));
            mediaPlayer.setOnCompletionListener(this);
        }

        if (!mediaPlayer.isPlaying()) {
            mediaPlayer.start();
        }

        return START_STICKY;
    }

    public void onDestroy() {
        filePath = null;
        if (mediaPlayer != null) {

            if (mediaPlayer.isPlaying())
                mediaPlayer.stop();

            mediaPlayer.release();
            mediaPlayer = null;
        }
    }

    public void pause() {
        if (mediaPlayer != null && mediaPlayer.isPlaying()) {
            mediaPlayer.pause();
        }
    }

    @Override
    public void onCompletion(MediaPlayer mp) {
        stopSelf();
    }
}
