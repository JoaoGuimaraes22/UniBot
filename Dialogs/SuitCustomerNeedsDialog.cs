using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;


namespace UniBotJG.Dialogs
{
    //If there are better options for user, this dialog 
    public class SuitCustomerNeedsDialog : ComponentDialog
    {
        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public SuitCustomerNeedsDialog(LuisSetup luisRecognizer, ILogger<SuitCustomerNeedsDialog> logger, UserState userState, NoPermissionDialog noPermission)
            : base(nameof(SuitCustomerNeedsDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            //AddDialog(new MainDialog());
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(noPermission);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetEmployeeAsync,
                GoToEmployeeAsync,
                EndAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Initial prompt
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok. We might have different options that will suit your needs. In order to provide you with a better experience an employee will receive you soon.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GoToEmployeeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Goes to NoPermissionDialog
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }

        private async Task<DialogTurnResult> EndAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //For safety
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
