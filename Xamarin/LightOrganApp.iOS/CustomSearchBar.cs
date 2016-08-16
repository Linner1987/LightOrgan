using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using UIKit;
using Foundation;

namespace LightOrganApp.iOS
{
    public class CustomSearchBar: UISearchBar
    {
        public CustomSearchBar(CGRect frame): base(frame)
        {
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            SetShowsCancelButton(false, false);
        }

        private int? IndexOfSearchFieldInSubviews()
        {
            int? index = null;
            var searchBarView = Subviews[0];

            for (var i = 0; i < searchBarView.Subviews.Length; i++)
            {
                if (searchBarView.Subviews[i] is UITextField)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public override void Draw(CGRect rect)
        {
            var index = IndexOfSearchFieldInSubviews();

            if (index != null)
            {
                var searchField = Subviews[0].Subviews[index.Value] as UITextField;

                searchField.TextColor = UIColor.White;
                searchField.BackgroundColor = UIColor.FromRGB(22, 64, 25);

                var glassIconView = searchField.LeftView as UIImageView;
                if (glassIconView != null)
                {
                    glassIconView.Image = glassIconView.Image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                    glassIconView.TintColor = UIColor.FromRGB(197, 225, 165);
                }

                var textFieldInsideSearchBarLabel = searchField.ValueForKey(new NSString("placeholderLabel")) as UILabel;
                if (textFieldInsideSearchBarLabel != null)
                    textFieldInsideSearchBarLabel.TextColor = UIColor.FromRGB(197, 225, 165);

                var clearButton = searchField.ValueForKey(new NSString("clearButton")) as UIButton;
                clearButton.SetImage(clearButton.ImageView?.Image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Normal);
                clearButton.SetImage(clearButton.ImageView?.Image?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate), UIControlState.Highlighted);
                clearButton.TintColor = UIColor.FromRGB(197, 225, 165);
            }

            base.Draw(rect);
        }
    }
}
