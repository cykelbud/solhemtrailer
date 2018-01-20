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

            services.AddSingleton<IScheduleQueries, Scheduler>();
            services.AddTransient<IAzureTableFactory, AzureTableFactory>();
            services.AddSingleton<IEventStore, AzureEventStore >();
            services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
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
            var commandHandler = new TrailerAggregate();
            var readmodel = app.ApplicationServices.GetService<IScheduleQueries>();
            var messageDispatcher = app.ApplicationServices.GetService<IMessageDispatcher>();
            var eventStore = app.ApplicationServices.GetService<IEventStore>();

            messageDispatcher.ScanInstance(commandHandler);
            messageDispatcher.ScanInstance(readmodel);

            // ladda readmodel för scheduler, en egen dispatcher 
            var dispatcher = new MessageDispatcher(eventStore);
            dispatcher.AddSubscriberFor<TrailerBookedEvent>((Scheduler)readmodel);
            dispatcher.AddSubscriberFor<TrailerBookingCanceledEvent>((Scheduler)readmodel);
            var allevents = eventStore.LoadEventsFor<object>(Constants.TrailerId);
            dispatcher.PublishEvents(allevents.Cast<IEvent>());




            app.UseStaticFiles();

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
