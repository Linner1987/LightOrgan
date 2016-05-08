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
    
    @IBInspectable var circleColor: UIColor = UIColor.redColor()
    
    
    required init?(coder aDecoder: NSCoder) {
        super.init(coder: aDecoder)
        configure()
    }
    
    override init(frame: CGRect) {
        super.init(frame: frame)
        configure()
    }
    
    func configure() {
        contentMode = .ScaleAspectFit
    }
    
    override func drawRect(rect: CGRect) {
        drawCircle(circleColor)
    }    
    
    func drawCircle(color: UIColor) {
        
        let context = UIGraphicsGetCurrentContext()
        
        let a = min(bounds.size.width, bounds.size.height)
        let leftX = CGRectGetMidX(self.bounds) - a / 2
        let topY = CGRectGetMidY(self.bounds) - a / 2
        let rectangle = CGRectMake(leftX, topY, a, a)
        
        CGContextSetFillColorWithColor(context, circleColor.CGColor)
        CGContextFillEllipseInRect(context, rectangle)
    }
}
