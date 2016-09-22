using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using LightOrganApp.iOS.Renderers;
using Xamarin.Forms;
using LightOrganApp;

[assembly: ExportRenderer(typeof(MainPage), typeof(MainPageRenderer))]
namespace LightOrganApp.iOS.Renderers
{
    public class MainPageRenderer: PageRenderer
    {
        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            base.ViewWillTransitionToSize(toSize, coordinator);
            CATransaction.Begin();
            CATransaction.DisableActions = true;

            coordinator.AnimateAlongsideTransition(ctx => { }, ctx => CATransaction.Commit());
        }
    }
}
