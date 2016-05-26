//
//  ViewController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 29/04/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer

class ViewController: UIViewController, NSStreamDelegate /*, MPMediaPickerControllerDelegate*/ {

    @IBOutlet weak var toolbar: UIToolbar!
    @IBOutlet weak var song: UILabel!
    
    @IBOutlet var playButton: UIBarButtonItem!
    var pauseButton: UIBarButtonItem!
    
    @IBOutlet var bassLight: CircleView!
    @IBOutlet var midLight: CircleView!
    @IBOutlet var trebleLight: CircleView!
    
    var player: MPMusicPlayerController!
    var collection: MPMediaItemCollection!
    
    @IBOutlet var toolbarHeightConstraint: NSLayoutConstraint!
    
    var outStream: NSOutputStream?
    
    
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
    
    override func viewWillAppear(animated: Bool) {
        super.viewWillAppear(animated)
        
        self.defaultsChanged()
        
        NSNotificationCenter.defaultCenter().addObserver(self, selector: #selector(ViewController.defaultsChanged), name: NSUserDefaultsDidChangeNotification, object: nil)
    
        //test
        onLightOrganDataUpdated(0.3, midLevel: 1, trebleLevel: 0)
    }
    
    override func viewWillDisappear(animated: Bool) {
        super.viewWillDisappear(animated)
        
        NSNotificationCenter.defaultCenter().removeObserver(self, name: NSUserDefaultsDidChangeNotification, object: nil)
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
        
        NSNotificationCenter.defaultCenter().removeObserver(self, name: MPMusicPlayerControllerNowPlayingItemDidChangeNotification, object: self.player)
        NSNotificationCenter.defaultCenter().removeObserver(self, name: MPMusicPlayerControllerPlaybackStateDidChangeNotification, object: self.player)
    }
    
    override func viewWillTransitionToSize(size: CGSize, withTransitionCoordinator coordinator: UIViewControllerTransitionCoordinator) {
        super.viewWillTransitionToSize(size, withTransitionCoordinator: coordinator)
        CATransaction.begin()
        CATransaction.setDisableActions(true)
        
        coordinator.animateAlongsideTransition({ (ctx) -> Void in
            
            }, completion: { (ctx) -> Void in
                CATransaction.commit()
        })
        
    }
    
    /*
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
        
        self.collection = mediaItemCollection
        self.player.setQueueWithItemCollection(self.collection)        
        
        var playbackState = self.player.playbackState as MPMusicPlaybackState
        if playbackState == .Playing {
            self.player.pause()
        }
        
        let item = self.collection.items[0] as MPMediaItem
        self.player.nowPlayingItem = item
        
        playbackState = self.player.playbackState as MPMusicPlaybackState
        self.player.play()
    }*/
    
    @IBAction func unwindToPlayer(sender: UIStoryboardSegue) {
        
        if let sourceViewController = sender.sourceViewController as? FileListViewController,
            mediaItemCollection = sourceViewController.didPickMediaItems {
            
            self.collection = mediaItemCollection
            self.player.setQueueWithItemCollection(self.collection)
            
            var playbackState = self.player.playbackState as MPMusicPlaybackState
            if playbackState == .Playing {
                self.player.pause()
            }
            
            let item = self.collection.items[0] as MPMediaItem
            self.player.nowPlayingItem = item
            
            playbackState = self.player.playbackState as MPMusicPlaybackState
            self.player.play()            
        }
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
        toolbarHeightConstraint.priority = (playbackState != .Playing && playbackState != .Paused) ? 999 : 250
        
        var items = self.toolbar.items!
        if playbackState == .Stopped || playbackState == .Paused {
            items[0] = self.playButton
        } else if playbackState == .Playing {
            items[0] = self.pauseButton
        }
        self.toolbar.setItems(items, animated: false)
    }
    
    func setLight(light: CircleView, ratio: CGFloat) {
        light.circleColor = getColorWithAlpha(light.circleColor, ratio: ratio)
    }
    
    func getColorWithAlpha(color: UIColor, ratio: CGFloat) -> UIColor {
        var r: CGFloat = 0
        var g: CGFloat = 0
        var b: CGFloat = 0
        var a: CGFloat = 0
        
        if color.getRed(&r, green: &g, blue: &b, alpha: &a) {
            return UIColor(
                red: r,
                green: g,
                blue: b,
                alpha: ratio
            )
        }
        
        return color
        
    }
    
    func onLightOrganDataUpdated(bassLevel: CGFloat, midLevel: CGFloat, trebleLevel: CGFloat) {
        setLight(bassLight, ratio: bassLevel)
        setLight(midLight, ratio: midLevel)
        setLight(trebleLight, ratio: trebleLevel)
        
        let bassValue = UInt8(round(255 * bassLevel))
        let midValue = UInt8(round(255 * midLevel))
        let trebleValue = UInt8(round(255 * trebleLevel))
        let bytes:[UInt8] = [bassValue, midValue, trebleValue]
        
        sendCommand(bytes)
    }
    
    func sendCommand(bytes: [UInt8]) {        
        let queue = dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0)
        
        dispatch_async(queue) {
            self.outStream?.write(UnsafePointer<UInt8>(bytes), maxLength: bytes.count)
        }
    }
    
    func createNewSocket(defaults: NSUserDefaults) {
        let host = defaults.stringForKey("remote_device_host_preference")
        let port = defaults.integerForKey("remote_device_port_preference")
        
        if host != nil && port > 0 {
            NSStream.getStreamsToHostWithName(host!, port: port, inputStream: nil, outputStream: &outStream)
            
            outStream?.delegate = self
            outStream?.scheduleInRunLoop(NSRunLoop.currentRunLoop(), forMode: NSDefaultRunLoopMode)
            outStream?.open()
        }
    }
    
    func releaseOutStream() {
        outStream?.delegate = nil
        outStream?.removeFromRunLoop(NSRunLoop.currentRunLoop(), forMode: NSDefaultRunLoopMode)
        outStream?.close()
        outStream = nil
    }
    
    func stream(aStream: NSStream, handleEvent eventCode: NSStreamEvent) {
        switch eventCode {
        case NSStreamEvent.EndEncountered:
            print("EndEncountered")
            releaseOutStream()
        case NSStreamEvent.ErrorOccurred:
            print("ErrorOccurred")
            releaseOutStream()
        case NSStreamEvent.HasSpaceAvailable:
            print("HasSpaceAvailable")
        case NSStreamEvent.None:
            print("None")
        case NSStreamEvent.OpenCompleted:
            print("OpenCompleted")
        default:
            print("Unknown")
        }
    }
    
    func defaultsChanged() {
        let defaults = NSUserDefaults.standardUserDefaults()
        let useRemoteDevice = defaults.boolForKey("use_remote_device_preference")
        
        if outStream != nil {
            releaseOutStream()
        }
        
        if useRemoteDevice {
            createNewSocket(defaults)
        }
    }
}

