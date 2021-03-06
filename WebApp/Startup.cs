using System.Linq;
using Azure;
using Core;
using Edument.CQRS;
using Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using ReadModels;
using SmsRelay;
using Swashbuckle.AspNetCore.Swagger;
using Trailer;

namespace solhemtrailer
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
            services.AddMvc()
                .AddJsonOptions(options =>
            {
                var settings = options.SerializerSettings;
                settings.ContractResolver = new DefaultContractResolver();
                // do something with settings
            });

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });

            services.AddSingleton<IScheduleQueries, Scheduler>();
            services.AddTransient<IAzureTableFactory, AzureTableFactory>();
            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
            services.AddSingleton<SmsNotifyer>();
            services.AddSingleton<TrailerAggregate>();

            //services.AddSingleton<IEventStore, InMemoryEventStore>();
            //services.AddSingleton<IRestClient, FakeRestClient>();
            services.AddSingleton<IEventStore, AzureEventStore>();
            services.AddSingleton<IRestClient, RestClient>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
            }

            // wireup
            var trailerAggregate = app.ApplicationServices.GetService<TrailerAggregate>();
            var relayNotifyer = app.ApplicationServices.GetService<SmsNotifyer>();
            var readmodel = app.ApplicationServices.GetService<IScheduleQueries>();
            var eventStore = app.ApplicationServices.GetService<IEventStore>();

            // ladda readmodel f�r scheduler, en egen dispatcher 
            var dispatcher = new MessageDispatcher(eventStore);
            dispatcher.AddSubscriberFor<TrailerBookedEvent>((Scheduler)readmodel);
            dispatcher.AddSubscriberFor<TrailerBookingCanceledEvent>((Scheduler)readmodel);
            var trailerEvents = eventStore.LoadEventsFor<object>(Constants.TrailerId);
            dispatcher.PublishEvents(trailerEvents);

            var messageDispatcher = app.ApplicationServices.GetService<IMessageDispatcher>();
            messageDispatcher.ScanInstance(trailerAggregate);
            messageDispatcher.ScanInstance(relayNotifyer);
            messageDispatcher.ScanInstance(readmodel);



            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }



    }
}
