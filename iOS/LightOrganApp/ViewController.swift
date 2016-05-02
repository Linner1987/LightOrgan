//
//  ViewController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 29/04/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer

class ViewController: UIViewController, MPMediaPickerControllerDelegate {

    @IBOutlet weak var toolbar: UIToolbar!
    @IBOutlet weak var song: UILabel!
    
    @IBOutlet var playButton: UIBarButtonItem!
    var pauseButton: UIBarButtonItem!
    
    var player: MPMusicPlayerController!
    var collection: MPMediaItemCollection!
    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        self.pauseButton = UIBarButtonItem(barButtonSystemItem: .Pause, target: self, action: #selector(ViewController.playPausePressed(_:)))
        self.pauseButton.style = .Plain
        
        self.player = MPMusicPlayerController.systemMusicPlayer()
        self.player.repeatMode = .All
        
        let notificationCenter = NSNotificationCenter.defaultCenter()
        notificationCenter.addObserver(self, selector: #selector(ViewController.nowPlayingItemChanged(_:)), name: MPMusicPlayerControllerNowPlayingItemDidChangeNotification, object: self.player)
        notificationCenter.addObserver(self, selector: #selector(ViewController.playbackStateChanged(_:)), name: MPMusicPlayerControllerPlaybackStateDidChangeNotification, object: self.player)
        self.player.beginGeneratingPlaybackNotifications()
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
        
        NSNotificationCenter.defaultCenter().removeObserver(self, name: MPMusicPlayerControllerNowPlayingItemDidChangeNotification, object: self.player)
        NSNotificationCenter.defaultCenter().removeObserver(self, name: MPMusicPlayerControllerPlaybackStateDidChangeNotification, object: self.player)
    }
    
    @IBAction func search(sender: AnyObject) {
        
        let picker = MPMediaPickerController(mediaTypes: MPMediaType.Music)
        picker.delegate = self
        picker.allowsPickingMultipleItems = true
        picker.prompt = NSLocalizedString("Select items to play", comment: "Select items to play")
        self.presentViewController(picker, animated: true, completion: nil)
    }
    
    func mediaPickerDidCancel(mediaPicker: MPMediaPickerController) {
        self.dismissViewControllerAnimated(true, completion: nil)
    }
    
    func mediaPicker(mediaPicker: MPMediaPickerController, didPickMediaItems mediaItemCollection: MPMediaItemCollection) {
        self.dismissViewControllerAnimated(true, completion: nil)
        
        if self.collection != nil {
            let oldItems: NSArray = self.collection.items
            let newItems: NSArray = oldItems.arrayByAddingObjectsFromArray(mediaItemCollection.items)
            self.collection = MPMediaItemCollection(items: newItems as! [MPMediaItem])
        } else {
            self.collection = mediaItemCollection
            self.player.setQueueWithItemCollection(self.collection)            
        }
        
        let item = self.collection.items[0] as MPMediaItem
        self.player.nowPlayingItem = item
        self.playPausePressed(self)
    }
    
    @IBAction func playPausePressed(sender: AnyObject) {
        let playbackState = self.player.playbackState as MPMusicPlaybackState
        if playbackState == .Stopped || playbackState == .Paused {
            self.player.play()
            
        } else if playbackState == .Playing {
            self.player.pause()
        }
    }
    
    func nowPlayingItemChanged(notification: NSNotification) {
        if let currentItem = self.player.nowPlayingItem as MPMediaItem? {
            self.song.text = currentItem.valueForProperty(MPMediaItemPropertyTitle) as? String
        } else {            
            self.song.text = nil
        }
    }
    
    func playbackStateChanged(notification: NSNotification) {
        let playbackState = self.player.playbackState as MPMusicPlaybackState
        
        self.toolbar.hidden = playbackState != .Playing && playbackState != .Paused
        
        var items = self.toolbar.items!
        if playbackState == .Stopped || playbackState == .Paused {
            items[0] = self.playButton
        } else if playbackState == .Playing {
            items[0] = self.pauseButton
        }
        self.toolbar.setItems(items, animated: false)
    }
}

