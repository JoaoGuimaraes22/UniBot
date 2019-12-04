﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UniBotJG.Bots;
using UniBotJG.Dialogs;

namespace UniBotJG
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
            //Creates storage for bot ****(implement blob/sql storage late)****
            var storage = new MemoryStorage();

            //Adds userState to MemoryStorage
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            //Adds conversationState to MemoryStorage
            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            //Set endpointRouting = false
            services.AddMvc(options => options.EnableEndpointRouting = false);

            //Set compability
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //QnA Register
            services.AddSingleton(new QnAMakerEndpoint
            {
                KnowledgeBaseId = Configuration.GetValue<string>($"QnAKnowledgebaseId"),
                EndpointKey = Configuration.GetValue<string>($"QnAAuthKey"),
                Host = Configuration.GetValue<string>($"QnAEndpointHostName")
            });

            //Luis Register
            services.AddSingleton<LuisSetup>();

            // The Dialog that will be run by the bot.

            services.AddSingleton<FinalDialog>();
            services.AddSingleton<GetAssistantDialog>();
            services.AddSingleton<GetHelpDialog>();
            services.AddSingleton<GetPhoneDialog>();
            services.AddSingleton<GiveOptionsDialog>();
            services.AddSingleton<GiveOptionsNotClientDialog>();
            services.AddSingleton<GoodbyeDialog>();
            services.AddSingleton<HaveAnAccountDialog>();
            services.AddSingleton<InfoSendDialog>();
            services.AddSingleton<InfoSendNotClientDialog>();
            services.AddSingleton<InitialServiceDialog>();
            services.AddSingleton<IsClientDialog>();
            services.AddSingleton<IsNotClientDialog>();
            services.AddSingleton<MainDialog>();
            services.AddSingleton<NoPermissionDialog>();
            services.AddSingleton<NoUnderstandDialog>();
            services.AddSingleton<ReEnterNIFDialog>();
            services.AddSingleton<SendContactDialog>();
            services.AddSingleton<SuitCustomerNeedsDialog>();
            services.AddSingleton<WhereToReceiveDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, UniBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
