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
    public class InitialServiceDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public InitialServiceDialog(LuisSetup luisRecognizer, ILogger<InitialServiceDialog> logger, UserState userState, NIFPermissionDialog nIFPermission, IsNotClientDialog isNot, NoUnderstandDialog noUnderstand)
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

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AreYouClientAsync,
                IfIsAsync,
                IfIsRetryAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AreYouClientAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Are you a client of Crédito Agrícola? ") }, cancellationToken);
        }

        private async Task<DialogTurnResult> IfIsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                userProfile.IsClient = true;
                return await stepContext.BeginDialogAsync(nameof(NIFPermissionDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(IsNotClientDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt=MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?")}, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> IfIsRetryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                userProfile.IsClient = true;
                return await stepContext.BeginDialogAsync(nameof(NIFPermissionDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(IsNotClientDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(NoUnderstandDialog), null , cancellationToken);
            }
        }




    }
}