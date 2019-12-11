using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace UniBotJG.Dialogs
{
    //Dialog when it's time to say goodbye
    public class GoodbyeDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public GoodbyeDialog(LuisSetup luisRecognizer, ILogger<GoodbyeDialog> logger, UserState userState)
            : base(nameof(GoodbyeDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SayonaraAsync,
                EndingAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> SayonaraAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Intial prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok, thank you. If you need additional assistance you can contact our direct line or speak with an employee at one of our branches​") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EndingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //The dialog re-runs, MainDialog gets called, dialog restarts
            return await stepContext.ReplaceDialogAsync(nameof(MainDialog), null, cancellationToken);
        }
    }
}