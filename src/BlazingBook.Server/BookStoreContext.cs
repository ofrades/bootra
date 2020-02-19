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
            
            // FIXME Correct approach
            modelBuilder.Entity<Order>().HasKey(pst => new { pst.OrderId });
            modelBuilder.Entity<Order>().HasOne(ps => ps.DeliveryAddress);
            modelBuilder.Entity<Order>().HasMany(ps => ps.Books);
            modelBuilder.Entity<Wish>().HasOne(pst => pst.BookBase);
            modelBuilder.Entity<BookCustom>().HasKey(pst => new { pst.Id });
            modelBuilder.Entity<BookCustom>().HasOne(ps => ps.BookBase);
            modelBuilder.Entity<BookCustom>().HasMany(ps => ps.Extras);
        }
    }
}