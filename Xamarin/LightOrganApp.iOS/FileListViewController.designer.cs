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
    [Register ("FileListViewController")]
    partial class FileListViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem doneButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (doneButton != null) {
                doneButton.Dispose ();
                doneButton = null;
            }
        }
    }
}