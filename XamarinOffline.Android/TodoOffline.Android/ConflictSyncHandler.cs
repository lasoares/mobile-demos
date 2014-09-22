using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace TodoOffline
{
    class ConflictSyncHandler: MobileServiceSyncHandler
    {
        public override Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            operation.Item[MobileServiceSystemColumns.Version] = "abc";
            return base.ExecuteTableOperationAsync(operation);
        }
    }
}