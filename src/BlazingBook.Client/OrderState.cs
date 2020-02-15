using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sotsera.Blazor.Toaster;

namespace BlazingBook.Client {
    public class OrderState {
        [Inject]
        private IToaster _toaster { get; set; }
        private readonly HttpClient _httpClient;

        public OrderState(HttpClient HttpClient, IToaster Toaster) {
            _httpClient = HttpClient;
            _toaster = Toaster;
        }
        public event Action OnChange;
        public bool ShowingConfigureDialog { get; private set; }
        public BookCustom ConfiguringBook { get; private set; }
        public Order Order { get; private set; } = new Order();
        public List<Wish> WishList { get; set; } = new List<Wish>();
        public List<BookBase> Bookbases { get; set; }
        public Root BooksApi { get; set; }
        public Result ResultApi { get; set; }
        public List<Author> Author { get; set; }
        public async Task AddToWishList(BookBase book = null, Result resultApi = null) {
            if (book == null) {
                book = new BookBase {
                Author = resultApi.Authors.Select(c => c.Name).SingleOrDefault(),
                BasePrice = 12.00m,
                Id = resultApi.Id,
                ImageUrl = "",
                Title = resultApi.Title
                };
            }
            if (WishList.Any(x => x.BookId == book.Id)) {
                _toaster.Info("Already Wished");
                // TODO: Toast Wish already exists in basket
                // } else if (WishList.Any(c => c.BookId == ConfiguringBook.BookBase.Id)) {
                //    return;
            } else {
                var fakeWish = new Wish { Id = -1, BookId = book.Id, Book = book, UserId = "" };
                try {
                    WishList.Add(fakeWish);
                    NotifyStateChanged();
                    var wishCreate = new WishCreate { BookId = book.Id, Book = book };
                    var res = await _httpClient.PostJsonAsync<Wish>("wishes", wishCreate);
                    fakeWish.Id = res.Id;
                    fakeWish.UserId = res.UserId;
                } catch (HttpRequestException) {
                    _toaster.Warning("Not Saved");
                    WishList.Remove(fakeWish);
                }
                NotifyStateChanged();
            }
        }

        public async Task GetWishes() {
            try {
                WishList = await _httpClient.GetJsonAsync<List<Wish>>("wishes");
                NotifyStateChanged();
            } catch (HttpRequestException ex) {
                throw ex;
            }
        }

        public async Task RemoveFromWishList(int id) {
            try {
                await _httpClient.DeleteAsync($"wishes/{id}");
            } catch (HttpRequestException ex) {
                throw ex;
            }
            await GetWishes();
            NotifyStateChanged();
        }
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
        public async Task SearchBooks(string search) {
            try {
                BooksApi = await _httpClient.GetJsonAsync<Root>($"http://gutendex.com/books?search={search}");
                _toaster.Info($"Search results: {BooksApi.Count.ToString()} books");
                NotifyStateChanged();
            } catch (Exception ex) {
                throw ex;
            }
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}