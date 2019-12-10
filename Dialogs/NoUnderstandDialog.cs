using System;
using System.Windows;
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
    public class NoUnderstandDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public NoUnderstandDialog(LuisSetup luisRecognizer, ILogger<NoUnderstandDialog> logger, UserState userState)
            : base(nameof(NoUnderstandDialog))
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
                GoToAssistantAsync
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetAssistantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions{Prompt = MessageFactory.Text("I’m sorry but I was not able to understand you or your request. In order to provide you with a better experience an employee will receive you as soon as possible​") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GoToAssistantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            //Application.Restart();
            //Environment.Exit(0);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}