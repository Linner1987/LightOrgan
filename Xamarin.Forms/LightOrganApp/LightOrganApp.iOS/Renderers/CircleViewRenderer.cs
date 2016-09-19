using Xamarin.Forms;
using LightOrganApp.iOS.Renderers;
using Xamarin.Forms.Platform.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(LightOrganApp.Controls.CircleView), typeof(CircleViewRenderer))]
namespace LightOrganApp.iOS.Renderers
{
    public class CircleViewRenderer: ViewRenderer<LightOrganApp.Controls.CircleView, LightOrganApp.iOS.CircleView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<LightOrganApp.Controls.CircleView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            var circle = new LightOrganApp.iOS.CircleView
            {
                CircleColor = Element.CircleColor.ToUIColor(),
                BackgroundColor = UIColor.Clear            
            };

            SetNativeControl(circle);
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (Control == null || Element == null)
                return;

            if (e.PropertyName == LightOrganApp.Controls.CircleView.CircleColorProperty.PropertyName)
            {
                Control.CircleColor = Element.CircleColor.ToUIColor();
            }
        }
    }
}