using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.AI.QnA;
using UniBotJG.Dialogs;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Schema;
using UniBotJG.CognitiveModels;
using UniBotJG.StateManagement;



namespace UniBotJG.Dialogs
{
    public class NoPermissionDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public NoPermissionDialog(LuisSetup luisRecognizer, ILogger<NoPermissionDialog> logger, UserState userState)
            : base(nameof(NoPermissionDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PreQnaAsync,
                QnaAsync,
                RetryEnd,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PreQnaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok, anyhting I can help you with?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> QnaAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            //var results = await UniBotQnA.GetAnswersAsync(stepContext);
            //var response = await qnaMaker.GetAnswersAsync(stepContext);

            // use answer found in qnaResults[0].answer
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(/*response[0].Answer*/"Test")}, cancellationToken);
        }

        private async Task<DialogTurnResult> RetryEnd(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }
        }
    }