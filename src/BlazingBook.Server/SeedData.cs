namespace BlazingBook.Server {
    public static class SeedData {
        public static void Initialize(BookStoreContext db) {
            var extras = new Extra[] {
                new Extra() {
                Name = "Calfskin",
                Price = 10.00m,
                },
                new Extra() {
                Name = "Custom Endsheets",
                Price = 4.00m,
                },
                new Extra() {
                Name = "Soft Leather Cover",
                Price = 8.50m,
                }
            };

            var bookbases = new BookBase[] {
                new BookBase() {
                Title = "Amor de Perdição",
                Author = "Camilo Castelo Branco",
                BasePrice = 11.99m,
                ImageUrl = "img/books/amorperdicao.png",
                },
                new BookBase() {
                Id = 2,
                Title = "Os Lusíadas",
                Author = "Luís de Camões",
                BasePrice = 10.50m,
                ImageUrl = "img/books/lusiadas.png",
                },
                new BookBase() {
                Id = 3,
                Title = "O Primo Basílio",
                Author = "Eça de Queiroz",
                BasePrice = 12.75m,
                ImageUrl = "img/books/primobasilio.png",
                },
                new BookBase() {
                Id = 4,
                Title = "Sermões",
                Author = "Padre António Vieira",
                BasePrice = 11.00m,
                ImageUrl = "img/books/sermoes.png",
                }
            };

            db.Extras.AddRange(extras);
            db.BookBases.AddRange(bookbases);
            db.SaveChanges();
        }
    }
}