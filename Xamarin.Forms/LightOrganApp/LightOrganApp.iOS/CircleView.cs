using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UIKit;

namespace LightOrganApp.iOS
{
    [Register("CircleView"), DesignTimeVisible(true)]
    public class CircleView: UIView
    {
        private UIColor circleColor;

        [Export("CircleColor"), Browsable(true)]
        public UIColor CircleColor
        {
            get { return circleColor; }
            set
            {
                circleColor = value;
                SetNeedsDisplay();
            }
        }        

        public CircleView(IntPtr p): base(p)
        {
            Initialize();
        }

        public CircleView()
        {
            Initialize();
        }

        private void Initialize()
        {
            circleColor = UIColor.Red;
            ContentMode = UIViewContentMode.Redraw;

            SetNeedsDisplay();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            DrawCircle(CircleColor);
        }

        private void DrawCircle(UIColor color)
        {
            using (var context = UIGraphics.GetCurrentContext())
            {
                var a = Math.Min(Bounds.Size.Width, Bounds.Size.Height);
                var leftX = Bounds.GetMidX() - a / 2;
                var topY = Bounds.GetMidY() - a / 2;
                var rectangle = new CGRect(leftX, topY, a, a);

                context.SetFillColor(CircleColor.CGColor);
                context.FillEllipseInRect(rectangle);
            }
        }
    }
}
