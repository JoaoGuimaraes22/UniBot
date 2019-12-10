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
    //Choice between phone or email
    public class InfoSendDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public InfoSendDialog(LuisSetup luisRecognizer, ILogger<InfoSendDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, SendContactDialog sendContact, GoodbyeDialog goodbye)
            : base(nameof(InfoSendDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noUnderstand);
            AddDialog(sendContact);
            AddDialog(goodbye);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WhereAsync,
                PhoneOrEmailAsync,
                RetryPhoneOrEmailAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> WhereAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initial Prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like the information on on your phone or email?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneOrEmailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            //If intent is email
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Email)
            {
                userProfile.ChoseEmail = true;
                return await stepContext.BeginDialogAsync(nameof(SendContactDialog), null, cancellationToken);
            }

            //If intent is phone
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Phone)
            {
                userProfile.ChosePhone = true;
                return await stepContext.BeginDialogAsync(nameof(SendContactDialog), null, cancellationToken);
            }

            //Retries
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> RetryPhoneOrEmailAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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

            //If intent us exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If intent is email
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Email)
            {
                userProfile.ChosePhone = true;
                return await stepContext.BeginDialogAsync(nameof(SendContactDialog), null, cancellationToken);
            }

            //If intent is phone
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Phone)
            {
                userProfile.ChoseEmail = true;
                return await stepContext.BeginDialogAsync(nameof(SendContactDialog), null, cancellationToken);
            }

            //Goes to NoUnderstandDiaog
            return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //For safety
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}
