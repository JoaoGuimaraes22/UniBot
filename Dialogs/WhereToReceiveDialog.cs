using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UniBotJG.CognitiveModels;


namespace UniBotJG.Dialogs
{
    //Asks if user wants to be assisted by an employee
    public class WhereToReceiveDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public WhereToReceiveDialog(LuisSetup luisRecognizer, ILogger<WhereToReceiveDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, GetAssistantDialog getAssistant, NoPermissionDialog noPermission, GoodbyeDialog goodbye)
            : base(nameof(WhereToReceiveDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noUnderstand);
            AddDialog(getAssistant);
            AddDialog(noPermission);
            AddDialog(goodbye);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                OkEmployeeAsync,
                EndOkEmployeeAsync,
                RetryEndOkEmployeeAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> OkEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Inital prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to be assisted by an employee that could provide a better experience?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndOkEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //If intent is exit
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is yes
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(GetAssistantDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
            }

            //Retries
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryEndOkEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //If intent is yes
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(GetAssistantDialog), null, cancellationToken);
            }

            //If intent is no
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
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
            //For safety
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}