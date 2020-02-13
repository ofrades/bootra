using System.Threading.Tasks;
using Blazored.Localisation;
using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Material;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Sotsera.Blazor.Toaster.Core.Models;

namespace BlazingBook.Client {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddBlazoredLocalisation();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddToaster(config => {
                //example customizations
                config.PositionClass = Defaults.Classes.Position.TopRight;
                config.PreventDuplicates = true;
                config.NewestOnTop = false;
            });
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

            var host = builder.Build();
            host.InitializeBlazoredLocalization();

            await host.RunAsync();
        }
    }
}