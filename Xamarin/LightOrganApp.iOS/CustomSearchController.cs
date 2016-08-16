using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace LightOrganApp.iOS
{
    public class CustomSearchController: UISearchController
    {
        Lazy<CustomSearchBar> _searchBar; 
        
        public CustomSearchController(UIViewController searchResultsController): base(searchResultsController)
        {
        }      

        private void InitSearchBar()
        {
            _searchBar = new Lazy<CustomSearchBar>(() =>
            {
                var result = new CustomSearchBar(CGRect.Empty);
                result.WeakDelegate = this;

                return result;
            });
        }

        public override UISearchBar SearchBar
        {
            get
            {
                if (_searchBar == null)
                    InitSearchBar();

                return _searchBar.Value;
            }
        }
    }
}
