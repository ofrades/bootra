using Microsoft.EntityFrameworkCore;

namespace BlazingBook.Server {
    public class BookStoreContext : DbContext {
        public BookStoreContext() { }
        public BookStoreContext(DbContextOptions options) : base(options) { }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Wish> Wishes { get; set; }
        public DbSet<Basket> BasketItems { get; set; }
        public DbSet<BookCustom> BookCustoms { get; set; }
        public DbSet<BookBase> BookBases { get; set; }
        public DbSet<Extra> Extras { get; set; }
        public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Configuring a many-to-many bookbase -> extra relationship that is friendly for serialisation
            modelBuilder.Entity<BookExtra>().HasKey(pst => new { pst.BookId });
            modelBuilder.Entity<BookExtra>().HasOne<BookCustom>().WithMany(ps => ps.Extras);
            modelBuilder.Entity<BookExtra>().HasOne(pst => pst.Extra).WithMany();
        }
    }
}