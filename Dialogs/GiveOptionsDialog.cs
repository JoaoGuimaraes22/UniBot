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
    public class GiveOptionsDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public GiveOptionsDialog(LuisSetup luisRecognizer, ILogger<GiveOptionsDialog> logger, UserState userState, WhereToReceiveDialog whereToReceive, NoUnderstandDialog noUnderstand, InfoSendDialog infoSend, AdvantagesDialog advantages)
            : base(nameof(GiveOptionsDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(whereToReceive);
            AddDialog(noUnderstand);
            AddDialog(infoSend);
            AddDialog(advantages);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetMoreInfo,
                CheckMoreInfo,
                RetryCheckMoreInfo,
                EndCheckMoreInfo,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> GetMoreInfo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We have an option that might suit your needs. The special account for emigrants is available for Portuguese emigrants that are over 18 years old and can be shared with your partner or son. Would you like to get more information?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckMoreInfo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            var userProfile = new UserProfile();
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(InfoSendDialog), null, cancellationToken);
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to be assisted by an employee that could provide a better experience?") });
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.MyAdvantages)
            {
                return await stepContext.BeginDialogAsync(nameof(AdvantagesDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> RetryCheckMoreInfo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            var userProfile = new UserProfile();
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                return await stepContext.BeginDialogAsync(nameof(WhereToReceiveDialog), null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. Thank you. If you need additional assistance you can contact our direct line or speak with an employee at one of our branches") });
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> EndCheckMoreInfo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }

   
}
