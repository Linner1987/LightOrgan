using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using Android.Text;
using Android.Views;
using Android.Widget;
using LightOrganApp.Droid.Utils;
using System;

namespace LightOrganApp.Droid.UI
{
    public class PlaybackControlsFragment : Android.App.Fragment
    {
        new static readonly string Tag = LogHelper.MakeLogTag(typeof(PlaybackControlsFragment));

        private ImageButton playPause;
        private TextView title;
        private TextView subtitle;
        private TextView extraInfo;
        private ImageView albumArt;
        private string artUrl;

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

        readonly Callback callback = new Callback();


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_playback_controls, container, false);

            playPause = rootView.FindViewById<ImageButton>(Resource.Id.play_pause);
            playPause.Enabled = true;
            playPause.Click += OnButtonClick;

            title = rootView.FindViewById<TextView>(Resource.Id.title);
            subtitle = rootView.FindViewById<TextView>(Resource.Id.artist);
            extraInfo = rootView.FindViewById<TextView>(Resource.Id.extra_info);
            albumArt = rootView.FindViewById<ImageView>(Resource.Id.album_art);

            callback.OnPlaybackStateChangedImpl = (state) =>
            {
                LogHelper.Debug(Tag, "Received playback state change to state ", state.State);
                OnPlaybackStateChanged(state);
            };

            callback.OnMetadataChangedImpl = (metadata) =>
            {
                if (metadata == null)
                {
                    return;
                }
                LogHelper.Debug(Tag, "Received metadata state change to mediaId=",
                        metadata.Description.MediaId,
                        " song=", metadata.Description.Title);
                OnMetadataChanged(metadata);
            };

            return rootView;
        }

        public override void OnStart()
        {
            base.OnStart();
            LogHelper.Debug(Tag, "fragment.onStart");
            var controller = ((FragmentActivity)Activity).SupportMediaController;
            if (controller != null)
            {
                OnConnected();
            }
        }
        
        public override void OnStop()
        {
            base.OnStop();
            LogHelper.Debug(Tag, "fragment.onStop");
            var controller = ((FragmentActivity)Activity).SupportMediaController;
            if (controller != null)
            {
                controller.UnregisterCallback(callback);
            }
        }

        public void OnConnected()
        {
            var controller = ((FragmentActivity)Activity).SupportMediaController;
            LogHelper.Debug(Tag, "onConnected, mediaController==null? ", controller == null);
            if (controller != null)
            {
                OnMetadataChanged(controller.Metadata);
                OnPlaybackStateChanged(controller.PlaybackState);
                controller.RegisterCallback(callback);
            }
        }

        private void OnMetadataChanged(MediaMetadataCompat metadata)
        {
            LogHelper.Debug(Tag, "onMetadataChanged ", metadata);
            if (Activity == null)
            {
                LogHelper.Warn(Tag, "onMetadataChanged called when getActivity null," +
                        "this should not happen if the callback was properly unregistered. Ignoring.");
                return;
            }
            if (metadata == null)
            {
                return;
            }

            title.Text = metadata.Description.Title;
            subtitle.Text = metadata.Description.Subtitle;
            string url = null;
            if (metadata.Description.IconUri != null)
            {
                url = metadata.Description.IconUri.ToString();
            }
            if (!TextUtils.Equals(url, artUrl))
            {
                artUrl = url;
                Bitmap art = metadata.Description.IconBitmap;
                AlbumArtCache cache = AlbumArtCache.Instance;
                if (art == null)
                {
                    art = cache.GetIconImage(artUrl);
                }
                if (art != null)
                {
                    albumArt.SetImageBitmap(art);
                }
                else
                {
                    cache.Fetch(artUrl, new AlbumArtCache.FetchListener()
                    {
                        OnFetched = (artUrl, bitmap, icon) => {
                            if (icon != null)
                            {
                                LogHelper.Debug(Tag, "album art icon of w=", icon.Width, " h=", icon.Height);
                                if (IsAdded)
                                {
                                    albumArt.SetImageBitmap(icon);
                                }
                            }
                        }
                    });                        
                }
            }
        }

        public void SetExtraInfo(string info)
        {
            if (info == null)
            {
                extraInfo.Visibility = ViewStates.Gone;
            }
            else
            {
                extraInfo.Text = info;
                extraInfo.Visibility = ViewStates.Visible; 
            }
        }

        private void OnPlaybackStateChanged(PlaybackStateCompat state)
        {
            LogHelper.Debug(Tag, "onPlaybackStateChanged ", state);
            if (Activity == null)
            {
                LogHelper.Warn(Tag, "onPlaybackStateChanged called when getActivity null," +
                        "this should not happen if the callback was properly unregistered. Ignoring.");
                return;
            }
            if (state == null)
            {
                return;
            }
            bool enablePlay = false;
            switch (state.State)
            {
                case PlaybackStateCompat.StatePaused:
                case PlaybackStateCompat.StateStopped:
                    enablePlay = true;
                    break;
                case PlaybackStateCompat.StateError:
                    LogHelper.Error(Tag, "error playbackstate: ", state.ErrorMessage);
                    Toast.MakeText(Activity, state.ErrorMessage, ToastLength.Long).Show();
                    break;
            }

            if (enablePlay)
            {
                playPause.SetImageDrawable(
                        ContextCompat.GetDrawable(Activity, Resource.Drawable.ic_play_arrow_black_36dp));
            }
            else
            {
                playPause.SetImageDrawable(
                        ContextCompat.GetDrawable(Activity, Resource.Drawable.ic_pause_black_36dp));
            }
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            var controller = ((FragmentActivity)Activity).SupportMediaController;
            var stateObj = controller.PlaybackState;
            int state = stateObj == null ?
                    PlaybackStateCompat.StateNone : stateObj.State;
            LogHelper.Debug(Tag, "Button pressed, in state " + state);
            switch (((View)sender).Id)
            {
                case Resource.Id.play_pause:
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
                    break;
            }
        }

        private void PlayMedia()
        {
            var controller = ((FragmentActivity)Activity).SupportMediaController;
            
            controller?.GetTransportControls().Play();            
        }

        private void PauseMedia()
        {
            var controller = ((FragmentActivity)Activity).SupportMediaController;
            
            controller?.GetTransportControls().Pause();            
        }
    }
}