using Foundation;
using MediaPlayer;
using System;
using System.Collections.Generic;
using UIKit;
using System.Linq;
using System.Threading.Tasks;

namespace LightOrganApp.iOS
{
    public partial class FileListViewController : UITableViewController
    {
        static NSString cellId = new NSString("reuseIdentifier");

        List<MPMediaItem> allMediaItems;
        List<MPMediaItem> filteredMediaItems;
        List<MPMediaItem> selectedMediaItems;
        public MPMediaItemCollection didPickMediaItems;

        UISearchController searchController;

        public FileListViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ConfigureSearchController();            

            TableView.Source = new TableSource(this);

            TableView.TableFooterView = new UIView();
            TableView.BackgroundView = new UIView();

            selectedMediaItems = new List<MPMediaItem>();

            LoadMediaItemsForMediaTypeAsync(MPMediaType.Music);          
        }

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }

        private void ConfigureSearchController()
        {
            searchController = new CustomSearchController((UIViewController)null)
            {
                WeakSearchResultsUpdater = this
            };
            searchController.DimsBackgroundDuringPresentation = false;
            searchController.SearchBar.SizeToFit();
            UISearchBar.Appearance.TintColor = UIColor.FromRGB(197, 225, 165);
            searchController.SearchBar.Placeholder = NSBundle.MainBundle.LocalizedString("searchMusic", "Search Music");
            searchController.SearchBar.WeakDelegate = this;
            DefinesPresentationContext = true;
            NavigationItem.TitleView = searchController.SearchBar;
            searchController.HidesNavigationBarDuringPresentation = false;
        }

        private async void LoadMediaItemsForMediaTypeAsync(MPMediaType mediaType)
        {
            await Task.Run(() =>
            {
                var query = new MPMediaQuery();
                var mediaTypeNumber = NSNumber.FromInt32((int)mediaType);
                var predicate = MPMediaPropertyPredicate.PredicateWithValue(mediaTypeNumber, MPMediaItem.MediaTypeProperty);

                query.AddFilterPredicate(predicate);

                allMediaItems = query.Items.ToList();
            });

            TableView.ReloadData();                       
        }

        private List<MPMediaItem> GetMediaItems()
        {
            if (searchController.Active && searchController.SearchBar.Text != "")
                return filteredMediaItems;
            else           
                return allMediaItems;
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (sender == doneButton)
            {
                if (selectedMediaItems != null && selectedMediaItems.Count > 0)
                    didPickMediaItems = new MPMediaItemCollection(selectedMediaItems.ToArray());                
            }
        }

        private bool MediaItemContainsString(MPMediaItem item, string searchText)
        {
            var b1 = false;
            var title = item.Title;
            if (title != null)
                b1 = title.ToString().ToLower().Contains(searchText.ToLower());

            var b2 = false;
            var artist = item.Artist;
            if (artist != null)
                b2 = artist.ToString().ToLower().Contains(searchText.ToLower());

            return b1 || b2;
        }

        private void FilterContentForSearchText(string searchText)
        {
            if (allMediaItems == null)
                return;

            filteredMediaItems = allMediaItems.Where(item => MediaItemContainsString(item, searchText)).ToList();

            TableView.ReloadData();
        }

        [Export("updateSearchResultsForSearchController:")]
        public virtual void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            FilterContentForSearchText(searchController.SearchBar.Text);
        }

        class TableSource : UITableViewSource
        {
            FileListViewController controller;

            public TableSource(FileListViewController controller)
            {
                this.controller = controller;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                var mediaItems = controller.GetMediaItems();

                if (mediaItems != null)
                    return mediaItems.Count;
                else
                    return 0;                
            }

            public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                cell.TintColor = UIColor.White;

                if (cell.TextLabel != null)
                    cell.TextLabel.TextColor = UIColor.White;

                if (cell.DetailTextLabel != null)
                    cell.DetailTextLabel.TextColor = UIColor.FromRGB(197, 225, 165);
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(cellId, indexPath);
                var row = indexPath.Row;
                var mediaItems = controller.GetMediaItems();

                var item = mediaItems[row];
                if (cell.TextLabel != null)
                    cell.TextLabel.Text = item.Title;

                var artist = NSBundle.MainBundle.LocalizedString("unknownArtist", "Unknown Artist");
                var artistVal = item.Artist;
                if (artistVal != null)
                    artist = artistVal;

                var length = (int)item.PlaybackDuration;

                if (cell.DetailTextLabel != null)
                    cell.DetailTextLabel.Text = $"{artist} {GetDisplayTime(length)}";

                if (controller.selectedMediaItems != null && controller.selectedMediaItems.Contains(item))
                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                else
                    cell.Accessory = UITableViewCellAccessory.None;

                cell.Tag = row;

                return cell;
            }

            private string GetDisplayTime(int seconds)
            {
                var h = seconds / 3600;
                var m = seconds / 60 - h * 60;
                var s = seconds - h * 3600 - m * 60;

                var str = "";

                if (h > 0)
                    str += $"{h}:";

                str += $"{m:00}:{s:00}";

                return str;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                var row = indexPath.Row;
                var mediaItems = controller.GetMediaItems();
                var item = mediaItems[row];

                if (controller.selectedMediaItems == null || !controller.selectedMediaItems.Contains(item))                
                    controller.selectedMediaItems?.Add(item);                
                else                
                    controller.selectedMediaItems.Remove(item);              

                tableView.ReloadData();
            }
        }        
    }
}