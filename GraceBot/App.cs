using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace GraceBot
{
    internal class App : IApp
    {
        private readonly IFactory _factory;
        private readonly IFilter _filter;
        private IExtendedActivity _extendedActivity;

        public App(IFactory factory)
        {
            _factory = factory;
            _filter = _factory.GetActivityFilter();
        }

        public async Task RunAsync(IExtendedActivity activity)
        {
            _extendedActivity = activity;
            if (_extendedActivity.Type == ActivityTypes.Message && await _filter.FilterAsync(_extendedActivity))
            {
                await ProcessActivityAsync();
            }
            else
            {
                HandleSystemMessage();
            }
        }

        private async Task ProcessActivityAsync()
        {
            var strEscaped = Uri.EscapeUriString(_extendedActivity.Text);
            var uri =
                "https://api.projectoxford.ai/luis/v2.0/apps/" + Environment.GetEnvironmentVariable("LUIS_ID") +
                "?subscription-key=" +
                Environment.GetEnvironmentVariable("LUIS_KEY") + "&q=" +
                strEscaped + "&verbose=true";
            using (var client = _factory.GetHttpClient())
            {
                var msg = await client.GetAsync(uri);
                if (msg.IsSuccessStatusCode)
                {
                    var response = JsonConvert.DeserializeObject<LuisResponse>(await msg.Content.ReadAsStringAsync());
                    if (response.topScoringIntent.intent == "GetDefinition")
                    {
                        foreach (Entity responseEntity in response.entities)
                        {
                            if (responseEntity.type == "subject")
                            {
                                await FindDefinitionForAsync(responseEntity.entity);
                            }
                        }
                    }
                }
            }
        }

        private void HandleSystemMessage()
        {
            switch (_extendedActivity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    // Implement user deletion here
                    // If we handle user deletion, return a real message
                    break;
                case ActivityTypes.ConversationUpdate:
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels
                    break;
                case ActivityTypes.ContactRelationUpdate:
                    // Handle add/remove from contact lists
                    // Activity.From + Activity.Action represent what happened
                    break;
                case ActivityTypes.Typing:
                    // Handle knowing tha the user is typing
                    break;
                case ActivityTypes.Ping:
                    break;
            }
        }

        private async Task FindDefinitionForAsync(string subject)
        {
            using (var reader =
                    new JsonTextReader(
                        new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\Words\\dictionary.json"))
            )
            {
                var definitions = new JsonSerializer().Deserialize<Dictionary<string, string>>(reader);
                using (var connector = new ConnectorClient(new Uri(_extendedActivity.ServiceUrl)))
                {
                    try
                    {
                        await connector.Conversations.ReplyToActivityAsync((Activity)_extendedActivity.CreateReply(definitions[subject.ToUpper()]));
                    }
                    catch (Exception)
                    {
                        await connector.Conversations.ReplyToActivityAsync((Activity)_extendedActivity.CreateReply(
                            "I don't know about that yet..."));
                    }
                }
            }
        }
    }
}