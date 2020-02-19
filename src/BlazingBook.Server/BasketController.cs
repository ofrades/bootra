using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingBook.Server {
    [Route("basket")]
    [ApiController]
    public class BasketController : Controller {
        private readonly BookStoreContext _db;

        public BasketController(BookStoreContext db) {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<Basket>>> GetBasket() {
            var query = await _db.BasketItems
                .Where(o => o.UserId == GetUserId())
                .Include(p => p.Books.BookBase)
                .Include(p => p.Books.Extras).ThenInclude(t => t.Extra)
                .OrderByDescending(s => s.Id).ToListAsync();
            return query;
        }

        [HttpPost]
        public async Task<ActionResult<Basket>> AddItemToBasket(Basket book) {
            var existingBookBase = _db.BookBases.Find(book.Books.BookBase.Id);
            var basketItem = new Basket { };
            if (existingBookBase == null) {
                await _db.BookBases.AddAsync(new BookBase {
                    Id = book.Books.Id,
                        Author = book.Books.BookBase.Author,
                        Title = book.Books.BookBase.Title,
                        BasePrice = book.Books.BookBase.BasePrice,
                });
                await _db.SaveChangesAsync();
            }
            basketItem = new Basket {
                UserId = GetUserId(),
                Books = new BookCustom {
                BookBase = new BookBase { 
                    Id = book.Books.BookBase.Id,
                    Title = book.Books.BookBase.Title },
                Extras = new List<BookExtra>(),
                }
            };
            // basketItem.Books.BookBase = null;
            foreach (var extra in basketItem.Books.Extras) {
                extra.Id = extra.Extra.Id;
                extra.BookId = basketItem.Books.Id;
            }

            _db.BasketItems.Attach(basketItem);
            await _db.SaveChangesAsync();
            return Ok(basketItem);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task DeleteBasketItem([FromRoute] int id) {
            var res = _db.BasketItems.Find(id);
            if (res == null) {
                BadRequest("Invalid book id");
            }
            _db.BasketItems.Remove(res);
            await _db.SaveChangesAsync();
        }

        private string GetUserId() {
            // This will be the user's twitter username
            return HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        }
    }
}