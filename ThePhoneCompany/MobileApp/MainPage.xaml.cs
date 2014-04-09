#define OFFLINE

using Microsoft.WindowsAzure.MobileServices;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace ThePhoneCompany
{
    #region Data objects 
    public class Job
    {
        [CreatedAt]
        public DateTimeOffset? CreatedAt { get; set; }
        public string Id { get; set; }

        [UpdatedAt]
        public DateTimeOffset? UpdatedAt { get; set; }
        [Version]
        public string Version { get; set; }

        public bool Completed { get; set; }

        public string Description { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }

        public override string ToString()
        {
            return "    Description: " + Description + "\n    Complete: " + Completed + "\n    Customer: " + CustomerName;
        }
    }

    public class Customer
    {
        [CreatedAt]
        public DateTimeOffset? CreatedAt { get; set; }
        public string Id { get; set; }
        [UpdatedAt]
        public DateTimeOffset? UpdatedAt { get; set; }
        [Version]
        public string Version { get; set; }

        public string CustomerName { get; set; }
    }
    #endregion

    public sealed partial class MainPage : Page
    {
        private MobileServiceCollection<Job, Job> jobs;
        private MobileServiceCollection<Customer, Customer> customers;

#if OFFLINE
        private IMobileServiceSyncTable<Job> jobsTable            = App.MobileService.GetSyncTable<Job>();
        private IMobileServiceSyncTable<Customer> customersTable  = App.MobileService.GetSyncTable<Customer>();
#else
        private IMobileServiceTable<Job> jobsTable            = App.MobileService.GetTable<Job>();
        private IMobileServiceTable<Customer> customersTable  = App.MobileService.GetTable<Customer>();
#endif

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task Initialize()
        {
#if OFFLINE
            if (!App.MobileService.SyncContext.IsInitialized) {
                var store = new MobileServiceSQLiteStore(App.LOCAL_DB_NAME);
                store.DefineTable<Job>();
                store.DefineTable<Customer>();

                await App.MobileService.SyncContext.InitializeAsync(store, new SyncHandler());
            }
#else
            ButtonSync.IsEnabled = false;
#endif

            await RefreshJobList();
        }


        private async void ButtonSync_Click(object sender, RoutedEventArgs e)
        {
#if OFFLINE
            string errorString = null;
            ButtonSync.IsEnabled = false;

            try {
                await App.MobileService.SyncContext.PushAsync();
                await this.jobsTable.PullAsync();
                await this.customersTable.PullAsync();
            }
            catch (MobileServicePushFailedException ex) {
                if (ex.PushResult.Status != MobileServicePushStatus.CancelledByOperation) // don't show a message if the user cancelled push
                    errorString = "Push failed: " + ex.PushResult.Status;
            }
            catch (Exception ex) {
                errorString = "Push failed: " + ex.Message;
            }

            if (errorString != null) {
                await new MessageDialog(errorString).ShowAsync();
            }

            await RefreshJobList();
            ButtonSync.IsEnabled = true;
#endif
        }


        private async Task RefreshJobList()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                jobs = await jobsTable
                    .OrderBy(x => x.CustomerName)
                    .ToCollectionAsync();

                customers = 
                    await customersTable
                    .OrderBy(x => x.CustomerName)
                    .ToCollectionAsync();

                ListItems.ItemsSource = jobs;
                TextCustomer.ItemsSource = customers;
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null) {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
        }

        private async Task InsertJob(Job job)
        {
            await jobsTable.InsertAsync(job);
            jobs.Insert(0, job);
        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = (CheckBox)sender;
            var job = checkbox.DataContext as Job;

            jobsTable.UpdateAsync(job);
        }

        private async Task CommitChanges(TextBox sender)
        {
            var job = sender.DataContext as Job;

            if (job.Description != sender.Text)
            {
                job.Description = sender.Text;
                await jobsTable.UpdateAsync(job);
            }
        }

        #region Event handlers
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
                await Initialize();
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var customer = TextCustomer.SelectedItem as Customer;

            if (customer != null)
            {
                var todoItem = new Job
                {
                    Description = TextJob.Text,
                    CustomerId = customer.Id,
                    CustomerName = customer.CustomerName
                };

                await InsertJob(todoItem);

                // clear the UI fields
                TextCustomer.SelectedIndex = -1;
                TextJob.Text = "";
                TextJob.Focus(FocusState.Programmatic);
            }
        }

        private void TextJob_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) {
                ButtonSave.Focus(FocusState.Programmatic);
            }
        }

        private void CheckEnableSave()
        {
            ButtonSave.IsEnabled = (TextJob.Text.Length > 0) && (TextCustomer.SelectedItem != null);
        }

        private void TextCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckEnableSave();
        }

        private void TextJob_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckEnableSave();
        }

        private async void JobDetail_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) {
                await CommitChanges((TextBox) sender);
                ListItems.Focus(FocusState.Programmatic);
            }
        }

        private async void TextJobDetail_LostFocus(object sender, RoutedEventArgs e)
        {
            await CommitChanges((TextBox) sender);
        }

        #endregion 
    }
}
