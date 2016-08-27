//
//  FileListViewController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 03/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer

class FileListViewController: UITableViewController, UISearchBarDelegate, UISearchResultsUpdating {
    
    enum RestorationKeys : String {
        case searchControllerIsActive
        case searchBarText
        case searchBarIsFirstResponder
    }
    
    struct SearchControllerRestorableState {
        var wasActive = false
        var wasFirstResponder = false
    }
    
    
    @IBOutlet var doneButton: UIBarButtonItem!
    
    var allMediaItems: [MPMediaItem]?
    var filteredMediaItems: [MPMediaItem]?
    var selectedMediaItems: [MPMediaItem]?
    var didPickMediaItems: MPMediaItemCollection?
    
    var searchController: UISearchController!
    
    var restoredState = SearchControllerRestorableState()
    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        self.configureSearchController()
        self.tableView.tableFooterView = UIView()
        self.tableView.backgroundView = UIView()
        
        selectedMediaItems = [MPMediaItem]()
        
        if #available(iOS 9.3, *) {
            MPMediaLibrary.requestAuthorization { (status) in
                if status == .authorized {
                    self.loadMediaItemsForMediaType(.music)
                } else {
                    self.displayMediaLibraryError()
                }
            }
        }
    }
    
    override func viewDidAppear(_ animated: Bool) {
        super.viewDidAppear(animated)
        
        if restoredState.wasActive {
            searchController.isActive = restoredState.wasActive
            restoredState.wasActive = false
            
            if restoredState.wasFirstResponder {
                searchController.searchBar.becomeFirstResponder()
                restoredState.wasFirstResponder = false
            }
        }
    }
    
    func searchBarSearchButtonClicked(_ searchBar: UISearchBar) {
        searchBar.resignFirstResponder()
    }
    
    func configureSearchController() {
        searchController = CustomSearchController(searchResultsController: nil)
        searchController.searchResultsUpdater = self
        searchController.dimsBackgroundDuringPresentation = false
        searchController.searchBar.sizeToFit()
        searchController.searchBar.barTintColor = .black
        
        searchController.searchBar.placeholder = NSLocalizedString("searchMusic", comment: "Search Music")
        searchController.searchBar.delegate = self
        definesPresentationContext = true
        navigationItem.titleView = searchController.searchBar
        searchController.hidesNavigationBarDuringPresentation = false;
    }
    
    func loadMediaItemsForMediaType(_ mediaType: MPMediaType) {
        
        let queue = DispatchQueue.global(qos: .default)
        
        queue.async {
            let query = MPMediaQuery()
            let mediaTypeNumber =  Int(mediaType.rawValue)
            let predicate = MPMediaPropertyPredicate(value: mediaTypeNumber,
                                                     forProperty: MPMediaItemPropertyMediaType)
            query.addFilterPredicate(predicate)
            
            self.allMediaItems = query.items
            
            DispatchQueue.main.async {
                self.tableView.reloadData()
            }
        }
    }
    
    fileprivate func getMediaItems() -> [MPMediaItem]? {
        if (searchController.isActive || restoredState.wasActive) && searchController.searchBar.text != "" {
            return filteredMediaItems
        } else {
            return allMediaItems
        }
    }
    
    @available(iOS 9.3, *)
    func displayMediaLibraryError() {
        var error: String
        switch MPMediaLibrary.authorizationStatus() {
        case .restricted:
            error = "Media library access restricted by corporate or parental settings"
        case .denied:
            error = "Media library access denied by user"
        default:
            error = "Unknown error"
        }
        
        let controller = UIAlertController(title: "Error", message: error, preferredStyle: .alert)
        controller.addAction(UIAlertAction(title: "OK", style: .default, handler: nil))
        present(controller, animated: true, completion: nil)
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    override func numberOfSections(in tableView: UITableView) -> Int {
        return 1
    }
    
    override func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        let mediaItems = getMediaItems()
        if mediaItems != nil {
            return mediaItems!.count;
        } else {
            return 0
        }
    }
    
    override func tableView(_ tableView: UITableView, willDisplay cell: UITableViewCell, forRowAt indexPath: IndexPath) {
        cell.textLabel?.textColor = .white
        cell.detailTextLabel?.textColor = .lightGray
    }
    
    override func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCell(withIdentifier: "reuseIdentifier", for: indexPath)
        let row = (indexPath as NSIndexPath).row
        let mediaItems = getMediaItems()
        
        
        let item = mediaItems![row] as MPMediaItem
        cell.textLabel?.text = item.value(forProperty: MPMediaItemPropertyTitle) as! String?
        
        var artist = NSLocalizedString("unknownArtist", comment: "Unknown Artist")
        if let artistVal = item.value(forProperty: MPMediaItemPropertyArtist) as? String {
            artist = artistVal
        }
        
        let length = item.value(forProperty: MPMediaItemPropertyPlaybackDuration) as! Int
        
        
        cell.detailTextLabel?.text = "\(artist)  \(getDisplayTime(length))"
        
        if selectedMediaItems!.contains(item) {
            cell.accessoryType = .checkmark
        } else {
            cell.accessoryType = .none
        }
        
        
        cell.tag = row
        
        return cell
    }
    
    fileprivate func getDisplayTime(_ seconds: Int) -> String {
        
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
    
    override func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        
        let row = (indexPath as NSIndexPath).row
        let mediaItems = getMediaItems()
        let item = mediaItems![row] as MPMediaItem
        
        if !selectedMediaItems!.contains(item) {
            selectedMediaItems!.append(item)
        } else {
            if let index = selectedMediaItems!.index(of: item) {
                selectedMediaItems!.remove(at: index)
            }
        }
        
        tableView.reloadData()
    }
    
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        
        if doneButton === sender as? UIBarButtonItem {
            if selectedMediaItems!.count > 0 {
                self.didPickMediaItems = MPMediaItemCollection(items: selectedMediaItems!)
            }
        }
    }
    
    
    func mediaItemContainsString(_ item: MPMediaItem, searchText: String) -> Bool {
        var b1 = false
        if let title = item.value(forProperty: MPMediaItemPropertyTitle) as? String {
            b1 = title.lowercased().contains(searchText.lowercased())
        }
        
        var b2 = false
        if let artist = item.value(forProperty: MPMediaItemPropertyArtist) as? String {
            b2 = artist.lowercased().contains(searchText.lowercased())
        }
        
        return b1 || b2
    }
    
    
    func filterContentForSearchText(_ searchText: String) {
        if self.allMediaItems == nil {
            return
        }
        
        self.filteredMediaItems = self.allMediaItems!.filter { item in
            return mediaItemContainsString(item, searchText: searchText)
        }
        
        tableView.reloadData()
    }
    
    func updateSearchResults(for searchController: UISearchController) {
        filterContentForSearchText(searchController.searchBar.text!)
    }
    
    
    override func encodeRestorableState(with coder: NSCoder) {
        super.encodeRestorableState(with: coder)
        
        
        coder.encode(searchController.isActive, forKey:RestorationKeys.searchControllerIsActive.rawValue)
        
        coder.encode(searchController.searchBar.isFirstResponder, forKey:RestorationKeys.searchBarIsFirstResponder.rawValue)
        
        coder.encode(searchController.searchBar.text, forKey:RestorationKeys.searchBarText.rawValue)
    }
    
    override func decodeRestorableState(with coder: NSCoder) {
        super.decodeRestorableState(with: coder)        
        
        
        restoredState.wasActive = coder.decodeBool(forKey: RestorationKeys.searchControllerIsActive.rawValue)
        
        restoredState.wasFirstResponder = coder.decodeBool(forKey: RestorationKeys.searchBarIsFirstResponder.rawValue)
        
        searchController.searchBar.text = coder.decodeObject(forKey: RestorationKeys.searchBarText.rawValue) as? String
    }
}





