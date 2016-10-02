using LightOrganApp.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightOrganApp.Services
{
    public interface IMusicService
    {
       Task<List<MediaItem>> GetItemsAsync();
    }
}
