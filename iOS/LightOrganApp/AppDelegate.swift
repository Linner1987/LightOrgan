//
//  AppDelegate.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 29/04/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit

@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {

    var window: UIWindow?


    func application(application: UIApplication, didFinishLaunchingWithOptions launchOptions: [NSObject: AnyObject]?) -> Bool {
        
        self.populateRegistrationDomain()
        
        // Override point for customization after application launch.
        return true
    }

    func applicationWillResignActive(application: UIApplication) {
        // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
        // Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
    }

    func applicationDidEnterBackground(application: UIApplication) {
        // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
        // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
    }

    func applicationWillEnterForeground(application: UIApplication) {
        // Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
    }

    func applicationDidBecomeActive(application: UIApplication) {
        // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
    }

    func applicationWillTerminate(application: UIApplication) {
        // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
    }
    
    
    func application(application: UIApplication, shouldSaveApplicationState coder: NSCoder) -> Bool {
        return true
    }
    
    func application(application: UIApplication, shouldRestoreApplicationState coder: NSCoder) -> Bool {
        return true
    }
    
    func populateRegistrationDomain() {
        let settingsBundleURL = NSBundle.mainBundle().URLForResource("Settings", withExtension: "bundle")
        
        let appDefaults = loadDefaultsFromSettingsPage("Root.plist", inSettingsBundleAtURL: settingsBundleURL!)
        
        let defaults = NSUserDefaults.standardUserDefaults()
        defaults.registerDefaults(appDefaults!)
        defaults.synchronize()
    }
    
    
    func loadDefaultsFromSettingsPage(plistName: String, inSettingsBundleAtURL settingsBundleURL: NSURL) -> [String:AnyObject]? {
        let settingsDict = NSDictionary(contentsOfURL: settingsBundleURL.URLByAppendingPathComponent(plistName))
        
        if settingsDict == nil {
            return nil;
        }
        
        let prefSpecifierArray = settingsDict!.valueForKey("PreferenceSpecifiers") as? [[String:AnyObject]]
        
        if prefSpecifierArray == nil {
            return nil;
        }
        
        var keyValuePairs: [String:AnyObject] = [:]
        
        for prefItem in prefSpecifierArray! {
            let prefItemType = prefItem["Type"] as? String
            let prefItemKey = prefItem["Key"] as? String
            let prefItemDefaultValue = prefItem["DefaultValue"] as? String
            
            if prefItemType == "PSChildPaneSpecifier" {
                let prefItemFile = prefItem["File"] as? String
                if let childPageKeyValuePairs = loadDefaultsFromSettingsPage(prefItemFile!, inSettingsBundleAtURL: settingsBundleURL) {
                    keyValuePairs += childPageKeyValuePairs
                }
            }
            else if prefItemKey != nil && prefItemDefaultValue != nil {
                keyValuePairs[prefItemKey!] = prefItemDefaultValue
            }
        }
        
        return keyValuePairs
    }
}


func += <K, V> (inout left: [K:V], right: [K:V]) {
    for (k, v) in right {
        left.updateValue(v, forKey: k)
    }
}
