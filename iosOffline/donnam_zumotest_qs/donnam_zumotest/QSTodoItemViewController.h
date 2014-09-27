//
//  QSTodoItemViewController.h
//  donnam_zumotest
//
//  Created by DonnaM on 9/18/14.
//  Copyright (c) 2014 MobileServices. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface QSTodoItemViewController : UIViewController<UITextFieldDelegate>

@property (nonatomic, weak) NSMutableDictionary *item;

@end
