using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace UniBotJG.Dialogs
{
    //Dialog when bot doesn't undestand x2
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
            //Initial prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("I’m sorry but I was not able to understand you or your request. In order to provide you with a better experience an employee will receive you as soon as possible​") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GoToAssistantAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Restarts dialog, goes to MainDialog
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
        }
    }
}