// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#import <WindowsAzureMobileServices/WindowsAzureMobileServices.h>
#import "QSTodoListViewController.h"
#import "QSTodoService.h"


#pragma mark * Private Interface


@interface QSTodoListViewController ()

// Private properties
@property (strong, nonatomic) QSTodoService *todoService;
@property (nonatomic)           BOOL            useRefreshControl;
@property (nonatomic)           NSInteger       editedItemIndex;
@property (strong, nonatomic)   NSMutableDictionary *editedItem;

@end


#pragma mark * Implementation


@implementation QSTodoListViewController

@synthesize todoService;
@synthesize itemText;
@synthesize activityIndicator;


#pragma mark * UIView methods

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    self.editedItemIndex = [indexPath row];
    self.editedItem = [[self.todoService.items objectAtIndex:[indexPath row]] mutableCopy];
    
    [self performSegueWithIdentifier:@"detailSegue" sender:self];
}

- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    if ([[segue identifier] isEqualToString:@"detailSegue"]) {
        QSTodoItemViewController *ivc = (QSTodoItemViewController *)[segue destinationViewController];
        ivc.item = self.editedItem;
    }
}

- (void)viewWillAppear:(BOOL)animated
{
    self.navigationController.navigationBar.hidden = YES;
    
    if (self.editedItem && self.editedItemIndex >= 0) {
        // Returning from the details view controller
        NSDictionary *item = [self.todoService.items objectAtIndex:self.editedItemIndex];
        
        BOOL changed = ![item isEqualToDictionary:self.editedItem];
        if (changed) {
            [self.tableView setUserInteractionEnabled:NO];
            
            // Change the appearance to look greyed out until we remove the item
            NSIndexPath *indexPath = [NSIndexPath indexPathForRow:self.editedItemIndex inSection:0];
            
            UITableViewCell *cell = [self.tableView cellForRowAtIndexPath:indexPath];
            cell.textLabel.textColor = [UIColor grayColor];
            
            // Ask the todoService to update the item, and remove the row if it's been completed
            [self.todoService updateItem:self.editedItem atIndex:self.editedItemIndex completion:^(NSUInteger index) {
                if ([[self.editedItem objectForKey:@"complete"] boolValue]) {
                    // Remove the row from the UITableView
                    [self.tableView deleteRowsAtIndexPaths:@[ indexPath ]
                                          withRowAnimation:UITableViewRowAnimationTop];
                } else {
                    [self.tableView reloadRowsAtIndexPaths:[NSArray arrayWithObject:indexPath]
                                          withRowAnimation:UITableViewRowAnimationAutomatic];
                }
                
                [self.tableView setUserInteractionEnabled:YES];
                
                self.editedItem = nil;
                self.editedItemIndex = -1;
            }];
        } else {
            self.editedItem = nil;
            self.editedItemIndex = -1;
        }
    }    
     
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    // Create the todoService - this creates the Mobile Service client inside the wrapped service
    self.todoService = [QSTodoService defaultService];
    
    // Set the busy method
    UIActivityIndicatorView *indicator = self.activityIndicator;
    self.todoService.busyUpdate = ^(BOOL busy)
    {
        if (busy)
        {
            [indicator startAnimating];
        } else
        {
            [indicator stopAnimating];
        }
    };
    
    // have refresh control reload all data from server
    [self.refreshControl addTarget:self
                            action:@selector(onRefresh:)
                  forControlEvents:UIControlEventValueChanged];

    // load the local data, but don't pull from server
    [self.todoService refreshDataOnSuccess:^
     {
         [self.refreshControl endRefreshing];
         [self.tableView reloadData];
     }];
}

- (void) refresh
{
    [self.refreshControl beginRefreshing];
    [self.todoService syncData:^
     {
          [self.refreshControl endRefreshing];
          [self.tableView reloadData];
     }];
}


#pragma mark * UITableView methods


- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"Cell";
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (cell == nil)
    {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    // Set the label on the cell and make sure the label color is black (in case this cell
    // has been reused and was previously greyed out
    cell.textLabel.textColor = [UIColor blackColor];
    
    NSDictionary *item = [self.todoService.items objectAtIndex:indexPath.row];
    cell.textLabel.text = [item objectForKey:@"text"];
    
    return cell;
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    // Always a single section
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    // Return the number of items in the todoService items array
    return [self.todoService.items count];
}


#pragma mark * UITextFieldDelegate methods


-(BOOL)textFieldShouldReturn:(UITextField *)textField
{
    [textField resignFirstResponder];
    return YES;
}


#pragma mark * UI Actions


- (IBAction)onAdd:(id)sender
{
    if (itemText.text.length  == 0)
    {
        return;
    }
    
    NSDictionary *item = @{ @"text" : itemText.text, @"complete" : @NO };
    UITableView *view = self.tableView;
    [self.todoService addItem:item completion:^(NSUInteger index)
    {
        NSIndexPath *indexPath = [NSIndexPath indexPathForRow:index inSection:0];
        [view insertRowsAtIndexPaths:@[ indexPath ]
                    withRowAnimation:UITableViewRowAnimationTop];
    }];
    
    itemText.text = @"";
}


- (void)onRefresh:(id) sender
{
    [self refresh];
}


@end
