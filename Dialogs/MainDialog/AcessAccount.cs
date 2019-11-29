/*
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
using UniBotJG.Dialogs.Greeting;

namespace UniBotJG.Dialogs.MainDialog
{
    public class AcessAccount : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAcessor;

        public AcessAccounts(UserState userState)
            : base(nameof(AcessAccount))
        {
            _userProfileAcessor = userState.CreateProperty<UserProfile>("UserProfile") ?? throw new ArgumentNullException(nameof(_userProfileAcessor));

            //Creates an array of a dialog set
            var waterfallSteps = new WaterfallStep[]
            {
                HowCanIHelpAsync,
                HaveAnAccountAsync,
                HaveAnAccountConfirmAsync,
                SuggestionsAsync
            };

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> HowCanIHelpAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext;
        }

        private async Task<DialogTurnResult> HaveAnAccountAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext;
        }

        private async Task<DialogTurnResult> HaveAnAccountConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext;
        }

        private async Task<DialogTurnResult> SuggestionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext;
        }
    }
}

*/