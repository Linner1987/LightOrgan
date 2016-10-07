using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Media;
using LightOrganApp.Messages;
using Plugin.Permissions;
using Xamarin.Forms;
using System;
using LightOrganApp.Droid.Utils;
using Android.Support.V4.Media.Session;
using System.Collections.Generic;
using System.Linq;
using Android.Text.Format;
using LightOrganApp.Model;

namespace LightOrganApp.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(MainActivity));

        private string mediaId;

        private MediaBrowserCompat mediaBrowser;

        class Callback : MediaControllerCompat.Callback
        {
            public Action<PlaybackStateCompat> OnPlaybackStateChangedImpl { get; set; }
            public Action<MediaMetadataCompat> OnMetadataChangedImpl { get; set; }

            public override void OnPlaybackStateChanged(PlaybackStateCompat state)
            {
                OnPlaybackStateChangedImpl(state);
            }

            public override void OnMetadataChanged(MediaMetadataCompat metadata)
            {
                OnMetadataChangedImpl(metadata);
            }
        }

        class ConnectionCallback : MediaBrowserCompat.ConnectionCallback
        {
            public Action OnConnectedImpl { get; set; }

            public override void OnConnected()
            {
                OnConnectedImpl();
            }
        }

        class SubscriptionCallback : MediaBrowserCompat.SubscriptionCallback
        {
            public Action<string, IList<MediaBrowserCompat.MediaItem>> OnChildrenLoadedImpl { get; set; }
            public Action<string> OnErrorImpl { get; set; }

            public override void OnChildrenLoaded(string parentId, IList<MediaBrowserCompat.MediaItem> children)
            {
                OnChildrenLoadedImpl(parentId, children);
            }

            public override void OnError(string id)
            {
                OnErrorImpl(id);
            }
        }

        readonly Callback mediaControllerCallback = new Callback();

        readonly ConnectionCallback connectionCallback = new ConnectionCallback();

        readonly SubscriptionCallback subscriptionCallback = new SubscriptionCallback();
      

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());

            // Connect a media browser just to get the media session token. There are other ways
            // this can be done, for example by sharing the session token directly.
            mediaBrowser = new MediaBrowserCompat(this,
                new ComponentName(this, Java.Lang.Class.FromType(typeof(MusicService))), connectionCallback, null);

            mediaControllerCallback.OnPlaybackStateChangedImpl = (state) =>
            {
                if (ShouldShowControls)
                {
                    ShowPlaybackControls();
                }
                else
                {
                    LogHelper.Debug(Tag, "mediaControllerCallback.onPlaybackStateChanged: " +
                            "hiding controls because state is ", state.State);
                    HidePlaybackControls();
                }

                LogHelper.Debug(Tag, "Received playback state change to state ", state.State);
                OnPlaybackStateChanged(state);
            };

            mediaControllerCallback.OnMetadataChangedImpl = (metadata) =>
            {
                if (ShouldShowControls)
                {
                    ShowPlaybackControls();
                }
                else
                {
                    LogHelper.Debug(Tag, "mediaControllerCallback.onMetadataChanged: " +
                        "hiding controls because metadata is null");
                    HidePlaybackControls();
                }

                if (metadata != null)
                {
                    LogHelper.Debug(Tag, "Received metadata state change to mediaId=",
                        metadata.Description.MediaId,
                        " song=", metadata.Description.Title);
                    OnMetadataChanged(metadata);
                }
            };

            connectionCallback.OnConnectedImpl = () =>
            {
                LogHelper.Debug(Tag, "onConnected");
                try
                {
                    ConnectToSession(mediaBrowser.SessionToken);
                }
                catch (RemoteException e)
                {
                    LogHelper.Error(Tag, e, "could not connect media controller");
                    HidePlaybackControls();
                }
            };

            subscriptionCallback.OnChildrenLoadedImpl = (parentId, children) =>
            {                
                OnChildrenLoaded(children);                
            };

            subscriptionCallback.OnErrorImpl = (id) =>
            {
                LogHelper.Error(Tag, "browse subscription onError, id=" + id);
            };

            WireUpMediaBrowser();
        }

        void WireUpMediaBrowser()
        {
            MessagingCenter.Subscribe<MediaBrowserConnectMessage>(this, nameof(MediaBrowserConnectMessage), message =>
            {
                mediaBrowser.Connect();
            });
        }

        protected override void OnStart()
        {
            base.OnStart();

            HidePlaybackControls();

            LogHelper.Debug(Tag, "fragment.onStart, mediaId=", mediaId,
                    "  onConnected=" + mediaBrowser.IsConnected);

            if (mediaBrowser.IsConnected)
            {
                OnConnected();
            }            
        }

        protected override void OnStop()
        {
            base.OnStop();
            LogHelper.Debug(Tag, "Activity onStop");

            SupportMediaController?.UnregisterCallback(mediaControllerCallback);

            if (mediaBrowser != null && mediaBrowser.IsConnected && mediaId != null)
            {
                mediaBrowser.Unsubscribe(mediaId);
            }

            mediaBrowser?.Disconnect();            
        }      

        private bool ShouldShowControls
        {
            get
            {
                var mediaController = SupportMediaController;
                if (mediaController == null ||
                    mediaController.Metadata == null ||
                    mediaController.PlaybackState == null)
                {
                    return false;
                }
                switch (mediaController.PlaybackState.State)
                {
                    case PlaybackStateCompat.StateError:
                    case PlaybackStateCompat.StateNone:
                    case PlaybackStateCompat.StateStopped:
                        return false;
                    default:
                        return true;
                }
            }
        }

        private void OnConnected()
        {
            mediaId = mediaBrowser.Root;

            // Unsubscribing before subscribing is required if this mediaId already has a subscriber
            // on this MediaBrowser instance. Subscribing to an already subscribed mediaId will replace
            // the callback, but won't trigger the initial callback.onChildrenLoaded.
            //
            // This is temporary: A bug is being fixed that will make subscribe
            // consistently call onChildrenLoaded initially, no matter if it is replacing an existing
            // subscriber or not. Currently this only happens if the mediaID has no previous
            // subscriber or if the media content changes on the service side, so we need to
            // unsubscribe first.
            mediaBrowser.Unsubscribe(mediaId);

            mediaBrowser.Subscribe(mediaId, subscriptionCallback);

            // Add MediaController callback so we can redraw the list when metadata changes: 
            var mediaController = SupportMediaController;
            if (mediaController != null)
            {
                OnMetadataChanged(mediaController.Metadata);
                OnPlaybackStateChanged(mediaController.PlaybackState);
                mediaController.RegisterCallback(mediaControllerCallback);
            }            
        }

        private void ConnectToSession(MediaSessionCompat.Token token)
        {
            SupportMediaController = new MediaControllerCompat(this, token);            

            if (ShouldShowControls)
            {
                ShowPlaybackControls();
            }
            else
            {
                LogHelper.Debug(Tag, "connectionCallback.onConnected: " +
                    "hiding controls because metadata is null");
                HidePlaybackControls();
            }

            OnConnected();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void OnChildrenLoaded(IList<MediaBrowserCompat.MediaItem> children)
        {
            var message = new MediaItemsLoadedMessage
            {
                Items = children
                .Where(item => item.IsPlayable)
                .Select(item => new LightOrganApp.Model.MediaItem(item.MediaId, item.Description.Title, item.Description.Subtitle, 
                    DateUtils.FormatElapsedTime(item.Description.Extras.GetLong(MediaMetadataCompat.MetadataKeyDuration) / 1000)))
                .ToList()
            };
            MessagingCenter.Send(message, nameof(MediaItemsLoadedMessage));
        }

        private void OnPlaybackStateChanged(PlaybackStateCompat state)
        {
            var message = new PlaybackStateChangedMessage
            {
                State = (PlaybackState)state.State
            };
            MessagingCenter.Send(message, nameof(PlaybackStateChangedMessage));     
        }

        private void OnMetadataChanged(MediaMetadataCompat metadata)
        {
            if (metadata == null)
                return;

            var message = new MetadataChangedMessage
            {
                Metadata = new MediaMetadata { Title = metadata.Description.Title, Artist = metadata.Description.Subtitle }
            };
            MessagingCenter.Send(message, nameof(MetadataChangedMessage));
        }

        private void ShowPlaybackControls()
        {
            var message = new ShowPlaybackControlsMessage();
            MessagingCenter.Send(message, nameof(ShowPlaybackControlsMessage));
        }

        private void HidePlaybackControls()
        {

        }

        private void OnPlayOrPause()
        {
            var controller = SupportMediaController;
            var stateObj = controller.PlaybackState;
            int state = stateObj == null ?
                    PlaybackStateCompat.StateNone : stateObj.State;            
            
            LogHelper.Debug(Tag, "Play button pressed, in state " + state);
            if (state == PlaybackStateCompat.StatePaused ||
                    state == PlaybackStateCompat.StateStopped ||
                    state == PlaybackStateCompat.StateNone)
            {
                PlayMedia();
            }
            else if (state == PlaybackStateCompat.StatePlaying ||
                    state == PlaybackStateCompat.StateBuffering ||
                    state == PlaybackStateCompat.StateConnecting)
            {
                PauseMedia();
            }                      
        }

        private void PlayMedia()
        {
            SupportMediaController?.GetTransportControls().Play();
        }

        private void PauseMedia()
        {
            SupportMediaController?.GetTransportControls().Pause();
        }

        private void PlayFromMediaId(string mediaId)
        {
            SupportMediaController.GetTransportControls()
                        .PlayFromMediaId(mediaId, null);
        }
    }
}

