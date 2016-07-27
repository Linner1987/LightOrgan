using System;
using System.Collections.Generic;
using Android.Views;
using LightOrganApp.Droid.Model;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using System.Linq;

namespace LightOrganApp.Droid.Utils
{
    public static class QueueHelper
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(QueueHelper));

        private const int RandomQueueSize = 10;

        public static List<MediaSessionCompat.QueueItem> GetPlayingQueue(string mediaId, MusicProvider musicProvider)
        {
            var tracks = musicProvider.SearchMusic("");

            if (tracks == null)
            {
                return null;
            }

            return ConvertToQueue(tracks, mediaId);
        }

        public static List<MediaSessionCompat.QueueItem> GetPlayingQueueFromSearch(string query, MusicProvider musicProvider)
        {
            LogHelper.Debug(Tag, "Creating playing queue for musics from search ", query);
            return ConvertToQueue(musicProvider.SearchMusic(query), query);
        }

        public static int GetMusicIndexOnQueue(IEnumerable<MediaSessionCompat.QueueItem> queue, string mediaId)
        {
            int index = 0;
            foreach (var item in queue)
            {
                if (mediaId == item.Description.MediaId)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static int GetMusicIndexOnQueue(IEnumerable<MediaSessionCompat.QueueItem> queue, long queueId)
        {
            int index = 0;
            foreach (var item in queue)
            {
                if (queueId == item.QueueId)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        static List<MediaSessionCompat.QueueItem> ConvertToQueue(IEnumerable<MediaMetadataCompat> tracks, params string[] categories)
        {
            var queue = new List<MediaSessionCompat.QueueItem>();
            int count = 0;
            foreach (var track in tracks)
            {                
                var trackCopy = new MediaMetadataCompat.Builder(track)
                    .PutString(MediaMetadataCompat.MetadataKeyMediaId, track.Description.MediaId)
                    .Build();

                var item = new MediaSessionCompat.QueueItem(trackCopy.Description, count++);
                queue.Add(item);
            }
            return queue;

        }

        public static List<MediaSessionCompat.QueueItem> GetRandomQueue(MusicProvider musicProvider)
        {            
            var shuffled = musicProvider.GetShuffledMusic();
            var result = shuffled.Take(RandomQueueSize).ToList();
            LogHelper.Debug(Tag, "getRandomQueue: result.size=", result.Count);

            return ConvertToQueue(result, "random");
        }

        public static bool IsIndexPlayable(int index, IList<MediaSessionCompat.QueueItem> queue)
        {
            return (queue != null && index >= 0 && index < queue.Count);
        }
    }
}