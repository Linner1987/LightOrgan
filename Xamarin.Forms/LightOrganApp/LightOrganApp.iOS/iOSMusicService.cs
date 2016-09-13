using LightOrganApp.Services;
using System.Collections.Generic;
using LightOrganApp.Model;
using LightOrganApp.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(iOSMusicService))]

namespace LightOrganApp.iOS
{
    public class iOSMusicService : IMusicService
    {
        public IEnumerable<MediaItem> GetItems()
        {
            var items = new List<MediaItem>();

            for (int i = 0; i < 15; i++)
                items.Add(new MediaItem($"Kawa {i}", $"Gang {i}", GetDisplayTime((i + 1) * 90)));

            return items;
        }

        private string GetDisplayTime(int seconds)
        {
            var h = seconds / 3600;
            var m = seconds / 60 - h * 60;
            var s = seconds - h * 3600 - m * 60;

            var str = "";

            if (h > 0)
                str += $"{h}:";

            str += $"{m:00}:{s:00}";

            return str;
        }
    }
}
