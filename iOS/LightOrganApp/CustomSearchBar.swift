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
}
