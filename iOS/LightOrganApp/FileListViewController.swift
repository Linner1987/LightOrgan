//
//  FileListViewController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 03/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer

class FileListViewController: UITableViewController {
    
    @IBOutlet var doneButton: UIBarButtonItem!
    
    var mediaItems: [MPMediaItem]?
    var didPickMediaItems: MPMediaItemCollection?
    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.tableView.tableFooterView = UIView()
        doneButton.enabled = false
        
        self.loadMediaItemsForMediaType(.Music)
    }
    
    func loadMediaItemsForMediaType(mediaType: MPMediaType){
        let query = MPMediaQuery()
        let mediaTypeNumber =  Int(mediaType.rawValue)
        let predicate = MPMediaPropertyPredicate(value: mediaTypeNumber,
                                                 forProperty: MPMediaItemPropertyMediaType)
        query.addFilterPredicate(predicate)
        self.mediaItems = query.items
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }

    override func numberOfSectionsInTableView(tableView: UITableView) -> Int {
        return 1
    }

    override func tableView(tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        if self.mediaItems != nil {
            return self.mediaItems!.count;
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
        let item = self.mediaItems![row] as MPMediaItem
        cell.textLabel?.text = item.valueForProperty(MPMediaItemPropertyTitle) as! String?
        
        var artist = NSLocalizedString("Unknown Artist", comment: "Unknown Artist")
        if let artistVal = item.valueForProperty(MPMediaItemPropertyArtist) as? String {
            artist = artistVal
        }
        
        let length = item.valueForProperty(MPMediaItemPropertyPlaybackDuration) as! Int
        
        
        cell.detailTextLabel?.text = "\(artist)  \(getDisplayTime(length))"
        
        
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
    
        let cell = tableView.cellForRowAtIndexPath(indexPath)
        cell!.accessoryType = .Checkmark
        
        checkDoneButton()
    }
    
    override func tableView(tableView: UITableView, didDeselectRowAtIndexPath indexPath: NSIndexPath) {
        
        let cell = tableView.cellForRowAtIndexPath(indexPath)        
        cell!.accessoryType = .None
        
        checkDoneButton()
    }
    
    private func checkDoneButton() {
        
        let selectedRows = self.tableView.indexPathsForSelectedRows ?? []
        doneButton.enabled = !selectedRows.isEmpty
    }
    
    override func prepareForSegue(segue: UIStoryboardSegue, sender: AnyObject?) {
        
        if doneButton === sender {
            
            if self.mediaItems == nil {
                return
            }
            
            let selectedRows = self.tableView.indexPathsForSelectedRows ?? []
            let noItemsAreSelected = selectedRows.isEmpty
            
            if !noItemsAreSelected {
                
                var items = [MPMediaItem]()
                
                for i in 0 ..< selectedRows.count {
                    
                    let index = selectedRows[i]
                    let item = self.mediaItems![index.row]
                    items.append(item)
                }
                
                self.didPickMediaItems = MPMediaItemCollection(items: items)
            }
        }
    }
    
    
    @IBAction func cancel(sender: UIBarButtonItem) {
        
        dismissViewControllerAnimated(true, completion: nil)
    }
}
