using System;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GraceBot
{
    internal class App : IApp
    {
        private readonly IFactory _factory;
        private readonly IFilter _filter;
        private readonly IDefinition _definition;
        private readonly IDbManager _dbManager;

        private Activity _activity;

        // constructor 
        public App(IFactory factory)
        {
            _factory = factory;
            _filter = _factory.GetActivityFilter();
            _definition = _factory.GetActivityDefinition();
            _dbManager = _factory.GetDbManager();
        }


        // Determine activity (message) type and process accordingly as an asynchronous operation.
        public async Task RunAsync(Activity activity)
        {
            // check Activity Type
            _activity = activity;

            if(_activity.Type != ActivityTypes.Message)
            {
                await HandleSystemMessage();
                return;
            }

            // check state
            var replying = await _factory.GetUserDataPropertyAsync<bool>("replying", _activity);
            if (replying)
            {
                await ProcessReplyAsync();
                return;
            }

            if(await _filter.FilterAsync(_activity))
            {
                // Save activity to database
                switch(_activity.Text.Split(' ')[0])
                {
                    case "/get":
                        {
                            await RetrieveQuestionsAsync();
                            break;
                        }
                    case "/replyActivity":
                        {
                            await ReplyToQuestionsAsync();
                            break;
                        }
                    default:
                        {
                            await ProcessActivityAsync();
                            break;
                        }
                }

            } else
            {
                await _factory.RespondAsync("Sorry, bad words detected, please try again.", _activity);
            }
        }


        // Receive a new activity (user's message), analyse the intent with LUIS and process accordingly
        // as an asynchronous operation.
        private async Task ProcessActivityAsync()
        {
            // save the activity to db
            await _factory.GetDbManager().AddActivity(_activity, ProcessStatus.Unprocessed);

            var strEscaped = Uri.EscapeUriString(_activity.Text);
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
                                    var result = _definition.FindDefinition(responseEntity.entity);
                                    if (result == null) goto default;
                                    var replyActivity = await _factory.RespondAsync(result, _activity);

                                    await _factory.GetDbManager().AddActivity(replyActivity);
                                }
                                await _factory.GetDbManager().UpdateActivity(_activity, ProcessStatus.BotReplied);
                                break;
                            }

                        default:
                            {
                                await SlackForwardAsync(_activity.Text);
                                break;
                            }
                    }
                }
            }
        }


        // Retrive unprocessed questions and display them in card view as an asynchronous operation.
        private async Task RetrieveQuestionsAsync()
        {
            var replyActivity = _activity.CreateReply("Unprocessed Questions:");
            replyActivity.Recipient = _activity.From;
            replyActivity.Type = "message";
            replyActivity.Attachments = new List<Attachment>();

            var unprocessedActivities = _factory.GetDbManager().FindUnprocessedQuestions(5);
            foreach (var ua in unprocessedActivities)
            {
                var cardButtons = new List<CardAction>();
                cardButtons.Add(new CardAction()
                {
                    Title = "Answer this question",
                    Type = "postBack",
                    Value = $"/replyActivity {ua.Id}"
                });

                var card = new HeroCard()
                {
                    Subtitle = $"{ua.From.Name} asked at {ua.Timestamp}",
                    Text = $"{ua.Text}",
                    Buttons = cardButtons
                };
                replyActivity.Attachments.Add(card.ToAttachment());
            }

            var connector = new ConnectorClient(new Uri(_activity.ServiceUrl));
            await connector.Conversations.ReplyToActivityAsync(replyActivity);
        }


        // Save user state data of a question clicked to reply, and notify which question is being answered
        // as an asynchronous operation.
        private async Task ReplyToQuestionsAsync()
        {
            var questionActivity = _factory.GetDbManager().FindActivity(_activity.Text.Split(' ')[1]);
            if (questionActivity == null)
            {
                //do something to handle
            }
            await _factory.SetUserDataPropertyAsync("replying", true, _activity);
            await _factory.SetUserDataPropertyAsync("replyingToQuestionID", questionActivity.Id, _activity);

            var markdown = $"You are answering ***{questionActivity.From.Name}***'s question:\n";
            markdown += "***\n";
            markdown += $"{questionActivity.Text}\n";
            markdown += "***\n";
            markdown += "**Please give your answer in the next replyActivity.**\n";

            await _factory.RespondAsync(markdown, _activity);
        }


        // Save the answer to database and give notification as an asynchronous operation.
        private async Task ProcessReplyAsync()
        {
            // get the userQuestion activity in order to update the process status
            var userQuestionActivity = _factory.GetDbManager().FindActivity(await _factory.GetUserDataPropertyAsync<string>("replyingToQuestionID", _activity));

            // set the ReplyToID of the answer acitivity to the AcitivityID of userQuestionAcitivity
            var rangerAnswerActivity = _activity;
            rangerAnswerActivity.ReplyToId = userQuestionActivity.Id;

            // save the rangerAnswerAcitivty to database.
            await _factory.GetDbManager().AddActivity(rangerAnswerActivity, ProcessStatus.BotReplied);

            // update the process status of userQuestionAcitivity
            await _factory.GetDbManager().UpdateActivity(userQuestionActivity, ProcessStatus.Processed);

            // reset replying state of the user
            await _factory.SetUserDataPropertyAsync("replying", false, _activity);

            await _factory.RespondAsync("Thanks, your answer has been received.", _activity);
        }


        // Forward an unprocessed question to a Slack channel and notify the user as an asynchronous operation.
        private async Task SlackForwardAsync(string msg)
        {
            var client = _factory.GetHttpClient();
            var uri = Environment.GetEnvironmentVariable("WEBHOOK_URL");
            
            var response = await client.PostMessageAsync(uri, new Payload()
            {
                Text = _activity.Text,
                Channel = "#5-grace-questions",
                Username = "GraceBot_UserEnquiry",
            });

            var reply = "Sorry, we currently don't have an answer for your question";
            if (response.IsSuccessStatusCode)
            {
                reply += "Your question has been sent to OMGTech! team, we will get back to you ASAP.";                
            }

            await _factory.RespondAsync(reply, _activity);
        }


        // Handle various system messages as an asynchronous operation.
        private async Task HandleSystemMessage()
        {
            switch (_activity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    {
                        await _factory.DeleteStateForUserAsync(_activity);
                        await _factory.RespondAsync($"The data of User {_activity.From.Id} has been deleted.", _activity);
                        break;
                    }
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
