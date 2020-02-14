using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BlazingBook.Server {
    [ApiController]
    public class UserController : Controller {
        private static UserInfo LoggedOutUser = new UserInfo { IsAuthenticated = false };

        [HttpGet("user")]
        public UserInfo GetUser() {
            return User.Identity.IsAuthenticated
                ? new UserInfo {
                    Name = User.Identity.Name,
                        Email = User.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).FirstOrDefault(),
                        Picture = User.FindFirst("urn:google:image").Value,
                        Locale = User.FindFirst("urn:google:locale").Value,
                        IsAuthenticated = true
                }
                : LoggedOutUser;
        }

        [HttpGet("user/signin")]
        public async Task SignIn(string redirectUri) {
            if (string.IsNullOrEmpty(redirectUri) || !Url.IsLocalUrl(redirectUri)) {
                redirectUri = "/";
            }

            await HttpContext.ChallengeAsync(
                GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = redirectUri });
        }

        [HttpGet("user/signout")]
        public async Task<IActionResult> SignOut() {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/");
        }
    }
}