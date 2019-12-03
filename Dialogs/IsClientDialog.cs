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
using System.Text.RegularExpressions;

namespace UniBotJG.Dialogs
{
    public class IsClientDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public IsClientDialog(LuisSetup luisRecognizer, ILogger<IsClientDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, GetHelpDialog getHelp)
            : base(nameof(IsClientDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noUnderstand);
            AddDialog(getHelp);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskNIFAsync,
                GetNIFAsync,
                ConfirmNIFAsync,
                ReConfirmNIFAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if (!_recognizer.IsConfigured)
            //{
            //    await stepContext.Context.SendActivityAsync(
            //    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
            //    return await stepContext.NextAsync(null, cancellationToken);
            //}
            //var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            //if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("To give you a more accurate service  I will need your NIF. Can you please provide me that information?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            var nifRegex = new Regex("^[0-9]+$");
            if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
            {
                userProfile.NIF = stepContext.Result.ToString();
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt=MessageFactory.Text($"To confirm, your NIF is {userProfile.NIF}, right?")}, cancellationToken);
            }
            else
            {
                userProfile.NIF = "None";
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("I'm sorry, this is not a valid NIF. Can you please repeat what you said") }, cancellationToken);

            }
        }

        private async Task<DialogTurnResult> ConfirmNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            
                var userProfile = new UserProfile();
            if(userProfile.NIF != "None")
            {
                if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
                {
                    return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
                }
                else
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt=MessageFactory.Text("Ok, please re-enter the NIF")});
                }
            }
            else
            {
                var nifRegex = new Regex("^[0-9]+$");
                if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
                {
                    userProfile.NIF = stepContext.Result.ToString();
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"To confirm, your NIF is {userProfile.NIF}, right?") }, cancellationToken);
                }
                else
                {
                    userProfile.NIF = "None";
                    return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);

                }
            }
        }

        private async Task<DialogTurnResult> ReConfirmNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if( userProfile.NIF != "None")
            {
                var nifRegex = new Regex("^[0-9]+$");
                if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
                {
                    userProfile.NIF = stepContext.Result.ToString();
                    return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
                }
                else
                {
                    return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
                }
            }
            else
            {
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
            }

        }
    }
}