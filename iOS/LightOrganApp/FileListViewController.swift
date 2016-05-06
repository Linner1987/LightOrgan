//
//  FileListViewController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 03/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer

class FileListViewController: UITableViewController, UISearchResultsUpdating {
    
    @IBOutlet var doneButton: UIBarButtonItem!
    
    var allMediaItems: [MPMediaItem]?
    var filteredMediaItems: [MPMediaItem]?
    var selectedMediaItems: [MPMediaItem]?
    var didPickMediaItems: MPMediaItemCollection?
    
    var searchController: UISearchController!
    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.configureSearchController()
        self.tableView.tableFooterView = UIView()
        self.tableView.backgroundView = UIView()
        
        selectedMediaItems = [MPMediaItem]()
        
        self.loadMediaItemsForMediaType(.Music)
    }
    
    func configureSearchController() {
        searchController = CustomSearchController(searchResultsController: nil)
        searchController.searchResultsUpdater = self
        searchController.dimsBackgroundDuringPresentation = false
        searchController.searchBar.sizeToFit()
        searchController.searchBar.barTintColor = .blackColor()
        searchController.searchBar.placeholder = "Search Music"        
        definesPresentationContext = true
        navigationItem.titleView = searchController.searchBar
        searchController.hidesNavigationBarDuringPresentation = false;
    }
    
    func loadMediaItemsForMediaType(mediaType: MPMediaType){
        
        let queue = dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0)
        
        dispatch_async(queue) {
            let query = MPMediaQuery()
            let mediaTypeNumber =  Int(mediaType.rawValue)
            let predicate = MPMediaPropertyPredicate(value: mediaTypeNumber,
                                                     forProperty: MPMediaItemPropertyMediaType)
            query.addFilterPredicate(predicate)
            
            self.allMediaItems = query.items
            
            dispatch_async(dispatch_get_main_queue()) {
                self.tableView.reloadData()
            }
        }
    }
    
    private func getMediaItems() -> [MPMediaItem]? {
        if searchController.active && searchController.searchBar.text != "" {
            return filteredMediaItems
        } else {
            return allMediaItems
        }
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    override func numberOfSectionsInTableView(tableView: UITableView) -> Int {
        return 1
    }
    
    override func tableView(tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        let mediaItems = getMediaItems()
        if mediaItems != nil {
            return mediaItems!.count;
        } else {
            return 0
        }
    }
    
    override func tableView(tableView: UITableView, willDisplayCell cell: UITableViewCell, forRowAtIndexPath indexPath: NSIndexPath) {
        cell.textLabel?.textColor = .whiteColor()
        cell.detailTextLabel?.textColor = .lightGrayColor()
    }
    
    override func tableView(tableView: UITableView, cellForRowAtIndexPath indexPath: NSIndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCellWithIdentifier("reuseIdentifier", forIndexPath: indexPath)
        let row = indexPath.row
        let mediaItems = getMediaItems()
        let item = mediaItems![row] as MPMediaItem
        cell.textLabel?.text = item.valueForProperty(MPMediaItemPropertyTitle) as! String?
        
        var artist = NSLocalizedString("Unknown Artist", comment: "Unknown Artist")
        if let artistVal = item.valueForProperty(MPMediaItemPropertyArtist) as? String {
            artist = artistVal
        }
        
        let length = item.valueForProperty(MPMediaItemPropertyPlaybackDuration) as! Int
        
        
        cell.detailTextLabel?.text = "\(artist)  \(getDisplayTime(length))"
        
        if selectedMediaItems!.contains(item) {
            cell.accessoryType = .Checkmark
        } else {
            cell.accessoryType = .None
        }
        
        cell.tag = row
        return cell
    }
    
    private func getDisplayTime(seconds: Int) -> String {
        
        let h = seconds / 3600
        let m = seconds / 60 - h * 60
        let s = seconds - h * 3600 - m * 60
        
        var str = "";
        
        if h > 0 {
            str += "\(h):"
        }
        str += String(format: "%02d:%02d", m, s)
        
        return str
    }
    
    override func tableView(tableView: UITableView, didSelectRowAtIndexPath indexPath: NSIndexPath) {
        
        let row = indexPath.row
        let mediaItems = getMediaItems()
        let item = mediaItems![row] as MPMediaItem
        
        if !selectedMediaItems!.contains(item) {
            selectedMediaItems!.append(item)
        } else {
            if let index = selectedMediaItems!.indexOf(item) {
                selectedMediaItems!.removeAtIndex(index)
            }
        }
        
        tableView.reloadData()
    }
    
    
    override func prepareForSegue(segue: UIStoryboardSegue, sender: AnyObject?) {
        
        if doneButton === sender {
            if selectedMediaItems!.count > 0 {
                self.didPickMediaItems = MPMediaItemCollection(items: selectedMediaItems!)
            }
        }
    }
    
    
    func mediaItemContainsString(item: MPMediaItem, searchText: String) -> Bool {
        var b1 = false
        if let title = item.valueForProperty(MPMediaItemPropertyTitle) as? String {
            b1 = title.lowercaseString.containsString(searchText.lowercaseString)
        }
        
        var b2 = false
        if let artist = item.valueForProperty(MPMediaItemPropertyArtist) as? String {
            b2 = artist.lowercaseString.containsString(searchText.lowercaseString)
        }
        
        return b1 || b2
    }
    
    
    func filterContentForSearchText(searchText: String) {
        if self.allMediaItems == nil {
            return
        }
        
        self.filteredMediaItems = self.allMediaItems!.filter { item in
            return mediaItemContainsString(item, searchText: searchText)
        }
        
        tableView.reloadData()
    }
    
    func updateSearchResultsForSearchController(searchController: UISearchController) {
        filterContentForSearchText(searchController.searchBar.text!)
    }
}





