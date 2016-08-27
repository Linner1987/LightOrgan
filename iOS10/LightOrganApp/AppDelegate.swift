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
    
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplicationLaunchOptionsKey: Any]?) -> Bool {
        
        self.populateRegistrationDomain()
        
        // Override point for customization after application launch.
        return true
    }
    
    func applicationWillResignActive(_ application: UIApplication) {
        // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
        // Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
    }

    func applicationDidEnterBackground(_ application: UIApplication) {
        // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
        // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
    }

    func applicationWillEnterForeground(_ application: UIApplication) {
        // Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
    }

    func applicationDidBecomeActive(_ application: UIApplication) {
        // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
    }

    func applicationWillTerminate(_ application: UIApplication) {
        // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
    }
    
    
    func application(_ application: UIApplication, shouldSaveApplicationState coder: NSCoder) -> Bool {
        return true
    }
    
    func application(_ application: UIApplication, shouldRestoreApplicationState coder: NSCoder) -> Bool {
        return true
    }
    
    func populateRegistrationDomain() {
        let settingsBundleURL = Bundle.main.url(forResource: "Settings", withExtension: "bundle")
        
        let appDefaults = loadDefaultsFromSettingsPage("Root.plist", inSettingsBundleAtURL: settingsBundleURL!)
        
        let defaults = UserDefaults.standard
        defaults.register(defaults: appDefaults!)
        defaults.synchronize()
    }
    
    
    func loadDefaultsFromSettingsPage(_ plistName: String, inSettingsBundleAtURL settingsBundleURL: URL) -> [String:AnyObject]? {
        let settingsDict = NSDictionary(contentsOf: settingsBundleURL.appendingPathComponent(plistName))
        
        if settingsDict == nil {
            return nil;
        }
        
        let prefSpecifierArray = settingsDict!.value(forKey: "PreferenceSpecifiers") as? [[String:AnyObject]]
        
        if prefSpecifierArray == nil {
            return nil;
        }
        
        var keyValuePairs: [String:AnyObject] = [:]
        
        for prefItem in prefSpecifierArray! {
            let prefItemType = prefItem["Type"] as? String
            let prefItemKey = prefItem["Key"] as? String
            let prefItemDefaultValue = prefItem["DefaultValue"]             
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


func += <K, V> (left: inout [K:V], right: [K:V]) {
    for (k, v) in right {
        left.updateValue(v, forKey: k)
    }
}
