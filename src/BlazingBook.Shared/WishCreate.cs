namespace BlazingBook {
    public class WishCreate {
        public int BookId { get; set; }
        public string UserId { get; set; }
        public BookBase BookBase { get; set; } = new BookBase();
    }
}