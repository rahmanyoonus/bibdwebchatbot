using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Cards
{
    public static class WelcomeCard
    {
        public static HeroCard GetPopularQuestions()
        {
            var heroCard = new HeroCard
            {
                Title = "Popular Questions",
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.ImBack, "How do I activate my debit card?", value: "Activate Debit Card"),
                    new CardAction(ActionTypes.ImBack, "How much am I etitled for financing?", value: "Etitled for financing"),
                    new CardAction(ActionTypes.ImBack, "How do I apply for MSME account?", value: "Apply for MSME account")
                },
            };

            return heroCard;
        }

        public static HeroCard GetHelps()
        {
            var heroCard = new HeroCard
            {
                Title = "Help",
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.ImBack, "How do I block my debit card", value: "Block my debit card"),
                    new CardAction(ActionTypes.ImBack, "My transaction failed when purchasing online. What do I do?", value: "Failed when purchasing online"),
                    new CardAction(ActionTypes.ImBack, "How to download e-Statement?", value: "Download e-Statement")
                },
            };

            return heroCard;
        }

        public static HeroCard GetDigitals()
        {
            var heroCard = new HeroCard
            {
                Title = "Digital Tutorial",
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.ImBack, "BIBD Wave", value: "BIBD Wave"),
                    new CardAction(ActionTypes.ImBack, "Personal QR", value: "Personal QR"),
                    new CardAction(ActionTypes.OpenUrl, "See more", value: "http://www.bibd.com.bn/personal/")
                },
            };

            return heroCard;
        }

        public static HeroCard GetForCustomers()
        {
            var heroCard = new HeroCard
            {
                Title = "For you",
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.OpenUrl, "View deals and offers", value: "http://www.bibd.com.bn/personal/"),
                    new CardAction(ActionTypes.ImBack, "View all promotions", value: "Promotions"),
                    new CardAction(ActionTypes.OpenUrl, "Vuew cards promotions", value: "http://www.bibd.com.bn/personal/")
                },
            };

            return heroCard;
        }

        public static HeroCard GetBusiness()
        {
            var heroCard = new HeroCard
            {
                Title = "Business",
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.ImBack, "We are a registered company without an account with BIBD. Are we able to apply for a Fleet Card?", value: "Apply for a Fleet Card"),
                    new CardAction(ActionTypes.ImBack, "What are the application criteria for corporate financing facilities?", value: "Application criteria for corporate financing facilities")
                },
            };

            return heroCard;
        }
        
        public static Attachment GetActivateCard(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

    }
}
