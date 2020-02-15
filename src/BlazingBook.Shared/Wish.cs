using System.Collections.Generic;

namespace BlazingBook {
    public class Wish {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string UserId { get; set; }
        public BookBase Book { get; set; } = new BookBase();
    }

    public class WishCreate {
        public int BookId { get; set; }
        public string UserId { get; set; }
        public BookBase Book { get; set; } = new BookBase();
    }
}