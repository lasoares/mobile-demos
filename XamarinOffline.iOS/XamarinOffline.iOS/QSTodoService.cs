using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using MonoTouch.UIKit;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace XamarinOffline
{
	public class QSTodoService : DelegatingHandler
	{
		static QSTodoService instance = new QSTodoService ();
		const string applicationURL = @"https://donnam-zumotest.azure-mobile.net/";
		const string applicationKey = @"RghPgwZBTNAXJoPXcwatxljJMQsfPx10";
		MobileServiceClient client;
		IMobileServiceSyncTable<TodoItem> todoTable;
		int busyCount = 0;

		public event Action<bool> BusyUpdate;

		internal QSTodoService ()
		{
			CurrentPlatform.Init ();
			SQLitePCL.CurrentPlatform.Init();

			// Initialize the Mobile Service client with your URL and key
			client = new MobileServiceClient (applicationURL, applicationKey, this);

			// Create an MSTable instance to allow us to work with the TodoItem table
			todoTable = client.GetSyncTable <TodoItem> ();
		}

		public async Task InitializeAsync()
		{
			string path = "localstore.db";
			var store = new MobileServiceSQLiteStore(path);
			store.DefineTable<TodoItem>();
			await client.SyncContext.InitializeAsync(store, new SyncHandler(client));
		}

		public static QSTodoService DefaultService {
			get {
				return instance;
			}
		}

		public List<TodoItem> Items { get; private set;}

		async public Task<List<TodoItem>> RefreshDataAsync ()
		{
			try {
				// This code refreshes the entries in the list view by querying the TodoItems table.
				// The query excludes completed TodoItems
				Items = await todoTable
					.Where (todoItem => todoItem.Complete == false).ToListAsync ();

			} catch (MobileServiceInvalidOperationException e) {
				Console.Error.WriteLine (@"ERROR {0}", e.Message);
				return null;
			}

			return Items;
		}

		public async Task InsertTodoItemAsync (TodoItem todoItem)
		{
			try {
				// This code inserts a new TodoItem into the database. When the operation completes
				// and Mobile Services has assigned an Id, the item is added to the CollectionView
				await todoTable.InsertAsync (todoItem);
				Items.Add (todoItem); 

			} catch (MobileServiceInvalidOperationException e) {
				Console.Error.WriteLine (@"ERROR {0}", e.Message);
			}
		}

		public async Task CompleteItemAsync (TodoItem item)
		{
			try {
				// This code takes a freshly completed TodoItem and updates the database. When the MobileService 
				// responds, the item is removed from the list 
				item.Complete = true;
				await todoTable.UpdateAsync (item);
				Items.Remove (item);

			} catch (MobileServiceInvalidOperationException e) {
				Console.Error.WriteLine (@"ERROR {0}", e.Message);
			}
		}

		internal void Busy (bool busy)
		{
			// assumes always executes on UI thread
			if (busy) {
				if (busyCount++ == 0 && BusyUpdate != null)
					BusyUpdate (true);

			} else {
				if (--busyCount == 0 && BusyUpdate != null)
					BusyUpdate (false);

			}
		}

		public async Task SyncAsync()
		{
			try
			{
				await this.client.SyncContext.PushAsync();
				await this.todoTable.PullAsync();
			}
			catch (MobileServiceInvalidOperationException e)
			{
				Console.Error.WriteLine(@"Sync Failed: {0}", e.Message);
			}
		}

		public async Task PushAsync()
		{
			try
			{
				await this.client.SyncContext.PushAsync();
			}
			catch (MobileServiceInvalidOperationException e)
			{
				Console.Error.WriteLine(@"Push Failed: {0}", e.Message);
			}
		}

		public async Task PullAsync()
		{
			try
			{
				await this.todoTable.PullAsync();
			}
			catch (MobileServiceInvalidOperationException e)
			{
				Console.Error.WriteLine(@"Pull Failed: {0}", e.Message);
			}
		}        

		#region implemented abstract members of HttpMessageHandler

		protected override async Task<System.Net.Http.HttpResponseMessage> SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			Busy (true);
			var response = await base.SendAsync (request, cancellationToken);

			Busy (false);
			return response;
		}

		#endregion
	}
}

