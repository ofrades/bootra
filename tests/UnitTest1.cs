using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazingBook;
using BlazingBook.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace tests {
    public class UnitTest2 {

        [Fact]
        public async Task Test2() {
            var context = CreateContext();
            var controller = new WishesController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.User = Mock.Of<ClaimsPrincipal>();
            var res = await controller.GetWishes();
            var bookList = res.Value;
            Assert.Equal(bookList.Count, await context.Wishes.CountAsync());
        }

        private static BookStoreContext CreateContext() {
            var options = new DbContextOptionsBuilder<BookStoreContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new BookStoreContext(options);
            context.Wishes.Add(new Wish { Id = 1, BookId = 1  });
            context.Wishes.Add(new Wish {  Id = 2, BookId = 2 });
            context.Wishes.Add(new Wish {  Id = 3, BookId = 3 });
            context.Wishes.Add(new Wish {  Id = 4, BookId = 4 });
            return context;
        }

        // [Fact]
        // public async Task Index_ReturnsAViewResult_WithAListOfBrainstormSessions() {
        //     // Arrange
        //     var context = CreateContext();
        //     var mockRepo = new Mock<BookStoreContext>();
        //     mockRepo.Setup(repo => repo.ListAsync())
        //         .ReturnsAsync(CreateContext());
        //     var controller = new WishesController(mockRepo.Object);

        //     // Act
        //     var result = await controller.GetWishes();
        //     var bookList = result.Value;
        //     Assert.Equal(bookList.Count, await context.Wishes.CountAsync());
        // }
    }
}