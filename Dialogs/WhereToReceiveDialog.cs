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
    public class WhereToReceiveDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public WhereToReceiveDialog(LuisSetup luisRecognizer, ILogger<WhereToReceiveDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, GetAssistantDialog getAssistant, FinalDialog finalDialog)
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
            AddDialog(finalDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                OkEmployeeAsync,
                EndOkEmployeeAsync,
                RetryEndOkEmployeeAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> OkEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to be assisted by an employee that could provide a better experience?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndOkEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(GetAssistantDialog), null, cancellationToken);
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(FinalDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryEndOkEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
                return await stepContext.BeginDialogAsync(nameof(GetAssistantDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.BeginDialogAsync(nameof(FinalDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }
    }
}