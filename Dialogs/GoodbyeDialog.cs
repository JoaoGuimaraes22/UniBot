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
    public class GoodbyeDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public GoodbyeDialog(LuisSetup luisRecognizer, ILogger<GoodbyeDialog> logger, UserState userState)
            : base(nameof(GoodbyeDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SayonaraAsync,
                EndingAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SayonaraAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (!_recognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
            //    return await stepContext.NextAsync(null, cancellationToken);
            //}
            //var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            //if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Isn't client") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }
}