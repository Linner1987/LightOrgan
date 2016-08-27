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
            if searchBarView.subviews[i].isKind(of: UITextField.self) {
                index = i
                break
            }
        }
        
        return index
    }
    
    
    override func draw(_ rect: CGRect) {
        
        if let index = indexOfSearchFieldInSubviews() {
            
            let searchField: UITextField = subviews[0].subviews[index] as! UITextField
            
            searchField.textColor = .white
            searchField.backgroundColor = UIColor(red: 0.14, green: 0.14, blue: 0.14, alpha: 1.0)
            
            if let glassIconView = searchField.leftView as? UIImageView {
                glassIconView.image = glassIconView.image?.withRenderingMode(UIImageRenderingMode.alwaysTemplate)
                glassIconView.tintColor = UIColor.white
            }
            
            let textFieldInsideSearchBarLabel = searchField.value(forKey: "placeholderLabel") as? UILabel
            textFieldInsideSearchBarLabel?.textColor = .white
            
            let clearButton = searchField.value(forKey: "clearButton") as! UIButton
            clearButton.setImage(clearButton.imageView?.image?.withRenderingMode(UIImageRenderingMode.alwaysTemplate), for: UIControlState())
            clearButton.setImage(clearButton.imageView?.image?.withRenderingMode(UIImageRenderingMode.alwaysTemplate), for: .highlighted)
            clearButton.tintColor = UIColor.white
        }
        
        super.draw(rect)
    }
}
