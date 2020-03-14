using System;
using System.Collections.Generic;

namespace CMS.Models.ViewModels.Account
{
    public class OrdersForUserVM
    {
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}