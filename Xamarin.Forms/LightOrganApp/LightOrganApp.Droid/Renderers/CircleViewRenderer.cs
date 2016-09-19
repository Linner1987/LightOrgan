using Xamarin.Forms;
using LightOrganApp.Droid.Renderers;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LightOrganApp.Controls.CircleView), typeof(CircleViewRenderer))]
namespace LightOrganApp.Droid.Renderers
{
    public class CircleViewRenderer: ViewRenderer<LightOrganApp.Controls.CircleView, LightOrganApp.Droid.UI.CircleView>
    {
        public CircleViewRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<LightOrganApp.Controls.CircleView> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || this.Element == null)
                return;

            var circle = new LightOrganApp.Droid.UI.CircleView(Forms.Context)
            {                
                CircleColor = Element.CircleColor.ToAndroid()                
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
                Control.CircleColor = Element.CircleColor.ToAndroid();
            }
        }
    }
}