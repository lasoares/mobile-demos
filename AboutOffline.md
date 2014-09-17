Offline Sync
------------

- Make apps resilient against intermittent network connectivity 
- Allow end-users to create and modify data even when there is no network access
- Sync data across multiple devices 
- Improve app responsiveness by caching server data locally on the device
- Detect and handle conflicts when the same record is modified by more than one client

Features
--------
- Lightweight
- Cross-platform 
  - Windows universal, iOS native, Android, Xamarin
- Support both "occasionally-connected" and "primarily online" scenarios
- Support both Node.js and .NET backends

How it works
------------
- Access data from Mobile Services tables even when app is offline
- Keep a local queue of Create, Update, Delete operations and synchronize with server when app is back online
- Detect conflicts when same item is changed both locally and on server
- Use soft delete to remove deleted records from client data stores

