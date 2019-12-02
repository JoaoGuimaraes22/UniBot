//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Threading;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Dialogs;
//using UniBotJG.Dialogs;
//using Microsoft.Bot.Builder.AI.Luis;
//using Microsoft.Extensions.Logging;
//using Microsoft.Bot.Schema;
//using UniBotJG.CognitiveModels;
//using UniBotJG.StateManagement;

//namespace UniBotJG.Dialogs
//{
//    public class FamilyServiceDialog : ComponentDialog
//    {
//        private readonly LuisSetup _recognizer;
//        protected readonly ILogger Logger;
//        private readonly UserState _userState;

//        public FamilyServiceDialog(LuisSetup luisRecognizer, ILogger<FamilyServiceDialog> logger, UserState userState, SuitCustomerNeedsDialog suitCustomerNeeds, LivesInPortugalDialog livesInPortugal)
//            : base(nameof(FamilyServiceDialog))
//        {
//            _recognizer = luisRecognizer;
//            _userState = userState;
//            Logger = logger;

//            //AddDialog(new MainDialog());
//            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
//            AddDialog(new TextPrompt(nameof(TextPrompt)));
//            AddDialog(suitCustomerNeeds);
//            AddDialog(livesInPortugal);

//            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
//            {
//                InitialStepAsync,
//            }));

//            InitialDialogId = nameof(WaterfallDialog);
//        }

//        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
//        {
//            return await stepContext.ReplaceDialogAsync(nameof(LivesInPortugalDialog), new PromptOptions { Prompt = MessageFactory.Text("Ok, I will need to gather some information in order to help you.\nDo you live in Portugal?") }, cancellationToken);
//            return await stepContext.ReplaceDialogAsync
//        }
//    }
//}

////private async Task<DialogTurnResult> LivesInPortugalAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
////{
////    if (!_recognizer.IsConfigured)
////    {
////        await stepContext.Context.SendActivityAsync(
////        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
////        return await stepContext.NextAsync(null, cancellationToken);
////    }
////    var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
////    if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
////    {
////        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you already have an account?") }, cancellationToken);
////    }
////    if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
////    {
////        var userProfile = new UserProfile
////        {
////            LivesInPortugal = false
////        };
////        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Do you already have an account?") }, cancellationToken);
////    }
////    else
////    {
////        return await stepContext.PromptAsync(nameof(FamilyServiceDialog), new PromptOptions { Prompt = MessageFactory.Text("Sorry I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);
////    }
////}


