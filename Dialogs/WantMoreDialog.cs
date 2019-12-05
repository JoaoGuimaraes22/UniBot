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
    public class WantMoreDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public WantMoreDialog(LuisSetup luisRecognizer, ILogger<WantMoreDialog> logger, UserState userState, InfoSendDialog infoSend, NoUnderstandDialog noUnderstandDialog)
            : base(nameof(WantMoreDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(infoSend);
            AddDialog(noUnderstandDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                MoreAsync,
                CheckMoreAsync,
                RetryCheckMoreAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> MoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("This account gives you freedom to perform your operations in Portugal and overseas; Flexibility to move it in the currency you desire and access to exclusive products. Would you like to get this and more detailed information on your phone or email?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckMoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(InfoSendDialog),null, cancellationToken);
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryCheckMoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(InfoSendDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }


    }
}