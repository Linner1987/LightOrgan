//
//  CustomPlayer.swift
//  AudioPoligono
//
//  Created by Marcin Kruszyński on 27/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import Foundation
import AVFoundation
import MediaPlayer

class CustomPlayer: NSObject {
    
    var engine: AVAudioEngine!
    var player: AVAudioPlayerNode!
    var mixer: AVAudioMixerNode!
    
    var currentItem: MPMediaItem?
    
    class func initSession() {
        
        NSNotificationCenter.defaultCenter().addObserver(self, selector: #selector(CustomPlayer.audioSessionInterrupted(_:)), name: AVAudioSessionInterruptionNotification, object: AVAudioSession.sharedInstance())
        
        try! AVAudioSession.sharedInstance().setCategory(AVAudioSessionCategoryPlayback)
        
        try! AVAudioSession.sharedInstance().setActive(true)
    }
    
    func setup() {
        engine = AVAudioEngine()
        player = AVAudioPlayerNode()
        mixer = engine.mainMixerNode
    }
    
    func setItems(mediaItemCollection: MPMediaItemCollection) {
        
        if mediaItemCollection.count > 0 {
            
            currentItem = mediaItemCollection.items[0] as MPMediaItem
            
            let url = currentItem!.assetURL
            
            if (url != nil) {
                
                //let engine = AVAudioEngine()
                //let player = AVAudioPlayerNode()
                
                let file = try! AVAudioFile(forReading: url!)
                
                //let mixer = engine.mainMixerNode
                engine.attachNode(player)
                engine.connect(player, to: mixer, format: file.processingFormat)
                player.scheduleFile(file, atTime: nil, completionHandler: nil)
                engine.prepare()
                
                do {
                    try engine.start()
                    player.play()
                    
                    updateMPNowPlayingInfoCenter()
                    
                } catch {
                    print("Error")
                }
            }
        }
    }
    
    func pause() {
        player.pause()
        
        updateMPNowPlayingInfoCenter()        
    }
    
    func play() {
        player.play()
        
        updateMPNowPlayingInfoCenter()
    }
    
    func updateMPNowPlayingInfoCenter() {
        if currentItem == nil {
            return;
        }
        
        MPNowPlayingInfoCenter.defaultCenter().nowPlayingInfo = [MPMediaItemPropertyTitle: (currentItem!.title != nil) ? currentItem!.title! : "",
                                                                 MPMediaItemPropertyArtist: (currentItem!.artist != nil) ? currentItem!.artist! : "",
                                                                 MPMediaItemPropertyPlaybackDuration: currentItem!.playbackDuration,
                                                                 MPNowPlayingInfoPropertyPlaybackRate: player.playing ? 1 : 0]
        player.playing
    }
    
    func audioSessionInterrupted(notification:NSNotification) {
        print("interruption received: \(notification)")
    }
    
    func remoteControlReceivedWithEvent(receivedEvent:UIEvent)  {
        if (receivedEvent.type == .RemoteControl) {
            switch receivedEvent.subtype {
            case .RemoteControlTogglePlayPause:
                if player.rate > 0.0 {
                    pause()
                } else {
                    play()
                }
            case .RemoteControlPlay:
                play()
            case .RemoteControlPause:
                pause()
            default:
                print("received sub type \(receivedEvent.subtype) Ignoring")
            }
        }
    }
}