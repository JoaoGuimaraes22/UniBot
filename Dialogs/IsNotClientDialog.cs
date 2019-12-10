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
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.AI.QnA;

namespace UniBotJG.Dialogs
{
    public class IsNotClientDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IsNotClientDialog(LuisSetup luisRecognizer, ILogger<IsNotClientDialog> logger, UserState userState, GiveOptionsNotClientDialog giveOptions, NoUnderstandDialog noUnderstand, GoodbyeDialog goodbye, IHttpClientFactory httpClient, IConfiguration configuration)
            : base(nameof(IsNotClientDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClient;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(giveOptions);
            AddDialog(noUnderstand);
            AddDialog(goodbye);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                HelpAsync,
                WhatToHelpAsync,
                RetryWhatToHelpAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> HelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. What can I help you with?") }, cancellationToken);
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
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily)
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsNotClientDialog), null, cancellationToken);
            }

            //Setting up Qna
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
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }

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
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily)
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsNotClientDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}