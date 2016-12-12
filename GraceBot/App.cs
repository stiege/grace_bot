using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace GraceBot
{
    internal class App : IApp
    {
        private readonly IFactory _factory;
        private readonly IFilter _filter;
        private readonly IDefinition _definition;
        private IExtendedActivity _extendedActivity;

        public App(IFactory factory)
        {
            _factory = factory;
            _filter = _factory.GetActivityFilter();
            _definition = _factory.GetActivityDefinition();
        }

        public async Task RunAsync(IExtendedActivity activity)
        {
            _extendedActivity = activity;
            if (_extendedActivity.Type == ActivityTypes.Message
                && await _filter.FilterAsync(_extendedActivity))
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
                    var response = JsonConvert.DeserializeObject<LuisResponse>(
                        await msg.Content.ReadAsStringAsync());
                    switch (response.topScoringIntent.intent)
                    {
                        case "GetDefinition":
                        {
                            foreach (var responseEntity in response.entities.Where(e => e.type == "subject"))
                            {
                                var result = _definition.FindDefinition(
                                                responseEntity.entity);
                                if(result == null)
                                {
                                    goto default;
                                }

                                var connector = new ConnectorClient(
                                    new Uri(_extendedActivity.ServiceUrl));
                                await connector.Conversations.ReplyToActivityAsync(
                                    (Activity)_extendedActivity.CreateReply(result
                                        ));
                            }
                            break;
                        }

                        default:
                        {
                            await SlackForwardAsync(_extendedActivity.Text);
                            break;
                        }
                    }
                }
            }
        }

        private async Task SlackForwardAsync(string msg)
        {
            var client = _factory.GetHttpClient();
            var uri = "https://hooks.slack.com/services/" + Environment.GetEnvironmentVariable("SLACK_WEBHOOK");
            
            var response = await client.PostMessageAsync(uri, new Payload()
            {
                Text = msg,
                Channel = "#5-grace-questions",
                Username = "GraceBot_UserEnquiry",
            });

            var reply = "Sorry, we currently don't have an answer for your question";
            if (response.IsSuccessStatusCode)
            {
                reply += "Your question has been sent to OMGTech! team, we will get back to you ASAP.";                
            }

            var connector = new ConnectorClient(new Uri(_extendedActivity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync((Activity)_extendedActivity.CreateReply(reply));
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
    }
}