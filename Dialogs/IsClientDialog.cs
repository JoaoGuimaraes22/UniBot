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
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Aks for NIF, if NIFPermissionwas given 
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. What is your Tax ID?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            var nifRegex = new Regex("^[0-9]+$");
            if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
            {
                //Check if NIF equals NIF Regex
                userProfile.NIF = stepContext.Result.ToString();
                return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
                //return await stepContext.NextAsync();
            }
            else
            {
                userProfile.NIF = "None";
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);

            }
        }
 

        private async Task<DialogTurnResult> ConfirmNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            var nifRegex = new Regex("^[0-9]+$");
            if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
            {
                userProfile.NIF = stepContext.Result.ToString();
                return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
            }
            else
            {
                userProfile.NIF = "None";
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);

            }
        }
    }
}