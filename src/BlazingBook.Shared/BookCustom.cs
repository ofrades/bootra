using System.Collections.Generic;
using System.Linq;

namespace BlazingBook {
    /// <summary>
    /// Represents a customized book as part of an order
    /// </summary>
    public class BookCustom {
        public const int DefaultSize = 5;
        public const int MinimumSize = 4;
        public const int MaximumSize = 6;

        public int Id { get; set; }

        public BookBase BookBase { get; set; }
        // public int BasketId { get; set; }

        // public int BookBaseId { get; set; }

        public int Size { get; set; }

        public List<BookExtra> Extras { get; set; }

        public decimal GetBasePrice() {
            return ((decimal) Size / (decimal) DefaultSize) * BookBase.BasePrice;
        }

        public decimal GetTotalPrice() {
            return GetBasePrice() + Extras.Sum(t => t.Extra.Price);
        }

        public string GetFormattedTotalPrice() {
            return GetTotalPrice().ToString("0.00");
        }
    }
}