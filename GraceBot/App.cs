using System;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GraceBot
{
    internal class App : IApp
    {
        private readonly IFactory _factory;
        private readonly IFilter _filter;
        private readonly IDefinition _definition;
        private readonly IDbManager _dbManager;
        private readonly ILuisManager _luisManager;
        private readonly ISlackManager _slackManager;
        private readonly IBotManager _botManager;

        private Activity _activity;

        // constructor 
        public App(IFactory factory)
        {
            _factory = factory;
            _filter = _factory.GetActivityFilter();
            _definition = _factory.GetActivityDefinition();
            _dbManager = _factory.GetDbManager();
            _luisManager = _factory.GetLuisManager();
            _slackManager = _factory.GetSlackManager();
            _botManager = _factory.GetBotManager();
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
            var replying = await _botManager.GetUserDataPropertyAsync<bool>("replying", _activity);
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
                await _botManager.ReplyToActivityAsync("Sorry, bad words detected, please try again.", _activity);
            }
        }


        // Receive a new activity (user's message), analyse the intent with LUIS and process accordingly
        // as an asynchronous operation.
        private async Task ProcessActivityAsync()
        {
            // save the activity to db
            await _factory.GetDbManager().AddActivity(_activity, ProcessStatus.Unprocessed);

            // get response from Luis
            var response =await _luisManager.GetResponse(_activity.Text);

            // Check Luis response
            if (response!=null)
            {
                switch (response.topScoringIntent.intent)
                {
                    case "GetDefinition":
                        {
                            foreach (var responseEntity in response.entities.Where(e => e.type == "subject"))
                            {
                                var result = _definition.FindDefinition(responseEntity.entity);
                                if (result == null) goto default;
                                var replyActivity = await _botManager.ReplyToActivityAsync(result, _activity);

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


        // Retrive unprocessed questions and display them in card view as an asynchronous operation.
        private async Task RetrieveQuestionsAsync()
        {
            // Get all unprocessd activities from database
            var unprocessedActivities = _factory.GetDbManager().FindUnprocessedQuestions();

            // Create reply activity          

            if (unprocessedActivities.Any())
            {
                var attachments =_botManager.GenerateQuestionsAttachments(unprocessedActivities);
               await _botManager.ReplyToActivityAsync("Unprocessed Questions:",_activity,attachments);

            }
            else
            {
                await _botManager.ReplyToActivityAsync("No Unprocessed Questions Found.", _activity);
            }

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
            await _botManager.SetUserDataPropertyAsync("replying", true, _activity);
            await _botManager.SetUserDataPropertyAsync("replyingToQuestionID", questionActivity.Id, _activity);

            var markdown = $"You are answering ***{questionActivity.From.Name}***'s question:\n";
            markdown += "***\n";
            markdown += $"{questionActivity.Text}\n";
            markdown += "***\n";
            markdown += "**Please give your answer in the next replyActivity.**\n";

            await _botManager.ReplyToActivityAsync(markdown, _activity);
        }


        // Save the answer to database and give notification as an asynchronous operation.
        private async Task ProcessReplyAsync()
        {
            // get the userQuestion activity in order to update the process status
            var userQuestionActivity = _factory.GetDbManager().FindActivity(await _botManager.GetUserDataPropertyAsync<string>("replyingToQuestionID", _activity));

            // set the ReplyToID of the answer acitivity to the AcitivityID of userQuestionAcitivity
            var rangerAnswerActivity = _activity;
            rangerAnswerActivity.ReplyToId = userQuestionActivity.Id;

            // save the rangerAnswerAcitivty to database.
            await _factory.GetDbManager().AddActivity(rangerAnswerActivity, ProcessStatus.BotReplied);

            // update the process status of userQuestionAcitivity
            await _factory.GetDbManager().UpdateActivity(userQuestionActivity, ProcessStatus.Processed);

            // reset replying state of the user
            await _botManager.SetUserDataPropertyAsync("replying", false, _activity);

            await _botManager.ReplyToActivityAsync("Thanks, your answer has been received.", _activity);
        }


        // Forward an unprocessed question to a Slack channel and notify the user as an asynchronous operation.
        private async Task SlackForwardAsync(string msg)
        {

            var forwardResult = await _slackManager.Forward(msg);

            var reply = "Sorry, we currently don't have an answer for your question";
            if (forwardResult)
            {
                reply += "Your question has been sent to OMGTech! team, we will get back to you ASAP.";                
            }

            await _botManager.ReplyToActivityAsync(reply, _activity);
        }


        // Handle various system messages as an asynchronous operation.
        private async Task HandleSystemMessage()
        {
            switch (_activity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    {
                        await _botManager.DeleteStateForUserAsync(_activity);
                        await _botManager.ReplyToActivityAsync($"The data of User {_activity.From.Id} has been deleted.", _activity);
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
