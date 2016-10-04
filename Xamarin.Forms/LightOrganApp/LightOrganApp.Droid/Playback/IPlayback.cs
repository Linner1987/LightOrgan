using static Android.Support.V4.Media.Session.MediaSessionCompat;
using Android.Media.Audiofx;

namespace LightOrganApp.Droid.Playback
{
    public interface IPlayback
    {
        void Start();
        void Stop(bool notifyListeners);
        int State { get; set; }
        bool IsConnected { get; }
        bool IsPlaying { get; }
        int CurrentStreamPosition { get; set; }
        void UpdateLastKnownStreamPosition();
        void Play(QueueItem item);
        void Pause();
        void SeekTo(int position);
        string CurrentMediaId { get; set; }
        IPlaybackCallback Callback { get; set; }     
    }

    public interface IPlaybackCallback
    {
        void OnCompletion();
        void OnPlaybackStatusChanged(int state);
        void OnError(string error);        
        void SetCurrentMediaId(string mediaId);
        void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate);
    }

}