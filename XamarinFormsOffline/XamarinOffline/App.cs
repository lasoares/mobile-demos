using System;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Net.Http;


namespace XamarinOffline
{
	public class App
	{
		public static Page GetMainPage ()
		{             
			return new TodoPage ();
		}
			
		public static MobileServiceClient MobileService = new MobileServiceClient(
			"https://your-mobile-service.azure-mobile.net/",
			"APP KEY",
			new LoggingHandler()
		);
	}

	public class TodoItem
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public bool Complete { get; set; } 
	}
}

