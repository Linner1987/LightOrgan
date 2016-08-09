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
        MPMediaItemCollection didPickMediaItems;

        public FileListViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = new TableSource(this);

            TableView.TableFooterView = new UIView();
            TableView.BackgroundView = new UIView();

            selectedMediaItems = new List<MPMediaItem>();

            LoadMediaItemsForMediaTypeAsync(MPMediaType.Music);          
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