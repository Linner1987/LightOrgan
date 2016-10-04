using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Media;
using LightOrganApp.Droid.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightOrganApp.Droid.Model
{
    public class MusicProvider
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(MusicProvider));

        public const string CustomMetadataTrackSource = "__SOURCE__";

        readonly Dictionary<string, MutableMediaMetadata> musicListById;

        enum State
        {
            NonInitialized,
            Initializing,
            Initialized
        };

        volatile State currentState = State.NonInitialized;

        public MusicProvider()
        {
            musicListById = new Dictionary<string, MutableMediaMetadata>();
        }

        public IEnumerable<MediaMetadataCompat> GetShuffledMusic()
        {
            if (currentState != State.Initialized)
            {
                return new List<MediaMetadataCompat>();
            }
            var shuffled = new List<MediaMetadataCompat>();
            foreach (var mutableMetadata in musicListById.Values)
            {
                shuffled.Add(mutableMetadata.Metadata);
            }
            shuffled.Shuffle();

            return shuffled;
        }

        public IEnumerable<MediaMetadataCompat> SearchMusic(string query)
        {
            if (currentState != State.Initialized)
            {
                return new List<MediaMetadataCompat>();
            }

            var result = new List<MediaMetadataCompat>();
            query = query.ToLower();
            foreach (var track in musicListById.Values)
            {
                if (track.Metadata.GetString(MediaMetadataCompat.MetadataKeyTitle).ToLower().Contains(query) ||
                    track.Metadata.GetString(MediaMetadataCompat.MetadataKeyArtist).ToLower().Contains(query))
                {
                    result.Add(track.Metadata);
                }
            }
            return result;
        }

        public MediaMetadataCompat GetMusic(string musicId)
        {
            return musicListById.ContainsKey(musicId) ? musicListById[musicId].Metadata : null;
        }

        public void UpdateMusicArt(string musicId, Bitmap albumArt, Bitmap icon)
        {
            var metadata = GetMusic(musicId);
            metadata = new MediaMetadataCompat.Builder(metadata)

                    // set high resolution bitmap in METADATA_KEY_ALBUM_ART. This is used, for
                    // example, on the lockscreen background when the media session is active.
                    .PutBitmap(MediaMetadataCompat.MetadataKeyAlbumArt, albumArt)

                    // set small version of the album art in the DISPLAY_ICON. This is used on
                    // the MediaDescription and thus it should be small to be serialized if
                    // necessary
                    .PutBitmap(MediaMetadataCompat.MetadataKeyDisplayIcon, icon)

                    .Build();

            var mutableMetadata = musicListById[musicId];
            if (mutableMetadata == null)
            {
                throw new Exception("Unexpected error: Inconsistent data structures in " +
                        "MusicProvider");
            }

            mutableMetadata.Metadata = metadata;
        }

        public void RetrieveMediaAsync(Context context, Action<bool> callback)
        {
            LogHelper.Debug(Tag, "retrieveMediaAsync called");
            if (currentState == State.Initialized)
            {
                callback?.Invoke(true);
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    if (currentState == State.NonInitialized)
                    {
                        currentState = State.Initializing;

                        var projection = new string[]
                        {
                            MediaStore.Audio.Media.InterfaceConsts.Id,
                            MediaStore.Audio.Media.InterfaceConsts.Artist,
                            MediaStore.Audio.Media.InterfaceConsts.Title,
                            MediaStore.Audio.Media.InterfaceConsts.Duration,
                            MediaStore.Audio.Media.InterfaceConsts.Data,
                            MediaStore.Audio.Media.InterfaceConsts.MimeType
                        };

                        var selection = MediaStore.Audio.Media.InterfaceConsts.IsMusic + "!= 0";
                        var sortOrder = MediaStore.Audio.Media.InterfaceConsts.DateAdded + " DESC";

                        var cursor = context.ContentResolver.Query(MediaStore.Audio.Media.ExternalContentUri, projection, selection, null, sortOrder);

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

                                var item = new MediaMetadataCompat.Builder()
                                        .PutString(MediaMetadataCompat.MetadataKeyMediaId, id)
                                        .PutString(CustomMetadataTrackSource, cursor.GetString(filePathIndex))
                                        .PutString(MediaMetadataCompat.MetadataKeyArtist, cursor.GetString(artistColumn))
                                        .PutString(MediaMetadataCompat.MetadataKeyTitle, cursor.GetString(titleColumn))
                                        .PutLong(MediaMetadataCompat.MetadataKeyDuration, cursor.GetInt(durationColumn))
                                        .Build();

                                musicListById.Add(id, new MutableMediaMetadata(id, item));

                            } while (cursor.MoveToNext());
                        }

                        currentState = State.Initialized;
                    }
                }
                finally
                {
                    if (currentState != State.Initialized)
                    {
                        // Something bad happened, so we reset state to NON_INITIALIZED to allow
                        // retries (eg if the network connection is temporary unavailable)
                        currentState = State.NonInitialized;
                    }
                }
            }).ContinueWith((antecedent) => {
                callback?.Invoke(currentState == State.Initialized);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);            
        }       

        public JavaList<MediaBrowserCompat.MediaItem> GetChildren(string mediaId, Resources resources)
        {
            var mediaItems = new JavaList<MediaBrowserCompat.MediaItem>();

            //zawsze lista (na razie bez kategorii)

            foreach (var m in musicListById.Values)
            {
                mediaItems.Add(CreateMediaItem(m.Metadata));
            }

            return mediaItems;
        }

        private MediaBrowserCompat.MediaItem CreateMediaItem(MediaMetadataCompat metadata)
        {
            // Since mediaMetadata fields are immutable, we need to create a copy, so we
            // can set a hierarchy-aware mediaID. We will need to know the media hierarchy
            // when we get a onPlayFromMusicID call, so we can create the proper queue based
            // on where the music was selected from (by artist, by genre, random, etc)
            var id = metadata.Description.MediaId;            

            var playExtras = new Bundle();
            playExtras.PutLong(MediaMetadataCompat.MetadataKeyDuration, metadata.GetLong(MediaMetadataCompat.MetadataKeyDuration));

            var desc = new MediaDescriptionCompat.Builder()
                            .SetMediaId(id)
                            .SetTitle(metadata.GetString(MediaMetadataCompat.MetadataKeyTitle))
                            .SetSubtitle(metadata.GetString(MediaMetadataCompat.MetadataKeyArtist))
                            .SetExtras(playExtras)
                            .Build();

            return new MediaBrowserCompat.MediaItem(desc,
                    MediaBrowserCompat.MediaItem.FlagPlayable);
        }
    }
}