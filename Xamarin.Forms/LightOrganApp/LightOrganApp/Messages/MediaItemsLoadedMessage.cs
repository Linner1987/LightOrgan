using LightOrganApp.Model;
using System.Collections.Generic;

namespace LightOrganApp.Messages
{
    public class MediaItemsLoadedMessage
    {
        public List<MediaItem> Items { get; set; }
    }
}
