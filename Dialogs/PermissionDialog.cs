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
    public class PermissionDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public PermissionDialog(LuisSetup luisRecognizer, ILogger<PermissionDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, SendContactInfoDialog sendContact)
            : base(nameof(PermissionDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noUnderstand);
            AddDialog(sendContact);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SavaDataAsync,
                GavePermissionAsync,
                RetryGavePermissionAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SavaDataAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you allow us to store and process that information to use in the future for Marketing purposes?")}, cancellationToken);
        }

        private async Task<DialogTurnResult> GavePermissionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                var userProfile = new UserProfile();
                userProfile.GavePermission = true;
                return await stepContext.BeginDialogAsync(nameof(SendContactInfoDialog));
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {

                return await stepContext.BeginDialogAsync(nameof(SendContactInfoDialog));
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryGavePermissionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                var userProfile = new UserProfile();
                userProfile.GavePermission = true;
                return await stepContext.BeginDialogAsync(nameof(SendContactInfoDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(SendContactInfoDialog));
            }
            else
            {
                return await stepContext.PromptAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }


    }
}