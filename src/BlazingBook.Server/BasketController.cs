using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace BlazingBook.Server {
    [Route("basket")]
    [ApiController]
    public class BasketController : Controller {
        
        private readonly BookStoreContext _db;
        private readonly ILogger _logger;

        public BasketController(BookStoreContext db, ILogger<BasketController> logger) {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Basket>>> GetBasket() {
            var query = new List<Basket>();
            try {
            query = await _db.BasketItems
                .Where(o => o.UserId == GetUserId())
                .Include(p => p.Books.BookBase)
                .Include(p => p.Books.Extras).ThenInclude(t => t.Extra)
                .OrderByDescending(s => s.Id).ToListAsync();
                
            } catch (Exception ex){
                _logger.LogCritical(ex, "Critical exception at {time}", DateTime.UtcNow);
            }
            if (query != null){
                _logger.LogTrace("{count} number of items in the basket", query.Count());
            } else {
                _logger.LogInformation("No items in the basket");
            }
            return query;
        }

        [HttpPost]
        public async Task<ActionResult<Basket>> AddItemToBasket(Basket book) {
            book = new Basket {
                UserId = GetUserId(),
                Books = book.Books,
            };

            _db.BasketItems.Attach(book);
            await _db.SaveChangesAsync();
            _logger.LogDebug("New book saved to the basket. Id: {id}", book.Id);
            return Ok(book);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteBasket() {
            var res = _db.BasketItems;
            if (res == null) {
                BadRequest("Basket already empty");
            }
            _db.BasketItems.RemoveRange(res);
            await _db.SaveChangesAsync();
            return Ok(res);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteBasketItem([FromRoute] int id) {
            var res = await _db.BasketItems.FindAsync(id);
            if (res == null) {
                BadRequest("Invalid book id");
            }
            _db.BasketItems.Remove(res);
            await _db.SaveChangesAsync();
            return Ok(res);
        }

        private string GetUserId() {
            // This will be the user's twitter username
            return HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        }
    }
}