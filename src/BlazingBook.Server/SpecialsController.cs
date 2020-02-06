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
            return (await _db.BookBases.ToListAsync()).OrderByDescending(s => s.BasePrice).ToList();
        }
    }
}