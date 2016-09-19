using Xamarin.Forms;

namespace LightOrganApp.Controls
{
    public class CircleView: View
    {
        public CircleView()
        {
        }

        public static readonly BindableProperty CircleColorProperty =
            BindableProperty.Create<CircleView, Color>(
                p => p.CircleColor, Color.Red);
        
        public Color CircleColor
        {
            get { return (Color)GetValue(CircleColorProperty); }
            set { SetValue(CircleColorProperty, value); }
        }
    }
}
