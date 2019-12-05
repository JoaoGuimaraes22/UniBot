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
    public class ReEnterNIFDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public ReEnterNIFDialog(LuisSetup luisRecognizer, ILogger<ReEnterNIFDialog> logger, UserState userState, GetHelpDialog getHelp, NoUnderstandDialog noUnderstand)
            : base(nameof(ReEnterNIFDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(getHelp);
            AddDialog(noUnderstand);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                StartNIFReAsync,
                CheckIfValidAsync,
                ConfirmAsync,
                EndRetriesAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> StartNIFReAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. Can you please re-enter your NIF.") }, cancellationToken);

        }

        private async Task<DialogTurnResult> CheckIfValidAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            
            var userProfile = new UserProfile();
            var nifRegex = new Regex("^[0-9]+$");
            if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
            {
                userProfile.NIF = stepContext.Result.ToString();
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"To confirm, your NIF is {userProfile.NIF}, right?") }, cancellationToken);
            }
            else
            {
                userProfile.NIF = "None";
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("I'm sorry, this is not a valid NIF. Can you please repeat what you said") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var userProfile = new UserProfile();
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (userProfile.NIF != "None")
            {
                if (luisResult.TopIntent().intent == LuisIntents.Intent.Yes)
                {
                    return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
                }
                if(luisResult.TopIntent().intent == LuisIntents.Intent.No)
                {
                    return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog));
                }
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("I'm sorry, this is not a valid NIF") }, cancellationToken);
            }
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("I'm sorry, this is not a valid NIF") }, cancellationToken);


        }

        private async Task<DialogTurnResult> EndRetriesAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);
        }
    }
}