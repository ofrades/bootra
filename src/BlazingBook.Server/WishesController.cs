using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingBook.Server {
    [Route("wishes")]
    [ApiController]
    public class WishesController : Controller {
        private readonly BookStoreContext _db;

        public WishesController(BookStoreContext db) {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<Wish>>> GetWishes() {
            var query = await _db.Wishes
                .Where(o => o.UserId == GetUserId())
                .Include(o => o.BookBase)
                .OrderByDescending(s => s.BookId).ToListAsync();

            return query;
        }

        [HttpPost]
        public async Task<ActionResult<Wish>> AddWishes(Wish wish) {
            var book = await _db.BookBases.FindAsync(wish.BookId);
            if (book != null) {
                wish = new Wish {
                    UserId = GetUserId(),
                    BookBase = book,
                    Id = wish.Id
                };
                _db.Wishes.Attach(wish).State = EntityState.Added;
            } else {
                wish = new Wish {
                    UserId = GetUserId(),
                    BookBase = wish.BookBase,
                    Id = wish.Id
                };
                _db.Wishes.Attach(wish).State = EntityState.Detached;
            }
            await _db.SaveChangesAsync();
            return Ok(wish);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task DeleteWishes([FromRoute] int id) {
            var res = await _db.Wishes.FindAsync(id);
            if (res == null) {
                BadRequest("Invalid book id");
            }
            _db.Wishes.Remove(res);
            await _db.SaveChangesAsync();
        }

        private string GetUserId() {
            // This will be the user's twitter username
            return HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        }
    }
}