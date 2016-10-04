
using System;
using Android.Media;
using LightOrganApp.Droid.Utils;
using Android.Net.Wifi;
using Android.Media.Audiofx;
using LightOrganApp.Droid.Model;
using Android.Content;
using Android.Net;
using Android.Support.V4.Media.Session;
using Android.Support.V4.Media;
using System.IO;
using static Android.Support.V4.Media.Session.MediaSessionCompat;


namespace LightOrganApp.Droid.Playback
{
    public class LocalPlayback : Java.Lang.Object, IPlayback, AudioManager.IOnAudioFocusChangeListener,
            MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener,
            MediaPlayer.IOnPreparedListener, MediaPlayer.IOnSeekCompleteListener,            
            Visualizer.IOnDataCaptureListener
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(LocalPlayback));

        public const float VolumeDuck = 0.2f;
        public const float VolumeNormal = 1.0f;

        const int AudioNoFocusNoDuck = 0;
        const int AudioNoFocusCanDuck = 1;
        const int AudioFocused = 2;

        readonly MusicService service;
        readonly WifiManager.WifiLock wifiLock;
        public int State { get; set; }
        bool playOnFocusGain;
        public IPlaybackCallback Callback { get; set; }
        readonly MusicProvider musicProvider;
        volatile bool audioNoisyReceiverRegistered;
        volatile int currentPosition;
        public string CurrentMediaId { get; set; }

        int audioFocus = AudioNoFocusNoDuck;
        readonly AudioManager audioManager;

        MediaPlayer mediaPlayer;
        Visualizer visualizer;
        bool canUseVisualizer;

        IntentFilter mAudioNoisyIntentFilter = new IntentFilter(AudioManager.ActionAudioBecomingNoisy);

        readonly BroadcastReceiver mAudioNoisyReceiver = new BroadcastReceiver();

        class BroadcastReceiver : Android.Content.BroadcastReceiver
        {           
            public Action<Context, Intent> OnReceiveImpl { get; set; }
            public override void OnReceive(Context context, Intent intent)
            {
                OnReceiveImpl(context, intent);
            }            
        }

        public LocalPlayback(MusicService service, MusicProvider musicProvider, bool canUseVisualizer)
        {
            this.service = service;
            this.musicProvider = musicProvider;
            this.canUseVisualizer = canUseVisualizer;            
            this.audioManager = (AudioManager)service.GetSystemService(Context.AudioService);
            wifiLock = ((WifiManager)service.GetSystemService(Context.WifiService))
               .CreateWifiLock(WifiMode.Full, "sample_lock");
            State = PlaybackStateCompat.StateNone;

            mAudioNoisyReceiver.OnReceiveImpl = (context, intent) => {
                if (AudioManager.ActionAudioBecomingNoisy == intent.Action)
                {
                    LogHelper.Debug(Tag, "Headphones disconnected.");
                    if (IsPlaying)
                    {
                        var i = new Intent(context, typeof(MusicService));
                        i.SetAction(MusicService.ActionCmd);
                        i.PutExtra(MusicService.CmdName, MusicService.CmdPause);
                        service.StartService(i);
                    }
                }
            };
        }

        public void Start()
        {

        }

        public void Stop(bool notifyListeners)
        {
            State = PlaybackStateCompat.StateStopped;

            if (notifyListeners && Callback != null)
            {
                Callback.OnPlaybackStatusChanged(State);
            }

            currentPosition = CurrentStreamPosition;
            GiveUpAudioFocus();
            UnregisterAudioNoisyReceiver();
            RelaxResources(true);            
        }

        public bool IsConnected => true;       

        public bool IsPlaying => playOnFocusGain || (mediaPlayer != null && mediaPlayer.IsPlaying);           

        public int CurrentStreamPosition
        {
            get
            {
                return mediaPlayer != null ? mediaPlayer.CurrentPosition : currentPosition;
            }
            set
            {
                currentPosition = value;
            }
        }        

        public void UpdateLastKnownStreamPosition()
        {
            if (mediaPlayer != null)
            {
                currentPosition = mediaPlayer.CurrentPosition;
            }
        }

        public void Play(QueueItem item)
        {
            playOnFocusGain = true;
            TryToGetAudioFocus();
            RegisterAudioNoisyReceiver();
            string mediaId = item.Description.MediaId;
            bool mediaHasChanged = mediaId != CurrentMediaId;
            if (mediaHasChanged)
            {
                currentPosition = 0;
                CurrentMediaId = mediaId;
            }

            if (State == PlaybackStateCompat.StatePaused && !mediaHasChanged && mediaPlayer != null)
            {
                ConfigMediaPlayerState();
            }
            else
            {
                State = PlaybackStateCompat.StateStopped;
                RelaxResources(false);
                MediaMetadataCompat track = musicProvider.GetMusic(item.Description.MediaId);

                string source = track.GetString(MusicProvider.CustomMetadataTrackSource);

                try
                {
                    CreateMediaPlayerIfNeeded();

                    State = PlaybackStateCompat.StateBuffering;

                    mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);
                    mediaPlayer.SetDataSource(source);

                    mediaPlayer.PrepareAsync();

                    wifiLock.Acquire();
                   
                    Callback?.OnPlaybackStatusChanged(State);
                }
                catch (IOException ex)
                {
                    LogHelper.Error(Tag, ex, "Exception playing song");                    
                    Callback?.OnError(ex.Message);                    
                }
            }
        }

        public void Pause()
        {
            if (State == PlaybackStateCompat.StatePlaying)
            {
                if (mediaPlayer != null && mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Pause();
                    currentPosition = mediaPlayer.CurrentPosition;
                }
                RelaxResources(false);
                GiveUpAudioFocus();
            }
            State = PlaybackStateCompat.StatePaused;
            
            Callback?.OnPlaybackStatusChanged(State);
            
            UnregisterAudioNoisyReceiver();
        }

        public void SeekTo(int position)
        {
            LogHelper.Debug(Tag, "seekTo called with ", position);

            if (mediaPlayer == null)
            {
                currentPosition = position;
            }
            else
            {
                if (mediaPlayer.IsPlaying)
                {
                    State = PlaybackStateCompat.StateBuffering;
                }
                mediaPlayer.SeekTo(position);
                
                Callback?.OnPlaybackStatusChanged(State);                
            }
        }

        void TryToGetAudioFocus()
        {
            LogHelper.Debug(Tag, "tryToGetAudioFocus");
            if (audioFocus != AudioFocused)
            {
                var result = audioManager.RequestAudioFocus(this, Android.Media.Stream.Music,
                    AudioFocus.Gain);
                if (result == AudioFocusRequest.Granted)
                {
                    audioFocus = AudioFocused;
                }
            }
        }

        void GiveUpAudioFocus()
        {
            LogHelper.Debug(Tag, "giveUpAudioFocus");
            if (audioFocus == AudioFocused)
            {
                if (audioManager.AbandonAudioFocus(this) == AudioFocusRequest.Granted)
                {
                    audioFocus = AudioNoFocusNoDuck;
                }
            }
        }

        void ConfigMediaPlayerState()
        {
            LogHelper.Debug(Tag, "configMediaPlayerState. mAudioFocus=", audioFocus);
            if (audioFocus == AudioNoFocusNoDuck)
            {
                if (State == PlaybackStateCompat.StatePlaying)
                {
                    Pause();
                }
            }
            else
            {  // we have audio focus:
                if (audioFocus == AudioNoFocusCanDuck)
                {
                    mediaPlayer.SetVolume(VolumeDuck, VolumeDuck);
                }
                else
                {
                    if (mediaPlayer != null)
                    {
                        mediaPlayer.SetVolume(VolumeNormal, VolumeNormal);
                    }
                }
                if (playOnFocusGain)
                {
                    if (mediaPlayer != null && !mediaPlayer.IsPlaying)
                    {
                        LogHelper.Debug(Tag, "configMediaPlayerState startMediaPlayer. seeking to ",
                            currentPosition);
                        if (currentPosition == mediaPlayer.CurrentPosition)
                        {
                            mediaPlayer.Start();
                            State = PlaybackStateCompat.StatePlaying;
                        }
                        else
                        {
                            mediaPlayer.SeekTo(currentPosition);
                            State = PlaybackStateCompat.StateBuffering;
                        }
                    }
                    playOnFocusGain = false;
                }
            }
            
            Callback?.OnPlaybackStatusChanged(State);            
        }

        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            LogHelper.Debug(Tag, "onAudioFocusChange. focusChange=", focusChange);
            if (focusChange == AudioFocus.Gain)
            {
                audioFocus = AudioFocused;
            }
            else if (focusChange == AudioFocus.Loss ||
              focusChange == AudioFocus.LossTransient ||
              focusChange == AudioFocus.LossTransientCanDuck)
            {
                bool canDuck = focusChange == AudioFocus.LossTransientCanDuck;
                audioFocus = canDuck ? AudioNoFocusCanDuck : AudioNoFocusNoDuck;                

                if (State == PlaybackStateCompat.StatePlaying && !canDuck)
                {                    
                    playOnFocusGain = true;
                }
            }
            else
            {
                LogHelper.Error(Tag, "onAudioFocusChange: Ignoring unsupported focusChange: ", focusChange);
            }
            ConfigMediaPlayerState();
        }

        public void OnSeekComplete(MediaPlayer mp)
        {
            LogHelper.Debug(Tag, "onSeekComplete from MediaPlayer:", mp.CurrentPosition);
            currentPosition = mp.CurrentPosition;
            if (State == PlaybackStateCompat.StateBuffering)
            {
                mediaPlayer.Start();
                State = PlaybackStateCompat.StatePlaying;
            }
            
            Callback?.OnPlaybackStatusChanged(State);            
        }

        public void OnCompletion(MediaPlayer mp)
        {
            LogHelper.Debug(Tag, "onCompletion from MediaPlayer");
            
            Callback?.OnCompletion();            
        }

        public void OnPrepared(MediaPlayer mp)
        {
            LogHelper.Debug(Tag, "onPrepared from MediaPlayer");
            ConfigMediaPlayerState();
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            LogHelper.Error(Tag, "Media player error: what=" + what + ", extra=" + extra);
            
            Callback?.OnError("MediaPlayer error " + what + " (" + extra + ")");
            
            return true;
        }

        void CreateMediaPlayerIfNeeded()
        {
            LogHelper.Debug(Tag, "createMediaPlayerIfNeeded. needed? ", (mediaPlayer == null));
            if (mediaPlayer == null)
            {
                mediaPlayer = new MediaPlayer();

                mediaPlayer.SetWakeMode(service.ApplicationContext,
                    Android.OS.WakeLockFlags.Partial);

                mediaPlayer.SetOnPreparedListener(this);
                mediaPlayer.SetOnCompletionListener(this);
                mediaPlayer.SetOnErrorListener(this);
                mediaPlayer.SetOnSeekCompleteListener(this);

                if (canUseVisualizer)
                {
                    visualizer = new Visualizer(mediaPlayer.AudioSessionId);
                    visualizer.SetCaptureSize(Visualizer.GetCaptureSizeRange()[1]);
                    visualizer.SetDataCaptureListener(this, Visualizer.MaxCaptureRate / 2, false, true);
                    visualizer.SetEnabled(true);
                }
            }
            else
            {
                mediaPlayer.Reset();
            }
        }

        void RelaxResources(bool releaseMediaPlayer)
        {
            LogHelper.Debug(Tag, "relaxResources. releaseMediaPlayer=", releaseMediaPlayer);            

            if (releaseMediaPlayer && mediaPlayer != null)
            {
                if (canUseVisualizer)
                {
                    visualizer.SetEnabled(false);
                    visualizer.Release();
                }

                mediaPlayer.Reset();
                mediaPlayer.Release();
                mediaPlayer = null;
            }

            if (wifiLock.IsHeld)
            {
                wifiLock.Release();
            }
        }

        void RegisterAudioNoisyReceiver()
        {
            if (!audioNoisyReceiverRegistered)
            {
                service.RegisterReceiver(mAudioNoisyReceiver, mAudioNoisyIntentFilter);
                audioNoisyReceiverRegistered = true;
            }
        }

        void UnregisterAudioNoisyReceiver()
        {
            if (audioNoisyReceiverRegistered)
            {
                service.UnregisterReceiver(mAudioNoisyReceiver);
                audioNoisyReceiverRegistered = false;
            }
        }

        public void OnWaveFormDataCapture(Visualizer visualizer, byte[] waveform, int samplingRate)
        {            
        }

        public void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate)
        {
            Callback?.OnFftDataCapture(visualizer, fft, samplingRate);
        }        
    }
}