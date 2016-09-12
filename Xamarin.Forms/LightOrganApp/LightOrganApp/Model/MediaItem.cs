using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightOrganApp.Model
{
    public class MediaItem
    {
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Duration { get; private set; }

        public MediaItem(string title, string artist, string duration)
        {
            Title = title;
            Artist = artist;
            Duration = duration;
        }
    }
}
