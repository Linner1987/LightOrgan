//
//  CustomAudioPlayer.swift
//  AudioPoligono
//
//  Created by Marcin Kruszyński on 27/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import Foundation
import AVFoundation
import MediaPlayer

let CustomAudioPlayerNowPlayingItemDidChangeNotification = "CustomAudioPlayerNowPlayingItemDidChangeNotification"
let CustomAudioPlayerPlaybackStateDidChangeNotification = "CustomAudioPlayerPlaybackStateDidChangeNotification"

public class CustomAudioPlayer: NSObject {
    
    var audioEngine: AVAudioEngine!
    var audioPlayer: AVAudioPlayerNode!
    
    var items: [MPMediaItem]?
    public var nowPlayingItem: MPMediaItem?
    
    var nowPlayingInfo: [String : AnyObject]?
    
    let audioSession: AVAudioSession
    let commandCenter: MPRemoteCommandCenter
    let nowPlayingInfoCenter: MPNowPlayingInfoCenter
    let notificationCenter: NSNotificationCenter
    
    override init() {
        self.audioSession = AVAudioSession.sharedInstance()
        self.commandCenter = MPRemoteCommandCenter.sharedCommandCenter()
        self.nowPlayingInfoCenter = MPNowPlayingInfoCenter.defaultCenter()
        self.notificationCenter = NSNotificationCenter.defaultCenter()
        
        super.init()
        
        self.notificationCenter.addObserver(self, selector: #selector(CustomAudioPlayer.handleInterruption(_:)), name: AVAudioSessionInterruptionNotification, object: self.audioSession)
        try! self.audioSession.setCategory(AVAudioSessionCategoryPlayback)
        try! self.audioSession.setActive(true)
        
        self.configureCommandCenter()
    }
    
    
    public var nextItem: MPMediaItem? {
        guard let items = self.items, nowPlayingItem = self.nowPlayingItem else { return nil }
        
        var nextItemIndex = items.indexOf(nowPlayingItem)! + 1
        if nextItemIndex >= items.count {
            nextItemIndex = 0
        }
        
        return items[nextItemIndex]
    }
    
    public var previousItem: MPMediaItem? {
        guard let items = self.items, nowPlayingItem = self.nowPlayingItem else { return nil }
        
        var previousItemIndex = items.indexOf(nowPlayingItem)! - 1
        if previousItemIndex < 0 {
            previousItemIndex = items.count - 1
        }
        
        return items[previousItemIndex]
    }
    
    public var isPlaying: Bool {
        return self.audioPlayer?.playing ?? false
    }
    
    public func playItems(itemCollection: MPMediaItemCollection, firstItem: MPMediaItem? = nil) {
        self.items = itemCollection.items
        
        if items?.count == 0 {
            self.endPlayback()
            return
        }
        
        let item = firstItem ?? self.items!.first!
        
        self.playItem(item)
    }
    
    func playItem(item: MPMediaItem) {
        
        guard let url = item.assetURL, let file = try? AVAudioFile(forReading: url) else {
            self.endPlayback()
            return
        }
        
        //try! self.audioSession.setActive(true)
        
        let audioEngine = AVAudioEngine()
        let audioPlayer = AVAudioPlayerNode()
        
        audioEngine.attachNode(audioPlayer)
        audioEngine.connect(audioPlayer, to: audioEngine.mainMixerNode, format: file.processingFormat)
        
        audioPlayer.scheduleFile(file, atTime: nil, completionHandler: {
            if self.nextItem == nil {
                self.endPlayback()
            }
            else {
                self.nextTrack()
            }
        })
        audioEngine.prepare()
        
        do {
            try audioEngine.start()
            audioPlayer.play()
            
            self.audioEngine = audioEngine
            self.audioPlayer = audioPlayer
            
            self.nowPlayingItem = item
            
            self.updateNowPlayingInfoForCurrentPlaybackItem()
            
            self.notifyOnTrackChanged()
            
        } catch {
            print("Error")
        }
    }
    
    public func togglePlayPause() {
        if self.isPlaying {
            self.pause()
        }
        else {
            self.play()
        }
    }
    
    public func play() {
        self.audioPlayer?.play()
        self.updateNowPlayingInfoElapsedTimeAndRate()
        self.notifyOnPlaybackStateChanged()
        
        //try! self.audioSession.setActive(true)
    }
    
    public func pause() {
        self.audioPlayer?.pause()
        self.updateNowPlayingInfoElapsedTimeAndRate()
        self.notifyOnPlaybackStateChanged()
        
        //try! self.audioSession.setActive(false)
    }
    
    public func nextTrack() {
        guard let nextItem = self.nextItem else { return }
        self.playItem(nextItem)
    }
    
    public func previousTrack() {
        guard let previousItem = self.previousItem else { return }
        self.playItem(previousItem)
    }
    
    func configureCommandCenter() {
        self.commandCenter.playCommand.addTargetWithHandler { [weak self] event -> MPRemoteCommandHandlerStatus in
            guard let sself = self else { return .CommandFailed }
            sself.play()
            return .Success
        }
        
        self.commandCenter.pauseCommand.addTargetWithHandler { [weak self] event -> MPRemoteCommandHandlerStatus in
            guard let sself = self else { return .CommandFailed }
            sself.pause()
            return .Success
        }
        
        self.commandCenter.nextTrackCommand.addTargetWithHandler { [weak self] event -> MPRemoteCommandHandlerStatus in
            guard let sself = self else { return .CommandFailed }
            sself.nextTrack()
            return .Success
        }
        
        self.commandCenter.previousTrackCommand.addTargetWithHandler { [weak self] event -> MPRemoteCommandHandlerStatus in
            guard let sself = self else { return .CommandFailed }
            sself.previousTrack()
            return .Success
        }
        
    }
    
    func updateNowPlayingInfoForCurrentPlaybackItem() {
        guard let _ = self.audioPlayer, nowPlayingItem = self.nowPlayingItem else {
            self.configureNowPlayingInfo(nil)
            return
        }
        
        let title = (nowPlayingItem.title != nil) ? nowPlayingItem.title! : ""
        let albumTitle = (nowPlayingItem.albumTitle != nil) ? nowPlayingItem.albumTitle! : ""
        let artist = (nowPlayingItem.artist != nil) ? nowPlayingItem.artist! : ""
        
        let nowPlayingInfo = [MPMediaItemPropertyTitle: title,
                              MPMediaItemPropertyAlbumTitle: albumTitle,
                              MPMediaItemPropertyArtist: artist,
                              MPMediaItemPropertyPlaybackDuration: nowPlayingItem.playbackDuration,
                              MPNowPlayingInfoPropertyPlaybackRate: NSNumber(float: 1.0)]
        
        
        self.configureNowPlayingInfo(nowPlayingInfo)
        
        self.updateNowPlayingInfoElapsedTimeAndRate()
    }
    
    private func currentTime(audioPlayer: AVAudioPlayerNode) -> NSTimeInterval {
        if  let nodeTime: AVAudioTime = audioPlayer.lastRenderTime, playerTime: AVAudioTime = audioPlayer.playerTimeForNodeTime(nodeTime) {
            return Double(Double(playerTime.sampleTime) / playerTime.sampleRate)
        }
        return 0
    }
    
    func updateNowPlayingInfoElapsedTimeAndRate() {
        guard var nowPlayingInfo = self.nowPlayingInfo, let _ = self.audioPlayer else { return }
        
        //nowPlayingInfo[MPNowPlayingInfoPropertyElapsedPlaybackTime] = NSNumber(double: currentTime(audioPlayer));
        nowPlayingInfo[MPNowPlayingInfoPropertyPlaybackRate] = self.isPlaying ? NSNumber(float: 1.0) : NSNumber(float: 0.0)
        self.configureNowPlayingInfo(nowPlayingInfo)
    }
    
    func configureNowPlayingInfo(nowPlayingInfo: [String: AnyObject]?) {
        self.nowPlayingInfoCenter.nowPlayingInfo = nowPlayingInfo
        self.nowPlayingInfo = nowPlayingInfo
    }
    
    func endPlayback() {
        self.nowPlayingItem = nil
        self.audioEngine = nil
        self.audioPlayer = nil
        
        self.updateNowPlayingInfoForCurrentPlaybackItem()
        self.notifyOnTrackChanged()
    }
    
    func handleInterruption(notification: NSNotification){
        
        let interruptionTypeAsObject =
            notification.userInfo![AVAudioSessionInterruptionTypeKey] as! NSNumber
        
        let interruptionType = AVAudioSessionInterruptionType(rawValue:
            interruptionTypeAsObject.unsignedLongValue)
        
        if let type = interruptionType {
            if type == .Ended{
                //resume
            }
        }
        
    }
    func notifyOnPlaybackStateChanged() {
        self.notificationCenter.postNotificationName(CustomAudioPlayerPlaybackStateDidChangeNotification, object: self)
    }
    
    func notifyOnTrackChanged() {
        self.notificationCenter.postNotificationName(CustomAudioPlayerNowPlayingItemDidChangeNotification, object: self)
    }
}
    



