using System.Threading.Tasks;
using Blazorise;
using Blazorise.Material;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BlazingBook.Client {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped<OrderState>();
            builder.Services.AddBlazorise(options => {
                options.ChangeTextOnKeyPress = true;
            })
            .AddMaterialProviders();
            
            // Add auth services
            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            builder.Services.AddOptions();

            await builder.Build().RunAsync();
        }
    }
}