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
    public class SendContactInfoDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public SendContactInfoDialog(LuisSetup luisRecognizer, ILogger<SendContactInfoDialog> logger, UserState userState)
            : base(nameof(SendContactInfoDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                WhatInfoAsync,
                //ValidateAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> WhatInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
            if (userProfile.GavePermission)
            {
                if (userProfile.ChosePhone)
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We will not store your contact information. What is your phone number? ") }, cancellationToken);
                }
                else
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We will not store your contact information. What is your email adress? ") }, cancellationToken);
                }
            }
            else
            {
                if (userProfile.ChosePhone)
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We will store your contact information. What is your phone number? ") }, cancellationToken);
                }
                else
                {
                    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We will store your contact information. What is your email adress? ") }, cancellationToken);
                }
            }

        }
    }
}
        /*
        private async Task<DialogTurnResult> ValidateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if (userProfile.GavePermission)
            {
                userProfile.Phone = stepContext.
            }
        }
    }
}
*/