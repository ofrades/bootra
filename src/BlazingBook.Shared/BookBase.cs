using System.Collections.Generic;

namespace BlazingBook {
    /// <summary>
    /// Represents a pre-configured template for a book a user can order
    /// </summary>
    public class BookBase {
        public int Id { get; set; }

        public string Title { get; set; }

        public decimal BasePrice { get; set; }

        public string Author { get; set; }

        public string ImageUrl { get; set; }

        public string GetFormattedBasePrice() => BasePrice.ToString("0.00");
    }
}