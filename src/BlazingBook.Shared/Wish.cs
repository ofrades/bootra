using System.Collections.Generic;

namespace BlazingBook {
    public class Wish {
        public int WishId { get; set; }

        public string UserId { get; set; }

        public List<Book> Books { get; set; } = new List<Book>();
    }
}