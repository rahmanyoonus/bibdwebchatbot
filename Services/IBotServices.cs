//using Microsoft.Bot.Builder.AI.Orchestrator;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Orchestrator;

namespace CoreBot.Services
{
    public interface IBotServices
    {
        QnAMaker QnAMakerService { get; }
        LuisRecognizer LuisDebitCardRecognizer { get; }
        OrchestratorRecognizer Dispatch { get; }
    }
}
