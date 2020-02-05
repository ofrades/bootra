using Microsoft.EntityFrameworkCore;

namespace BlazingBook.Server {
    public class BookStoreContext : DbContext {
        public BookStoreContext() { }

        public BookStoreContext(DbContextOptions options) : base(options) { }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<BookSpecial> Specials { get; set; }

        public DbSet<Extra> Extras { get; set; }

        public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Configuring a many-to-many special -> extra relationship that is friendly for serialisation
            modelBuilder.Entity<BookExtra>().HasKey(pst => new { pst.BookId, pst.ExtraId });
            modelBuilder.Entity<BookExtra>().HasOne<Book>().WithMany(ps => ps.Extras);
            modelBuilder.Entity<BookExtra>().HasOne(pst => pst.Extra).WithMany();
        }
    }
}