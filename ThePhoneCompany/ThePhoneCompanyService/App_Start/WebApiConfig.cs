using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using ThePhoneCompanyService.DataObjects;
using ThePhoneCompanyService.Models;
using System;
using System.Diagnostics;

namespace ThePhoneCompanyService
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            // config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            Database.SetInitializer(new donnam_net1Initializer());
        }
    }

    public class donnam_net1Initializer : DropCreateDatabaseIfModelChanges<ThePhoneCompanyBackendContext>
    {
        protected override void Seed(ThePhoneCompanyBackendContext context)
        {
            Debug.WriteLine("Seeding the database");

            var orders = new List<Job>
            {
                new Job { Id = Guid.NewGuid().ToString(), Description = "Cable box", CustomerId = "1", CustomerName = "Donna" },
                new Job { Id = Guid.NewGuid().ToString(), Description = "Digital converter", CustomerId = "2", CustomerName="Yavor" },
                new Job { Id = Guid.NewGuid().ToString(), Description = "Cable", CustomerId = "3", CustomerName="Hasan" },
            };

            var customers = new List<Customer>
            {
                new Customer { Id = "1", CustomerName = "Donna" },
                new Customer { Id = "2", CustomerName = "Yavor" },
                new Customer { Id = "3", CustomerName = "Hasan" },
            };

            customers.ForEach(customer => context.Customers.Add(customer));
            orders.ForEach(customer => context.Jobs.Add(customer));

            base.Seed(context);
        }
    }
}

