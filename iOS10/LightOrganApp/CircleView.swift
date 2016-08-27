//
//  CircleView.swift
//  LightOrganApp
//
//  Created by Marcin Kruszyński on 07/05/16.
//  Copyright © 2016 Marcin Kruszyński. All rights reserved.
//

import UIKit

@IBDesignable
class CircleView: UIView {
    
    @IBInspectable var circleColor: UIColor = UIColor.red
    
    
    required init?(coder aDecoder: NSCoder) {
        super.init(coder: aDecoder)
        configure()
    }
    
    override init(frame: CGRect) {
        super.init(frame: frame)
        configure()
    }
    
    func configure() {
        contentMode = .redraw
    }
    
    override func draw(_ rect: CGRect) {
        drawCircle(circleColor)
    }    
    
    func drawCircle(_ color: UIColor) {
        
        let context = UIGraphicsGetCurrentContext()
        
        let a = min(bounds.size.width, bounds.size.height)
        let leftX = self.bounds.midX - a / 2
        let topY = self.bounds.midY - a / 2
        let rectangle = CGRect(x: leftX, y: topY, width: a, height: a)
        
        context?.setFillColor(circleColor.cgColor)
        context?.fillEllipse(in: rectangle)
    }
}
