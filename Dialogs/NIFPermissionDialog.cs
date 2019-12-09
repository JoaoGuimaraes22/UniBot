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
    public class NIFPermissionDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public NIFPermissionDialog(LuisSetup luisRecognizer, ILogger<NIFPermissionDialog> logger, UserState userState, IsClientDialog isClient, NoUnderstandDialog noUnderstand, IsNotClientDialog isNotClient)
            : base(nameof(NIFPermissionDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(isClient);
            AddDialog(noUnderstand);
            AddDialog(isNotClient);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GiveNIFAsync,
                PermissionAsync,
                RetryPermissionAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GiveNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("To give you a more accurate service I will need your Tax ID. Do you agree to provide your Tax ID?​") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PermissionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(IsClientDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(IsNotClientDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryPermissionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(IsClientDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(IsNotClientDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }
        

        }
    }