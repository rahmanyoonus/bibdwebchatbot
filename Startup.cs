// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using CoreBot;
using CoreBot.Dialogs;
using CoreBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Orchestrator;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.BotBuilderSamples;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            OrchestratorConfig = configuration.GetSection("Orchestrator").Get<OrchestratorConfig>();
        }

        public OrchestratorConfig OrchestratorConfig { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<OrchestratorRecognizer>(InitializeOrchestrator());

            // Create the bot services(QnA) as a singleton.
            services.AddSingleton<IBotServices, BotServices>();


            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Register LUIS recognizer
            //services.AddSingleton<FlightBookingRecognizer>();

            // Register the BookingDialog.
            //services.AddSingleton<BookingDialog>();
            
            //Register Account Recommend Dialog
            services.AddSingleton<AccountRecommendDialog>();
            //Register Credit card Dialog
            services.AddSingleton<CreditCardDialog>();


            // The MainDialog that will be run by the bot.
            services.AddSingleton<RootDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, DialogAndWelcomeBot<RootDialog>>();
            //services.AddTransient<IBot, DialogAndWelcomeBot<AccountRecommendDialog>>();
            services.AddTransient<IBot, DialogAndWelcomeBot<CreditCardDialog>>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }

        private OrchestratorRecognizer InitializeOrchestrator()
        {
            string modelFolder = Path.GetFullPath(OrchestratorConfig.ModelFolder);
            string snapshotFile = Path.GetFullPath(OrchestratorConfig.SnapshotFile);
            OrchestratorRecognizer orc = new OrchestratorRecognizer()
            {
                ModelFolder = modelFolder,
                SnapshotFile = snapshotFile
            };
            return orc;
        }
    }
}
