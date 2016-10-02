using LightOrganApp.Services;
using System.Collections.Generic;
using LightOrganApp.Model;
using LightOrganApp.iOS;
using System.Threading.Tasks;
using MediaPlayer;
using Foundation;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(iOSMusicService))]

namespace LightOrganApp.iOS
{
    public class iOSMusicService : IMusicService
    {
        public async Task<List<MediaItem>> GetItemsAsync()
        {
            return await Task.Run(() =>
            {
                var query = new MPMediaQuery();
                var mediaTypeNumber = NSNumber.FromInt32((int)MPMediaType.Music);
                var predicate = MPMediaPropertyPredicate.PredicateWithValue(mediaTypeNumber, MPMediaItem.MediaTypeProperty);

                query.AddFilterPredicate(predicate);

                var unknownArtist = NSBundle.MainBundle.LocalizedString("unknownArtist", "Unknown Artist");

                return query.Items.Select(item => new MediaItem(item.Title, (item.Artist != null) ? item.Artist : unknownArtist, GetDisplayTime((int)item.PlaybackDuration))).ToList();
            });          
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
