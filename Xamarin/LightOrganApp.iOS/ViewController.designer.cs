// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace LightOrganApp.iOS
{
    [Register ("ViewController")]
    partial class ViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        LightOrganApp.iOS.CircleView bassLight { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        LightOrganApp.iOS.CircleView midLight { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem playButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel song { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIToolbar toolbar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint toolbarHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        LightOrganApp.iOS.CircleView trebleLight { get; set; }

        [Action ("playPausePressed:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void playPausePressed (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (bassLight != null) {
                bassLight.Dispose ();
                bassLight = null;
            }

            if (midLight != null) {
                midLight.Dispose ();
                midLight = null;
            }

            if (playButton != null) {
                playButton.Dispose ();
                playButton = null;
            }

            if (song != null) {
                song.Dispose ();
                song = null;
            }

            if (toolbar != null) {
                toolbar.Dispose ();
                toolbar = null;
            }

            if (toolbarHeightConstraint != null) {
                toolbarHeightConstraint.Dispose ();
                toolbarHeightConstraint = null;
            }

            if (trebleLight != null) {
                trebleLight.Dispose ();
                trebleLight = null;
            }
        }
    }
}