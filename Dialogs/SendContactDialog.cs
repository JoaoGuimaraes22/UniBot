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
    public class SendContactDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public SendContactDialog(LuisSetup luisRecognizer, ILogger<SendContactDialog> logger, UserState userState/*, GetHelpDialog getHelp*/)
            : base(nameof(SendContactDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            //AddDialog(getHelp);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SendAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SendAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = new UserProfile();
            if (userProfile.ChosePhone == true)
            {
                //Send SMS
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thank You. More information on the Special Account for emigrants was sent to your phone.") }, cancellationToken);
            }
            else
            {
                //Send email
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thank You. More information on the Special Account for emigrants was sent to your email adress.") }, cancellationToken);

            }

        }

        //private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    return await stepContext.BeginDialogAsync(nameof(GetHelpDialog), null, cancellationToken);
        //}
    }
}