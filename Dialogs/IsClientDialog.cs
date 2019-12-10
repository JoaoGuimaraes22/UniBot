﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UniBotJG.CognitiveModels;
using UniBotJG.StateManagement;

namespace UniBotJG.Dialogs
{
    //Asks what user Tax ID is
    public class IsClientDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public IsClientDialog(LuisSetup luisRecognizer, ILogger<IsClientDialog> logger, UserState userState, NoUnderstandDialog noUnderstand, GetHelpDialog getHelp, GoodbyeDialog goodbye)
            : base(nameof(IsClientDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noUnderstand);
            AddDialog(getHelp);
            AddDialog(goodbye);


            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskNIFAsync,
                GetNIFAsync,
                ConfirmNIFAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initial Prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. What is your Tax ID?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures NIF regex validator
            var nifRegex = new Regex("^[0-9]+$");

            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //Instantiates UserProfile storage
            var userProfile = new UserProfile();

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If NIF is validated / is equal to Regex
            if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
            {
                //Saves NIF to storage
                userProfile.NIF = stepContext.Result.ToString();
                return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
            }

            //Sets NIF in storage to "None" and retries
            else
            {
                userProfile.NIF = "None";
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Sorry, I didn’t understand you. Can you please repeat what you said?") }, cancellationToken);

            }
        }


        private async Task<DialogTurnResult> ConfirmNIFAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Configures NIF regex validator
            var nifRegex = new Regex("^[0-9]+$");

            //Configures LUIS
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);

            //Instantiates UserProfile storage
            var userProfile = new UserProfile();

            //If intent is exit/cancel
            if (luisResult.TopIntent().intent == LuisIntents.Intent.Exit)
            {
                return await stepContext.BeginDialogAsync(nameof(GoodbyeDialog), null, cancellationToken);
            }

            //If NIF is validated / is equal to Regex
            if (nifRegex.IsMatch(stepContext.Result.ToString()) && (stepContext.Result.ToString().Length == 9))
            {
                //Saves NIF to storage
                userProfile.NIF = stepContext.Result.ToString();
                return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
            }

            //Sets NIF storage to "None", goes to NoUnderstandDialog
            else
            {
                userProfile.NIF = "None";
                return await stepContext.BeginDialogAsync(nameof(NoUnderstandDialog), null, cancellationToken);

            }
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //For safety
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}