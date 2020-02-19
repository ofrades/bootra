namespace BlazingBook
{
	public class Wish {
        public int Id { get; set; }
        public string UserId { get; set; }
        public BookBase BookBase { get; set; } = new BookBase();
    }
}