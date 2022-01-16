using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MongoDB;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongo().AddMongoRepository<InventoryItem>("inventoryitems");

            Random jitterer = new Random();
            //register the catalog client.
            services.AddHttpClient<CatalogClient>(client =>
            {
                client.BaseAddress = new System.Uri("https://localhost:7183");
            })
            // defines how many seconds to wait for an external api (catalog service) before failing.
            //.Or<TimeoutRejectedException>() -> if fails for the timepout plicy, then retry. this combines the two policies.
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5, //amount of retries.
                   //this will rise to the power of 2 each attempt, 1st to 2 = 1, 2 pow 2 = 4... 8... etc + some random milliseconds.
                retryAttempt => System.TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                // just to know what is going on, use this:
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    //JUST FOR DEMO PORPUSES!... DO NOT USE A SERVICE PROVIDER TO LOG YOUR SHIT (optional, please remove)
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<CatalogClient>>()?
                    .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3, //3 fail request before opening the circuit
                TimeSpan.FromSeconds(15), // time the circuit will be open.
                                          //log to see what is going on (optional, please remove)
                onBreak: (outcome, timespan) =>
                 {
                     var serviceProvider = services.BuildServiceProvider();
                     serviceProvider.GetService<ILogger<CatalogClient>>()?
                     .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");

                 },
                onReset: () =>
                 {
                     var serviceProvider = services.BuildServiceProvider();
                     serviceProvider.GetService<ILogger<CatalogClient>>()?
                     .LogWarning($"Closing the circuit ...");

                 }

            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
