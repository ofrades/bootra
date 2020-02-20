﻿using System;
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

        [Inject]
        private NavigationManager _navigationManager { get; set; }
        private readonly HttpClient _httpClient;

        public OrderState(HttpClient HttpClient, IToaster Toaster, NavigationManager NavigationManager) {
            _httpClient = HttpClient;
            _toaster = Toaster;
            _navigationManager = NavigationManager;
        }
        public event Action OnChange;
        public bool ShowingConfigureDialog { get; private set; }
        public BookCustom ConfiguringBook { get; private set; }
        public Order Order { get; private set; } = new Order();
        public List<Basket> BasketList { get; set; } = new List<Basket>();
        public List<Wish> WishList { get; set; } = new List<Wish>();
        public List<BookBase> Bookbases { get; set; }
        public Root BooksApi { get; set; }
        public bool isSubmitting { get; set; }
        public Result ResultApi { get; set; }
        public List<Author> Author { get; set; }
        public async Task AddToWishList(BookBase book) {
            if (WishList.Any(x => x.BookBase.Id == book.Id)) {
                _toaster.Info("Already Wished");
            } else {
                var fakeWish = new Wish { Id = -1, BookBase = book, UserId = "" };
                try {
                    WishList.Add(fakeWish);
                    NotifyStateChanged();
                    var wishCreate = new WishCreate { BookId = book.Id, BookBase = book };
                    var res = await _httpClient.PostJsonAsync<Wish>("wishes", wishCreate);
                    fakeWish.Id = res.Id;
                    fakeWish.UserId = res.UserId;
                } catch (HttpRequestException ex) {
                    _toaster.Warning($"Not Saved {ex.Message}");
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
        public async Task GetBasketList() {
            try {
                BasketList = await _httpClient.GetJsonAsync<List<Basket>>("basket");
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
                Size = BookCustom.DefaultSize,
                Extras = new List<BookExtra>(),
            };

            NotifyStateChanged();
            ShowingConfigureDialog = true;
        }
        public async Task ConfirmConfigureBookDialog() {
            var basketItem = new Basket {
                Books = ConfiguringBook,
            };
            try {
                BasketList.Add(basketItem);
                var res = await _httpClient.PostJsonAsync<Basket>("basket", basketItem);
                if (WishList.Any(c => c.BookBase.Id == ConfiguringBook.BookBase.Id)) {
                    await RemoveFromWishList(WishList.Where(c => c.BookBase.Id == ConfiguringBook.BookBase.Id).Select(c => c.Id).Single());
                }
                NotifyStateChanged();
                ConfiguringBook = null;
                ShowingConfigureDialog = false;

            } catch (HttpRequestException ex) {
                _toaster.Warning($"Error {ex}");
            }
        }
        public async Task AddBook(Result result) {
            try {
                await _httpClient.PostJsonAsync<Result>("bookbase", result);
                await GetBooks();
                NotifyStateChanged();
            } catch (HttpRequestException ex) {
                _toaster.Warning($"Error adding book {ex}");
            }
        }
        public async Task GetBooks() {
            try {
                Bookbases = await _httpClient.GetJsonAsync<List<BookBase>>("bookbase");
                NotifyStateChanged();
            } catch (HttpRequestException ex) {
                _toaster.Warning($"Error getting books {ex}");
            }
        }
        public async Task RemoveBook(int id) {
            try {
                await _httpClient.DeleteAsync($"bookbase/{id}");
                NotifyStateChanged();
                _toaster.Success($"Deleted Book with id: {id}");
            } catch (HttpRequestException ex) {
                _toaster.Warning($"Error removing book {ex}");
            }
        }
        public void CancelConfigureBookDialog() {
            ConfiguringBook = null;
            ShowingConfigureDialog = false;
        }

        public async Task PlaceOrder() {
            Order.Books = BasketList.Select(c => c.Books).ToList();
            isSubmitting = true;

            try {
                await _httpClient.PostJsonAsync<int>("orders", Order);
                foreach (var item in Order.Books) {
                    RemoveConfiguredBook(item);
                }
                ResetOrder();
                _navigationManager.NavigateTo($"myorders");
            } finally {
                isSubmitting = false;
            }
        }

        public async void RemoveConfiguredBook(BookCustom book) {
            var basketItem = BasketList.FirstOrDefault(c => c.Books.Id == book.Id);
            try {
                BasketList.Remove(basketItem);
                await _httpClient.DeleteAsync($"basket/{basketItem.Id}");
                NotifyStateChanged();
            } catch (Exception ex) {
                _toaster.Warning($"Error removing book {ex}");
            }
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
                _toaster.Warning($"No results {ex}");
            }
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}