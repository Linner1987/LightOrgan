using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Support.V4.Media;
using LightOrganApp.Droid.Playback;
using Android.Media.Audiofx;
using LightOrganApp.Droid.Utils;
using LightOrganApp.Droid.Model;
using Android.Support.V4.Media.Session;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Preferences;
using Android.Text;
using LightOrganApp.Shared;
using System.Threading.Tasks;

namespace LightOrganApp.Droid
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "android.media.browse.MediaBrowserService" })]
    public class MusicService : MediaBrowserServiceCompat, IPlaybackServiceCallback
    {
        public const string ActionCmd = "com.example.android.uamp.ACTION_CMD";
        public const string CmdName = "CMD_NAME";
        public const string CmdPause = "CMD_PAUSE";

        public const string ActionLightOrganDataChanged = "com.apps.kruszyn.lightorganapp.droid.ACTION_LIGHT_ORGAN_DATA_CHANGED";
        public const string BassLevel = "bass_level";
        public const string MidLevel = "mid_level";
        public const string TrebleLevel = "treble_level";

        static readonly string Tag = LogHelper.MakeLogTag(typeof(MusicService));
        const int StopDelay = 30000;
        const string MediaIdRoot = "__ROOT__";

        MusicProvider musicProvider;
        PlaybackManager playbackManager;

        MediaSessionCompat session;
        MediaNotificationManager mediaNotificationManager;
        DelayedStopHandler delayedStopHandler;

        PackageValidator packageValidator;

        LightOrganProcessor lightOrganProcessor;

        LightsRemoteController remoteController;

        PreferenceListener prefListener;


        public MusicService()
        {
            delayedStopHandler = new DelayedStopHandler(this);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            LogHelper.Debug(Tag, "onCreate");

            musicProvider = new MusicProvider();

            var context = ApplicationContext;

            var hasReadExternalStoragePermission = ActivityCompat.CheckSelfPermission(context, Manifest.Permission.ReadExternalStorage);

            if (hasReadExternalStoragePermission == Permission.Granted)
            {                
                musicProvider.RetrieveMediaAsync(context, null);
            }
            else
            {
                var msg = Resources.GetString(Resource.String.permission_denied_msg, "READ_EXTERNAL_STORAGE");
                Toast.MakeText(context, msg, ToastLength.Short).Show();
            }

            packageValidator = new PackageValidator(this);

            lightOrganProcessor = new LightOrganProcessor();
            lightOrganProcessor.LightOrganDataUpdated += (s, e) =>
            {
                OnLightOrganDataUpdated(e.Data);
            };

            var queueManager = new QueueManager(musicProvider, Resources,
               new QueueManager.MetadataUpdateListener
               {
                    OnMetadataChanged = (metadata) =>
                    {
                        session.SetMetadata(metadata);
                    }, 
                          
                    OnMetadataRetrieveError = () =>
                    {
                        playbackManager.UpdatePlaybackState(Resources.GetString(Resource.String.no_metadata_msg));
                    },
                            
                    OnCurrentQueueIndexUpdated = (queueIndex) =>
                    {
                        playbackManager.HandlePlayRequest();
                    },
        
                    OnQueueUpdated = (title, newQueue) =>
                    {
                        session.SetQueue(newQueue);
                        session.SetQueueTitle(title);
                    }
                });

            var hasRecordAudioPermission = ActivityCompat.CheckSelfPermission(context, Manifest.Permission.RecordAudio);

            if (hasRecordAudioPermission != Permission.Granted)
            {
                var msg = Resources.GetString(Resource.String.permission_denied_msg, "RECORD_AUDIO");
                Toast.MakeText(context, msg, ToastLength.Short).Show();
            }

            var playback = new LocalPlayback(this, musicProvider, hasRecordAudioPermission == Permission.Granted);
            playbackManager = new PlaybackManager(this, Resources, musicProvider, queueManager, playback);

            // Start a new MediaSession
            session = new MediaSessionCompat(this, "MusicService");
            SessionToken = session.SessionToken;
            session.SetCallback(playbackManager.GetMediaSessionCallback());
            session.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons |
                    MediaSessionCompat.FlagHandlesTransportControls);

            var intent = new Intent(context, typeof(MainActivity));
            var pi = PendingIntent.GetActivity(context, 99 /*request code*/,
                intent, PendingIntentFlags.UpdateCurrent);
            session.SetSessionActivity(pi);

            playbackManager.UpdatePlaybackState(null);

            try
            {
                mediaNotificationManager = new MediaNotificationManager(this);
            }
            catch (Exception e)
            {
                throw new Exception("Could not create a MediaNotificationManager", e);
            }
        }
        
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var preferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);

            var useRemoteDevice = preferences.GetBoolean(PreferencesHelper.KeyPrefUseRemoteDevice, false);

            if (useRemoteDevice)
                Task.Run(async () => await CreateNewRemoteController(preferences));

            prefListener = new PreferenceListener();
            prefListener.OnSharedPreferenceChangedImpl = OnPreferenceChanged;
            preferences.RegisterOnSharedPreferenceChangeListener(prefListener);


            if (intent != null)
            {
                var action = intent.Action;
                var command = intent.GetStringExtra(CmdName);
                if (ActionCmd == action)
                {
                    if (CmdPause == command)
                    {
                        playbackManager.HandlePauseRequest();
                    }
                }
                else
                {
                    // Try to handle the intent as a media button event wrapped by MediaButtonReceiver
                    MediaButtonReceiver.HandleIntent(session, intent);
                }
            }
            // Reset the delay handler to enqueue a message to stop the service if
            // nothing is playing.
            delayedStopHandler.RemoveCallbacksAndMessages(null);
            delayedStopHandler.SendEmptyMessageDelayed(0, StopDelay);

            return StartCommandResult.Sticky;
        }

        public async override void OnDestroy()
        {
            LogHelper.Debug(Tag, "onDestroy");

            await ReleaseRemoteController();

            // Service is being killed, so make sure we release our resources
            playbackManager.HandleStopRequest(null);
            mediaNotificationManager.StopNotification();
            delayedStopHandler.RemoveCallbacksAndMessages(null);
            session.Release();
        }

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            LogHelper.Debug(Tag, "OnGetRoot: clientPackageName=" + clientPackageName,
                "; clientUid=" + clientUid + " ; rootHints=", rootHints);           
            // To ensure you are not allowing any arbitrary app to browse your app's contents, you
            // need to check the origin:
            if (!packageValidator.IsCallerAllowed(this, clientPackageName, clientUid))
            {
                // If the request comes from an untrusted package, return null. No further calls will
                // be made to other media browsing methods.
                LogHelper.Warn(Tag, "OnGetRoot: IGNORING request from untrusted package "
                + clientPackageName);
                return null;
            }

            return new BrowserRoot(MediaIdRoot, null);
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            LogHelper.Debug(Tag, "OnLoadChildren: parentMediaId=", parentId);
            result.SendResult(musicProvider.GetChildren(parentId, Resources));
        }

        public void OnPlaybackStart()
        {
            if (!session.Active)
            {
                session.Active = true;
            }

            delayedStopHandler.RemoveCallbacksAndMessages(null);

            // The service needs to continue running even after the bound client (usually a
            // MediaController) disconnects, otherwise the music playback will stop.
            // Calling startService(Intent) will keep the service running until it is explicitly killed.
            StartService(new Intent(ApplicationContext, typeof(MusicService)));
        }

        public void OnPlaybackStop()
        {
            // Reset the delayed stop handler, so after STOP_DELAY it will be executed again,
            // potentially stopping the service.
            delayedStopHandler.RemoveCallbacksAndMessages(null);
            delayedStopHandler.SendEmptyMessageDelayed(0, StopDelay);
            StopForeground(true);
        }

        public void OnNotificationRequired()
        {
            mediaNotificationManager.StartNotification();
        }        

        public void OnPlaybackStateUpdated(PlaybackStateCompat newState)
        {
            session.SetPlaybackState(newState);
        }

        public void OnFftDataCapture(Visualizer visualizer, byte[] fft, int samplingRate)
        {
            lightOrganProcessor.ProcessFftData(visualizer, fft, samplingRate);
        }

        private async void OnLightOrganDataUpdated(LightOrganData data)
        {
            Intent broadcastIntent = new Intent();
            broadcastIntent.SetAction(ActionLightOrganDataChanged);
            broadcastIntent.PutExtra(BassLevel, data.BassLevel);
            broadcastIntent.PutExtra(MidLevel, data.MidLevel);
            broadcastIntent.PutExtra(TrebleLevel, data.TrebleLevel);
            LocalBroadcastManager.GetInstance(this).SendBroadcast(broadcastIntent);

            byte bassValue = (byte)Math.Round(255 * data.BassLevel);
            byte midValue = (byte)Math.Round(255 * data.MidLevel);
            byte trebleValue = (byte)Math.Round(255 * data.TrebleLevel);

            byte[] bytes = new byte[3];
            bytes[0] = bassValue;
            bytes[1] = midValue;
            bytes[2] = trebleValue;

            await SendCommand(bytes);
        }

        private async Task CreateNewRemoteController(ISharedPreferences preferences)
        {
            var host = preferences.GetString(PreferencesHelper.KeyPrefRemoteDeviceHost, "");
            //int port = preferences.GetInt(PreferencesHelper.KeyPrefRemoteDevicePort, 0);
            int port = 0;
            int.TryParse(preferences.GetString(PreferencesHelper.KeyPrefRemoteDevicePort, "0"), out port);

            if (!TextUtils.IsEmpty(host) && port > 0)
            {
                remoteController = new LightsRemoteController();
                await remoteController.ConnectAsync(host, port);
            }
        }

        private async Task SendCommand(byte[] bytes)
        {
            if (remoteController != null)
                await remoteController.SendCommandAsync(bytes);
        }

        private async Task ReleaseRemoteController()
        {
            if (remoteController != null)
            {
                await remoteController.CloseAsync();
                remoteController = null;
            }
        }

        private async void OnPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {

                bool useRemoteDevice = sharedPreferences.GetBoolean(PreferencesHelper.KeyPrefUseRemoteDevice, false);

                if (key == PreferencesHelper.KeyPrefUseRemoteDevice)
                {
                    if (remoteController != null && !useRemoteDevice)
                    {
                        await ReleaseRemoteController();
                    }
                    else if (remoteController == null && useRemoteDevice)
                    {
                        await CreateNewRemoteController(sharedPreferences);
                    }
                }
                else if (key == PreferencesHelper.KeyPrefRemoteDeviceHost || key == PreferencesHelper.KeyPrefRemoteDevicePort)
                {
                    if (useRemoteDevice)
                    {
                        await ReleaseRemoteController();
                        await CreateNewRemoteController(sharedPreferences);
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(Tag, e);
            }
        }


        class DelayedStopHandler : Handler
        {
            readonly WeakReference<MusicService> weakReference;

            public DelayedStopHandler(MusicService service)
            {
                weakReference = new WeakReference<MusicService>(service);
            }

            public override void HandleMessage(Message msg)
            {
                MusicService service;
                weakReference.TryGetTarget(out service);
                if (service != null && service.playbackManager.Playback != null)
                {
                    if (service.playbackManager.Playback.IsPlaying)
                    {
                        LogHelper.Debug(Tag, "Ignoring delayed stop since the media player is in use.");
                        return;
                    }
                    LogHelper.Debug(Tag, "Stopping service with delay handler.");
                    service.StopSelf();                    
                }
            }
        }

        class PreferenceListener: Java.Lang.Object, ISharedPreferencesOnSharedPreferenceChangeListener
        {  
            public Action<ISharedPreferences,string> OnSharedPreferenceChangedImpl { get; set; }

            public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
            {
                OnSharedPreferenceChangedImpl(sharedPreferences, key);
            }
        }
}
}