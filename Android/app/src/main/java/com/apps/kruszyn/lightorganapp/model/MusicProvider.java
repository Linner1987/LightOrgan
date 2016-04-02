
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

package com.apps.kruszyn.lightorganapp.model;

import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.res.Resources;
import android.database.Cursor;
import android.os.AsyncTask;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.provider.MediaStore;
import android.support.v4.app.ActivityCompat;
import android.support.v4.media.MediaBrowserCompat;
import android.support.v4.media.MediaDescriptionCompat;
import android.support.v4.media.MediaMetadataCompat;
import android.text.TextUtils;
import android.widget.Toast;

import com.apps.kruszyn.lightorganapp.utils.LogHelper;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;


/**
 * Simple data provider for music tracks. The actual metadata source is delegated to a
 * MusicProviderSource defined by a constructor argument of this class.
 */
public class MusicProvider {

    private static final String TAG = LogHelper.makeLogTag(MusicProvider.class);

    public static final String CUSTOM_METADATA_TRACK_SOURCE = "__SOURCE__";

    private final ConcurrentMap<String, MutableMediaMetadata> mMusicListById;

    enum State {
        NON_INITIALIZED, INITIALIZING, INITIALIZED
    }

    private volatile State mCurrentState = State.NON_INITIALIZED;

    public interface Callback {
        void onMusicCatalogReady(boolean success);
    }

    public MusicProvider() {
        mMusicListById = new ConcurrentHashMap<>();
    }

    /**
     * Get an iterator over a shuffled collection of all songs
     */
    public Iterable<MediaMetadataCompat> getShuffledMusic() {
        if (mCurrentState != State.INITIALIZED) {
            return Collections.emptyList();
        }
        List<MediaMetadataCompat> shuffled = new ArrayList<>(mMusicListById.size());
        for (MutableMediaMetadata mutableMetadata: mMusicListById.values()) {
            shuffled.add(mutableMetadata.metadata);
        }
        Collections.shuffle(shuffled);
        return shuffled;
    }

    public Iterable<MediaMetadataCompat> searchMusic(String query) {
        if (mCurrentState != State.INITIALIZED) {
            return Collections.emptyList();
        }

        ArrayList<MediaMetadataCompat> result = new ArrayList<>();
        query = query.toLowerCase();
        for (MutableMediaMetadata track : mMusicListById.values()) {
            if (track.metadata.getString(MediaMetadataCompat.METADATA_KEY_TITLE).toLowerCase().contains(query) ||
                    track.metadata.getString(MediaMetadataCompat.METADATA_KEY_ARTIST).toLowerCase().contains(query)) {
                result.add(track.metadata);
            }
        }
        return result;
    }

    /**
     * Return the MediaMetadataCompat for the given musicID.
     *
     * @param musicId The unique, non-hierarchical music ID.
     */
    public MediaMetadataCompat getMusic(String musicId) {
        return mMusicListById.containsKey(musicId) ? mMusicListById.get(musicId).metadata : null;
    }

    public synchronized void updateMusicArt(String musicId, Bitmap albumArt, Bitmap icon) {
        MediaMetadataCompat metadata = getMusic(musicId);
        metadata = new MediaMetadataCompat.Builder(metadata)

                // set high resolution bitmap in METADATA_KEY_ALBUM_ART. This is used, for
                // example, on the lockscreen background when the media session is active.
                .putBitmap(MediaMetadataCompat.METADATA_KEY_ALBUM_ART, albumArt)

                        // set small version of the album art in the DISPLAY_ICON. This is used on
                        // the MediaDescription and thus it should be small to be serialized if
                        // necessary
                .putBitmap(MediaMetadataCompat.METADATA_KEY_DISPLAY_ICON, icon)

                .build();

        MutableMediaMetadata mutableMetadata = mMusicListById.get(musicId);
        if (mutableMetadata == null) {
            throw new IllegalStateException("Unexpected error: Inconsistent data structures in " +
                    "MusicProvider");
        }

        mutableMetadata.metadata = metadata;
    }

    /**
     * Get the list of music tracks from a server and caches the track information
     * for future reference, keying tracks by musicId and grouping by genre.
     */
    public void retrieveMediaAsync(Context context, final Callback callback) {
        LogHelper.d(TAG, "retrieveMediaAsync called");
        if (mCurrentState == State.INITIALIZED) {
            if (callback != null) {
                // Nothing to do, execute callback immediately
                callback.onMusicCatalogReady(true);
            }
            return;
        }

        // Asynchronously load the music catalog in a separate thread
        new AsyncTask<Context, Void, State>() {
            @Override
            protected State doInBackground(Context... params) {
                retrieveMedia(params[0]);
                return mCurrentState;
            }

            @Override
            protected void onPostExecute(State current) {
                if (callback != null) {
                    callback.onMusicCatalogReady(current == State.INITIALIZED);
                }
            }
        }.execute(context);
    }

    private synchronized void retrieveMedia(Context context) {

        try {
            if (mCurrentState == State.NON_INITIALIZED) {
                mCurrentState = State.INITIALIZING;

                String[] projection = new String[] {
                        MediaStore.Audio.Media._ID,
                        MediaStore.Audio.Media.ARTIST,
                        MediaStore.Audio.Media.TITLE,
                        MediaStore.Audio.Media.DURATION,
                        MediaStore.Audio.Media.DATA,
                        MediaStore.Audio.Media.MIME_TYPE
                };

                String selection = MediaStore.Audio.Media.IS_MUSIC + "!= 0";
                String sortOrder = MediaStore.Audio.Media.DATE_ADDED + " DESC";

                Cursor cursor = context.getContentResolver().query(MediaStore.Audio.Media.EXTERNAL_CONTENT_URI,projection,selection,null,sortOrder);

                if (cursor != null && cursor.moveToFirst()) {
                    do {
                        int idColumn = cursor.getColumnIndex(MediaStore.Audio.Media._ID);
                        int artistColumn = cursor.getColumnIndex(MediaStore.Audio.Media.ARTIST);
                        int titleColumn = cursor.getColumnIndex(MediaStore.Audio.Media.TITLE);
                        int durationColumn = cursor.getColumnIndex(MediaStore.Audio.Media.DURATION);
                        int filePathIndex = cursor.getColumnIndexOrThrow(MediaStore.Audio.Media.DATA);

                        String id = Long.toString(cursor.getLong(idColumn));

                        MediaMetadataCompat item = new MediaMetadataCompat.Builder()
                                .putString(MediaMetadataCompat.METADATA_KEY_MEDIA_ID, id)
                                .putString(CUSTOM_METADATA_TRACK_SOURCE, cursor.getString(filePathIndex))
                                .putString(MediaMetadataCompat.METADATA_KEY_ARTIST, cursor.getString(artistColumn))
                                .putString(MediaMetadataCompat.METADATA_KEY_TITLE, cursor.getString(titleColumn))
                                .putLong(MediaMetadataCompat.METADATA_KEY_DURATION, cursor.getInt(durationColumn))
                                .build();

                        mMusicListById.put(id, new MutableMediaMetadata(id, item));

                    } while (cursor.moveToNext());
                }

                mCurrentState = State.INITIALIZED;
            }
        } finally {
            if (mCurrentState != State.INITIALIZED) {
                // Something bad happened, so we reset state to NON_INITIALIZED to allow
                // retries (eg if the network connection is temporary unavailable)
                mCurrentState = State.NON_INITIALIZED;
            }
        }
    }

    public List<MediaBrowserCompat.MediaItem> getChildren(String mediaId, Resources resources) {
        List<MediaBrowserCompat.MediaItem> mediaItems = new ArrayList<>();

        //zawsze lista (na razie bez kategorii)

        for (MutableMediaMetadata m : mMusicListById.values()) {
            mediaItems.add(createMediaItem(m.metadata));
        }

        return mediaItems;
    }

    private MediaBrowserCompat.MediaItem createMediaItem(MediaMetadataCompat metadata) {
        // Since mediaMetadata fields are immutable, we need to create a copy, so we
        // can set a hierarchy-aware mediaID. We will need to know the media hierarchy
        // when we get a onPlayFromMusicID call, so we can create the proper queue based
        // on where the music was selected from (by artist, by genre, random, etc)
        String id = metadata.getDescription().getMediaId();
        //MediaMetadataCompat copy = new MediaMetadataCompat.Builder(metadata)
        //        .putString(MediaMetadataCompat.METADATA_KEY_MEDIA_ID, id)
        //        .build();

        Bundle playExtras = new Bundle();
        playExtras.putLong(MediaMetadataCompat.METADATA_KEY_DURATION, metadata.getLong(MediaMetadataCompat.METADATA_KEY_DURATION));

        MediaDescriptionCompat desc =
                new MediaDescriptionCompat.Builder()
                        .setMediaId(id)
                        .setTitle(metadata.getString(MediaMetadataCompat.METADATA_KEY_TITLE))
                        .setSubtitle(metadata.getString(MediaMetadataCompat.METADATA_KEY_ARTIST))
                        .setExtras(playExtras)
                        .build();

        return new MediaBrowserCompat.MediaItem(desc /*copy.getDescription()*/,
                MediaBrowserCompat.MediaItem.FLAG_PLAYABLE);
    }

}

