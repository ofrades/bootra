using System.Collections.Generic;

namespace BlazingBook {
	public class Basket {
		public int Id { get; set; }
		public string UserId { get; set; }
		public BookCustom Books { get; set; } = new BookCustom();
		public decimal GetTotalPrice() => Books.GetTotalPrice();
	}
}