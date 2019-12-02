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
    public class MainDialog : ComponentDialog
    {

        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;

        public MainDialog(LuisSetup luisRecognizer, ILogger<MainDialog> logger, UserState userState, FamilyServiceDialog familyServiceDialog, GetAssistantDialog getAssistantDialog)
            : base(nameof(MainDialog))
        {
            _recognizer = luisRecognizer;
            _userState = userState;
            Logger = logger;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(familyServiceDialog);
            AddDialog(getAssistantDialog);

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                IntroStepAsync,
                InitialIntentAsync,
                //FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.


            var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> InitialIntentAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_recognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                var luisResult = await _recognizer.RecognizeAsync<LuisIntents>(stepContext.Context, cancellationToken);
                switch (luisResult.TopIntent().intent)
                {
                    case LuisIntents.Intent.ServiceToShareWithFamily:
                        await stepContext.BeginDialogAsync(nameof(FamilyServiceDialog), null, cancellationToken);
                        break;
                    default:
                        int promptNumber = 0;
                        if (promptNumber == 0)
                        {
                            promptNumber++;
                            await stepContext.ReplaceDialogAsync(nameof(MainDialog), new PromptOptions { Prompt = MessageFactory.Text("I'm sorry, I didn't understand that. Could you please say it in a another way?") }, cancellationToken);
                            break;
                        }
                        else
                        {
                            await stepContext.BeginDialogAsync(nameof(GetAssistantDialog), null, cancellationToken);
                            break;
                        }
                }
                return await stepContext.BeginDialogAsync(nameof(GetAssistantDialog), null, cancellationToken);
            }

        }
    }

    
}
