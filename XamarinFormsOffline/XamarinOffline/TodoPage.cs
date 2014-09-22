using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace XamarinOffline
{
	public class TodoPage : ContentPage
	{
		async Task Refresh()
		{
			itemsView.ItemsSource = await syncTable
				.OrderBy (x => x.Text)
				.Select (x => x.Text)
				.ToListAsync (); 
		}

		protected async override void OnAppearing ()
		{
			var store = new MobileServiceSQLiteStore ("localstore.db");
			store.DefineTable<TodoItem> ();

			await App.MobileService.SyncContext.InitializeAsync (store);
			await Refresh ();
			base.OnAppearing ();
		}

		private ListView itemsView;
		private IMobileServiceSyncTable<TodoItem> syncTable;

		public TodoPage()
		{
			syncTable = App.MobileService.GetSyncTable<TodoItem>();

			itemsView = new ListView();

			var textBox = new Entry { HorizontalOptions = LayoutOptions.FillAndExpand };

			var addButton = new Button { Text = "Add" };
			var syncButton = new Button { Text = "Sync" };

			this.Content = new StackLayout {
				Padding = 20,
				Orientation = StackOrientation.Vertical,
				Children = {
					new StackLayout {
						Orientation = StackOrientation.Horizontal,
						Children = {
							textBox, addButton, syncButton
						}
					},
					itemsView
				}
			};

			// event handlers
			addButton.Clicked += async (sender, e) => {
				await syncTable.InsertAsync(new TodoItem { Text = textBox.Text, Complete = false });
				await Refresh();
				textBox.Text = "";
			};

			syncButton.Clicked += async (sender, e) => {
				syncButton.IsEnabled = false;

				await App.MobileService.SyncContext.PushAsync();
				await syncTable.PullAsync("todoItems", syncTable.CreateQuery());
				await Refresh();

				syncButton.IsEnabled = true;
			};
		}
	}
}

