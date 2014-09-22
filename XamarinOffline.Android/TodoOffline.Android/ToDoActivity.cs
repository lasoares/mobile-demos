using System;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.IO;
using Android.Content;


namespace TodoOffline
{
    [Activity (MainLauncher = true, 
               Icon="@drawable/ic_launcher", Label="@string/app_name",
               Theme="@style/AppTheme")]
    public class ToDoActivity : Activity
    {
        //Mobile Service Client reference
        private MobileServiceClient client;

        //Mobile Service Table used to access data
        private IMobileServiceSyncTable<ToDoItem> toDoTable;

        //Adapter to sync the items list with the view
        private ToDoItemAdapter adapter;

        //EditText containing the "New ToDo" text
        private EditText textNewToDo;

        //Progress spinner to use for table operations
        private ProgressBar progressBar;

        const string applicationURL = @"https://nodetestapp.azure-mobile.net/";
        const string applicationKey = @"uXNQnEHadBVMcbztfclTblLSjAUldy66";

        protected override async void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Activity_To_Do);

            progressBar = FindViewById<ProgressBar> (Resource.Id.loadingProgressBar);

            // Initialize the progress bar
            progressBar.Visibility = ViewStates.Gone;

            // Create ProgressFilter to handle busy state
            var progressHandler = new ProgressHandler ();
            progressHandler.BusyStateChange += (busy) => {
                if (progressBar != null) 
                    progressBar.Visibility = busy ? ViewStates.Visible : ViewStates.Gone;
            };

            try {
                CurrentPlatform.Init ();
                // Create the Mobile Service Client instance, using the provided
                // Mobile Service URL and key
                client = new MobileServiceClient (
                    applicationURL,
                    applicationKey, progressHandler);

                string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "test.db");
                
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
                var store = new MobileServiceSQLiteStore(path);
                store.DefineTable<ToDoItem>();
                await client.SyncContext.InitializeAsync(store, new ConflictSyncHandler());

                // Get the Mobile Service Table instance to use
                toDoTable = client.GetSyncTable <ToDoItem> ();

                textNewToDo = FindViewById<EditText> (Resource.Id.textNewToDo);

                // Create an adapter to bind the items with the view
                adapter = new ToDoItemAdapter (this, Resource.Layout.Row_List_To_Do);
                var listViewToDo = FindViewById<ListView> (Resource.Id.listViewToDo);
                listViewToDo.Adapter = adapter;

                // Load the items from the Mobile Service
                await RefreshItemsFromTableAsync ();

            } catch (Java.Net.MalformedURLException) {
                CreateAndShowDialog (new Exception ("There was an error creating the Mobile Service. Verify the URL"), "Error");
            } catch (Exception e) {
                CreateAndShowDialog (e, "Error");
            }
        }

        //Initializes the activity menu
        public override bool OnCreateOptionsMenu (IMenu menu)
        {
            MenuInflater.Inflate (Resource.Menu.activity_main, menu);
            return true;
        }

        //Select an option from the menu
        public override bool OnOptionsItemSelected (IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_refresh) {
                OnRefreshItemsSelected ();
            }
            else if (item.ItemId == Resource.Id.menu_pull)
            {
                OnPullItemSelected();
            }
            return true;
        }

        async void OnPullItemSelected()
        {
            MobileServicePushFailedException error = null;

            try
            {
                await this.toDoTable.PullAsync();
            }
            catch (MobileServicePushFailedException ex)
            {
                error = ex;    
            }

            if (error != null)
            {
                foreach (MobileServiceTableOperationError opError in error.PushResult.Errors)
                {
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage(opError.Item.ToString());
                    builder.SetTitle("Push failed");
                    builder.SetPositiveButton("Client wins", async (which, e) =>
                    {
                        opError.Item[MobileServiceSystemColumns.Version] = opError.Result[MobileServiceSystemColumns.Version];
                        await toDoTable.UpdateAsync(opError.Item);
                        OnRefreshItemsSelected();
                    });
                    builder.SetNegativeButton("Server wins", async (which, e) =>
                    {
                        await opError.CancelAndUpdateItemAsync(opError.Result);
                        OnRefreshItemsSelected();
                    });
                    builder.Create().Show();
                }
            }
             
             OnRefreshItemsSelected();
        }

        // Called when the refresh menu opion is selected
        async void OnRefreshItemsSelected ()
        {
            await RefreshItemsFromTableAsync ();
        }

        //Refresh the list with the items in the Mobile Service Table
        async Task RefreshItemsFromTableAsync ()
        {
            try {
                // Get the items that weren't marked as completed and add them in the
                // adapter
                var list = await toDoTable.OrderBy(t=>t.Text).ToListAsync ();

                adapter.Clear ();

                foreach (ToDoItem current in list)
                    adapter.Add (current);

            } catch (Exception e) {
                CreateAndShowDialog (e, "Error");
            }
        }

        public async Task CheckItem (ToDoItem item)
        {
            if (client == null) {
                return;
            }

            // Set the item as completed and update it in the table
            try {
                await toDoTable.UpdateAsync (item);

            } catch (Exception e) {
                CreateAndShowDialog (e, "Error");
            }
        }

        [Java.Interop.Export()]
        public async void AddItem (View view)
        {
            if (client == null || string.IsNullOrWhiteSpace (textNewToDo.Text)) {
                return;
            }

            // Create a new item
            var item = new ToDoItem {
                Text = textNewToDo.Text,
                Complete = false
            };

            try {
                // Insert the new item
                await toDoTable.InsertAsync (item);

                if (!item.Complete) {
                    adapter.Add (item);
                }
            } catch (Exception e) {
                CreateAndShowDialog (e, "Error");
            }

            textNewToDo.Text = "";
        }

        void CreateAndShowDialog (Exception exception, String title)
        {
            CreateAndShowDialog (exception.Message, title);
        }

        void CreateAndShowDialog (string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder (this);

            builder.SetMessage (message);
            builder.SetTitle (title);
            builder.Create ().Show ();
        }

        class ProgressHandler : DelegatingHandler
        {
            int busyCount = 0;

            public event Action<bool> BusyStateChange;

            #region implemented abstract members of HttpMessageHandler

            protected override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                 //assumes always executes on UI thread
                if (busyCount++ == 0 && BusyStateChange != null)
                    BusyStateChange (true);

                var response = await base.SendAsync (request, cancellationToken);

                // assumes always executes on UI thread
                if (--busyCount == 0 && BusyStateChange != null)
                    BusyStateChange (false);

                return response;
            }

            #endregion

        }
    }
}


