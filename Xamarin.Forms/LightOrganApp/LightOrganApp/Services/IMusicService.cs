using LightOrganApp.Model;
using System.Collections.Generic;

namespace LightOrganApp.Services
{
    public interface IMusicService
    {
        IEnumerable<MediaItem> GetItems();
    }
}
