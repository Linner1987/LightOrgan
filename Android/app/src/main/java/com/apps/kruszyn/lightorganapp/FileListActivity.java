package com.apps.kruszyn.lightorganapp;


import android.content.Intent;
import android.support.annotation.NonNull;
import android.support.v4.media.MediaBrowserCompat;
import android.support.v4.media.MediaMetadataCompat;
import android.support.v4.media.session.MediaControllerCompat;
import android.support.v4.media.session.PlaybackStateCompat;
import android.support.v4.view.MenuItemCompat;
import android.os.Bundle;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.SearchView;
import android.support.v7.widget.Toolbar;
import android.text.TextUtils;
import android.text.format.DateUtils;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import com.apps.kruszyn.lightorganapp.ui.BaseActivity;
import com.apps.kruszyn.lightorganapp.utils.LogHelper;

import java.util.ArrayList;
import java.util.List;

public class FileListActivity extends BaseActivity implements SearchView.OnQueryTextListener {

    private static final String TAG = LogHelper.makeLogTag(FileListActivity.class);

    static final String QUERY_STRING = "queryString";
    static final String SEARCH_OPEN = "searchOpen";
    private String searchText = null;
    private String queryToSave = null;
    private boolean searchOpen = false;
    private SearchView mSearchView;

    private RecyclerView mRecyclerView;
    private List<MediaBrowserCompat.MediaItem> mModel;
    private SimpleItemRecyclerViewAdapter mAdapter;
    private RecyclerView.LayoutManager mLayoutManager;


    private String mMediaId;

    // Receive callbacks from the MediaController. Here we update our state such as which queue
    // is being shown, the current title and description and the PlaybackState.
    private final MediaControllerCompat.Callback mMediaControllerCallback =
            new MediaControllerCompat.Callback() {
                @Override
                public void onMetadataChanged(MediaMetadataCompat metadata) {
                    super.onMetadataChanged(metadata);
                    if (metadata == null) {
                        return;
                    }
                    LogHelper.d(TAG, "Received metadata change to media ",
                            metadata.getDescription().getMediaId());
                    mAdapter.notifyDataSetChanged();
                }

                @Override
                public void onPlaybackStateChanged(@NonNull PlaybackStateCompat state) {
                    super.onPlaybackStateChanged(state);
                    LogHelper.d(TAG, "Received state change: ", state);
                    mAdapter.notifyDataSetChanged();
                }
            };

    private final MediaBrowserCompat.SubscriptionCallback mSubscriptionCallback =
            new MediaBrowserCompat.SubscriptionCallback() {
                @Override
                public void onChildrenLoaded(@NonNull String parentId,
                                             @NonNull List<MediaBrowserCompat.MediaItem> children) {
                    try {
                        mModel = children;
                        searchFiles();
                    } catch (Throwable t) {
                        LogHelper.e(TAG, "Error on childrenloaded", t);
                    }
                }

                @Override
                public void onError(@NonNull String id) {
                    LogHelper.e(TAG, "browse subscription onError, id=" + id);
                }
            };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_file_list);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        mRecyclerView = (RecyclerView) findViewById(R.id.item_list);

        mRecyclerView.setHasFixedSize(true);

        mLayoutManager = new LinearLayoutManager(this);
        mRecyclerView.setLayoutManager(mLayoutManager);

        mModel = new ArrayList<>();
        mAdapter = new SimpleItemRecyclerViewAdapter(mModel);
        mRecyclerView.setAdapter(mAdapter);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_file_list, menu);

        final MenuItem item = menu.findItem(R.id.action_search);
        mSearchView = (SearchView) MenuItemCompat.getActionView(item);
        mSearchView.setMaxWidth(Integer.MAX_VALUE);
        mSearchView.setQueryHint(getResources().getString(R.string.search_songs));
        mSearchView.setOnQueryTextListener(this);

        MenuItemCompat.setOnActionExpandListener(item,
                new MenuItemCompat.OnActionExpandListener() {
                    @Override
                    public boolean onMenuItemActionCollapse(MenuItem item) {

                        searchOpen = false;

                        mAdapter.setFilter(mModel);

                        return true;
                    }

                    @Override
                    public boolean onMenuItemActionExpand(MenuItem item) {

                        searchOpen = true;

                        if (searchText != null) {
                            queryToSave = new StringBuffer(searchText).toString();
                        }

                        return true;
                    }
                });

        if (searchOpen) {
            MenuItemCompat.expandActionView(item);

            if (!TextUtils.isEmpty(queryToSave))
                mSearchView.setQuery(queryToSave, false);

            mSearchView.clearFocus();
        }

        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.

        switch (item.getItemId()) {
            case R.id.action_search:

                return true;

            case R.id.action_settings:

                return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onStart() {
        super.onStart();

        // fetch browsing information to fill the listview:
        MediaBrowserCompat mediaBrowser = getMediaBrowser();

        LogHelper.d(TAG, "fragment.onStart, mediaId=", mMediaId,
                "  onConnected=" + mediaBrowser.isConnected());

        if (mediaBrowser.isConnected()) {
            onConnected();
        }
    }

    @Override
    public void onStop() {
        super.onStop();
        MediaBrowserCompat mediaBrowser = getMediaBrowser();
        if (mediaBrowser != null && mediaBrowser.isConnected() && mMediaId != null) {
            mediaBrowser.unsubscribe(mMediaId);
        }

        MediaControllerCompat controller = getSupportMediaController();
        if (controller != null) {
            controller.unregisterCallback(mMediaControllerCallback);
        }
    }

    public void onConnected() {

        mMediaId = getMediaBrowser().getRoot();

        // Unsubscribing before subscribing is required if this mediaId already has a subscriber
        // on this MediaBrowser instance. Subscribing to an already subscribed mediaId will replace
        // the callback, but won't trigger the initial callback.onChildrenLoaded.
        //
        // This is temporary: A bug is being fixed that will make subscribe
        // consistently call onChildrenLoaded initially, no matter if it is replacing an existing
        // subscriber or not. Currently this only happens if the mediaID has no previous
        // subscriber or if the media content changes on the service side, so we need to
        // unsubscribe first.
        getMediaBrowser().unsubscribe(mMediaId);

        getMediaBrowser().subscribe(mMediaId, mSubscriptionCallback);

        // Add MediaController callback so we can redraw the list when metadata changes:
        MediaControllerCompat controller = getSupportMediaController();
        if (controller != null) {
            controller.registerCallback(mMediaControllerCallback);
        }
    }



    @Override
    public boolean onQueryTextSubmit(String query) {
        return false;
    }

    @Override
    public boolean onQueryTextChange(String newText) {

        searchText = newText;

        searchFiles();

        return true;
    }

    @Override
    public void onSaveInstanceState(Bundle savedInstanceState) {

        savedInstanceState.putBoolean(SEARCH_OPEN, searchOpen);
        savedInstanceState.putString(QUERY_STRING, searchText);

        super.onSaveInstanceState(savedInstanceState);
    }

    public void onRestoreInstanceState(Bundle savedInstanceState) {

        super.onRestoreInstanceState(savedInstanceState);

        searchOpen = savedInstanceState.getBoolean(SEARCH_OPEN);
        searchText = savedInstanceState.getString(QUERY_STRING);
    }

    private List<MediaBrowserCompat.MediaItem> filter(List<MediaBrowserCompat.MediaItem> items, String query) {

        if (TextUtils.isEmpty(query))
            return items;

        query = query.trim().toLowerCase();

        final List<MediaBrowserCompat.MediaItem> filteredList = new ArrayList<>();
        for (MediaBrowserCompat.MediaItem item : items) {
            final String text1 = item.getDescription().getTitle().toString().trim().toLowerCase();
            final String text2 = item.getDescription().getSubtitle().toString().trim().toLowerCase();
            if (text1.contains(query) || text2.contains(query)) {
               filteredList.add(item);
            }
        }
        return filteredList;
    }

    private void searchFiles() {
        final List<MediaBrowserCompat.MediaItem> filteredList = filter(mModel, searchText);
        mAdapter.setFilter(filteredList);
    }

    @Override
    protected void onMediaControllerConnected() {
        onConnected();
    }

    public void onMediaItemSelected(MediaBrowserCompat.MediaItem item) {

        if (item.isPlayable()) {
            getSupportMediaController().getTransportControls()
                    .playFromMediaId(item.getMediaId(), null);

            Intent intent = new Intent(this, MainActivity.class);
            startActivity(intent);
        }
    }

    public class SimpleItemRecyclerViewAdapter
            extends RecyclerView.Adapter<SimpleItemRecyclerViewAdapter.ViewHolder> {

        private List<MediaBrowserCompat.MediaItem> mValues;

        public SimpleItemRecyclerViewAdapter(List<MediaBrowserCompat.MediaItem> items) {
            mValues = items;
        }

        @Override
        public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
            View view = LayoutInflater.from(parent.getContext())
                    .inflate(R.layout.file_list_item, parent, false);
            return new ViewHolder(view);
        }

        @Override
        public void onBindViewHolder(final ViewHolder holder, int position) {
            holder.mItem = mValues.get(position);
            holder.mTitleView.setText(mValues.get(position).getDescription().getTitle());
            holder.mArtistView.setText(mValues.get(position).getDescription().getSubtitle());
            holder.mDurationView.setText(DateUtils.formatElapsedTime(mValues.get(position).getDescription()
                    .getExtras().getLong(MediaMetadataCompat.METADATA_KEY_DURATION) / 1000));

            holder.mView.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {

                    MediaBrowserCompat.MediaItem item = holder.mItem;

                    if (item != null) {
                        onMediaItemSelected(item);
                    }
                }
            });
        }

        @Override
        public int getItemCount() {
            return mValues.size();
        }

        public void setFilter(List<MediaBrowserCompat.MediaItem> items){
            mValues = items;
            notifyDataSetChanged();
        }

        public class ViewHolder extends RecyclerView.ViewHolder {
            public final View mView;
            public final TextView mTitleView;
            public final TextView mArtistView;
            public final TextView mDurationView;
            public MediaBrowserCompat.MediaItem mItem;

            public ViewHolder(View view) {
                super(view);
                mView = view;
                mTitleView = (TextView) view.findViewById(R.id.title);
                mArtistView = (TextView) view.findViewById(R.id.artist);
                mDurationView = (TextView) view.findViewById(R.id.duration);
            }
        }
    }
}
