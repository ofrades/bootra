// using Blazorise;
// using Blazorise.Bootstrap;
// using Microsoft.AspNetCore.Components.Authorization;
// using Microsoft.AspNetCore.Components.Builder;
// using Microsoft.Extensions.DependencyInjection;

// namespace BlazingBook.Client {
//     public class Startup {
//         public void ConfigureServices(IServiceCollection services) {
//             services.AddScoped<OrderState>();
//             services
//                 .AddBlazorise(options => {
//                     options.ChangeTextOnKeyPress = true; // optional
//                 })
//                 .AddBootstrapProviders();
//             // Add auth services
//             services.AddAuthorizationCore();
//             services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
//         }

//         public void Configure(IComponentsApplicationBuilder app) {
//             app.Services.UseBootstrapProviders();
//             app.AddComponent<App>("app");
//         }
//     }
// }