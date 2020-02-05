using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingBook.Server {
    [Route("extras")]
    [ApiController]
    public class ExtrasController : Controller {
        private readonly BookStoreContext _db;

        public ExtrasController(BookStoreContext db) {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<Extra>>> GetExtras() {
            return await _db.Extras.OrderBy(t => t.Name).ToListAsync();
        }
    }
}