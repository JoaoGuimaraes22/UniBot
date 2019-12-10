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
    public class TrueNoToMainDialog : ComponentDialog
    {
        //If user doesn't give permission
        private readonly LuisSetup _recognizer;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        protected readonly ILogger Logger;
        private readonly UserState _userState;
        public TrueNoToMainDialog(LuisSetup luisRecognizer, ILogger<TrueNoToMainDialog> logger, UserState userState, IConfiguration configuration, IHttpClientFactory httpClientFactory, GoodbyeDialog goodbye, NoPermissionDialog noPermission)
            : base(nameof(TrueNoToMainDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(goodbye);
            AddDialog(noPermission);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PreQnaAsync,
                QnaAsync,
                RetryEnd,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PreQnaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initial prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok, anyhting I can help you with?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> QnaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Instantiates UserProfile storage
            var userProfile = new UserProfile();
            userProfile.GavePermission = true;

            //Configures QnA
            var httpClient = _httpClientFactory.CreateClient();

            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                EndpointKey = _configuration["QnAEndpointKey"],
                Host = _configuration["QnAEndpointHostName"]
            },
            null,
            httpClient);

            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            // The actual call to the QnA Maker service.
            //Sets QnA Score Threshold
            var qnaOptions = new QnAMakerOptions();
            qnaOptions.ScoreThreshold = 0.4F;

            //Gets QnA response
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context, qnaOptions);
            if (response != null && response.Length > 0)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }

            //Retries
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. If you want to end this conversation, say 'NO' and if you want to continue, say 'YES'.") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryEnd(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //Configures QnA
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
            //Sets QnA Score Threshold
            var qnaOptions = new QnAMakerOptions();
            qnaOptions.ScoreThreshold = 0.4F;

            //Gets QnA response
            var response = await qnaMaker.GetAnswersAsync(stepContext.Context, qnaOptions);
            if (response != null && response.Length > 0)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{response[0].Answer}. To continue, say 'YES'.") }, cancellationToken);
            }

            //Goes to NoPermissionDialog
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //For safe
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}