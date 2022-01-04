using AdaptiveCards.Templating;
using CoreBot.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class UserInfoDialog : ComponentDialog
    {
        private const string GetNameStepMsgText = "Set an appointment with us to proceed with the application. Please provide me your full name.";
        private const string GetPhoneStepMsgText = "May I help your phone number?";
        private const string GetEmailStepMsgText = "Next, may I have your email address?";
        private string GetBranchStepMsgText = "Perfect. Now, please select the branch would you like to go to";
        private const string GetDateStepMsgText = "Please state the date and time";
        private List<Branch> list;
        public UserInfoDialog() : base(nameof(UserInfoDialog))
        {
            list = new List<Branch>();
            list.Add(new Branch(1, "Main Branch"));
            list.Add(new Branch(2, "Second Branch"));
            list.Add(new Branch(3, "Third Branch"));
            list.Add(new Branch(4, "Four Branch"));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt("promptTextEmail", EmailPromptValidator));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            //AddDialog(new DateResolverDialog());
            AddDialog(new WaterfallDialog(nameof(UserInfoDialog), new WaterfallStep[]
            {
                GetNameStepAsync,
                GetPhoneStepAsync,
                GetEmailStepAsync,
                GetBranchStepAsync,
                GetDateStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(UserInfoDialog);
        }

        private async Task<DialogTurnResult> GetNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Options;
            var promptMessage = MessageFactory.Text(GetNameStepMsgText, GetNameStepMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetPhoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Options;
            appointmentDetail.FullName = (string)stepContext.Result;
            var promptMessage = MessageFactory.Text(GetPhoneStepMsgText, GetPhoneStepMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
        private async Task<DialogTurnResult> GetEmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Options;
            appointmentDetail.Phone = (string)stepContext.Result;
            var promptMessage = MessageFactory.Text(GetEmailStepMsgText, GetEmailStepMsgText, InputHints.ExpectingInput);
            var repromptMessage = MessageFactory.Text("Please input valid email", "Please input valid email", InputHints.ExpectingInput);
            return await stepContext.PromptAsync("promptTextEmail", new PromptOptions { Prompt = promptMessage, RetryPrompt = repromptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetBranchStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Options;
            appointmentDetail.Email = (string)stepContext.Result;
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            var listObj = new { list };
            AdaptiveCardTemplate template = Card.GetSelectOptionCardTemplate();
            var card = template.Expand(listObj);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(card)
            };
            reply.Attachments.Add(adaptiveCardAttachment);
            await stepContext.Context.SendActivityAsync(GetBranchStepMsgText);
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            //var promptMessage = MessageFactory.Text(GetBranchStepMsgText, GetBranchStepMsgText, InputHints.IgnoringInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("", inputHint: InputHints.IgnoringInput) }, cancellationToken);
        }
        private async Task<DialogTurnResult> GetDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var branchID = (string)stepContext.Result;
            Branch branch = list.Where(m => m.Id == branchID).FirstOrDefault();
            if (branch == null)
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 1;
                GetBranchStepMsgText = "Please choose a branch";
                return await GetBranchStepAsync(stepContext, cancellationToken);
            }
            var appointmentDetail = (AppointmentDetail)stepContext.Options;
            appointmentDetail.Branch = branch.Name;
            var promptMessage = MessageFactory.Text(GetDateStepMsgText, GetDateStepMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Options;
            appointmentDetail.DateTime = (string)stepContext.Result;
            return await stepContext.EndDialogAsync(appointmentDetail, cancellationToken);
        }

        private Task<bool> EmailPromptValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var validator = new EmailAddressAttribute();
                return Task.FromResult(validator.IsValid(promptContext.Recognized.Value));
            }

            return Task.FromResult(false);
        }

    }
}
