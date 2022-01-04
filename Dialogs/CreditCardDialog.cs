using AdaptiveCards.Templating;
using CoreBot.Cards;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class CreditCardDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        private const string AppointmentInfo = "value-appointmentInfo";
        private readonly UserState UserState;
        private IBotServices BotServices;

        private const string CardStepMsgText = "We offer a range of credit cards to suit your needs. Please select the following cards to know more";

        public CreditCardDialog(ILogger<CreditCardDialog> logger, UserState userState, IBotServices botServices, IConfiguration configuration) : base(nameof(AccountRecommendDialog))
        {
            Logger = logger;
            UserState = userState;
            BotServices = botServices;

            AddDialog(new QnAMakerBaseDialog(botServices, configuration));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new UserInfoDialog());
            
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CardStepAsync,
                AskStepAsync,
                ExplainStepAsync,
                InfoStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("CreditCardDialog.CardStepAsync");
            stepContext.Values[AppointmentInfo] = new AppointmentDetail();
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the purpose of your account"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 2."),
                Choices = GetCardTypes(),
             
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }
        private async Task<DialogTurnResult> AskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("CreditCardDialog.AskStepAsync");
            var appointmentDetail = (AppointmentDetail)stepContext.Values[AppointmentInfo];
            appointmentDetail.Type = ((FoundChoice)stepContext.Result).Value ?? appointmentDetail.Type;
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What would you like to know?"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 4."),
                Choices = GetOptions(),
                Style = ListStyle.HeroCard
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }   
        
        private async Task<DialogTurnResult> ExplainStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("CreditCardDialog.ExplainStepAsync");
            return await stepContext.BeginDialogAsync(nameof(QnAMakerDialog), null, cancellationToken);

        }
        private async Task<DialogTurnResult> InfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Values[AppointmentInfo];
            if (!stepContext.Result.Equals("Aplly Now"))
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
                return await ExplainStepAsync(stepContext, cancellationToken);
            }
            return await stepContext.BeginDialogAsync(nameof(UserInfoDialog), appointmentDetail, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Result;
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            AdaptiveCardTemplate template = Card.GetAdaptiveCardTemplate();
            var card = template.Expand(appointmentDetail);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(card),
            };
            reply.Attachments.Add(adaptiveCardAttachment);
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            var accessor = UserState.CreateProperty<AppointmentDetail>(nameof(AppointmentDetail));
            await accessor.SetAsync(stepContext.Context, appointmentDetail, cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }


        private IList<Choice> GetCardTypes()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "World MasterCard", Synonyms = new List<string>() {"world" } },
                new Choice() { Value = "Virtual MasterCard", Synonyms = new List<string>() {"virtual" } }
            };


            return cardOptions;
        } 
        
        private IList<Choice> GetOptions()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Eligibity", Synonyms = new List<string>() { "eligibity", "eli" } },
                new Choice() { Value = "Fees & Tariffs", Synonyms = new List<string>() { "fees", "fee", "tariffs", "tariff" } },
                new Choice() { Value = "Benefits & privileges", Synonyms = new List<string>() { "privileges", "benefits" } },
                new Choice() { Value = "Apply Now", Synonyms = new List<string>() { "apply"} },
                new Choice() { Value = "Others", Synonyms = new List<string>() { "others"} },
            };

            return cardOptions;
        }
    }
}
