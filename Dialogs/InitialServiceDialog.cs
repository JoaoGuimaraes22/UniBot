using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UniBotJG.CognitiveModels;
using UniBotJG.StateManagement;


namespace UniBotJG.Dialogs
{
    //Asks if user is CA client
    public class InitialServiceDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public InitialServiceDialog(LuisSetup luisRecognizer, ILogger<InitialServiceDialog> logger, UserState userState, NIFPermissionDialog nIFPermission, IsNotClientDialog isNot, NoUnderstandDialog noUnderstand, GoodbyeDialog goodbye)
            : base(nameof(InitialServiceDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(nIFPermission);
            AddDialog(isNot);
            AddDialog(noUnderstand);
            AddDialog(goodbye);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AreYouClientAsync,
                IfIsAsync,
                IfIsRetryAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AreYouClientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initial Prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Are you a client of Crédito Agrícola? ") }, cancellationToken);
        }

        private async Task<DialogTurnResult> IfIsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //Instantiates UserProfile storage
            var userProfile = new UserProfile();

            //If intent is exit
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is yes
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                //Sets storage IsClient to true
                userProfile.IsClient = true;
                return await stepContext.BeginDialogAsync(nameof(NIFPermissionDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(IsNotClientDialog), null, cancellationToken);
            }

            //Retries
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IfIsRetryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //Instantiates UserProfile storage
            var userProfile = new UserProfile();

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is yes
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                userProfile.IsClient = true;
                return await stepContext.BeginDialogAsync(nameof(NIFPermissionDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(IsNotClientDialog), null, cancellationToken);
            }

            //Goes to NoUnderstandDialog
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //For safety
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }




    }
}