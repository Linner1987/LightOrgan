using Android.Support.V4.Media;

namespace LightOrganApp.Droid.UI
{
    public interface IMediaBrowserProvider
    {
        MediaBrowserCompat MediaBrowser { get; }
    }
}