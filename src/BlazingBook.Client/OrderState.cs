using System;
using System.Collections.Generic;

namespace BlazingBook.Client {
    public class OrderState {

        public event Action OnChange;
        public bool ShowingConfigureDialog { get; private set; }

        public Book ConfiguringBook { get; private set; }

        public Order Order { get; private set; } = new Order();
        public Wish Wish { get; private set; } = new Wish();

        public void ShowConfigureBookDialog(BookSpecial special) {
            ConfiguringBook = new Book() {
                Special = special,
                SpecialId = special.Id,
                Size = Book.DefaultSize,
                Extras = new List<BookExtra>(),
            };
            NotifyStateChanged();
            ShowingConfigureDialog = true;
        }

        public void CancelConfigureBookDialog() {
            ConfiguringBook = null;

            ShowingConfigureDialog = false;
        }

        public void ConfirmConfigureBookDialog() {
            Order.Books.Add(ConfiguringBook);
            NotifyStateChanged();

            ConfiguringBook = null;
            ShowingConfigureDialog = false;
        }

        public void RemoveConfiguredBook(Book book) {
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