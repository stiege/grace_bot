using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace GraceBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message && await filter(activity))
            {
                await doResponse(activity);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<bool> filter(Activity activity)
        {
            foreach (var badWord in File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "\\BadWords\\en"))
            {
                if (activity.Text.ToLower().Contains(badWord.ToLower()))
                {
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(activity.CreateReply("..."));
                    return false;
                }
            }
            return true;
        }

        private static async Task doResponse(Activity activity)
        {
            string strEscaped = Uri.EscapeUriString(activity.Text);
            string uri =
                "https://api.projectoxford.ai/luis/v2.0/apps/" + Environment.GetEnvironmentVariable("LUIS_ID") +
                "?subscription-key=" +
                Environment.GetEnvironmentVariable("LUIS_KEY") + "&q=" +
                strEscaped + "&verbose=true";
            using (var client = new HttpClient())
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
                                await findDefinitionFor(responseEntity.entity, activity);
                            }
                        }
                    }
                }
            }
        }

        private static async Task findDefinitionFor(string subject, Activity activity)
        {
            using (var reader =
                    new JsonTextReader(
                        new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "\\Words\\dictionary.json"))
            )
            {
                var definitions = new JsonSerializer().Deserialize<Dictionary<string, string>>(reader);
                using (var connector = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    try
                    {
                        await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(definitions[subject.ToUpper()]));
                    }
                    catch (Exception)
                    {
                        await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(
                            "I don't know about that yet..."));
                    }
                    
                }
            }
        }

        private static void log(string val)
        {
            System.Diagnostics.Debug.WriteLine(val);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}