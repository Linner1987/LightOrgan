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
        [Export("CircleColor"), Browsable(true)]
        public UIColor CircleColor { get; set; }

        public CircleView(IntPtr p): base(p)
        {
            
        }

        public CircleView()
        {
            
        }
    }
}
