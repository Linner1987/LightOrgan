//
//  EZAudioPlayerEx.swift
//  AudioPoligono
//
//  Created by Marcin Kruszyński on 29/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import Foundation
import AVFoundation
import MediaPlayer
import EZAudio

let EZAudioPlayerExNowPlayingItemDidChangeNotification = "EZAudioPlayerExNowPlayingItemDidChangeNotification"
let EZAudioPlayerExPlaybackStateDidChangeNotification = "EZAudioPlayerExPlaybackStateDidChangeNotification"


public class EZAudioPlayerEx: NSObject, EZAudioPlayerDelegate, EZAudioFFTDelegate {
    
    let FFTWindowSize: vDSP_Length  = 4096
    
    let LOW_MAX_VALUE: Double = 5000
    let MID_MAX_VALUE: Double = 6000
    let HIGH_MAX_VALUE: Double = 2000
    
    let LOW_FREQUENCY: Double = 50
    let MID_FREQUENCY: Double = 3000
    let HIGH_FREQUENCY: Double = 16000
    
    var audioPlayer: EZAudioPlayer!
    var sampleRate: Float = 0
    
    var items: [MPMediaItem]?
    public var nowPlayingItem: MPMediaItem?
    
    var nowPlayingInfo: [String : AnyObject]?
    
    //let audioSession: AVAudioSession
    let commandCenter: MPRemoteCommandCenter
    let nowPlayingInfoCenter: MPNowPlayingInfoCenter
    let notificationCenter: NSNotificationCenter
    
    var fft: EZAudioFFTRolling!
    
    override init() {
        //self.audioSession = AVAudioSession.sharedInstance()
        self.commandCenter = MPRemoteCommandCenter.sharedCommandCenter()
        self.nowPlayingInfoCenter = MPNowPlayingInfoCenter.defaultCenter()
        self.notificationCenter = NSNotificationCenter.defaultCenter()
        
        super.init()
        
        //self.notificationCenter.addObserver(self, selector: #selector(CustomAudioPlayer.handleInterruption(_:)), name: AVAudioSessionInterruptionNotification, object: self.audioSession)
        //try! self.audioSession.setCategory(AVAudioSessionCategoryPlayback)
        //try! self.audioSession.setActive(true)
        
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
        return self.audioPlayer?.isPlaying ?? false
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
        
        let file = EZAudioFile(URL: item.assetURL)
        
        let player = EZAudioPlayer(delegate: self)
        
        player.playAudioFile(file)
        
        self.audioPlayer = player
        
        self.sampleRate = Float(file.clientFormat.mSampleRate)
        self.fft = EZAudioFFTRolling(windowSize: FFTWindowSize, sampleRate: self.sampleRate, delegate: self)
        
        self.nowPlayingItem = item
        
        self.updateNowPlayingInfoForCurrentPlaybackItem()
        
        self.notifyOnTrackChanged()
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
        self.updateNowPlayingInfoElapsedTime()
        self.notifyOnPlaybackStateChanged()
    }
    
    public func pause() {
        self.audioPlayer?.pause()
        self.updateNowPlayingInfoElapsedTime()
        self.notifyOnPlaybackStateChanged()
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
        
        self.updateNowPlayingInfoElapsedTime()
    }
    
    func updateNowPlayingInfoElapsedTime() {
        guard var nowPlayingInfo = self.nowPlayingInfo, let _ = self.audioPlayer else { return }
        
        nowPlayingInfo[MPNowPlayingInfoPropertyElapsedPlaybackTime] = NSNumber(double: audioPlayer.currentTime);
        self.configureNowPlayingInfo(nowPlayingInfo)
    }
    
    func configureNowPlayingInfo(nowPlayingInfo: [String: AnyObject]?) {
        self.nowPlayingInfoCenter.nowPlayingInfo = nowPlayingInfo
        self.nowPlayingInfo = nowPlayingInfo
    }
    
    func endPlayback() {
        self.nowPlayingItem = nil
        self.audioPlayer?.delegate = nil
        self.audioPlayer = nil
        self.sampleRate = 0
        
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
        self.notificationCenter.postNotificationName(EZAudioPlayerExPlaybackStateDidChangeNotification, object: self)
    }
    
    func notifyOnTrackChanged() {
        self.notificationCenter.postNotificationName(EZAudioPlayerExNowPlayingItemDidChangeNotification, object: self)
    }
    
    public func audioPlayer(audioPlayer: EZAudioPlayer!, playedAudio buffer: UnsafeMutablePointer<UnsafeMutablePointer<Float>>, withBufferSize bufferSize: UInt32, withNumberOfChannels numberOfChannels: UInt32, inAudioFile audioFile: EZAudioFile!) {
        
        self.fft.computeFFTWithBuffer(buffer[0], withBufferSize: bufferSize)
    }
    
    public func audioPlayer(audioPlayer: EZAudioPlayer!, reachedEndOfAudioFile audioFile: EZAudioFile!) {
        if self.nextItem == nil {
            self.endPlayback()
        }
        else {
            self.nextTrack()
        }
    }
    
    public func fft(fft: EZAudioFFT!, updatedWithFFTData fftData: UnsafeMutablePointer<Float>, bufferSize: vDSP_Length) {
        
        //bass
        var energySum: Double = 0
        
        var k: Int = 2
        let captureSize: Double = Double(bufferSize) / 2
        let sampleRate: Double = Double(self.sampleRate) / 2000
        
        var nextFrequency: Double = (Double(k / 2) * sampleRate) / (captureSize)
        while nextFrequency < LOW_FREQUENCY {
            energySum += getAmplitude(fftData[k] * 255, i: fftData[k + 1] * 255)
            k += 2
            nextFrequency = (Double(k / 2) * sampleRate) / (captureSize)
        }
        var sampleAvgAudioEnergy: Double = energySum / ((Double(k) * 1.0) / 2.0)
        let bassLevel: Double = getRatioAmplitude(sampleAvgAudioEnergy, maxValue: LOW_MAX_VALUE)
        
        
        //mid
        energySum = 0
        while nextFrequency < MID_FREQUENCY {
            energySum += getAmplitude(fftData[k] * 255, i: fftData[k + 1] * 255)
            k += 2
            nextFrequency = (Double(k / 2) * sampleRate) / (captureSize)
        }
        sampleAvgAudioEnergy = energySum / ((Double(k) * 1.0) / 2.0)
        let midLevel: Double = getRatioAmplitude(sampleAvgAudioEnergy, maxValue: MID_MAX_VALUE)
        
        
        //treble
        energySum = 0
        while ((nextFrequency < HIGH_FREQUENCY) && (k < Int(bufferSize))) {
            energySum += getAmplitude(fftData[k] * 255, i: fftData[k + 1] * 255)
            k += 2
            nextFrequency = (Double(k / 2) * sampleRate) / (captureSize)
        }
        sampleAvgAudioEnergy = energySum / ((Double(k) * 1.0) / 2.0)
        let trebleLevel: Double = getRatioAmplitude(sampleAvgAudioEnergy, maxValue: HIGH_MAX_VALUE)
        
        print("bass=\(bassLevel) mid=\(midLevel) treble=\(trebleLevel)")
    }
    
    func getAmplitude(r: Float, i: Float) -> Double {
        return Double(sqrt(r * r + i * i))
    }
    
    func getRatioAmplitude(energy: Double, maxValue: Double) -> Double {
        var value: Double = energy * 1000
        if value > maxValue {
            value = maxValue
        }
    
        var v:Double = value / maxValue
    
        if v < 0.05 {
           v = 0.05
        }
    
        return v;
    }
}