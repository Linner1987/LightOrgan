using System.Collections.Generic;
using LightOrganApp.Model;
using LightOrganApp.Services;
using Android.Text.Format;
using LightOrganApp.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidMusicService))]

namespace LightOrganApp.Droid
{
    public class AndroidMusicService : IMusicService
    {
        public IEnumerable<MediaItem> GetItems()
        {
            var items = new List<MediaItem>();

            for (int i = 0; i < 12; i++)
                items.Add(new MediaItem($"Kawa {i}", $"Gang {i}", DateUtils.FormatElapsedTime((i+1)*90)));

            return items;
        }
    }
}