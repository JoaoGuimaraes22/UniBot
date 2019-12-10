using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UniBotJG.CognitiveModels;


namespace UniBotJG.Dialogs
{
    //This dialog is used when user wants an assistan/employee to help him/she
    public class GetAssistantDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public GetAssistantDialog(LuisSetup luisRecognizer, ILogger<GetAssistantDialog> logger, UserState userState, NoPermissionDialog noPermission, SuitCustomerNeedsDialog suitCustomer, NoUnderstandDialog noUnderstand, GoodbyeDialog goodbye)
            : base(nameof(GetAssistantDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noPermission);
            AddDialog(suitCustomer);
            AddDialog(noUnderstand);
            AddDialog(goodbye);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WantAsync,
                RetryWantAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> WantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            //If intent is yes
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(SuitCustomerNeedsDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
            }

            //Retries
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I was not able to understand that. Can you please repeat what you said?​") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryWantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            //If intent is yes
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(SuitCustomerNeedsDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No && luisResult.TopIntent().score > 0.70)
            {
                return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
            }

            //Goes to NoUnderstandDialog
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //For safety, stopping infinite looping
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}