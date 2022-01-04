// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreBot.Cards;
using CoreBot.Services;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples.Bots
{
    // This IBot implementation can run any type of Dialog. The use of type parameterization is to allows multiple different bots
    // to be run at different endpoints within the same project. This can be achieved by defining distinct Controller types
    // each with dependency on distinct IBot types, this way ASP Dependency Injection can glue everything together without ambiguity.
    // The ConversationState is used by the Dialog system. The UserState isn't, however, it might have been used in a Dialog implementation,
    // and the requirement is that all BotState objects are saved at the end of a turn.
    public class DialogBot<T> : ActivityHandler
        where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        private IBotServices _botServices;

        protected readonly IStatePropertyAccessor<DialogState> DialogStateProperty;
        public DialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<DialogBot<T>> logger, IBotServices botServices)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
            _botServices = botServices;
            DialogStateProperty = ConversationState.CreateProperty<DialogState>(nameof(DialogState));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity.");
            var dcContext = DialogStateProperty.GetAsync(turnContext, () => new DialogState(), cancellationToken).Result;
            
            if (dcContext.DialogStack.Count == 0)
            {
                var dc = new DialogContext(new DialogSet(), turnContext, dcContext);
                //// Top intent tell us which cognitive service to use.
                var allScores = await _botServices.Dispatch.RecognizeAsync(dc, (Activity)turnContext.Activity, cancellationToken);
                var topIntent = allScores.Intents.First().Key;

                // Next, we call the dispatcher with the top intent.
                await DispatchToTopIntentAsync(turnContext, topIntent, cancellationToken);
            }
            else
            {
                SetPostBackValue(ref turnContext);
                await Dialog.RunAsync(turnContext, DialogStateProperty, cancellationToken);

            }
           
        }


        private async Task DispatchToTopIntentAsync(ITurnContext<IMessageActivity> turnContext, string intent, CancellationToken cancellationToken)
        {
            switch (intent)
            {
                case "bibd-luis":
                    await ProcessLuisAsync(turnContext, cancellationToken);
                    break;
                case "bibd-qna":
                    await ProcessQnAAsync(turnContext, cancellationToken);
                    break;
                default:
                    Logger.LogInformation($"Dispatch unrecognized intent: {intent}.");
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Dispatch unrecognized intent: {intent}."), cancellationToken);
                    break;
            }
        }


        private async Task ProcessLuisAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("ProcessDebitCardAsync");

            // Retrieve LUIS result for HomeAutomation.
            var recognizerResult = await _botServices.LuisDebitCardRecognizer.RecognizeAsync(turnContext, cancellationToken);
            var result = recognizerResult.Properties["luisResult"] as LuisResult;

            var topIntent = result.TopScoringIntent.Intent;
            switch (topIntent)
            {
                case "DebitCard":
                default:
                    if (result.Entities.FirstOrDefault().Entity.Equals("activate"))
                    {
                        var cardAttachment = WelcomeCard.GetActivateCard(Path.Combine(".", "Cards", "activeDebitCard.json"));
                        //turnContext.Activity.Attachments = new List<Attachment>() { cardAttachment };
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Regconize top intent {topIntent}."), cancellationToken);
                        if (result.Entities.Count > 0)
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Text($"Regconize entities were found in the message:\n\n{string.Join("\n\n", result.Entities.Select(i => i.Entity))}"), cancellationToken);
                        }
                    }
                    break;
                case "AccountRecommend":
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    break;  
                case "CreditCard":
                    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                    break;
            }

        }

        private async Task ProcessQnAAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("ProcessQnAAsync");

            var results = await _botServices.QnAMakerService.GetAnswersAsync(turnContext);
            if (results.Any())
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(results.First().Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
            }
        }

        private void SetPostBackValue(ref ITurnContext<IMessageActivity> turnContext)
        {
            var token = JToken.Parse(turnContext.Activity.ChannelData.ToString());
            if (token["postBack"] != null && Convert.ToBoolean(token["postBack"]))
            {
                JToken commandToken = JToken.Parse(turnContext.Activity.Value.ToString());
                string command = commandToken["action"].ToString();

                if (command.ToLowerInvariant() == "choose")
                {
                    turnContext.Activity.Text = commandToken["selectOption"].ToString();
                }
            }
        }
    }
}
