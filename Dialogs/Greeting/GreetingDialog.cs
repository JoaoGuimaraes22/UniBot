using Microsoft.Bot.Builder; 
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniBotJG.StateManagement;
using System.Threading;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Bot.Builder.Dialogs.Choices;
//using UniBotJG.Dialogs.MainDialog;
using UniBotJG.Dialogs;

namespace UniBotJG.Dialogs.Greeting
{
    public class GreetingDialog : ComponentDialog
    {
        //Acesses UserProfile class
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly IStatePropertyAccessor<ConversationData>_conversationDataAcessor;
        public GreetingDialog(UserState userState, ConversationState conversationState)
            : base(nameof(GreetingDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile") ?? throw new ArgumentNullException(nameof(_userProfileAccessor));
            _conversationDataAcessor = conversationState.CreateProperty<ConversationData>("ConversationData") ?? throw new ArgumentNullException(nameof(_conversationDataAcessor));
            //Creates an array of a dialog set
            var waterfallSteps = new WaterfallStep[]
            {
                GetToKnowYouAsync,
                GetToKnowYouPermissionAsync,
                NameStepAsync,
                NameConfirmStepAsync,
                AgeStepAsync,
                AgeConfirmStepAsync,
                SummaryStepAsync,
                NextStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            // AddDialog(new AcessAccount());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>))); /*AgePromptValidatorAsync*/
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            

            //Initial child Dialog to run 
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> GetToKnowYouAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string askPermission = "To provide a better service, we would like it if you could provide us with your name and age.\nWould you like to give this info?";
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text($"{askPermission}") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetToKnowYouPermissionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Gets bool True if Yes and bool False if No from GetToKnowYouAsync
            //Yes
            if ((bool)stepContext.Result)
            {
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                userProfile.GavePermission = true;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok, thanks.\nPlease enter your name.") }, cancellationToken);
            }
            //No
            //Do later
            // separate into more watterfall dialogs || retry prompts
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok then, anything I can help?")}, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Saves name to storage
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Name = (string)stepContext.Result;

            //Confirms name
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Thanks, to confirm, your name is {userProfile.Name}, right?") });
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Ask for age after name confirmation
            //Yes
            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = MessageFactory.Text("Ok, thanks.\nNow, please enter your age.") });
            }
            //No
            //Do later 
            // separate into more watterfall dialogs || retry prompts
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok then, anything I can help?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Saves name to storage
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Age = (int)stepContext.Result;

            //Confirms name
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Thank you!\nTo confirm, you're {userProfile.Age} years old, right?") });
        }

        private async Task<DialogTurnResult> AgeConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Ask for age after name confirmation
            //Yes
            if ((bool)stepContext.Result)
            {
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Ok, thanks.\nSo you're name is {userProfile.Name}, and you're {userProfile.Age} years old. Is this information correct?") });
            }
            //No
            //Do later 
            // separate into more watterfall dialogs || retry prompts
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok then, anything I can help") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //Ask for age after name confirmation
            //Yes
            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text($"Alright! How can I help?") });
            }
            //No
            //Do later 
            // separate into more watterfall dialogs || retry prompts
            else
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Ok then, anything I can help?") }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> NextStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //TODO:
            //Call LUIS to check intent
            //See what customer wants
            //For now just to the 'FamillyMemberAcessAccount' intent

            // ***** True ***** return await stepContext.BeginDialogAsync(nameof(AcessAccount), null, cancellationToken)

            //For Testing
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }



    }
}
