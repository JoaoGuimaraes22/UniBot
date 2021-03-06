﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using UniBotJG.Dialogs;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using UniBotJG.CognitiveModels;
using UniBotJG.StateManagement;


namespace UniBotJG.Dialogs
{
    public class TemplateDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public TemplateDialog(LuisSetup luisRecognizer, ILogger<NoPermissionDialog> logger, UserState userState)
            : base(nameof(TemplateDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                TestAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> TestAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (!_recognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
            //    return await stepContext.NextAsync(null, cancellationToken);
            //}
            //var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            //if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Isn't client") }, cancellationToken);
        }

        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly IConfiguration _configuration;

        //(IHttpClientFactory httpClient, IConfiguration configuration)
        //var httpClient = _httpClientFactory.CreateClient();
        //var qnaMaker = new QnAMaker(new QnAMakerEndpoint
        //{
        //    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
        //    EndpointKey = _configuration["QnAEndpointKey"],
        //    Host = _configuration["QnAEndpointHostName"]
        //},
        //    null,
        //    httpClient);

        //// The actual call to the QnA Maker service.
        //var response = await qnaMaker.GetAnswersAsync(stepContext.Context);
    }
}