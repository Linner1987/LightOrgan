using Android.App;
using Android.OS;
using Android.Support.V4.Media;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using LightOrganApp.Droid.UI;
using LightOrganApp.Droid.Utils;
using System.Collections.Generic;
using System;
using Android.Text.Format;
using Android.Content;
using Android.Provider;
using Android.Support.V4.View;
using Android.Runtime;
using Android.Text;

namespace LightOrganApp.Droid
{
    [Activity(Label = "@string/file_list_activity_name", Theme = "@style/AppTheme.NoActionBar")]
    public class FileListActivity : BaseActivity, Android.Support.V7.Widget.SearchView.IOnQueryTextListener
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(FileListActivity));

        public const string QueryString = "queryString";
        public const string SearchOpen = "searchOpen";
        private string searchText = null;
        private string queryToSave = null;
        private bool searchOpen = false;
        private Android.Support.V7.Widget.SearchView mSearchView;        

        private RecyclerView mRecyclerView;
        private List<MediaBrowserCompat.MediaItem> mModel;
        private SimpleItemRecyclerViewAdapter mAdapter;
        private RecyclerView.LayoutManager mLayoutManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_file_list);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.item_list);
            mRecyclerView.HasFixedSize = true;

            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            mModel = new List<MediaBrowserCompat.MediaItem>();

            mAdapter = new SimpleItemRecyclerViewAdapter(mModel);
            mAdapter.ItemClick += OnItemClick;
            mRecyclerView.SetAdapter(mAdapter);            
        }


        //TEMP!!!
        public const string CustomMetadataTrackSource = "__SOURCE__";
        protected override void OnStart()
        {
            base.OnStart();

            try
            {

                mModel = new List<MediaBrowserCompat.MediaItem>();

                var projection = new String[]
                {
                    MediaStore.Audio.Media.InterfaceConsts.Id,
                    MediaStore.Audio.Media.InterfaceConsts.Artist,
                    MediaStore.Audio.Media.InterfaceConsts.Title,
                    MediaStore.Audio.Media.InterfaceConsts.Duration,
                    MediaStore.Audio.Media.InterfaceConsts.Data,
                    MediaStore.Audio.Media.InterfaceConsts.MimeType
                };

                var selection = MediaStore.Audio.Media.InterfaceConsts.IsMusic + "!= 0";
                if (!TextUtils.IsEmpty(searchText))
                    selection += " AND " + MediaStore.Audio.Media.InterfaceConsts.Title + " LIKE '%" + searchText + "%'";

                var sortOrder = MediaStore.Audio.Media.InterfaceConsts.DateAdded + " DESC";

                var cursor = ContentResolver.Query(MediaStore.Audio.Media.ExternalContentUri, projection, selection, null, sortOrder);                

                if (cursor != null && cursor.MoveToFirst())
                {
                    do
                    {
                        int idColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
                        int artistColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
                        int titleColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
                        int durationColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Duration);
                        int filePathIndex = cursor.GetColumnIndexOrThrow(MediaStore.Audio.Media.InterfaceConsts.Data);

                        var id = cursor.GetLong(idColumn).ToString();

                        MediaMetadataCompat metadata = new MediaMetadataCompat.Builder()
                                .PutString(MediaMetadataCompat.MetadataKeyMediaId, id)
                                .PutString(CustomMetadataTrackSource, cursor.GetString(filePathIndex))
                                .PutString(MediaMetadataCompat.MetadataKeyArtist, cursor.GetString(artistColumn))
                                .PutString(MediaMetadataCompat.MetadataKeyTitle, cursor.GetString(titleColumn))
                                .PutLong(MediaMetadataCompat.MetadataKeyDuration, cursor.GetInt(durationColumn))
                                .Build();

                        var id2 = metadata.Description.MediaId;

                        Bundle playExtras = new Bundle();
                        playExtras.PutLong(MediaMetadataCompat.MetadataKeyDuration, metadata.GetLong(MediaMetadataCompat.MetadataKeyDuration));

                        MediaDescriptionCompat desc =
                            new MediaDescriptionCompat.Builder()
                                    .SetMediaId(id2)
                                    .SetTitle(metadata.GetString(MediaMetadataCompat.MetadataKeyTitle))
                                    .SetSubtitle(metadata.GetString(MediaMetadataCompat.MetadataKeyArtist))
                                    .SetExtras(playExtras)
                                    .Build();

                        mModel.Add(new MediaBrowserCompat.MediaItem(desc, MediaBrowserCompat.MediaItem.FlagPlayable));

                    } while (cursor.MoveToNext());
                }

            }
            catch (Exception e)
            {
                
            }

            SearchFiles();
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // Inflate the menu; this adds items to the action bar if it is present.
            MenuInflater.Inflate(Resource.Menu.menu_file_list, menu);

            var item = menu.FindItem(Resource.Id.action_search);
            var searchView = MenuItemCompat.GetActionView(item);
            mSearchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>(); 
            mSearchView.MaxWidth = int.MaxValue;
            mSearchView.QueryHint = Resources.GetText(Resource.String.search_songs);
            mSearchView.SetOnQueryTextListener(this);

            MenuItemCompat.SetOnActionExpandListener(item, new FileListSearchViewExpandListener(this));

            if (searchOpen)
            {
                MenuItemCompat.ExpandActionView(item);

                if (!TextUtils.IsEmpty(queryToSave))
                    mSearchView.SetQuery(queryToSave, false);

                mSearchView.ClearFocus();
            }

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle action bar item clicks here. The action bar will
            // automatically handle clicks on the Home/Up button, so long
            // as you specify a parent activity in AndroidManifest.xml.

            switch (item.ItemId)
            {
                case Resource.Id.action_search:

                    return true;

                case Resource.Id.action_settings:

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }        

        public bool OnQueryTextSubmit(string query)
        {
            return false;
        }

        public bool OnQueryTextChange(string newText)
        {
            searchText = newText;

            SearchFiles();

            return true;
        }       

        protected override void OnSaveInstanceState(Bundle savedInstanceState)
        {
            savedInstanceState.PutBoolean(SearchOpen, searchOpen);
            savedInstanceState.PutString(QueryString, searchText);

            base.OnSaveInstanceState(savedInstanceState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);

            searchOpen = savedInstanceState.GetBoolean(SearchOpen);
            searchText = savedInstanceState.GetString(QueryString);
        }

        private List<MediaBrowserCompat.MediaItem> Filter(List<MediaBrowserCompat.MediaItem> items, string query)
        {

            if (TextUtils.IsEmpty(query))
                return items;

            query = query.Trim().ToLower();

            var filteredList = new List<MediaBrowserCompat.MediaItem>();
            foreach (var item in items)
            {
                var text1 = item.Description.Title.ToString().Trim().ToLower();
                var text2 = item.Description.Subtitle.ToString().Trim().ToLower();
                if (text1.Contains(query) || text2.Contains(query))
                {
                    filteredList.Add(item);
                }
            }
            return filteredList;
        }

        private void SearchFiles()
        {
            var filteredList = Filter(mModel, searchText);
            mAdapter.SetFilter(filteredList);
        }

        private void OnItemClick(object sender, MediaBrowserCompat.MediaItem item)
        {
            if (item.IsPlayable)
            {
                //getSupportMediaController().getTransportControls()
                //        .playFromMediaId(item.MediaId, null);

                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }
        }        

        private class FileListSearchViewExpandListener: Java.Lang.Object, MenuItemCompat.IOnActionExpandListener
        {
            private readonly FileListActivity _activity;

            public FileListSearchViewExpandListener(FileListActivity activity)
            {
                _activity = activity;
            }

            public bool OnMenuItemActionCollapse(IMenuItem item)
            {
                _activity.searchOpen = false;

                _activity.mAdapter.SetFilter(_activity.mModel);

                return true;
            }

            public bool OnMenuItemActionExpand(IMenuItem item)
            {
                _activity.searchOpen = true;

                if (_activity.searchText != null)
                {
                    _activity.queryToSave = _activity.searchText;
                }

                return true;
            }
        }
    }

    public class MediaItemViewHolder : RecyclerView.ViewHolder
    {
        public TextView TitleView { get; private set; }
        public TextView ArtistView { get; private set; }
        public TextView DurationView { get; private set; }
        public MediaBrowserCompat.MediaItem Item { get; set; }

        public MediaItemViewHolder(View view, Action<MediaBrowserCompat.MediaItem> listener) : base(view)
        {
            TitleView = view.FindViewById<TextView>(Resource.Id.title);
            ArtistView = view.FindViewById<TextView>(Resource.Id.artist);
            DurationView = view.FindViewById<TextView>(Resource.Id.duration);

            view.Click += (sender, e) => listener(Item);
        }
    }

    public class SimpleItemRecyclerViewAdapter : RecyclerView.Adapter
    {
        private List<MediaBrowserCompat.MediaItem> mValues;

        public event EventHandler<MediaBrowserCompat.MediaItem> ItemClick;

        public SimpleItemRecyclerViewAdapter(List<MediaBrowserCompat.MediaItem> items)
        {
            mValues = items;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.file_list_item, parent, false);

            var vh = new MediaItemViewHolder(view, OnClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MediaItemViewHolder vh = holder as MediaItemViewHolder;

            vh.Item = mValues[position];
            vh.TitleView.Text = mValues[position].Description.Title;
            vh.ArtistView.Text = mValues[position].Description.Subtitle;
            vh.DurationView.Text = DateUtils.FormatElapsedTime(mValues[position].Description
                    .Extras.GetLong(MediaMetadataCompat.MetadataKeyDuration) / 1000);
        }

        public override int ItemCount
        {
            get {  return mValues.Count;  }
        }

        public void SetFilter(List<MediaBrowserCompat.MediaItem> items)
        {
            mValues = items;
            NotifyDataSetChanged();
        }

        void OnClick(MediaBrowserCompat.MediaItem item)
        {
            ItemClick?.Invoke(this, item);
        }
    }    
}