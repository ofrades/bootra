using System;
using System.Collections.Generic;
using Blazored.LocalStorage;

namespace BlazingBook.Client {
    public class OrderState {
        private readonly ILocalStorageService _localStorage;
        public OrderState(ILocalStorageService localStorage) {
            _localStorage = localStorage;
        }
        public event Action OnChange;
        public bool ShowingConfigureDialog { get; private set; }

        public BookCustom ConfiguringBook { get; private set; }

        public Order Order { get; private set; } = new Order();
        public List<BookBase> Wish { get; set; } = new List<BookBase>();

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

        public void AddToWishList(BookBase book) {
            Wish.Add(book);
            _localStorage.SetItemAsync("wish", book);
            NotifyStateChanged();
        }

        public async void GetFromLocalStorage() {
            var wish = await _localStorage.GetItemAsync<BookBase>("wish");
            var basket = await _localStorage.GetItemAsync<BookCustom>("basket");
            Wish.Clear();
            Wish.Add(wish);
            Order.Books.Clear();
            Order.Books.Add(basket);
            NotifyStateChanged();
        }
        public void RemoveFromWishList(BookBase book) {
            Wish.Remove(book);
            NotifyStateChanged();
        }

        public void CancelConfigureBookDialog() {
            ConfiguringBook = null;

            ShowingConfigureDialog = false;
        }

        public void ConfirmConfigureBookDialog() {
            Order.Books.Add(ConfiguringBook);
            _localStorage.SetItemAsync("basket", ConfiguringBook);
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