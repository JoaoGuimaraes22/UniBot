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

namespace UniBotJG.Dialogs
{
    public class MainDialog : ComponentDialog
    {

        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public MainDialog(LuisSetup luisRecognizer, ILogger<MainDialog> logger, UserState userState,  NoUnderstandDialog noUnderstand, InitialServiceDialog initialService, NoPermissionDialog noPermission)
            : base(nameof(MainDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(noUnderstand);
            AddDialog(initialService);
            AddDialog(noPermission);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                AllowInfoStoreAsync,
                RetryAllowInfoStoreAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {   
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var messageText = stepContext.Options?.ToString() ?? "Hi welcome to Crédito Agrícola, Do you allow me to store and process your personal information in order to provide you with a better experience?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            // Use the text provided in FinalStepAsync or the default if it is the first time.
        }

        private async Task<DialogTurnResult> AllowInfoStoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
                {
                    return await stepContext.BeginDialogAsync(nameof(InitialServiceDialog), null, cancellationToken);
                }
                if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
                {
                    return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
                }

                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);

            }
        }

        private async Task<DialogTurnResult> RetryAllowInfoStoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
                {
                    return await stepContext.BeginDialogAsync(nameof(InitialServiceDialog), null, cancellationToken);
                }
                if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
                {
                    return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
                }

                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);

            }
        }
    }
}
