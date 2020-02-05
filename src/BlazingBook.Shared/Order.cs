using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazingBook {
    public class Order {
        public int OrderId { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedTime { get; set; }

        public Address DeliveryAddress { get; set; } = new Address();

        public List<Book> Books { get; set; } = new List<Book>();

        public decimal GetTotalPrice() => Books.Sum(p => p.GetTotalPrice());

        public string GetFormattedTotalPrice() => GetTotalPrice().ToString("0.00");
    }
}