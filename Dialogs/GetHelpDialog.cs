using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UniBotJG.CognitiveModels;
using UniBotJG.StateManagement;

namespace UniBotJG.Dialogs
{
    //Initial "How can I help you"
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
            //Initial prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thank you. This information is only going to be used to give you a more personalized services. How can I help you?​") }, cancellationToken);
        }

        private async Task<DialogTurnResult> WhatToHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Sets up LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is "i do not live in portugal but i have family here and i wanted to know if they are able to access my account?​"
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
            }

            //Instantiates UserProfile storage
            var userProfile = new UserProfile();

            //Setting up QnA
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
            // Sets score threshold
            var qnaOptions = new QnAMakerOptions();
            qnaOptions.ScoreThreshold = 0.4F;

            // Sets QnA response
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context, qnaOptions);
            if (response != null && response.Length > 0)
            {
                userProfile.NotService = true;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }

            //Retries, goes next ggwp
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);

        }

        private async Task<DialogTurnResult> RetryWhatToHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            //Sets up LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is "i do not live in portugal but i have family here and i wanted to know if they are able to access my account?​"
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
            }

            //Instantiates UserProfile storage
            var userProfile = new UserProfile();

            //Setting up QnA
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
            // Sets score threshold
            var qnaOptions = new QnAMakerOptions();
            qnaOptions.ScoreThreshold = 0.4F;
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context, qnaOptions);

            // Sets QnA response
            if (response != null && response.Length > 0)
            {
                userProfile.NotService = true;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }

            //Goes to NoUnderstandDialog
            return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> QnaGoToAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Goes to QnA Dialog
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Used for safety/stoppage of infinite loops
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}