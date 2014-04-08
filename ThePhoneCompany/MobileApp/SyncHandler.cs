using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ThePhoneCompany
{
    public class SyncHandler : IMobileServiceSyncHandler
    {
        MobileServiceClient client = App.MobileService;
        const string LOCAL_VERSION = "Use local version";
        const string SERVER_VERSION = "Use server version";

        public virtual Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            return Task.FromResult(0);
        }

        public virtual async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            MobileServicePreconditionFailedException error;

            do
            {
                error = null;

                try
                {
                    return await operation.ExecuteAsync();
                }
                catch (MobileServicePreconditionFailedException ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    var localItem = operation.Item.ToObject<Job>();
                    var serverValue = error.Value;

                    IUICommand command = await ShowConflictDialog(localItem, serverValue.ToObject<Job>());

                    if (command.Label == LOCAL_VERSION)
                    {
                        // Overwrite the server version and try the operation again by continuing the loop
                        operation.Item[MobileServiceSystemColumns.Version] = serverValue[MobileServiceSystemColumns.Version];
                        continue;
                    }
                    else if (command.Label == SERVER_VERSION)
                    {
                        return (JObject)serverValue;
                    }
                    else
                    {
                        operation.AbortPush();
                    }
                }
            } while (error != null);

            return null;
        }

        private static async Task<IUICommand> ShowConflictDialog(Job localItem, Job serverItem)
        {
            var dialog = new MessageDialog(
                "How do you want to resolve this conflict?\n\n" + "Local item: \n" + localItem +
                "\n\nServer item:\n" + serverItem,
                title: "Conflict between local and server versions");
            dialog.Commands.Add(new UICommand(LOCAL_VERSION));
            dialog.Commands.Add(new UICommand(SERVER_VERSION));
            dialog.Commands.Add(new UICommand("Cancel"));

            IUICommand command = await dialog.ShowAsync();
            return command;
        }
    }
}
