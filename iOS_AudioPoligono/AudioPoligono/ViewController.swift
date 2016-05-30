//
//  ViewController.swift
//  AudioPoligono
//
//  Created by Marcin Kruszyński on 26/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit
import MediaPlayer


class ViewController: UIViewController, MPMediaPickerControllerDelegate {
    
    var player: EZAudioPlayerEx!
    
    /*
    required init?(coder aDecoder: NSCoder)
    {
        super.init(coder: aDecoder)
        
        let appDelegate = UIApplication.sharedApplication().delegate as! AppDelegate
        self.player = appDelegate.getPlayer()
    }*/    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        try! AVAudioSession.sharedInstance().setActive(true)
        try! AVAudioSession.sharedInstance().setCategory(AVAudioSessionCategoryPlayAndRecord/*AVAudioSessionCategoryPlayback*/)
        self.player = EZAudioPlayerEx()
        
        //player.setup()
        //CustomPlayer.initSession()
    }
    
    //override func viewWillAppear(animated: Bool) {
        //super.viewWillAppear(animated)
        
        //UIApplication.sharedApplication().beginReceivingRemoteControlEvents()
        //becomeFirstResponder()
    //}
    
    //override func viewDidDisappear(animated: Bool) {
        //super.viewDidDisappear(animated)
        
        //UIApplication.sharedApplication().endReceivingRemoteControlEvents()
        //resignFirstResponder()
    //}
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }

    @IBAction func onButtonTap(sender: AnyObject) {
        
        /*
        let url = NSBundle.mainBundle().URLForResource("short", withExtension: "mp3")!
        let file = try! AVAudioFile(forReading: url)
        
        engine.attachNode(player)
        engine.connect(player, to: mixer, format: file.processingFormat)
        player.scheduleFile(file, atTime: nil, completionHandler: nil)
        engine.prepare()
        
        do {
            try engine.start()
            player.play()
            
        } catch {
            print("Error")
        } */
        
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
        
        if mediaItemCollection.count > 0 {
            self.player.playItems(mediaItemCollection)
        }
    }
    
    //override func remoteControlReceivedWithEvent(event: UIEvent?) {
    //    if event != nil {
    //        player.remoteControlReceivedWithEvent(event!)
    //    }
    //}
    
    //override func canBecomeFirstResponder() -> Bool {
    //    return true
    //}
}

