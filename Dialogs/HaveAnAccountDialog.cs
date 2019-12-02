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
    public class HaveAnAccountDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public HaveAnAccountDialog(LuisSetup luisRecognizer, ILogger<HaveAnAccountDialog> logger, UserState userState)
            : base(nameof(HaveAnAccountDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskForAccountAsync,
                HasAccountAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> AskForAccountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you already have an account? ") }, cancellationToken);
        }

        private async Task<DialogTurnResult> HasAccountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            var userProfile = new UserProfile();
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            if ((luisResult.TopIntent().intent == LuisIntents.Intent.Yes && userProfile.LivesInPortugal) || ((luisResult.TopIntent().intent == LuisIntents.Intent.Yes) && (userProfile.LivesInPortugal == false)) || ((luisResult.TopIntent().intent == LuisIntents.Intent.No) && (userProfile.LivesInPortugal)))
            {
                return await stepContext.BeginDialogAsync(nameof(SuitCustomerNeedsDialog));
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(GiveOptionsDialog), null, cancellationToken);
            }
        }
    }
}