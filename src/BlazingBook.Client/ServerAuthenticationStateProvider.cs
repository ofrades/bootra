using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
namespace BlazingBook.Client {
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider {
        private readonly HttpClient _httpClient;
        public ServerAuthenticationStateProvider(HttpClient httpClient) {
            _httpClient = httpClient;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            var userInfo = await _httpClient.GetJsonAsync<UserInfo>("user");
            var identity = userInfo.IsAuthenticated
                ? new ClaimsIdentity(new [] {
                    new Claim(ClaimTypes.Name, userInfo.Name),
                    new Claim(ClaimTypes.Email, userInfo.Email),
                    new Claim("Picture", userInfo.Picture),
                    new Claim("Locale", userInfo.Locale),
                }, "serverauth")
                : new ClaimsIdentity();
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}