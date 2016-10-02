using System.Collections.Generic;
using Android.Text.Format;
using LightOrganApp.Droid;
using System.Threading.Tasks;
using Android.Provider;
using Xamarin.Forms;
using LightOrganApp.Model;
using LightOrganApp.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidMusicService))]

namespace LightOrganApp.Droid
{
    public class AndroidMusicService : IMusicService
    {
        public async Task<List<MediaItem>> GetItemsAsync()
        {
            return await Task.Run(() =>
            {
                var items = new List<MediaItem>();

                try
                {
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

                    var cursor = Forms.Context.ContentResolver.Query(MediaStore.Audio.Media.ExternalContentUri, projection, selection, null, sortOrder);

                    if (cursor != null && cursor.MoveToFirst())
                    {
                        do
                        {
                            int idColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
                            int artistColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
                            int titleColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
                            int durationColumn = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Duration);
                            int filePathIndex = cursor.GetColumnIndexOrThrow(MediaStore.Audio.Media.InterfaceConsts.Data);                            

                            var item = new MediaItem(cursor.GetString(titleColumn), cursor.GetString(artistColumn), DateUtils.FormatElapsedTime(cursor.GetInt(durationColumn) / 1000));
                            items.Add(item);                          

                        } while (cursor.MoveToNext());
                    }                        
                }
                catch
                {                   
                }

                return items;
            });            
        }       
    }
}