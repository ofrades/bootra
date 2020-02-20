using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BlazingBook.Server {
    [Route("books")]
    [ApiController]
    public class BooksController : Controller {
        private readonly BookStoreContext _db;

        public BooksController(BookStoreContext db) {
            _db = db;
        }
    }
}