using System.Security.Claims;
using System.Security.Principal;

namespace BlazingBook {
    public class UserInfo {
        public bool IsAuthenticated { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Locale { get; set; }
    }
}