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
    public class GetPhoneDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public GetPhoneDialog(LuisSetup luisRecognizer, ILogger<GetPhoneDialog> logger, UserState userState, FinalDialog finalDialog)
            : base(nameof(GetPhoneDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(finalDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PleaseSendAsync,
                FinalAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PleaseSendAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (!_recognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
            //    return await stepContext.NextAsync(null, cancellationToken);
            //}
            //var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            //if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. In order to receive information about the Special Account for Emigrants you should send a text message saying “Emigrant” to the following number 151015. Say anything if you want to continue this conversation.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(FinalDialog), null, cancellationToken);
        }
    }
}