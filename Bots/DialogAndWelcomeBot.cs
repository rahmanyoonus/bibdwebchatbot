// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Cards;
using CoreBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T>
        where T : Dialog
    {
        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IBotServices botServices)
            : base(conversationState, userState, dialog, logger, botServices)
        {
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                // To learn more about Adaptive Cards, see https://aka.ms/msbot-adaptivecards for more details.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello. I am Arif, your virtual assistant. How can I help you today?"), cancellationToken);
                    var welcomeCard = GetWelComeCards();
                    var cardResponse = MessageFactory.Attachment(welcomeCard, ssml: "Welcome to Bot Framework!");
                    cardResponse.AttachmentLayout = "carousel";
                    await turnContext.SendActivityAsync(cardResponse, cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text("To see all the topics I can help you with simply tap the menu"), cancellationToken);
                    // Run the Dialog with the new message Activity.
                    //await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }

        // Load attachment from embedded resource.
        private List<Attachment> GetWelComeCards()
        {
            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);
            attachments.Add(WelcomeCard.GetPopularQuestions().ToAttachment());
            attachments.Add(WelcomeCard.GetHelps().ToAttachment());
            attachments.Add(WelcomeCard.GetForCustomers().ToAttachment());
            attachments.Add(WelcomeCard.GetDigitals().ToAttachment());
            attachments.Add(WelcomeCard.GetBusiness().ToAttachment());
            reply.AttachmentLayout = "carousel";
            return attachments;
        }
    }
}
