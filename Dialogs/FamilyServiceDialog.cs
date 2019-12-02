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
    public class FamilyServiceDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public FamilyServiceDialog(LuisSetup luisRecognizer, ILogger<FamilyServiceDialog> logger, UserState userState, SuitCustomerNeedsDialog suitCustomerNeeds)
            : base(nameof(FamilyServiceDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(suitCustomerNeeds);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                InitialStepAsync,
                HasAccountAsync,
                ExtraInfoAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok, I will need to gather some information in order to help you.\nDo you live in Portugal?") }, cancellationToken);
        }

        //private async Task<DialogTurnResult> LivesInPortugalAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    if (!_recognizer.IsConfigured)
        //    {
        //        await stepContext.Context.SendActivityAsync(
        //        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
        //        return await stepContext.NextAsync(null, cancellationToken);
        //    }
        //    var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
        //    if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
        //    {
        //        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you already have an account?") }, cancellationToken);
        //    }
        //    if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
        //    {
        //        var userProfile = new UserProfile
        //        {
        //            LivesInPortugal = false
        //        };
        //        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you already have an account?") }, cancellationToken);
        //    }
        //    else
        //    {
        //        return await stepContext.PromptAsync(nameof(FamilyServiceDialog), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
        //    }
        //}

        private async Task<DialogTurnResult> HasAccountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if (((bool)stepContext.Result && userProfile.LivesInPortugal) || ((bool)stepContext.Result && (userProfile.LivesInPortugal == false)) || (((bool)stepContext.Result == false) && userProfile.LivesInPortugal))
            {
                return await stepContext.BeginDialogAsync(nameof(SuitCustomerNeedsDialog));
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We have an option that might suit your needs. The special account for emigrants is available for Portuguese emigrants that are over 18 years old and can be shared with your partner or son. Would you like to get more information?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ExtraInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            {
                //return await stepContext.BeginDialogAsync(nameof(AskForInfoDialog));
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Worked")}, cancellationToken);
            }
            if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
            {
                //return await stepContext.BeginDialogAsync(nameof(WantAssistandDialog));
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Worked") }, cancellationToken);
            }
            if (luisResult.TopIntent().intent == LuisIntents.Intent.MyAdvantages)
            {
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions { Prompt = MessageFactory.Text("This account gives you freedom to perform your operations in Portugal and overseas, flexibility to move it in the currency you desire and access to exclusive products.\nWould you like to get more information?")}, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(FamilyServiceDialog), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?")}, cancellationToken);
            }
        }
    }
}
