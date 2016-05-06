//
//  CustomSearchBar.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 06/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit

class CustomSearchBar: UISearchBar {
    
    
    override func layoutSubviews() {
        super.layoutSubviews()
        setShowsCancelButton(false, animated: false)
    }
    
    
    func indexOfSearchFieldInSubviews() -> Int! {
        var index: Int!
        let searchBarView = subviews[0]
        
        for i in 0 ..< searchBarView.subviews.count {
            if searchBarView.subviews[i].isKindOfClass(UITextField) {
                index = i
                break
            }
        }
        
        return index
    }
    
    
    override func drawRect(rect: CGRect) {
        
        if let index = indexOfSearchFieldInSubviews() {
            
            let searchField: UITextField = subviews[0].subviews[index] as! UITextField
            
            searchField.textColor = .whiteColor()
            searchField.backgroundColor = UIColor(red: 0.14, green: 0.14, blue: 0.14, alpha: 1.0)
        }
        
        super.drawRect(rect)
    }
}
