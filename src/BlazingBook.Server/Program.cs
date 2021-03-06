﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazingBook.Server {
    public class Program {
        public static void Main(string[] args) {
            var host = CreateHostBuilder(args).Build();

            // Initialize the database
            var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
            using(var scope = scopeFactory.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<BookStoreContext>();
                if (db.Database.EnsureCreated()) {
                    SeedData.Initialize(db);
                }
            }
            host.Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging((context, logging) => {
                logging.ClearProviders();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddConsole();
            })
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();
            });
    }
}