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
    public class FinalDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public FinalDialog(LuisSetup luisRecognizer, ILogger<FinalDialog> logger, UserState userState, NoPermissionDialog noPermission)
            : base(nameof(FinalDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noPermission);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AnythingAsync,
                QnA,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AnythingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is there anything else I can help you with?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> QnA(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }

/*
        private async Task<DialogTurnResult> AnythingElseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily)
            {
                
                if(userProfile.IsClient == true)
                {
                    return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt= MessageFactory.Text("Working")}, cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync(nameof(GiveOptionsNotClientDialog), null, cancellationToken);
                }
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(FinalDialog), null, cancellationToken);
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I was not able to understand that. Can you please repeat what you said?")}, cancellationToken );
            }
        }

        private async Task<DialogTurnResult> RetryAnythingElseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (luisResult.TopIntent().intent == LuisIntents.Intent.ServiceToShareWithFamily)
            {
                if (userProfile.IsClient == true)
                {
                    return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync(nameof(GiveOptionsNotClientDialog), null, cancellationToken);
                }
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(FinalDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(NoUnderstandDialog), null, cancellationToken);
                */

    }
}
