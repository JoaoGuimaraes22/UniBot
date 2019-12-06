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
using Twilio;
using MailKit;
using System.Management.Automation.Runspaces;
using System.ComponentModel;
using MailKit.Net.Smtp;
using MimeKit;
using System.Data;
using System.Drawing;
using System.Text;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace UniBotJG.Dialogs
{
    public class SendContactDialog : ComponentDialog
    {

        private readonly LuisSetup _recognizer;
        protected readonly ILogger Logger;
        private readonly UserState _userState;


        public SendContactDialog(LuisSetup luisRecognizer, ILogger<SendContactDialog> logger, UserState userState)
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
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SendAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
          
            var userProfile = new UserProfile();
            if (userProfile.ChoseEmail == true)
            {
                /*
                //Send email
                //Send email
                SmtpClient client = new SmtpClient();
                client.Timeout = 10000;
                client.AuthenticationMechanisms.Remove("XOAUTH2");

                BodyBuilder builder = new BodyBuilder();
                builder.HtmlBody = "TestHtmlBody";

                //Define the mail headers
                MimeMessage mail = new MimeMessage();
                mail.Subject = "TestSubject";
                mail.Body = builder.ToMessageBody();
                mail.From.Add(new MailboxAddress("sebastiao.guimaraes22@gmail.com"));
                mail.To.Add(new MailboxAddress("sebastiao.guimaraes22@gmail.com"));
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 465, false);
                client.Authenticate("sebastiao.guimaraes22@gmail.com", "tiaotiao22");
                client.Send(mail);
                client.Disconnect(true);
                */
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thank You. More information on the Special Account for emigrants was sent to your email adress. Is there anything helse I can help you with?") }, cancellationToken);
            }
            else
            {

                //Send SMS

                const string accountSid = "AC59c88e4b3c40aabb389d6d6b6d42d237";
                const string authToken = "4abd5296a1ab492e18c83c1a51a9e53c";
                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    body: "If you are reading this, it works",
                    from: new PhoneNumber("+14109284731"),
                    to: new PhoneNumber($"+351 915 109 181")
                );
                
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Thank You. More information on the Special Account for emigrants was sent to your phone. Is there anything helse I can help you with?") }, cancellationToken);

            }

        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(NoPermissionDialog), null, cancellationToken);
        }
    }
}