using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Support.V7.App;
using Android.Widget;
using LightOrganApp.Droid.Utils;
using System;
using System.Collections.Generic;

namespace LightOrganApp.Droid.UI
{
    public abstract class BaseActivity: AppCompatActivity, IMediaBrowserProvider
    {
        static readonly string Tag = LogHelper.MakeLogTag(typeof(BaseActivity));

        private MediaBrowserCompat mediaBrowser;
        private PlaybackControlsFragment controlsFragment;

        private const int RequestCodeAskPermissions = 123;              

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

        class ConnectionCallback: MediaBrowserCompat.ConnectionCallback
        {
            public Action OnConnectedImpl { get; set; }

            public override void OnConnected()
            {
                OnConnectedImpl();
            }
        }

        readonly Callback mediaControllerCallback = new Callback();

        readonly ConnectionCallback connectionCallback = new ConnectionCallback();


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            LogHelper.Debug(Tag, "Activity onCreate");

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
        }

        protected override void OnStart()
        {
            base.OnStart();
            LogHelper.Debug(Tag, "Activity onStart");

            controlsFragment = FragmentManager.FindFragmentById<PlaybackControlsFragment>(Resource.Id.fragment_playback_controls);
            if (controlsFragment == null)
            {
                throw new Exception("Mising fragment with id 'controls'. Cannot continue.");
            }

            HidePlaybackControls();

            var permissionsList = new List<string>();

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == Permission.Granted)
                mediaBrowser.Connect();
            else
                permissionsList.Add(Manifest.Permission.ReadExternalStorage);

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != Permission.Granted)
                permissionsList.Add(Manifest.Permission.RecordAudio);

            if (permissionsList.Count > 0)
            {
                ActivityCompat.RequestPermissions(this, permissionsList.ToArray(), RequestCodeAskPermissions);
                return;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            LogHelper.Debug(Tag, "Activity onStop");
            
            SupportMediaController?.UnregisterCallback(mediaControllerCallback);
            
            mediaBrowser.Disconnect();
        }

        public MediaBrowserCompat MediaBrowser => mediaBrowser;       

        protected virtual void OnMediaControllerConnected()
        {
            // empty implementation, can be overridden by clients.
        }

        protected void ShowPlaybackControls()
        {
            LogHelper.Debug(Tag, "showPlaybackControls");

            FragmentManager.BeginTransaction()
                .SetCustomAnimations(
                    Resource.Animator.slide_in_from_bottom, Resource.Animator.slide_out_to_bottom,
                    Resource.Animator.slide_in_from_bottom, Resource.Animator.slide_out_to_bottom)
                .Show(controlsFragment)
                .Commit();
        }

        protected void HidePlaybackControls()
        {
            LogHelper.Debug(Tag, "hidePlaybackControls");
            FragmentManager.BeginTransaction()
                .Hide(controlsFragment)
                .Commit();
        }

        protected bool ShouldShowControls
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

        private void ConnectToSession(MediaSessionCompat.Token token) 
        {
            var mediaController = new MediaControllerCompat(this, token);
            SupportMediaController = mediaController;
            mediaController.RegisterCallback(mediaControllerCallback);

            if (ShouldShowControls) {
                ShowPlaybackControls();
            } else {
                LogHelper.Debug(Tag, "connectionCallback.onConnected: " +
                    "hiding controls because metadata is null");
                HidePlaybackControls();
            }
            
            controlsFragment?.OnConnected();            

            OnMediaControllerConnected();
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestCodeAskPermissions:
                    {

                        var perms = new Dictionary<string, Permission>();

                        perms[Manifest.Permission.ReadExternalStorage] = Permission.Granted;
                        perms[Manifest.Permission.RecordAudio] = Permission.Granted;

                        var readExternalStorageRequested = false;
                        for (int i = 0; i < permissions.Length; i++)
                        {
                            perms[permissions[i]] = grantResults[i];

                            if (permissions[i] == Manifest.Permission.ReadExternalStorage)
                                readExternalStorageRequested = true;
                        }

                        if (readExternalStorageRequested)
                        {
                            if (perms[Manifest.Permission.ReadExternalStorage] == Permission.Granted)
                                mediaBrowser.Connect();
                            else
                                Finish();
                        }

                        if (perms[Manifest.Permission.ReadExternalStorage] == Permission.Granted
                                && perms[Manifest.Permission.RecordAudio] == Permission.Granted)
                        {

                        }
                        else
                        {
                            Toast.MakeText(this, Resource.String.not_all_permissions_msg, ToastLength.Short)
                                    .Show();
                        }
                    }
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }
    }
}