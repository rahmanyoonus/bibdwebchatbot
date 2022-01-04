using AdaptiveCards.Templating;
using CoreBot.Cards;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
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
    public class AccountRecommendDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        private const string AppointmentInfo = "value-appointmentInfo";
        private readonly UserState UserState;
        private IBotServices BotServices;
        
        public AccountRecommendDialog(ILogger<AccountRecommendDialog> logger, UserState userState, IBotServices botServices) : base(nameof(AccountRecommendDialog))
        {
            Logger = logger;
            UserState = userState;
            BotServices = botServices;
           
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            //AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new UserInfoDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                PurposeStepAsync,
                RecommendStepAsync,
                ExplainStepAsync,
                InfoStepAsync,
                FinalStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PurposeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("AccountRecommendDialog.PurposeStepAsync");
            stepContext.Values[AppointmentInfo] = new AppointmentDetail();
            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("Please select the purpose of your account"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 6."),
                Choices = GetCardTypes()
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> RecommendStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("AccountRecommendDialog.ShowCardStepAsync");
            var appointmentDetail = (AppointmentDetail)stepContext.Values[AppointmentInfo];
            appointmentDetail.Purpose = ((FoundChoice)stepContext.Result).Value;
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Decide which type of card(s) we are going to show the user
            switch (((FoundChoice)stepContext.Result).Value)
            {
                case "Savings":
                    reply.Attachments.Add(Card.GenerateHeroCard("General Saver", "An easy access to your money for a day to day activies", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "General Saver"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Easy Saver", "A flexibility in allocating your funds with higher returns",
                        new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Easy Saver"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Purpose Saver", "Achive your goals much easier with disciplined saving", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Purpose Saver"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Haj Saver", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Haj Saver"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    break;

                case "Earn higher interest":
                    reply.Attachments.Add(Card.GenerateHeroCard("Term Deposit BND", "An easy access to your money for a day to day activies", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Term Deposit BND"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Term Deposit Foreign", "A flexibility in allocating your funds with higher returns",
                        new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Term Deposit Foreign"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Term Deposit Foreignr", "Achive your goals much easier with disciplined saving", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Term Deposit Foreign"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Term Deposit Foreign (Corporate)", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Term Deposit Foreign (Corporate)"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment()); 
                    
                    reply.Attachments.Add(Card.GenerateHeroCard("BIBD Aspirasi", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "BIBD Aspirasi"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    break;

                case "Cheque Facilities":
                    reply.Attachments.Add(Card.GenerateHeroCard("General Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "General Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Tiered Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Tiered Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    break;

                case "Small business":
                    reply.Attachments.Add(Card.GenerateHeroCard("MSME Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "MSME Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    break;
                case "Payroll":
                    reply.Attachments.Add(Card.GenerateHeroCard("Tiered Corporate Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Tiered Corporate Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    
                    reply.Attachments.Add(Card.GenerateHeroCard("General Corporate Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "General Corporate Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    break;
                default:
                    reply.Attachments.Add(Card.GenerateHeroCard("Current Foreign Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Current Foreign Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Savings Foreign Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Savings Foreign Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());

                    reply.Attachments.Add(Card.GenerateHeroCard("Corporate Foreign Current Account", "Aimed at assisting you in fulfilling your ambition of performing Haj or Umrah", new List<CardAction>()
                    {
                          new CardAction(ActionTypes.OpenUrl, "Learn more", value: "http://www.bibd.com.bn/personal/"),
                          new CardAction(ActionTypes.ImBack, "Apply", value: "Corporate Foreign Current Account"),
                    }, new List<CardImage>()
                    {
                        new CardImage(url: "https://i.ibb.co/4smCCMV/img-thumb-1.jpg")
                    }).ToAttachment());
                    break;
            }

            reply.AttachmentLayout = "carousel";
            // Send the card(s) to the user as an attachment to the activity
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions() { Prompt = MessageFactory.Text("") }, cancellationToken);
            
        }
        private async Task<DialogTurnResult> ExplainStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("AccountRecommendDialog.ExplainStepAsync");
            var appointmentDetail = (AppointmentDetail)stepContext.Values[AppointmentInfo];
            appointmentDetail.Type = (string)stepContext.Result;
            // Create the PromptOptions which contain the prompt and re-prompt messages.
            // PromptOptions also contains the list of choices available to the user.
            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text("What would you like to know?"),
                RetryPrompt = MessageFactory.Text("That was not a valid choice, please select a card or number from 1 to 7."),
                Choices = GetOptions(),
            };

            // Prompt the user with the configured PromptOptions.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }
        private async Task<DialogTurnResult> InfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var appointmentDetail = (AppointmentDetail)stepContext.Values[AppointmentInfo];
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
                new Choice() { Value = "Savings", Synonyms = new List<string>() {"save" } },
                new Choice() { Value = "Earn higher interest", Synonyms = new List<string>() {"earn" } },
                new Choice() { Value = "Cheque Facilities", Synonyms = new List<string>() {"cheque" } },
                new Choice() { Value = "Small business", Synonyms = new List<string>() {"business" } },
                new Choice() { Value = "Flexible remittances", Synonyms = new List<string>() {"facility"} },
                new Choice() { Value = "Payroll"},
            };


            return cardOptions;
        } 
        
        private IList<Choice> GetOptions()
        {
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Eligibity", Synonyms = new List<string>() { "eligibity", "eli" } },
                new Choice() { Value = "Requirements", Synonyms = new List<string>() { "requirements", "req", "require" } },
                new Choice() { Value = "Rates", Synonyms = new List<string>() { "rates"} },
                new Choice() { Value = "Features & benefits", Synonyms = new List<string>() {"features", "benefits" } },
                new Choice() { Value = "Shariah Concept", Synonyms = new List<string>() {"shariah", "concept" } },
                new Choice() { Value = "Flexible remittances", Synonyms = new List<string>() {"facility" } },
                new Choice() { Value = "See previous", Synonyms = new List<string>() {"previous", "back" } },
            };

            return cardOptions;
        }
    }
}
