using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ThePhoneCompanyService.DataObjects
{
    public class Job : EntityData
    {
        public string Description { get; set; }

        public bool Completed { get; set; }

        public string CustomerId { get; set; }

        public string CustomerName { get; set; }
    }
}