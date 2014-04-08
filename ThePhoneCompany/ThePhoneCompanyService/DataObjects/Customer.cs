using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ThePhoneCompanyService.DataObjects
{
    public class Customer : EntityData
    {
        public string CustomerName { get; set; }
    }
}