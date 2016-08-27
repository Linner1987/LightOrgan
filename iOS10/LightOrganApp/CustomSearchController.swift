//
//  CustomSearchController.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 06/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit

class CustomSearchController: UISearchController, UISearchBarDelegate {
    
    lazy var _searchBar: CustomSearchBar = {
        [unowned self] in
        let result = CustomSearchBar(frame: CGRect.zero)
        result.delegate = self
        
        return result
        }()
    
    override var searchBar: UISearchBar {
        get {
            return _searchBar
        }
    }
}
