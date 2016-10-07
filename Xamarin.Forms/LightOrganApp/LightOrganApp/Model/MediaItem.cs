using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightOrganApp.Model
{
    public class MediaItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Duration { get; set; }

        public MediaItem(string id, string title, string artist, string duration)
        {
            Id = id;
            Title = title;
            Artist = artist;
            Duration = duration;
        }
    }
}
