using Foundation;
using MediaPlayer;
using System;

using UIKit;

namespace LightOrganApp.iOS
{
    public partial class ViewController : UIViewController
    {
        UIBarButtonItem pauseButton;

        MPMusicPlayerController player;
        MPMediaItemCollection collection;

        NSObject notificationToken1;
        NSObject notificationToken2;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            pauseButton = new UIBarButtonItem(UIBarButtonSystemItem.Pause, PauseAction);
            pauseButton.Style = UIBarButtonItemStyle.Plain;
            pauseButton.TintColor = UIColor.White;

            player = MPMusicPlayerController.SystemMusicPlayer;
            player.RepeatMode = MPMusicRepeatMode.All;

            var notificationCenter = NSNotificationCenter.DefaultCenter;
            notificationToken1 = notificationCenter.AddObserver(MPMusicPlayerController.NowPlayingItemDidChangeNotification, NowPlayingItemChanged, player);
            notificationToken2 = notificationCenter.AddObserver(MPMusicPlayerController.PlaybackStateDidChangeNotification, PlaybackStateChanged, player);
            player.BeginGeneratingPlaybackNotifications();
        }            

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.

            notificationToken1.Dispose();
            notificationToken2.Dispose();
        }

        [Action("unwindToPlayer:")]
        public void UnwindToYellowViewController(UIStoryboardSegue sender)
        {
            var sourceViewController = sender.SourceViewController as FileListViewController;

            if (sourceViewController == null)
                return;

            var mediaItemCollection = sourceViewController.didPickMediaItems;

            if (mediaItemCollection != null)
            {
                collection = mediaItemCollection;
                player.SetQueue(collection);

                var playbackState = player.PlaybackState;
                if (playbackState == MPMusicPlaybackState.Playing)
                    player.Pause();

                player.NowPlayingItem = collection.Items[0];

                playbackState = player.PlaybackState;
                player.Play();
            }
        }

        private void PauseAction(object sender, EventArgs e)
        {
            playPausePressed((UIKit.UIBarButtonItem)sender);
        }

        partial void playPausePressed(UIKit.UIBarButtonItem sender)
        {
            var playbackState = player.PlaybackState;
            if (playbackState == MPMusicPlaybackState.Stopped || playbackState == MPMusicPlaybackState.Paused) 
                player.Play();        
            else if (playbackState == MPMusicPlaybackState.Playing)
                player.Pause();
        }

        private void NowPlayingItemChanged(NSNotification notification)
        {
            var currentItem = player.NowPlayingItem;
            if (currentItem != null)
                song.Text = currentItem.Title;
            else
                song.Text = null;
        }

        private void PlaybackStateChanged(NSNotification notification)
        {
            var playbackState = player.PlaybackState;

            toolbar.Hidden = playbackState != MPMusicPlaybackState.Playing && playbackState != MPMusicPlaybackState.Paused;
            toolbarHeightConstraint.Priority = (playbackState != MPMusicPlaybackState.Playing && playbackState != MPMusicPlaybackState.Paused) ? 999 : 250;

            var items = toolbar.Items;
            if (playbackState == MPMusicPlaybackState.Stopped || playbackState == MPMusicPlaybackState.Paused)
                items[0] = playButton;
            else if (playbackState == MPMusicPlaybackState.Playing)
                items[0] = pauseButton;
            toolbar.SetItems(items, false);
        }
    }
}