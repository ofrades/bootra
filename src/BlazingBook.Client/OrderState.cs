using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazingBook.Client {
    public class OrderState {
        private readonly HttpClient _httpClient;
        public OrderState(HttpClient HttpClient) {
            _httpClient = HttpClient;
        }
        public event Action OnChange;
        public bool ShowingConfigureDialog { get; private set; }
        public BookCustom ConfiguringBook { get; private set; }
        public Order Order { get; private set; } = new Order();
        public List<Wish> WishList { get; set; } = new List<Wish>();

        public void ShowConfigureBookDialog(BookBase bookbase) {
            ConfiguringBook = new BookCustom() {
                BookBase = bookbase,
                BookBaseId = bookbase.Id,
                Size = BookCustom.DefaultSize,
                Extras = new List<BookExtra>(),
            };
            NotifyStateChanged();
            ShowingConfigureDialog = true;
        }

        public async Task AddToWishList(BookBase book) {
            if (WishList.Any(x => x.BookId == book.Id)) {
                // TODO: Toast Wish already exists
                return;
            } else {
                var fakeWish = new Wish { Id = -1, BookId = book.Id, Book = book, UserId = ""};
                try {
                    WishList.Add(fakeWish);
                    NotifyStateChanged();
                    var wishCreate = new WishCreate { BookId = book.Id };
                    var res = await _httpClient.PostJsonAsync<Wish>("wishes", wishCreate);
                    fakeWish.Id = res.Id;
                    fakeWish.UserId = res.UserId;
                } catch (HttpRequestException) {
                    // TODO: Toast Wish not saved
                    WishList.Remove(fakeWish);
                }
                NotifyStateChanged();
            }
        }

        public async Task GetWishes() {
            WishList = await _httpClient.GetJsonAsync<List<Wish>>("wishes");
            NotifyStateChanged();
        }

        public async Task RemoveFromWishList(int id) {
            await _httpClient.DeleteAsync($"wishes/{id}");
            await GetWishes();
            NotifyStateChanged();
        }

        public void CancelConfigureBookDialog() {
            ConfiguringBook = null;
            ShowingConfigureDialog = false;
        }

        public async Task ConfirmConfigureBookDialog() {
            Order.Books.Add(ConfiguringBook);
            if (WishList.Any(c => c.BookId == ConfiguringBook.BookBase.Id)) {
                await RemoveFromWishList(WishList.Where(c => c.BookId == ConfiguringBook.BookBase.Id).Select(c => c.Id).Single());
            }
            // var basket = await _localStorage.GetItemAsync<BookCustom>("basket");
            NotifyStateChanged();

            ConfiguringBook = null;
            ShowingConfigureDialog = false;
        }

        public void RemoveConfiguredBook(BookCustom book) {
            Order.Books.Remove(book);
            NotifyStateChanged();
        }

        public void ResetOrder() {
            Order = new Order();
        }

        public void ReplaceOrder(Order order) {
            Order = order;
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}