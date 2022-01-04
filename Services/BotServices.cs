using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Orchestrator;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;

namespace CoreBot.Services
{
    public class BotServices : IBotServices
    {
        public QnAMaker QnAMakerService { get; private set; }
        public LuisRecognizer LuisDebitCardRecognizer { get; private set; }
        public OrchestratorRecognizer Dispatch { get; private set; }
        public BotServices(IConfiguration configuration, OrchestratorRecognizer dispatcher)
        {
            QnAMakerService = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
                Host = GetHostname(configuration["QnAEndpointHostName"]),
                EndpointKey = GetEndpointKey(configuration)
            });
            LuisDebitCardRecognizer = CreateLuisRecognizer(configuration, "LuisWeatherAppId");
            Dispatch = dispatcher;
        }

        private static string GetHostname(string hostname)
        {
            if (!hostname.StartsWith("https://"))
            {
                hostname = string.Concat("https://", hostname);
            }

            if (!hostname.Contains("/v5.0") && !hostname.EndsWith("/qnamaker"))
            {
                hostname = string.Concat(hostname, "/qnamaker");
            }

            return hostname;
        }

        private static string GetEndpointKey(IConfiguration configuration)
        {
            var endpointKey = configuration["QnAEndpointKey"];

            if (string.IsNullOrWhiteSpace(endpointKey))
            {
                // This features sample is copied as is for "azure bot service" default "createbot" template.
                // Post this sample change merged into "azure bot service" template repo, "Azure Bot Service"
                // will make the web app config change to use "QnAEndpointKey".But, the the old "QnAAuthkey"
                // required for backward compact. This is a requirement from docs to keep app setting name
                // consistent with "QnAEndpointKey". This is tracked in Github issue:
                // https://github.com/microsoft/BotBuilder-Samples/issues/2532

                endpointKey = configuration["QnAAuthKey"];
            }

            return endpointKey;

        }
        private LuisRecognizer CreateLuisRecognizer(IConfiguration configuration, string appIdKey = "")
        {
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                "https://" + configuration["LuisAPIHostName"]);

            // Set the recognizer options depending on which endpoint version you want to use.
            // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
            var recognizerOptions = new LuisRecognizerOptionsV2(luisApplication)
            {
                IncludeAPIResults = true,
                PredictionOptions = new LuisPredictionOptions()
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                }
            };

            return new LuisRecognizer(recognizerOptions);
        }

    }
}
