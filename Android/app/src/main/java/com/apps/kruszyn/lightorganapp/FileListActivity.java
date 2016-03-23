package com.apps.kruszyn.lightorganapp;

import android.content.Context;
import android.content.Intent;
import android.support.v4.view.MenuItemCompat;
import android.support.v7.app.AppCompatActivity;
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
import android.widget.AutoCompleteTextView;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.List;

public class FileListActivity extends AppCompatActivity implements SearchView.OnQueryTextListener {

    static final String QUERY_STRING = "queryString";
    static final String SEARCH_OPEN = "searchOpen";
    private String searchText = null;
    private String queryToSave = null;
    private boolean searchOpen = false;
    private SearchView mSearchView;

    private RecyclerView mRecyclerView;
    private List<MediaFileItem> mModel;
    private SimpleItemRecyclerViewAdapter mAdapter;
    private RecyclerView.LayoutManager mLayoutManager;

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

        mModel = SampleData.MEDIA_FILE_ITEMS;
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
        mSearchView.setQueryHint("Search songs");
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
    public boolean onQueryTextSubmit(String query) {
        return false;
    }

    @Override
    public boolean onQueryTextChange(String newText) {

        searchText = newText;

        final List<MediaFileItem> filteredList = filter(mModel, newText);
        mAdapter.setFilter(filteredList);

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

    private List<MediaFileItem> filter(List<MediaFileItem> items, String query) {
        query = query.trim().toLowerCase();

        final List<MediaFileItem> filteredList = new ArrayList<>();
        for (MediaFileItem item : items) {
            final String text1 = item.title.trim().toLowerCase();
            final String text2 = item.artist.trim().toLowerCase();
            if (text1.contains(query) || text2.contains(query)) {
                filteredList.add(item);
            }
        }
        return filteredList;
    }


    public class SimpleItemRecyclerViewAdapter
            extends RecyclerView.Adapter<SimpleItemRecyclerViewAdapter.ViewHolder> {

        private List<MediaFileItem> mValues;

        public SimpleItemRecyclerViewAdapter(List<MediaFileItem> items) {
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
            holder.mTitleView.setText(mValues.get(position).title);
            holder.mArtistView.setText(mValues.get(position).artist);
            holder.mDurationView.setText(DateUtils.formatElapsedTime(mValues.get(position).duration / 1000));

            holder.mView.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {

                    Context context = v.getContext();

                    //to do
                }
            });
        }

        @Override
        public int getItemCount() {
            return mValues.size();
        }

        public void setFilter(List<MediaFileItem> items){
            mValues = items;
            notifyDataSetChanged();
        }

        public class ViewHolder extends RecyclerView.ViewHolder {
            public final View mView;
            public final TextView mTitleView;
            public final TextView mArtistView;
            public final TextView mDurationView;
            public MediaFileItem mItem;

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
