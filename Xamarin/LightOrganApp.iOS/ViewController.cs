﻿using Foundation;
using MediaPlayer;
using System;

using UIKit;
using CoreGraphics;
using CoreAnimation;
using LightOrganApp.Shared;
using System.Threading.Tasks;

namespace LightOrganApp.iOS
{
    public partial class ViewController : UIViewController
    {
        UIBarButtonItem pauseButton;

        MPMusicPlayerController player;
        MPMediaItemCollection collection;

        NSObject notificationToken1;
        NSObject notificationToken2;
        NSObject notificationToken3;

        LightsRemoteController remoteController;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            toolbar.Translucent = false;

            pauseButton = new UIBarButtonItem(UIBarButtonSystemItem.Pause, PauseAction);
            pauseButton.Style = UIBarButtonItemStyle.Plain;
            pauseButton.TintColor = UIColor.White;

            player = MPMusicPlayerController.SystemMusicPlayer;
            player.RepeatMode = MPMusicRepeatMode.All;

            var notificationCenter = NSNotificationCenter.DefaultCenter;
            notificationToken1 = notificationCenter.AddObserver(MPMusicPlayerController.NowPlayingItemDidChangeNotification, NowPlayingItemChanged, player);
            notificationToken2 = notificationCenter.AddObserver(MPMusicPlayerController.PlaybackStateDidChangeNotification, PlaybackStateChanged, player);
            notificationToken3 = notificationCenter.AddObserver(NSUserDefaults.DidChangeNotification, DefaultsChanged);
            player.BeginGeneratingPlaybackNotifications();
        }          

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.

            notificationToken1.Dispose();
            notificationToken2.Dispose();
            notificationToken3.Dispose();
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);
            CATransaction.Begin();
            CATransaction.DisableActions = true;

            coordinator.AnimateAlongsideTransition(ctx => { }, ctx => CATransaction.Commit());
        }

        [Action("unwindToPlayer:")]
        public void UnwindToPlayer(UIStoryboardSegue sender)
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

        private void SetLight(CircleView light, float ratio)
        {
            light.CircleColor = light.CircleColor.ColorWithAlpha(ratio);
        }

        private async void OnLightOrganDataUpdated(float bassLevel, float midLevel, float trebleLevel)
        {
            SetLight(bassLight, bassLevel);
            SetLight(midLight, midLevel);
            SetLight(trebleLight, trebleLevel);

            var bassValue = (byte)Math.Round(255 * bassLevel);
            var midValue = (byte)Math.Round(255 * midLevel);
            var trebleValue = (byte)Math.Round(255 * trebleLevel);

            var bytes = new byte[] { bassValue, midValue, trebleValue };

            await SendCommand(bytes);
        }       

        private async Task CreateNewRemoteController(NSUserDefaults defaults)
        {
            var host = defaults.StringForKey("remote_device_host_preference");
            var port = defaults.IntForKey("remote_device_port_preference");

            if (!string.IsNullOrEmpty(host) && port > 0)
            {
                remoteController = new LightsRemoteController();
                await remoteController.ConnectAsync(host, (int)port);
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

        private async void DefaultsChanged(NSNotification notification)
        {
            try
            {
                var defaults = NSUserDefaults.StandardUserDefaults;
                var useRemoteDevice = defaults.BoolForKey("use_remote_device_preference");

                if (remoteController != null)
                    await ReleaseRemoteController();

                if (useRemoteDevice)
                {
                    await CreateNewRemoteController(defaults);                   
                }

                //test
                OnLightOrganDataUpdated(0.3f, 1, 0);
            }
            catch (Exception)
            {
                
            }
        }
    }
}