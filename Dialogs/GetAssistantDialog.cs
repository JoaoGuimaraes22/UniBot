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

namespace UniBotJG.Dialogs
{
    public class GetAssistantDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public GetAssistantDialog(LuisSetup luisRecognizer, ILogger<GetAssistantDialog> logger, UserState userState)
            : base(nameof(GetAssistantDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetAssistantAsync,
                RestartAsync,
                GoToAssistantAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetAssistantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions{Prompt = MessageFactory.Text("I'm sorry, I can't understant what you are saying. I'll redirect you to a specialist if you need to. If you wish to restart the dialog, enter 'Yes'")}, cancellationToken);
        }
        private async Task<DialogTurnResult> RestartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GoToAssistantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }
}