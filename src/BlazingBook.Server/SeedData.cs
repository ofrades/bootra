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

            var specials = new BookSpecial[] {
                new BookSpecial() {
                Name = "Amor de Perdição",
                Description = "Camilo Castelo Branco",
                BasePrice = 11.99m,
                ImageUrl = "img/books/amorperdicao.png",
                },
                new BookSpecial() {
                Id = 2,
                Name = "Os Lusíadas",
                Description = "Luís de Camões",
                BasePrice = 10.50m,
                ImageUrl = "img/books/lusiadas.png",
                },
                new BookSpecial() {
                Id = 3,
                Name = "O Primo Basílio",
                Description = "Eça de Queiroz",
                BasePrice = 12.75m,
                ImageUrl = "img/books/primobasilio.png",
                },
                new BookSpecial() {
                Id = 4,
                Name = "Sermões",
                Description = "Padre António Vieira",
                BasePrice = 11.00m,
                ImageUrl = "img/books/sermoes.png",
                }
            };

            db.Extras.AddRange(extras);
            db.Specials.AddRange(specials);
            db.SaveChanges();
        }
    }
}