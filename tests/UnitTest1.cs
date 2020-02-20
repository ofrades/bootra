using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using BlazingBook;
using BlazingBook.Server;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace tests {
    public class UnitTest2 {

        [Fact]
        public async Task GetWishes_() {
            var mock = new Mock<ILogger<WishesController>>();
            var _logger = mock.Object;
            var _db = CreateContext();
            var controller = new WishesController(_db, _logger);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = Mock.Of<ClaimsPrincipal>();
            var res = await controller.GetWishes();
            var bookList = res.Value;
            Assert.Equal(bookList.Count, await _db.Wishes.CountAsync());
        }

        [Fact]
        public async Task AddWish_() {
            var mock = new Mock<ILogger<WishesController>>();
            var _logger = mock.Object;
            var _db = CreateContext();
            var controller = new WishesController(_db, _logger);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = Mock.Of<ClaimsPrincipal>();
            var newBook = new BookBase { Author = "asd", Title = "asd", BasePrice = 10.00m, Id = 10 };
            var newWish = new Wish { Id = 10, UserId = "miguel", BookBase = newBook };
            var res = await controller.AddWishes(newWish);
            var bookList = res.Value;
            Assert.Equal(newWish, await _db.Wishes.FindAsync(newWish.Id));
        }

        [Fact]
        public async Task DeleteWish_() {
            var mock = new Mock<ILogger<WishesController>>();
            var _logger = mock.Object;
            var _db = CreateContext();
            var controller = new WishesController(_db, _logger);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = Mock.Of<ClaimsPrincipal>();
            var newBook = new BookBase { Author = "asd", Title = "asd", BasePrice = 10.00m, Id = 10 };
            var wish = new Wish { Id = 10, UserId = "miguel", BookBase = newBook };
            var res = await controller.DeleteWishes(wish.Id);
            Assert.Equal(1, await _db.Wishes.CountAsync());
        }

        private static BookStoreContext CreateContext() {
            var options = new DbContextOptionsBuilder<BookStoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new BookStoreContext(options);
            context.Wishes.Add(new Wish {
                Id = 1,
                    BookBase = new BookBase {
                        Id = 1,
                            BasePrice = 12.00m,
                            Title = "Lusíadas",
                            Author = "Luís",  
                    },
                    UserId = "Manuel"
            });
            context.Wishes.Add(new Wish {
                Id = 3,
                    BookBase = new BookBase {
                        Id = 3,
                            BasePrice = 11.00m,
                            Title = "Serras",
                            Author = "Eça",  
                    },
                    UserId = "João"
            });
            context.Wishes.Add(new Wish {
                Id = 2,
                    BookBase = new BookBase {
                        Id = 2,
                            BasePrice = 13.00m,
                            Title = "Paixão",
                            Author = "Camilo",  
                    },
                    UserId = "Manuel"
            });
            return context;
        }
    }
}