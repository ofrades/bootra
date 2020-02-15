using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazingBook {
    public class Order {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedTime { get; set; }
        public Address DeliveryAddress { get; set; } = new Address();
        public List<BookCustom> Books { get; set; } = new List<BookCustom>();
        public decimal GetTotalPrice() => Books.Sum(p => p.GetTotalPrice());
        public string GetFormattedTotalPrice() => GetTotalPrice().ToString("0.00");
        public string TotalPrice { get; set; }
    }
}