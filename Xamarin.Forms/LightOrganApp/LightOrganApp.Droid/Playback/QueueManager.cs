using System.Collections.Generic;
using LightOrganApp.Droid.Utils;
using LightOrganApp.Droid.Model;
using Android.Content.Res;
using Android.Support.V4.Media.Session;
using Android.OS;
using System;
using Android.Support.V4.Media;

namespace LightOrganApp.Droid.Playback
{
    public class QueueManager
    {        
        static readonly string Tag = LogHelper.MakeLogTag(typeof(QueueManager));

        private MusicProvider musicProvider;
        private MetadataUpdateListener listener;
        private Resources resources;

        // "Now playing" queue:
        private IList<MediaSessionCompat.QueueItem> playingQueue;
        private int currentIndex;

        public QueueManager(MusicProvider musicProvider,
                        Resources resources,
                        MetadataUpdateListener listener)
        {
            this.musicProvider = musicProvider;
            this.listener = listener;
            this.resources = resources;

            playingQueue = new SynchronizedList<MediaSessionCompat.QueueItem>();
            currentIndex = 0;
        }

        private void SetCurrentQueueIndex(int index)
        {
            if (index >= 0 && index < playingQueue.Count)
            {
                currentIndex = index;
                listener.OnCurrentQueueIndexUpdated(currentIndex);
            }
        }

        public bool SetCurrentQueueItem(long queueId)
        {
            // set the current index on queue from the queue Id:
            int index = QueueHelper.GetMusicIndexOnQueue(playingQueue, queueId);
            SetCurrentQueueIndex(index);
            return index >= 0;
        }

        public bool SetCurrentQueueItem(string mediaId)
        {
            // set the current index on queue from the music Id:
            int index = QueueHelper.GetMusicIndexOnQueue(playingQueue, mediaId);
            SetCurrentQueueIndex(index);
            return index >= 0;
        }

        public bool SkipQueuePosition(int amount)
        {
            int index = currentIndex + amount;
            if (index < 0)
            {
                // skip backwards before the first song will keep you on the first song
                index = 0;
            }
            else
            {
                // skip forwards when in last song will cycle back to start of the queue
                index %= playingQueue.Count;
            }
            if (!QueueHelper.IsIndexPlayable(index, playingQueue))
            {
                LogHelper.Error(Tag, "Cannot increment queue index by ", amount,
                        ". Current=", currentIndex, " queue length=", playingQueue.Count);
                return false;
            }
            currentIndex = index;
            return true;
        }

        public void SetQueueFromSearch(string query, Bundle extras)
        {
            SetCurrentQueue(resources.GetString(Resource.String.search_queue_title),
                    QueueHelper.GetPlayingQueueFromSearch(query, musicProvider));
        }

        public void SetRandomQueue()
        {
            SetCurrentQueue(resources.GetString(Resource.String.random_queue_title),
                    QueueHelper.GetRandomQueue(musicProvider));
        }

        public void SetQueueFromMusic(string mediaId)
        {
            LogHelper.Debug(Tag, "setQueueFromMusic", mediaId);

            // The mediaId used here is not the unique musicId. This one comes from the
            // MediaBrowser, and is actually a "hierarchy-aware mediaID": a concatenation of
            // the hierarchy in MediaBrowser and the actual unique musicID. This is necessary
            // so we can build the correct playing queue, based on where the track was
            // selected from.
            bool canReuseQueue = SetCurrentQueueItem(mediaId);
            
            if (!canReuseQueue)
            {
                var queueTitle = resources.GetString(Resource.String.browse_musics_by_genre_subtitle, mediaId);
                SetCurrentQueue(queueTitle, QueueHelper.GetPlayingQueue(mediaId, musicProvider), mediaId);
            }
            UpdateMetadata();
        }

        public MediaSessionCompat.QueueItem CurrentMusic
        {
            get
            {
                if (!QueueHelper.IsIndexPlayable(currentIndex, playingQueue))
                {
                    return null;
                }
                return playingQueue[currentIndex];
            }
        }

        public int CurrentQueueSize
        {
            get
            {
                if (playingQueue == null)
                {
                    return 0;
                }
                return playingQueue.Count;
            }           
        }

        protected void SetCurrentQueue(string title, List<MediaSessionCompat.QueueItem> newQueue)
        {
            SetCurrentQueue(title, newQueue, null);
        }

        protected void SetCurrentQueue(string title, List<MediaSessionCompat.QueueItem> newQueue,
                                   string initialMediaId)
        {
            playingQueue = newQueue;
            int index = 0;
            if (initialMediaId != null)
            {
                index = QueueHelper.GetMusicIndexOnQueue(playingQueue, initialMediaId);
            }
            currentIndex = Math.Max(index, 0);
            listener.OnQueueUpdated(title, newQueue);
        }

        public void UpdateMetadata()
        {
            MediaSessionCompat.QueueItem currentMusic = CurrentMusic;
            if (currentMusic == null)
            {
                listener.OnMetadataRetrieveError();
                return;
            }
            string musicId = currentMusic.Description.MediaId;
            var metadata = musicProvider.GetMusic(musicId);
            if (metadata == null)
            {
                throw new ArgumentException("Invalid musicId " + musicId);
            }

            listener.OnMetadataChanged(metadata);

            // Set the proper album artwork on the media session, so it can be shown in the
            // locked screen and in other places.
            if (metadata.Description.IconBitmap == null &&
                    metadata.Description.IconUri != null)
            {
                var albumUri = metadata.Description.IconUri.ToString();
                AlbumArtCache.Instance.Fetch(albumUri, new AlbumArtCache.FetchListener() {
                
                    OnFetched = (artUrl, bitmap, icon) =>
                    {
                        musicProvider.UpdateMusicArt(musicId, bitmap, icon);

                        // If we are still playing the same music, notify the listeners:
                        var cm = CurrentMusic;
                        if (cm == null)
                        {
                            return;
                        }
                        var currentPlayingId = cm.Description.MediaId;
                        if (musicId == currentPlayingId)
                        {
                            listener.OnMetadataChanged(musicProvider.GetMusic(currentPlayingId));
                        }
                    }
                });
            }
        }

        public class MetadataUpdateListener
        {
            public Action<MediaMetadataCompat> OnMetadataChanged;
            public Action OnMetadataRetrieveError;
            public Action<int> OnCurrentQueueIndexUpdated;
            public Action<string, List<MediaSessionCompat.QueueItem>> OnQueueUpdated;            
        }
    }
}