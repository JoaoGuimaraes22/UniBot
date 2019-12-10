using System;
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
using Microsoft.Bot.Builder.AI.QnA;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace UniBotJG.Dialogs
{
    public class GetHelpDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GetHelpDialog(LuisSetup luisRecognizer, ILogger<GetHelpDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, GiveOptionsDialog giveOptions, GoodbyeDialog goodbye, IHttpClientFactory httpClient, IConfiguration configuration, NoPermissionDialog noPermission)
            : base(nameof(GetHelpDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            _configuration = configuration;
            _httpClientFactory = httpClient;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noUnderstand);
            AddDialog(giveOptions);
            AddDialog(goodbye);
            AddDialog(noPermission);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskHelpAsync,
                WhatToHelpAsync,
                RetryWhatToHelpAsync,
                QnaGoToAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thank you. This information is only going to be used to give you a more personalized services. How can I help you?​") }, cancellationToken);
        }

        private async Task<DialogTurnResult> WhatToHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
            }

            var userProfile = new UserProfile();

            var httpClient = _httpClientFactory.CreateClient();
            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                    EndpointKey = _configuration["QnAEndpointKey"],
                    Host = _configuration["QnAEndpointHostName"]
                },
                    null,
                    httpClient);

            // The actual call to the QnA Maker service.
            var qnaOptions = new QnAMakerOptions();
            qnaOptions.ScoreThreshold = 0.4F;
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context, qnaOptions);
            if (response != null && response.Length > 0)
            {
                userProfile.NotService = true;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);

        }

        private async Task<DialogTurnResult> RetryWhatToHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily && luisResult.TopIntent().score > 0.75)
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
            }
            var userProfile = new UserProfile();
            var httpClient = _httpClientFactory.CreateClient();
            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAEndpointKey"],
                Host = _configuration["QnAEndpointHostName"]
            },
                    null,
                    httpClient);
            // The actual call to the QnA Maker service.
            var qnaOptions = new QnAMakerOptions();
            qnaOptions.ScoreThreshold = 0.4F;
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context, qnaOptions);
            if (response != null && response.Length > 0)
            {
                userProfile.NotService = true;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }
            return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> QnaGoToAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}