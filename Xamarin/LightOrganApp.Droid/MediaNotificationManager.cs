using System;

using Android.App;
using Android.Content;
using LightOrganApp.Droid.Utils;
using Android.Support.V4.Media.Session;
using Android.Support.V4.Media;
using Android.Graphics;
using Android.Support.V4.App;

namespace LightOrganApp.Droid
{
    public class MediaNotificationManager : BroadcastReceiver
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(MediaNotificationManager));

        const int NotificationId = 412;
        const int RequestCode = 100;

        public const string ActionPause = "com.apps.kruszyn.lightorganapp.droid.pause";
        public const string ActionPlay = "com.apps.kruszyn.lightorganapp.droid.play";
        public const string ActionPrev = "com.apps.kruszyn.lightorganapp.droid.prev";
        public const string ActionNext = "com.apps.kruszyn.lightorganapp.droid.next";

        readonly MusicService service;
        MediaSessionCompat.Token sessionToken;
        MediaControllerCompat controller;
        MediaControllerCompat.TransportControls transportControls;

        PlaybackStateCompat playbackState;
        MediaMetadataCompat metadata;

        readonly NotificationManagerCompat notificationManager;

        PendingIntent pauseIntent;
        PendingIntent playIntent;
        PendingIntent previousIntent;
        PendingIntent nextIntent;

        int notificationColor;

        bool started;

        class MediaCallback : MediaControllerCompat.Callback
        {
            public Action<PlaybackStateCompat> OnPlaybackStateChangedImpl { get; set; }
            public Action<MediaMetadataCompat> OnMetadataChangedImpl { get; set; }
            public Action OnSessionDestroyedImpl { get; set; }

            public override void OnPlaybackStateChanged(PlaybackStateCompat state)
            {
                OnPlaybackStateChangedImpl(state);
            }

            public override void OnMetadataChanged(MediaMetadataCompat meta)
            {
                OnMetadataChangedImpl(meta);
            }

            public override void OnSessionDestroyed()
            {
                base.OnSessionDestroyed();
                OnSessionDestroyedImpl();
            }
        }

        readonly MediaCallback mCb = new MediaCallback();

        public MediaNotificationManager(MusicService serv)
        {
            service = serv;
            UpdateSessionToken();

            notificationColor = ResourceHelper.GetThemeColor(service,
                Android.Resource.Attribute.ColorPrimary, Color.DarkGray);

            notificationManager = NotificationManagerCompat.From(service);

            string pkg = service.PackageName;
            pauseIntent = PendingIntent.GetBroadcast(service, RequestCode,
                new Intent(ActionPause).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
            playIntent = PendingIntent.GetBroadcast(service, RequestCode,
                new Intent(ActionPlay).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
            previousIntent = PendingIntent.GetBroadcast(service, RequestCode,
                new Intent(ActionPrev).SetPackage(pkg), PendingIntentFlags.CancelCurrent);
            nextIntent = PendingIntent.GetBroadcast(service, RequestCode,
                new Intent(ActionNext).SetPackage(pkg), PendingIntentFlags.CancelCurrent);

            notificationManager.CancelAll();

            mCb.OnPlaybackStateChangedImpl = (state) => {
                playbackState = state;
                LogHelper.Debug(Tag, "Received new playback state", state);
                if (state.State == PlaybackStateCompat.StateStopped ||
                    state.State == PlaybackStateCompat.StateNone)
                {
                    StopNotification();
                }
                else
                {
                    Notification notification = CreateNotification();
                    if (notification != null)
                    {
                        notificationManager.Notify(NotificationId, notification);
                    }
                }
            };

            mCb.OnMetadataChangedImpl = (meta) => {
                metadata = meta;
                LogHelper.Debug(Tag, "Received new metadata ", metadata);
                Notification notification = CreateNotification();
                if (notification != null)
                {
                    notificationManager.Notify(NotificationId, notification);
                }
            };

            mCb.OnSessionDestroyedImpl = () => {
                LogHelper.Debug(Tag, "Session was destroyed, resetting to the new session token");
                try
                {
                    UpdateSessionToken();
                }
                catch(Exception e)
                {
                    LogHelper.Error(Tag, e, "could not connect media controller");
                }
            };
        }

        public void StartNotification()
        {
            if (!started)
            {
                metadata = controller.Metadata;
                playbackState = controller.PlaybackState;

                // The notification must be updated after setting started to true
                Notification notification = CreateNotification();
                if (notification != null)
                {
                    controller.RegisterCallback(mCb);
                    var filter = new IntentFilter();
                    filter.AddAction(ActionNext);
                    filter.AddAction(ActionPause);
                    filter.AddAction(ActionPlay);
                    filter.AddAction(ActionPrev);
                    service.RegisterReceiver(this, filter);

                    service.StartForeground(NotificationId, notification);
                    started = true;
                }
            }
        }

        public void StopNotification()
        {
            if (started)
            {
                started = false;
                controller.UnregisterCallback(mCb);
                try
                {
                    notificationManager.Cancel(NotificationId);
                    service.UnregisterReceiver(this);
                }
                catch (ArgumentException)
                {
                    // ignore if the receiver is not registered.
                }
                service.StopForeground(true);
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;
            LogHelper.Debug(Tag, "Received intent with action " + action);
            switch (action)
            {
                case ActionPause:
                    transportControls.Pause();
                    break;
                case ActionPlay:
                    transportControls.Play();
                    break;
                case ActionNext:
                    transportControls.SkipToNext();
                    break;
                case ActionPrev:
                    transportControls.SkipToPrevious();
                    break;
                default:
                    LogHelper.Warn(Tag, "Unknown intent ignored. Action=", action);
                    break;
            }
        }

        void UpdateSessionToken()
        {
            var freshToken = service.SessionToken;
            if (sessionToken == null && freshToken != null ||
                sessionToken != null && sessionToken != freshToken)
            {                
                controller?.UnregisterCallback(mCb);
                
                sessionToken = freshToken;
                if (sessionToken != null)
                {
                    controller = new MediaControllerCompat(service, sessionToken);
                    transportControls = controller.GetTransportControls();
                    if (started)
                    {
                        controller.RegisterCallback(mCb);
                    }
                }                    
            }
        }            

        Notification CreateNotification()
        {
            LogHelper.Debug(Tag, "updateNotificationMetadata. mMetadata=" + metadata);
            if (metadata == null || playbackState == null)
            {
                return null;
            }

            var notificationBuilder = new Android.Support.V7.App.NotificationCompat.Builder(service);
            int playPauseButtonPosition = 0;

            // If skip to previous action is enabled
            if ((playbackState.Actions & PlaybackStateCompat.ActionSkipToPrevious) != 0)
            {
                notificationBuilder.AddAction(Resource.Drawable.ic_skip_previous_white_24dp,
                    "Previous", previousIntent);

                playPauseButtonPosition = 1;
            }

            AddPlayPauseAction(notificationBuilder);

            // If skip to next action is enabled
            if ((playbackState.Actions & PlaybackStateCompat.ActionSkipToNext) != 0)
            {
                notificationBuilder.AddAction(Resource.Drawable.ic_skip_next_white_24dp,
                    "Next", nextIntent);
            }

            MediaDescriptionCompat description = metadata.Description;

            string fetchArtUrl = null;
            Bitmap art = null;
            if (description.IconUri != null)
            {
                String artUrl = description.IconUri.ToString();
                art = AlbumArtCache.Instance.GetBigImage(artUrl);
                if (art == null)
                {
                    fetchArtUrl = artUrl;
                    art = BitmapFactory.DecodeResource(service.Resources,
                        Resource.Drawable.ic_default_art);
                }
            }

            notificationBuilder
                .SetStyle(new Android.Support.V7.App.NotificationCompat.MediaStyle()
                    .SetShowActionsInCompactView(
                        new[] { playPauseButtonPosition })  // show only play/pause in compact view
                    .SetMediaSession(sessionToken))
                .SetColor(notificationColor)
                .SetSmallIcon(Resource.Drawable.ic_notification)
                .SetVisibility(Android.Support.V7.App.NotificationCompat.VisibilityPublic)
                .SetUsesChronometer(true)
                //.SetContentIntent(CreateContentIntent())
                .SetContentTitle(description.Title)
                .SetContentText(description.Subtitle)
                .SetLargeIcon(art);

            SetNotificationPlaybackState(notificationBuilder);
            if (fetchArtUrl != null)
            {
                FetchBitmapFromURL(fetchArtUrl, notificationBuilder);
            }

            return notificationBuilder.Build();
        }

        void AddPlayPauseAction(Android.Support.V7.App.NotificationCompat.Builder builder)
        {
            LogHelper.Debug(Tag, "updatePlayPauseAction");
            string label;
            int icon;
            PendingIntent intent;

            if (playbackState.State == PlaybackStateCompat.StatePlaying)
            {
                label = "Pause";
                icon = Resource.Drawable.ic_pause_white_24dp;
                intent = pauseIntent;
            }
            else
            {
                label = "Play";
                icon = Resource.Drawable.ic_play_arrow_white_24dp;
                intent = playIntent;
            }
            builder.AddAction(new Android.Support.V7.App.NotificationCompat.Action(icon, label, intent));
        }

        void SetNotificationPlaybackState(Android.Support.V7.App.NotificationCompat.Builder builder)
        {
            var beginningOfTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            LogHelper.Debug(Tag, "updateNotificationPlaybackState. mPlaybackState=" + playbackState);
            if (playbackState == null || !started)
            {
                LogHelper.Debug(Tag, "updateNotificationPlaybackState. cancelling notification!");
                service.StopForeground(true);
                return;
            }
            if (playbackState.State == PlaybackStateCompat.StatePlaying
                && playbackState.Position >= 0)
            {
                var timespan = ((long)(DateTime.UtcNow - beginningOfTime).TotalMilliseconds) - playbackState.Position;
                LogHelper.Debug(Tag, "updateNotificationPlaybackState. updating playback position to ", timespan / 1000, " seconds");

                builder.SetWhen(timespan).SetShowWhen(true).SetUsesChronometer(true);
            }
            else
            {
                LogHelper.Debug(Tag, "updateNotificationPlaybackState. hiding playback position");
                builder.SetWhen(0).SetShowWhen(false).SetUsesChronometer(false);
            }

            // Make sure that the notification can be dismissed by the user when we are not playing:
            builder.SetOngoing(playbackState.State == PlaybackStateCompat.StatePlaying);
        }

        void FetchBitmapFromURL(string bitmapUrl, Android.Support.V7.App.NotificationCompat.Builder builder)
        {
            AlbumArtCache.Instance.Fetch(bitmapUrl, new AlbumArtCache.FetchListener()
            {
                OnFetched = (artUrl, bitmap, icon) => {
                    if (metadata != null && metadata.Description != null && metadata.Description.IconUri != null &&
                        artUrl == metadata.Description.IconUri.ToString())
                    {
                        LogHelper.Debug(Tag, "fetchBitmapFromURLAsync: set bitmap to ", artUrl);
                        builder.SetLargeIcon(bitmap);
                        notificationManager.Notify(NotificationId, builder.Build());
                    }
                }
            });
        }
    }
}