//
//  ViewController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 29/04/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer

class ViewController: UIViewController, StreamDelegate /*, MPMediaPickerControllerDelegate*/ {

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
    
    var outStream: OutputStream?
    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        self.pauseButton = UIBarButtonItem(barButtonSystemItem: .pause, target: self, action: #selector(ViewController.playPausePressed(_:)))
        self.pauseButton.style = .plain
        
        self.player = MPMusicPlayerController.systemMusicPlayer()
        self.player.repeatMode = .all
        
        let notificationCenter = NotificationCenter.default
        notificationCenter.addObserver(self, selector: #selector(ViewController.nowPlayingItemChanged(_:)), name: NSNotification.Name.MPMusicPlayerControllerNowPlayingItemDidChange, object: self.player)
        notificationCenter.addObserver(self, selector: #selector(ViewController.playbackStateChanged(_:)), name: NSNotification.Name.MPMusicPlayerControllerPlaybackStateDidChange, object: self.player)
        self.player.beginGeneratingPlaybackNotifications()
    }
    
    override func viewWillAppear(_ animated: Bool) {
        super.viewWillAppear(animated)
        
        self.defaultsChanged()
        
        NotificationCenter.default.addObserver(self, selector: #selector(ViewController.defaultsChanged), name: UserDefaults.didChangeNotification, object: nil)
        
        //test
        onLightOrganDataUpdated(0.6, midLevel: 0.3, trebleLevel: 1)
    }
    
    override func viewWillDisappear(_ animated: Bool) {
        super.viewWillDisappear(animated)
        
        NotificationCenter.default.removeObserver(self, name: UserDefaults.didChangeNotification, object: nil)
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
        
        NotificationCenter.default.removeObserver(self, name: NSNotification.Name.MPMusicPlayerControllerNowPlayingItemDidChange, object: self.player)
        NotificationCenter.default.removeObserver(self, name: NSNotification.Name.MPMusicPlayerControllerPlaybackStateDidChange, object: self.player)
    }
    
    override func viewWillTransition(to size: CGSize, with coordinator: UIViewControllerTransitionCoordinator) {
        super.viewWillTransition(to: size, with: coordinator)
        CATransaction.begin()
        CATransaction.setDisableActions(true)
        
        coordinator.animate(alongsideTransition: { (ctx) -> Void in
            
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
    
    @IBAction func unwindToPlayer(_ sender: UIStoryboardSegue) {
        
        if let sourceViewController = sender.source as? FileListViewController,
            let mediaItemCollection = sourceViewController.didPickMediaItems {
            
            self.collection = mediaItemCollection
            self.player.setQueue(with: self.collection)
            
            var playbackState = self.player.playbackState as MPMusicPlaybackState
            if playbackState == .playing {
                self.player.pause()
            }
            
            let item = self.collection.items[0] as MPMediaItem
            self.player.nowPlayingItem = item
            
            playbackState = self.player.playbackState as MPMusicPlaybackState
            self.player.play()            
        }
    }
    
    @IBAction func playPausePressed(_ sender: AnyObject) {
        let playbackState = self.player.playbackState as MPMusicPlaybackState
        if playbackState == .stopped || playbackState == .paused {
            self.player.play()
            
        } else if playbackState == .playing {
            self.player.pause()
        }
    }
    
    func nowPlayingItemChanged(_ notification: Notification) {
        if let currentItem = self.player.nowPlayingItem as MPMediaItem? {
            self.song.text = currentItem.value(forProperty: MPMediaItemPropertyTitle) as? String
        } else {            
            self.song.text = nil
        }
    }
    
    func playbackStateChanged(_ notification: Notification) {
        let playbackState = self.player.playbackState as MPMusicPlaybackState
        
        self.toolbar.isHidden = playbackState != .playing && playbackState != .paused
        toolbarHeightConstraint.priority = (playbackState != .playing && playbackState != .paused) ? 999 : 250
        
        var items = self.toolbar.items!
        if playbackState == .stopped || playbackState == .paused {
            items[0] = self.playButton
        } else if playbackState == .playing {
            items[0] = self.pauseButton
        }
        self.toolbar.setItems(items, animated: false)
    }
    
    func setLight(_ light: CircleView, ratio: CGFloat) {
        light.circleColor = getColorWithAlpha(light.circleColor, ratio: ratio)
    }
    
    func getColorWithAlpha(_ color: UIColor, ratio: CGFloat) -> UIColor {
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
    
    func onLightOrganDataUpdated(_ bassLevel: CGFloat, midLevel: CGFloat, trebleLevel: CGFloat) {
        setLight(bassLight, ratio: bassLevel)
        setLight(midLight, ratio: midLevel)
        setLight(trebleLight, ratio: trebleLevel)
        
        let bassValue = UInt8(round(255 * bassLevel))
        let midValue = UInt8(round(255 * midLevel))
        let trebleValue = UInt8(round(255 * trebleLevel))
        let bytes:[UInt8] = [bassValue, midValue, trebleValue]
        
        sendCommand(bytes)
    }
    
    func sendCommand(_ bytes: [UInt8]) {        
        let queue = DispatchQueue.global(qos: .default)
        
        queue.async {
            self.outStream?.write(UnsafePointer<UInt8>(bytes), maxLength: bytes.count)
        }
    }
    
    func createNewSocket(_ defaults: UserDefaults) {
        let host = defaults.string(forKey: "remote_device_host_preference")
        let port = defaults.integer(forKey: "remote_device_port_preference")
        
        if host != nil && port > 0 {
            Stream.getStreamsToHost(withName: host!, port: port, inputStream: nil, outputStream: &outStream)
            
            outStream?.delegate = self
            outStream?.schedule(in: RunLoop.current, forMode: RunLoopMode.defaultRunLoopMode)
            outStream?.open()
        }
    }
    
    func releaseOutStream() {
        outStream?.delegate = nil
        outStream?.remove(from: RunLoop.current, forMode: RunLoopMode.defaultRunLoopMode)
        outStream?.close()
        outStream = nil
    }
    
    func stream(_ aStream: Stream, handle eventCode: Stream.Event) {
        switch eventCode {
        case Stream.Event.endEncountered:
            print("EndEncountered")
            releaseOutStream()
        case Stream.Event.errorOccurred:
            print("ErrorOccurred")
            releaseOutStream()
        case Stream.Event.hasSpaceAvailable:
            print("HasSpaceAvailable")
        case Stream.Event():
            print("None")
        case Stream.Event.openCompleted:
            print("OpenCompleted")
        default:
            print("Unknown")
        }
    }
    
    func defaultsChanged() {
        let defaults = UserDefaults.standard
        let useRemoteDevice = defaults.bool(forKey: "use_remote_device_preference")
        
        if outStream != nil {
            releaseOutStream()
        }
        
        if useRemoteDevice {
            createNewSocket(defaults)
        }
    }
}

