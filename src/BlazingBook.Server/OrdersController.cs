using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IronPdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Scriban;
using SendGrid;
using SendGrid.Helpers.Mail;
using WebPush;

namespace BlazingBook.Server {
        [Route("orders")]
        [ApiController]
        [Authorize]
        public class OrdersController : Controller {
            private readonly BookStoreContext _db;
            private readonly IConfiguration _config;
            public OrdersController(BookStoreContext db, IConfiguration config) {
                _db = db;
                _config = config;
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

            [HttpPost]
            public async Task<ActionResult<int>> PlaceOrder(Order order) {
                order.CreatedTime = DateTime.Now;
                order.TotalPrice = order.GetFormattedTotalPrice();
                order.UserId = GetUserId();

                // Enforce existence of Book.BookBaseId and Extra.ExtraId
                // in the database - prevent the submitter from making up
                // new bookbases and extras
                foreach (var book in order.Books) {
                    // book.BookBaseId = book.BookBase.Id;
                    book.BookBase = null;

                    foreach (var extra in book.Extras) {
                        // extra.ExtraId = extra.Extra.Id;
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

                private async Task TrackAndSendNotificationsAsync(Order order, NotificationSubscription subscription) {
                    // In a realistic case, some other backend process would track
                    // order delivery progress and send us notifications when it
                    // changes. Since we don't have any such process here, fake it.
                    await Task.Delay(OrderWithStatus.PreparationDuration);
                    // await SendNotificationAsync(order, subscription, "Your order has been dispatched!");

                    await Task.Delay(OrderWithStatus.DeliveryDuration);
                    await ExecuteEmail(order, $"Bootra: Your order: {order.OrderId} is now delivered. Enjoy!");
                    await SendNotificationAsync(order, subscription, $"Bootra: Your order: {order.OrderId} is now delivered. Enjoy!");
                }

                private async Task SendNotificationAsync(Order order, NotificationSubscription subscription, string message) {
                    // For a real application, generate your own
                    var publicKey = _config["Notification:publicKey"];
                    var privateKey = _config["Notification:privateKey"];
                    var subject = _config["Notification:subject"];

                    var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
                    var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
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
                private async Task ExecuteEmail(Order order, string message) {
                    var file = await System.IO.File.ReadAllTextAsync("template.sbn");
                    var template = Template.Parse(file);
                    var result = template.Render(order, memberInfo => memberInfo.Name);
                    var Renderer = new HtmlToPdf();
                    var pdfFromString = (await Renderer.RenderHtmlAsPdfAsync(result)).BinaryData;
                    var fileFromUrl = Convert.ToBase64String(pdfFromString);
                    var apiKey = _config["SendGrid:apiKey"];
                    var client = new SendGridClient(apiKey);
                    var from = new EmailAddress(_config["SendGrid:emailFrom"], _config["SendGrid:nameFrom"]);
                    var subject = message;
                    var to = new EmailAddress(order.DeliveryAddress.Email, order.DeliveryAddress.Name);
                    var plainTextContent = "";
                    var htmlContent = "<h1>From BOOTRA:</h1><br><strong>Your order PDF is attached!</strong>";
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                    msg.AddAttachment($"{order.UserId}{order.OrderId}.pdf", fileFromUrl);
                    var response = await client.SendEmailAsync(msg);
                }
            }
        }