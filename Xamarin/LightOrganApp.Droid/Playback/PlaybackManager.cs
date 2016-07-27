
using System;
using Android.Media.Audiofx;
using LightOrganApp.Droid.Utils;
using LightOrganApp.Droid.Model;
using Android.Content.Res;
using Android.Support.V4.Media.Session;
using Android.OS;
using Java.Lang;

namespace LightOrganApp.Droid.Playback
{
    public class PlaybackManager : IPlaybackCallback
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(PlaybackManager));

        private MusicProvider musicProvider;
        private QueueManager queueManager;
        private Resources resources;
        private IPlaybackServiceCallback serviceCallback;

        public IPlayback Playback { get; private set; }
        
        readonly MediaSessionCallback mediaSessionCallback = new MediaSessionCallback();

        public MediaSessionCompat.Callback GetMediaSessionCallback()
        {
            return mediaSessionCallback; 
        }

        public PlaybackManager(IPlaybackServiceCallback serviceCallback, Resources resources,
                           MusicProvider musicProvider, QueueManager queueManager,
                           IPlayback playback)
        {
            this.musicProvider = musicProvider;
            this.serviceCallback = serviceCallback;
            this.resources = resources;
            this.queueManager = queueManager;

            mediaSessionCallback.OnPlayImpl = () =>
            {
                LogHelper.Debug(Tag, "play");
                if (queueManager.CurrentMusic == null)
                {
                    queueManager.SetRandomQueue();
                }
                HandlePlayRequest();
            };

            mediaSessionCallback.OnSkipToQueueItemImpl = (long queueId) =>
            {
                LogHelper.Debug(Tag, "OnSkipToQueueItem:" + queueId);
                queueManager.SetCurrentQueueItem(queueId);
                HandlePlayRequest();
                queueManager.UpdateMetadata();
            };

            mediaSessionCallback.OnSeekToImpl = (long position) =>
            {
                LogHelper.Debug(Tag, "onSeekTo:", position);
                playback.SeekTo((int)position);
            };

            mediaSessionCallback.OnPlayFromMediaIdImpl = (string mediaId, Bundle extras) =>
            {
                LogHelper.Debug(Tag, "playFromMediaId mediaId:", mediaId, "  extras=", extras);
                queueManager.SetQueueFromMusic(mediaId);
                HandlePlayRequest();
            };

            mediaSessionCallback.OnPauseImpl = () =>
            {
                LogHelper.Debug(Tag, "pause. current state=" + Playback.State);
                HandlePauseRequest();
            };

            mediaSessionCallback.OnStopImpl = () =>
            {
                LogHelper.Debug(Tag, "stop. current state=" + Playback.State);
                HandleStopRequest(null);
            };

            mediaSessionCallback.OnSkipToNextImpl = () =>
            {
                LogHelper.Debug(Tag, "skipToNext");
                if (queueManager.SkipQueuePosition(1))
                {
                    HandlePlayRequest();
                }
                else
                {
                    HandleStopRequest("Cannot skip");
                }
                queueManager.UpdateMetadata();
            };

            mediaSessionCallback.OnSkipToPreviousImpl = () =>
            {
                if (queueManager.SkipQueuePosition(-1))
                {
                    HandlePlayRequest();
                }
                else
                {
                    HandleStopRequest("Cannot skip");
                }
                queueManager.UpdateMetadata();
            };

            mediaSessionCallback.OnPlayFromSearchImpl = (string query, Bundle extras) =>
            {
                LogHelper.Debug(Tag, "playFromSearch  query=", query, " extras=", extras);

                Playback.State = PlaybackStateCompat.StateConnecting;
                queueManager.SetQueueFromSearch(query, extras);
                HandlePlayRequest();
                queueManager.UpdateMetadata();
            };

            Playback = playback;
            Playback.Callback = this;
        }

        public void HandlePlayRequest()
        {
            LogHelper.Debug(Tag, "handlePlayRequest: mState=" + Playback.State);
            MediaSessionCompat.QueueItem currentMusic = queueManager.CurrentMusic;
            if (currentMusic != null)
            {
                serviceCallback.OnPlaybackStart();
                Playback.Play(currentMusic);
            }
        }

        public void HandlePauseRequest()
        {
            LogHelper.Debug(Tag, "handlePauseRequest: mState=" + Playback.State);
            if (Playback.IsPlaying)
            {
                Playback.Pause();
                serviceCallback.OnPlaybackStop();
            }
        }

        public void HandleStopRequest(string withError)
        {
            LogHelper.Debug(Tag, "handleStopRequest: mState=" + Playback.State + " error=", withError);
            Playback.Stop(true);
            serviceCallback.OnPlaybackStop();
            UpdatePlaybackState(withError);
        }

        public void UpdatePlaybackState(string error)
        {
            LogHelper.Debug(Tag, "updatePlaybackState, playback state=" + Playback.State);
            long position = PlaybackStateCompat.PlaybackPositionUnknown;
            if (Playback != null && Playback.IsConnected)
            {
                position = Playback.CurrentStreamPosition;
            }

            //noinspection ResourceType
            PlaybackStateCompat.Builder stateBuilder = new PlaybackStateCompat.Builder()
                    .SetActions(GetAvailableActions());

            //setCustomAction(stateBuilder);
            int state = Playback.State;

            // If there is an error message, send it to the playback state:
            if (error != null)
            {
                // Error states are really only supposed to be used for errors that cause playback to
                // stop unexpectedly and persist until the user takes action to fix it.
                stateBuilder.SetErrorMessage(error);
                state = PlaybackStateCompat.StateError;
            }
            //noinspection ResourceType
            stateBuilder.SetState(state, position, 1.0f, SystemClock.ElapsedRealtime());

            // Set the activeQueueItemId if the current index is valid.
            MediaSessionCompat.QueueItem currentMusic = queueManager.CurrentMusic;
            if (currentMusic != null)
            {
                stateBuilder.SetActiveQueueItemId(currentMusic.QueueId);
            }

            serviceCallback.OnPlaybackStateUpdated(stateBuilder.Build());

            if (state == PlaybackStateCompat.StatePlaying ||
                    state == PlaybackStateCompat.StatePaused)
            {
                serviceCallback.OnNotificationRequired();
            }
        }

        private long GetAvailableActions()
        {
            long actions =
                    PlaybackStateCompat.ActionPlay |
                    PlaybackStateCompat.ActionPlayFromMediaId |
                    PlaybackStateCompat.ActionPlayFromSearch |
                    PlaybackStateCompat.ActionSkipToPrevious |
                    PlaybackStateCompat.ActionSkipToNext;

            if (Playback.IsPlaying)
            {
                actions |= PlaybackStateCompat.ActionPause;
            }

            return actions;
        }

        public void OnCompletion()
        {
            // The media player finished playing the current song, so we go ahead
            // and start the next.
            if (queueManager.SkipQueuePosition(1))
            {
                HandlePlayRequest();
                queueManager.UpdateMetadata();
            }
            else
            {
                // If skipping was not possible, we stop and release the resources:
                HandleStopRequest(null);
            }
        }

        public void OnPlaybackStatusChanged(int state)
        {
            UpdatePlaybackState(null);
        }

        public void OnError(string error)
        {
            UpdatePlaybackState(error);
        }

        public void SetCurrentMediaId(string mediaId)
        {
            LogHelper.Debug(Tag, "setCurrentMediaId", mediaId);
            queueManager.SetQueueFromMusic(mediaId);
        }

        public void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate)
        {
            serviceCallback.OnFftDataCapture(visualizer, fft, samplingRate);
        }

        public void SwitchToPlayback(IPlayback playback, bool resumePlaying)
        {
            if (playback == null)
            {
                throw new ArgumentException("Playback cannot be null");
            }
            // suspend the current one.
            int oldState = Playback.State;
            int pos = Playback.CurrentStreamPosition;
            var currentMediaId = Playback.CurrentMediaId;
            Playback.Stop(false);
            Playback.Callback = this;
            Playback.CurrentStreamPosition = pos < 0 ? 0 : pos;
            Playback.CurrentMediaId = currentMediaId;
            Playback.Start();
            // finally swap the instance
            Playback = playback;
            switch (oldState)
            {
                case PlaybackStateCompat.StateBuffering:
                case PlaybackStateCompat.StateConnecting:
                case PlaybackStateCompat.StatePaused:
                    Playback.Pause();
                    break;
                case PlaybackStateCompat.StatePlaying:
                    MediaSessionCompat.QueueItem currentMusic = queueManager.CurrentMusic;
                    if (resumePlaying && currentMusic != null)
                    {
                        Playback.Play(currentMusic);
                    }
                    else if (!resumePlaying)
                    {
                        Playback.Pause();
                    }
                    else
                    {
                        Playback.Stop(true);
                    }
                    break;
                case PlaybackStateCompat.StateNone:
                    break;
                default:
                    LogHelper.Debug(Tag, "Default called. Old state is ", oldState);
                    break;
            }
        }

        class MediaSessionCallback : MediaSessionCompat.Callback
        {
            public Action OnPlayImpl { get; set; }
            public Action<long> OnSkipToQueueItemImpl { get; set; }
            public Action<long> OnSeekToImpl { get; set; }
            public Action<string, Bundle> OnPlayFromMediaIdImpl { get; set; }
            public Action OnPauseImpl { get; set; }
            public Action OnStopImpl { get; set; }
            public Action OnSkipToNextImpl { get; set; }
            public Action OnSkipToPreviousImpl { get; set; }
            public Action<string, Bundle> OnPlayFromSearchImpl { get; set; }

            public override void OnPlay()
            {
                OnPlayImpl();
            }

            public override void OnSkipToQueueItem(long queueId)
            {
                OnSkipToQueueItemImpl(queueId);
            }

            public override void OnSeekTo(long position)
            {
                OnSeekToImpl(position);
            }

            public override void OnPlayFromMediaId(string mediaId, Bundle extras)
            {
                OnPlayFromMediaIdImpl(mediaId, extras);
            }

            public override void OnPause()
            {
                OnPauseImpl();
            }

            public override void OnStop()
            {
                OnStopImpl();
            }

            public override void OnSkipToNext()
            {
                OnSkipToNextImpl();
            }

            public override void OnSkipToPrevious()
            {
                OnSkipToPreviousImpl();
            }

            public override void OnPlayFromSearch(string query, Bundle extras)
            {
                OnPlayFromSearchImpl(query, extras);
            }
        }
    }

    public interface IPlaybackServiceCallback
    {
        void OnPlaybackStart();

        void OnNotificationRequired();

        void OnPlaybackStop();

        void OnPlaybackStateUpdated(PlaybackStateCompat newState);

        void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate);
    }
}