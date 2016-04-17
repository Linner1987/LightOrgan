/*
 * Copyright (C) 2014 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.apps.kruszyn.lightorganapp.ui;

import android.Manifest;
import android.app.ActivityManager;
import android.content.ComponentName;
import android.content.pm.PackageManager;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.os.Bundle;
import android.os.RemoteException;
import android.support.annotation.NonNull;
import android.support.v4.app.ActivityCompat;
import android.support.v4.media.MediaBrowserCompat;
import android.support.v4.media.MediaMetadataCompat;
import android.support.v4.media.session.MediaControllerCompat;
import android.support.v4.media.session.MediaSessionCompat;
import android.support.v4.media.session.PlaybackStateCompat;
import android.support.v7.app.AppCompatActivity;
import android.text.TextUtils;
import android.widget.Toast;

import com.apps.kruszyn.lightorganapp.MusicService;
import com.apps.kruszyn.lightorganapp.R;
import com.apps.kruszyn.lightorganapp.utils.LogHelper;
import com.apps.kruszyn.lightorganapp.utils.ResourceHelper;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Base activity for activities that need to show a playback control fragment when media is playing.
 */
public abstract class BaseActivity extends AppCompatActivity implements MediaBrowserProvider {

    private static final String TAG = LogHelper.makeLogTag(BaseActivity.class);

    private MediaBrowserCompat mMediaBrowser;
    private PlaybackControlsFragment mControlsFragment;

    final private int REQUEST_CODE_ASK_PERMISSIONS = 123;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        LogHelper.d(TAG, "Activity onCreate");

        // Connect a media browser just to get the media session token. There are other ways
        // this can be done, for example by sharing the session token directly.
        mMediaBrowser = new MediaBrowserCompat(this,
            new ComponentName(this, MusicService.class), mConnectionCallback, null);
    }

    @Override
    protected void onStart() {
        super.onStart();
        LogHelper.d(TAG, "Activity onStart");

        mControlsFragment = (PlaybackControlsFragment) getFragmentManager()
            .findFragmentById(R.id.fragment_playback_controls);
        if (mControlsFragment == null) {
            throw new IllegalStateException("Mising fragment with id 'controls'. Cannot continue.");
        }

        hidePlaybackControls();

        final List<String> permissionsList = new ArrayList<String>();

        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED)
            mMediaBrowser.connect();
        else
            permissionsList.add(Manifest.permission.READ_EXTERNAL_STORAGE);

        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.RECORD_AUDIO) != PackageManager.PERMISSION_GRANTED)
            permissionsList.add(Manifest.permission.RECORD_AUDIO);

        if (permissionsList.size() > 0) {

            ActivityCompat.requestPermissions(this,
                    permissionsList.toArray(new String[permissionsList.size()]),
                    REQUEST_CODE_ASK_PERMISSIONS);
            return;
        }
    }

    @Override
    protected void onStop() {
        super.onStop();
        LogHelper.d(TAG, "Activity onStop");
        if (getSupportMediaController() != null) {
            getSupportMediaController().unregisterCallback(mMediaControllerCallback);
        }
        mMediaBrowser.disconnect();
    }

    @Override
    public MediaBrowserCompat getMediaBrowser() {
        return mMediaBrowser;
    }

    protected void onMediaControllerConnected() {
        // empty implementation, can be overridden by clients.
    }

    protected void showPlaybackControls() {
        LogHelper.d(TAG, "showPlaybackControls");

        getFragmentManager().beginTransaction()
            .setCustomAnimations(
                R.animator.slide_in_from_bottom, R.animator.slide_out_to_bottom,
                R.animator.slide_in_from_bottom, R.animator.slide_out_to_bottom)
            .show(mControlsFragment)
            .commit();
    }

    protected void hidePlaybackControls() {
        LogHelper.d(TAG, "hidePlaybackControls");
        getFragmentManager().beginTransaction()
            .hide(mControlsFragment)
            .commit();
    }

    /**
     * Check if the MediaSession is active and in a "playback-able" state
     * (not NONE and not STOPPED).
     *
     * @return true if the MediaSession's state requires playback controls to be visible.
     */
    protected boolean shouldShowControls() {
        MediaControllerCompat mediaController = getSupportMediaController();
        if (mediaController == null ||
            mediaController.getMetadata() == null ||
            mediaController.getPlaybackState() == null) {
            return false;
        }
        switch (mediaController.getPlaybackState().getState()) {
            case PlaybackStateCompat.STATE_ERROR:
            case PlaybackStateCompat.STATE_NONE:
            case PlaybackStateCompat.STATE_STOPPED:
                return false;
            default:
                return true;
        }
    }

    private void connectToSession(MediaSessionCompat.Token token) throws RemoteException {
        MediaControllerCompat mediaController = new MediaControllerCompat(this, token);
        setSupportMediaController(mediaController);
        mediaController.registerCallback(mMediaControllerCallback);

        if (shouldShowControls()) {
            showPlaybackControls();
        } else {
            LogHelper.d(TAG, "connectionCallback.onConnected: " +
                "hiding controls because metadata is null");
            hidePlaybackControls();
        }

        if (mControlsFragment != null) {
            mControlsFragment.onConnected();
        }

        onMediaControllerConnected();
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        switch (requestCode) {
            case REQUEST_CODE_ASK_PERMISSIONS: {

                    Map<String, Integer> perms = new HashMap<String, Integer>();

                    perms.put(Manifest.permission.READ_EXTERNAL_STORAGE, PackageManager.PERMISSION_GRANTED);
                    perms.put(Manifest.permission.RECORD_AUDIO, PackageManager.PERMISSION_GRANTED);

                    boolean readExternalStorageRequested = false;
                    for (int i = 0; i < permissions.length; i++) {
                        perms.put(permissions[i], grantResults[i]);

                        if (TextUtils.equals(permissions[i],Manifest.permission.READ_EXTERNAL_STORAGE))
                            readExternalStorageRequested = true;
                    }

                    if (readExternalStorageRequested) {
                        if (perms.get(Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED)
                            mMediaBrowser.connect();
                        else
                            finish();
                    }

                    if (perms.get(Manifest.permission.READ_EXTERNAL_STORAGE) == PackageManager.PERMISSION_GRANTED
                            && perms.get(Manifest.permission.RECORD_AUDIO) == PackageManager.PERMISSION_GRANTED) {

                    } else {
                        Toast.makeText(this, R.string.not_all_permissions_msg, Toast.LENGTH_SHORT)
                                .show();
                    }
                }
                break;
            default:
                super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    // Callback that ensures that we are showing the controls
    private final MediaControllerCompat.Callback mMediaControllerCallback =
        new MediaControllerCompat.Callback() {
            @Override
            public void onPlaybackStateChanged(@NonNull PlaybackStateCompat state) {
                if (shouldShowControls()) {
                    showPlaybackControls();
                } else {
                    LogHelper.d(TAG, "mediaControllerCallback.onPlaybackStateChanged: " +
                            "hiding controls because state is ", state.getState());
                    hidePlaybackControls();
                }
            }

            @Override
            public void onMetadataChanged(MediaMetadataCompat metadata) {
                if (shouldShowControls()) {
                    showPlaybackControls();
                } else {
                    LogHelper.d(TAG, "mediaControllerCallback.onMetadataChanged: " +
                        "hiding controls because metadata is null");
                    hidePlaybackControls();
                }
            }
        };

    private final MediaBrowserCompat.ConnectionCallback mConnectionCallback =
        new MediaBrowserCompat.ConnectionCallback() {
            @Override
            public void onConnected() {
                LogHelper.d(TAG, "onConnected");
                try {
                    connectToSession(mMediaBrowser.getSessionToken());
                } catch (RemoteException e) {
                    LogHelper.e(TAG, e, "could not connect media controller");
                    hidePlaybackControls();
                }
            }
        };

}
