using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingBook.Server {
    [Route("bookbase")]
    [ApiController]
    public class BookBasesController : Controller {
        private readonly BookStoreContext _db;

        public BookBasesController(BookStoreContext db) {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookBase>>> GetBookBases() {
            return (await _db.BookBases.ToListAsync()).OrderByDescending(s => s.Id).ToList();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> RemoveBookBases([FromRoute] int id) {
            var res = await _db.BookBases.FindAsync(id);
            if (res == null) {
                BadRequest("Invalid book id");
            }
            _db.BookBases.Remove(res);
            await _db.SaveChangesAsync();
            return Ok(res);
        }

        [HttpPost]
        public async Task<ActionResult> AddBook(Result result) {
            var existingBookBase = await _db.BookBases.FindAsync(result.Id);
            if (existingBookBase == null) {
                await _db.BookBases.AddAsync(new BookBase {
                    Id = result.Id,
                        Author = result.Authors.Select(c => c.Name).FirstOrDefault(),
                        Title = result.Title,
                        BasePrice = result.BasePrice,
                });
            }

            await _db.SaveChangesAsync();
            return Ok("Saved to the database");
        }
    }
}