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
    public class AdvantagesDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public AdvantagesDialog(LuisSetup luisRecognizer, ILogger<AdvantagesDialog> logger, UserState userState, InfoSendDialog infoSend, WhereToReceiveDialog whereToReceive, NoUnderstandDialog noUnderstand)
            : base(nameof(AdvantagesDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(infoSend);
            AddDialog(whereToReceive);
            AddDialog(noUnderstand);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AdvantagesAsync,
                AnswerAdvantagesAsync,
                FinalAdvantagesAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AdvantagesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("This account gives you freedom to perform your operations in Portugal and overseas; Flexibility to move it in the currency you desire and access to exclusive products Would you like to get more information? ")}, cancellationToken);
        }

        private async Task<DialogTurnResult> AnswerAdvantagesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
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
            if(luisResult.TopIntent().intent == LuisIntents.Intent.No){
                userProfile.HelpMe = true;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to be assisted by an employee that could provide a better experience?")}, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalAdvantagesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }

            var userProfile = new UserProfile();
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if(userProfile.HelpMe == true)
            {
                if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
                {
                    return await stepContext.BeginDialogAsync(nameof(WhereToReceiveDialog), null, cancellationToken);
                }
                if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. Thank you. If you need additional assistance you can contact our direct line or speak with an employee at one of our branches") }, cancellationToken);
                }
            }
            else
            {
                if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
                {
                    return await stepContext.BeginDialogAsync(nameof(InfoSendDialog), null, cancellationToken);
                }
                if (luisResult.TopIntent().intent == LuisIntents.Intent.No)
                {
                    userProfile.HelpMe = true;
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Would you like to be assisted by an employee that could provide a better experience?") }, cancellationToken);
                }
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog) , null, cancellationToken);
                
            }
            return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);

        }
    }
}