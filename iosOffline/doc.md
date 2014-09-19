# Getting started with offline sync

## Updating the framework

The offline sync feature of Mobile Services is currently in beta and available through a separate download. This section describes how to upgrade the SDK that you are using in the app.

1.  Remove the existing framework `WindowsAzureMobileServices.framework` and select **Move to Trash** to really delete the files.

![][1]

2.  Download the [beta Mobile Services iOS SDK][].

3.  In your Xcode project, add the new Mobile Services framework to your project. Drag the WindowsAzureMobileServices.framework folder from the Finder into your project, and check the box **Copy items into destination group's folder (if needed)**.

![][2]

## Add Core Data support

1.  Open the application target, and in its **Build phases**, under the **Link Binary With Libraries**, add **CoreData.framework**.

    ![][3]

2.  Add Core Data code to the header. Open QSAppDelegate.h and replace
    with the following declarations:

        #import <UIKit/UIKit.h>  
        #import <CoreData/CoreData.h>  
          
        @interface QSAppDelegate : UIResponder <UIApplicationDelegate>  
          
        @property (strong, nonatomic) UIWindow *window;  
          
        @property (readonly, strong, nonatomic) NSManagedObjectContext *managedObjectContext;  
        @property (readonly, strong, nonatomic) NSManagedObjectModel *managedObjectModel;  
        @property (readonly, strong, nonatomic) NSPersistentStoreCoordinator *persistentStoreCoordinator;  
          
        - (void)saveContext;  
        - (NSURL *)applicationDocumentsDirectory;  
          
        @end


3. Open the implementation file QSAppDelegate.m and add the following codefor the core data stack methods. This is similar to the code youfrom the new application template in Xcode with the **Use Core Data** option, with the main difference that this code uses uses a private queue concurrency type when initializing the `NSManagedContextObject`.

        #import "QSAppDelegate.h"

        @implementation QSAppDelegate

        @synthesize managedObjectContext = _managedObjectContext;
        @synthesize managedObjectModel = _managedObjectModel;
        @synthesize persistentStoreCoordinator = _persistentStoreCoordinator;

        - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
        {
            return YES;
        }

        - (void)saveContext
        {
            NSError *error = nil;
            NSManagedObjectContext *managedObjectContext = self.managedObjectContext;
            if (managedObjectContext != nil) {
                if ([managedObjectContext hasChanges] && ![managedObjectContext save:&error]) {
                    // Replace this implementation with code to handle the error appropriately.
                    // abort() causes the application to generate a crash log and terminate. You should not use this function in a shipping application, although it may be useful during development.
                    NSLog(@"Unresolved error %@, %@", error, [error userInfo]);
                    abort();
                }
            }
        }

        #pragma mark - Core Data stack

        // Returns the managed object context for the application.
        // If the context doesn't already exist, it is created and bound to the persistent store coordinator for the application.
        - (NSManagedObjectContext *)managedObjectContext
        {
            if (_managedObjectContext != nil) {
                return _managedObjectContext;
            }

            NSPersistentStoreCoordinator *coordinator = [self persistentStoreCoordinator];
            if (coordinator != nil) {
                _managedObjectContext = [[NSManagedObjectContext alloc] initWithConcurrencyType:NSPrivateQueueConcurrencyType];
                [_managedObjectContext setPersistentStoreCoordinator:coordinator];
            }
            return _managedObjectContext;
        }

        // Returns the managed object model for the application.
        // If the model doesn't already exist, it is created from the application's model.
        - (NSManagedObjectModel *)managedObjectModel
        {
            if (_managedObjectModel != nil) {
                return _managedObjectModel;
            }
            NSURL *modelURL = [[NSBundle mainBundle] URLForResource:@"QSTodoDataModel" withExtension:@"momd"];
            _managedObjectModel = [[NSManagedObjectModel alloc] initWithContentsOfURL:modelURL];
            return _managedObjectModel;
        }

        // Returns the persistent store coordinator for the application.
        // If the coordinator doesn't already exist, it is created and the application's store added to it.
        - (NSPersistentStoreCoordinator *)persistentStoreCoordinator
        {
            if (_persistentStoreCoordinator != nil) {
                return _persistentStoreCoordinator;
            }

            NSURL *storeURL = [[self applicationDocumentsDirectory] URLByAppendingPathComponent:@"qstodoitem.sqlite"];

            NSError *error = nil;
            _persistentStoreCoordinator = [[NSPersistentStoreCoordinator alloc] initWithManagedObjectModel:[self managedObjectModel]];
            if (![_persistentStoreCoordinator addPersistentStoreWithType:NSSQLiteStoreType configuration:nil URL:storeURL options:nil error:&error]) {
                /*
                 Replace this implementation with code to handle the error appropriately.

                 abort() causes the application to generate a crash log and terminate. You should not use this function in a shipping application, although it may be useful during development.

                 Typical reasons for an error here include:
                 * The persistent store is not accessible;
                 * The schema for the persistent store is incompatible with current managed object model.
                 Check the error message to determine what the actual problem was.

                 If the persistent store is not accessible, there is typically something wrong with the file path. Often, a file URL is pointing into the application's resources directory instead of a writeable directory.

                 If you encounter schema incompatibility errors during development, you can reduce their frequency by:
                 * Simply deleting the existing store:
                 [[NSFileManager defaultManager] removeItemAtURL:storeURL error:nil]

                 * Performing automatic lightweight migration by passing the following dictionary as the options parameter:
                 @{NSMigratePersistentStoresAutomaticallyOption:@YES, NSInferMappingModelAutomaticallyOption:@YES}

                 Lightweight migration will only work for a limited set of schema changes; consult "Core Data Model Versioning and Data Migration Programming Guide" for details.

                 */

                NSLog(@"Unresolved error %@, %@", error, [error userInfo]);
                abort();
            }

            return _persistentStoreCoordinator;
        }

        #pragma mark - Application's Documents directory

        // Returns the URL to the application's Documents directory.
        - (NSURL *)applicationDocumentsDirectory
        {
            return [[[NSFileManager defaultManager] URLsForDirectory:NSDocumentDirectory inDomains:NSUserDomainMask] lastObject];
        }

        @end

At this point the application is (almost) ready to use Core Data, but it's not doing anything with it.

## Defining the model

The last thing we need to do to use the Core Data framework is to define the data model which will be stored in the persistent store. If you're not familiar with Core Data you can think of it as a simplified "schema" of the local database. We need to define the tables used by the application, but we also need to define two tables which will be used by the Mobile Services framework itself: one table to track the items which need to be synchronized with the server and one table to record any errors which may happen during this synchronization.

1.  In the project, select **New File**, and under the Core Data section, select **Data Model**.

    ![][2]

2.  Enter the name QSTodoDataModel and click Create.

3.  Select the data model in the folder view, then add the entities required for the application, by selecting the **Add Entity** button in the bottom of the page.

    ![][3]

4.  Add three entities, named `TodoItem`, `MS_TableOperations` and `MS_TableOperationErrors` with attributes defined as below. The first table to store the items themselves; the last two are framework-specific tables required for the offline feature to work.

    **TodoItem**

    | Attribute  |  Type   |
    |----------- |  ------ |
    | id         | String  |
    | complete   | Boolean |
    | text       | String  |
    | ms_version | String  |

    **MS_TableOperations**

    | Attribute  |    Type     |
    |----------- |   ------    |
    | id         | Integer 64  |
    | properties | Binary Data |
    | itemId     | String      |
    | table      | String      |

    **MS_TableOperationErrors**

    | Attribute  |    Type     |
    |----------- |   ------    |
    | id         | String      |
    | properties | Binary Data |

5.  Save the model, and build the project to make sure that everything is fine.

Now we have finished setting up the application to work with Core Data, but the app is not using it yet.

## Add offline sync support

To make the app work offline, this section will walk through the following changes:

-   Instead of using the regular `MSTable` class to access the mobile service, we'll use `MSSyncTable`. A sync table is basically a local table which knows how to push changes made locally to the remote table, and pull items from that table locally.
-   The *synchronization context* in the `MSClient` (a new property) must be initialized with the data source that we choose (in our case, the Core Data-based store implementation). The context is responsible for tracking which items have been changed locally, and sending those to the server when a push operation is started.

1.  In the implementation for the `QSTodoService` class, rename the private property `table` to `syncTable` and change its type to `MSSyncTable`:
    
        [@property] (nonatomic, strong) MSSyncTable *syncTable;

2.  In QSTodoService.m, add the line `#include "QSAppDelegate.h"`.

3.  Ihe initializer for the `QSTodoService` class, remove the line that creates the MSTable object and replace it with the following:

        QSAppDelegate *delegate = (QSAppDelegate* )[[UIApplication
        sharedApplication] delegate]; NSManagedObjectContext *context =
        delegate.managedObjectContext; MSCoreDataStore* store =
        [[MSCoreDataStore alloc] initWithManagedObjectContext:context];

        self.client.syncContext = [[MSSyncContext alloc]
        initWithDelegate:nil dataSource:store callback:nil];

        // Create an MSSyncTable instance to allow us to work with the
        TodoItem table self.syncTable = [self.client
        syncTableWithName:@"TodoItem"];

    This code initializes the sync context of the client with a data source and with no sync delegate. For the purposes of this tutorial, we will ignore sync conflicts, which are covered in the next tutorial [Handling conflicts with offline data sync].

4.  Add the declaration of the method `syncData` to QSTodoService.h:

        -   (void)syncData:(QSCompletionBlock)completion;

5.  Add the following definition of `syncData` to QSTodoService, which will update the sync table with remote changes:

        - (void)syncData:(QSCompletionBlock)completion
        {
            // Create a predicate that finds items where complete is false
            NSPredicate * predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
            
            MSQuery *query = [self.syncTable queryWithPredicate:predicate];
            
            // Pulls data from the remote server into the local table. We're only
            // pulling the items which we want to display (complete == NO).
            [self.syncTable pullWithQuery:query completion:^(NSError *error) {
                [self logErrorIfNotNil:error];
                [self refreshDataOnSuccess:completion];
            }];
        }

6.  Update the implementation of `refreshDataOnSuccess` to loads the data from the local table into the `items` property of the service:

        - (void) refreshDataOnSuccess:(QSCompletionBlock)completion
        {
            NSPredicate * predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
            MSQuery *query = [self.syncTable queryWithPredicate:predicate];
            
            [query orderByAscending:@"text"];
            [query readWithCompletion:^(NSArray *results, NSInteger totalCount, NSError *error) {
                [self logErrorIfNotNil:error];
                
                self.items = [results mutableCopy];
                
                // Let the caller know that we finished
                dispatch_async(dispatch_get_main_queue(), ^{
                    completion();
                });
            }];
        }

7.  Replace the implementation of `addItem` with the code below, which adds new Todo items into the sync table. The operation will then be *queued* and will not be sent to the remote service until the sync table explicitly pushes changes.

        -(void)addItem:(NSDictionary *)item completion:(QSCompletionWithIndexBlock)completion
        {
            // Insert the item into the TodoItem table and add to the items array on completion
            [self.syncTable insert:item completion:^(NSDictionary *result, NSError *error)
             {
                 [self logErrorIfNotNil:error];
                 
                 NSUInteger index = [items count];
                 [(NSMutableArray *)items insertObject:result atIndex:index];
                 
                 // Let the caller know that we finished
                 dispatch_async(dispatch_get_main_queue(), ^{
                     completion(index);
                 });
             }];
        }

8.  Replace the implementation of `completeItem` with the code below.

         -(void)completeItem:(NSDictionary *)item completion:(QSCompletionWithIndexBlock)completion
        {
            // Cast the public items property to the mutable type (it was created as mutable)
            NSMutableArray *mutableItems = (NSMutableArray *) items;
            
            // Set the item to be complete (we need a mutable copy)
            NSMutableDictionary *mutable = [item mutableCopy];
            [mutable setObject:@YES forKey:@"complete"];
            
            // Replace the original in the items array
            NSUInteger index = [items indexOfObjectIdenticalTo:item];
            [mutableItems replaceObjectAtIndex:index withObject:mutable];
            
            // Update the item in the TodoItem table and remove from the items array on completion
            [self.syncTable update:mutable completion:^(NSError *error) {
                
                [self logErrorIfNotNil:error];
                
                NSUInteger index = [items indexOfObjectIdenticalTo:mutable];
                if (index != NSNotFound)
                {
                    [mutableItems removeObjectAtIndex:index];
                }
                
                // Let the caller know that we have finished
                dispatch_async(dispatch_get_main_queue(), ^{
                    completion(index);
                });
            }];
        }

    Note that there is a minor API difference between `MSSyncTable` and `MSTable`: for `MSTable` update operations, the completion block returns the updated item, since a script (or controller action) in the server can modify the item being updated and the client should receive the modification. For local tables, the updated items are not modified, so the completion block doesn't have a parameter for that updated item.

9. In QSTodoListViewController.m, change the implementation of `viewDidLoad`. Replace the call to `[self refresh]` at the end of the method with the following code:

        // load the local data, but don't pull from server
        [self.todoService refreshDataOnSuccess:^
         {
             [self.refreshControl endRefreshing];
             [self.tableView reloadData];
         }];

10. In QSTodoListViewController, change the implementation of `refresh` to call `syncData` instead of `refreshDataOnSuccess`:

        - (void) refresh
        {
            [self.refreshControl beginRefreshing];
            [self.todoService syncData:^
             {
                  [self.refreshControl endRefreshing];
                  [self.tableView reloadData];
             }];
        }


### Threading considerations

Note that after the code was changed to use the sync table, we wrapped calls to completion blocks in a dispatch to the main thread:

        dispatch_async(dispatch_get_main_queue(), ^{
            completion();
        });

During the initialization of the sync context, the code did not pass a callback parameter, so the framework creates a default serial queue that dispatches the results of all sync table operations into into a background thread. Though it is a performance improvement to execute data operations in the background, this app modifies UI components and therefore needs to dispatch code back to the UI thread.

## Test the app

1.  Run the app. Notice that the list of items in the app is empty. As a result of the code changes in the previous section, the app no longer reads items from the mobile service, but rather from the local store.

2.  Add items to the To Do list.

3.  Log into the Microsoft Azure Management portal and look at the database for your mobile service. If your service uses the JavaScript backend for mobile services, you can browse the data from the **Data** tab of the mobile service. If you are using the .NET backend for your mobile service, you can click on the **Manage** button for your database in the SQL Azure Extension to execute a query against your table.

Notice the data has not been synchronized between the database and the
local store.

4.  In the app, perform the refresh gesture by dragging from the top. This causes the app to call `syncData` and `refreshDataOnSuccess` and will update the list with items from the local store. The new items have now been saved to your Mobile Service.

## Summary

In order to support the offline features of mobile services, we used the class `MSSyncTable` and initialized the sync context with a local store. In this case the local store was based on Core Data.

The normal CRUD operations for mobile services work as if the app is still connected but, all the operations occur against the local store.

-   To bring data from the service into the local tables, we need to perform a *pull* operation. This is executed in the sync table, which allows pulling data on a per-table basis.
-   Notice that we can (and often should) pull only a subset of the table so that we don't overload the client with information that it may not need.
-   When the client performs some changes (inserts, deletes, updates) in the items locally, those changes are stored in the sync context to be sent to the server. A *push* operation sends the tracked changes to the remote server. You would call push in the sync context via the `pushWithCompletion:` method.
-   In this tutorial, you may have noticed that there are no calls to the push method, but yet new data was still sent to the server after a refresh gesture. What is happening is that *before a pull is executed, any pending operations are pushed to the server*. That is done to prevent conflicts: if an item is modified locally and in the service, we want to make sure that the service has the ability to reject the changes (thus the push) by returning a conflict response to the push request.


[1]: images/006-RemovePreviousVersionOfFramework.png
[beta Mobile Services iOS SDK]: http://aka.ms/Gc6fex
[2]: images/007-AddCoreDataFramework.png
[3]: images/008-AddCoreDataModel.png
[4]: images/009-AddEntity.png

[Handling conflicts with offline data sync]: tbd
