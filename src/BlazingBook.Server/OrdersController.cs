using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPush;

namespace BlazingBook.Server {
    [Route("orders")]
    [ApiController]
    [Authorize]
    public class OrdersController : Controller {
        private readonly BookStoreContext _db;

        public OrdersController(BookStoreContext db) {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderWithStatus>>> GetOrders() {
            var orders = await _db.Orders
                .Where(o => o.UserId == GetUserId())
                .Include(o => o.Books).ThenInclude(p => p.BookBase)
                .Include(o => o.Books).ThenInclude(p => p.Extras).ThenInclude(t => t.Extra)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderWithStatus>> GetOrderWithStatus(int orderId) {
            var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .Where(o => o.UserId == GetUserId())
                .Include(o => o.Books).ThenInclude(p => p.BookBase)
                .Include(o => o.Books).ThenInclude(p => p.Extras).ThenInclude(t => t.Extra)
                .SingleOrDefaultAsync();

            if (order == null) {
                return NotFound();
            }

            return OrderWithStatus.FromOrder(order);
        }

        [HttpPost]
        public async Task<ActionResult<int>> PlaceOrder(Order order) {
            order.CreatedTime = DateTime.Now;
            order.UserId = GetUserId();

            // Enforce existence of Book.BookBaseId and Extra.ExtraId
            // in the database - prevent the submitter from making up
            // new bookbases and extras
            foreach (var book in order.Books) {
                book.BookBaseId = book.BookBase.Id;
                book.BookBase = null;

                foreach (var extra in book.Extras) {
                    extra.ExtraId = extra.Extra.Id;
                    extra.Extra = null;
                }
            }

            _db.Orders.Attach(order);
            await _db.SaveChangesAsync();

            // In the background, send push notifications if possible
            var subscription = await _db.NotificationSubscriptions.Where(e => e.UserId == GetUserId()).SingleOrDefaultAsync();
            if (subscription != null) {
                _ = TrackAndSendNotificationsAsync(order, subscription);
            }

            return order.OrderId;
        }

        private string GetUserId() {
            // This will be the user's twitter username
            return HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        }

        private static async Task TrackAndSendNotificationsAsync(Order order, NotificationSubscription subscription) {
            // In a realistic case, some other backend process would track
            // order delivery progress and send us notifications when it
            // changes. Since we don't have any such process here, fake it.
            await Task.Delay(OrderWithStatus.PreparationDuration);
            await SendNotificationAsync(order, subscription, "Your order has been dispatched!");

            await Task.Delay(OrderWithStatus.DeliveryDuration);
            await SendNotificationAsync(order, subscription, "Your order is now delivered. Enjoy!");
        }

        private static async Task SendNotificationAsync(Order order, NotificationSubscription subscription, string message) {
            // For a real application, generate your own
            var publicKey = "BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o";
            var privateKey = "OrubzSz3yWACscZXjFQrrtDwCKg-TGFuWhluQ2wLXDo";

            var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
            var vapidDetails = new VapidDetails("mailto:<someone@example.com>", publicKey, privateKey);
            var webPushClient = new WebPushClient();
            try {
                var payload = JsonSerializer.Serialize(new {
                    message,
                    url = $"myorders/{order.OrderId}",
                });
                await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
            } catch (Exception ex) {
                Console.Error.WriteLine("Error sending push notification: " + ex.Message);
            }
        }
    }
}