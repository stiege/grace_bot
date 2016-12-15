using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Controllers;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

            var stateClient = _extendedActivity.GetStateClient();
            var userData = await stateClient.BotState.GetUserDataAsync(_extendedActivity.ChannelId, _extendedActivity.From.Id);
            var replying = userData.GetProperty<bool>("replying");

            if (replying != null&& replying)
            {
                await ProcessReplyAsync();
            }

            else if(_extendedActivity.Type == ActivityTypes.Message
                && await _filter.FilterAsync(_extendedActivity))
            {
                // Save activity to database
                switch(_extendedActivity.Text.Split(' ')[0])
                {
                    case "/get":
                        {
                            await RetrieveQuestionsAsync();
                            break;
                        }
                    case "/reply":
                        {
                            await ReplyToQuestionsAsync();
                            break;
                        }
                    default:
                        {
                            await DbController.AddOrUpdateActivityInDb(_extendedActivity as ExtendedActivity, ProcessStatus.Unprocessed);
                            await ProcessActivityAsync();
                            break;
                        }
                }

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

                                var reply = _extendedActivity.CreateReply(result);
                                await DbController.AddOrUpdateActivityInDb(new ExtendedActivity(reply as Activity), ProcessStatus.BotMessage);

                                var connector = new ConnectorClient(
                                new Uri(_extendedActivity.ServiceUrl));
                                await connector.Conversations.ReplyToActivityAsync(reply as Activity
                                );                                    

                                // Update activity in database , set ProcessStatus of this activity to BotReplied
                                await DbController.AddOrUpdateActivityInDb(_extendedActivity as ExtendedActivity, ProcessStatus.BotReplied);
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

        private async Task RetrieveQuestionsAsync()
        {
            var reply = (Activity)_extendedActivity.CreateReply("Unprocessed Questions:");
            reply.Recipient = _extendedActivity.From;
            reply.Type = "message";
            reply.Attachments = new List<Attachment>();

            var eas = DbController.FindUnprocessedQuestions();
            foreach (var ea in eas)
            {
                var cardButtons = new List<CardAction>();
                cardButtons.Add(new CardAction()
                {
                    Title = "Answer this question",
                    Type = "postBack",
                    Value = $"/reply {ea.Id}"
                });

                var card = new HeroCard()
                {
                    Subtitle = $"{ea.From.Name} asked at {ea.Timestamp}",
                    Text = $"{ea.Text}",
                    Buttons = cardButtons
                };
                reply.Attachments.Add(card.ToAttachment());
            }

            var connector = new ConnectorClient(new Uri(_extendedActivity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private async Task ReplyToQuestionsAsync()
        {

            var replyToActivity = DbController.FindExtendedActivity(_extendedActivity.Text.Split(' ')[1]);
            if (replyToActivity == null)
            {
                //do something to handle
            }
            var stateClient = _extendedActivity.GetStateClient();
            var userData = await stateClient.BotState.GetUserDataAsync(_extendedActivity.ChannelId, _extendedActivity.From.Id);
            userData.SetProperty("replying", true);
            userData.SetProperty("replyingToActivity", replyToActivity.Id);
            await stateClient.BotState.SetUserDataAsync(_extendedActivity.ChannelId, _extendedActivity.From.Id, userData);

            var markdown = $"You are answering ***{replyToActivity.From.Name}***'s question:\n";
            markdown += "***\n";
            markdown += $"{replyToActivity.Text}\n";
            markdown += "***\n";
            markdown += "**Please give your answer in the next reply.**\n";

            var connector = new ConnectorClient(new Uri(_extendedActivity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync((Activity)_extendedActivity.CreateReply(markdown));
        }

        private async Task ProcessReplyAsync()
        {
            // set the extendedActivity to save to a replied activity
            await DbController.AddOrUpdateActivityInDb(_extendedActivity as ExtendedActivity, ProcessStatus.BotReplied);

            var stateClient = _extendedActivity.GetStateClient();
            var userData = await stateClient.BotState.GetUserDataAsync(_extendedActivity.ChannelId, _extendedActivity.From.Id);
            userData.SetProperty("replying", false);
            await stateClient.BotState.SetUserDataAsync(_extendedActivity.ChannelId, _extendedActivity.From.Id, userData);

            var connector = new ConnectorClient(new Uri(_extendedActivity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync((Activity)_extendedActivity.CreateReply("Thanks, your answer has been received."));
        }

        private async Task SlackForwardAsync(string msg)
        {
            var client = _factory.GetHttpClient();
            var uri = Environment.GetEnvironmentVariable("WEBHOOK_URL");
            
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