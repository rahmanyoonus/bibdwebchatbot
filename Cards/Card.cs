using AdaptiveCards.Templating;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Cards
{
    public static class Card
    {
        public static HeroCard GenerateHeroCard(string title, string subtitle, List<CardAction> buttons, List<CardImage> images = null)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Images = images,
                Subtitle = subtitle,
                Buttons = buttons
            };
            return heroCard;
        }
        
        public static AdaptiveCardTemplate GetAdaptiveCardTemplate()
        {
            var adaptiveCardTemplateJson = File.ReadAllText(Path.Combine(".", "Cards", "cardAppointment.json"));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardTemplateJson);
            return template;
        }
         public static AdaptiveCardTemplate GetSelectOptionCardTemplate()
        {
            var adaptiveCardTemplateJson = File.ReadAllText(Path.Combine(".", "Cards", "selectOptionCard.json"));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCardTemplateJson);
            return template;
        }

    }
}
