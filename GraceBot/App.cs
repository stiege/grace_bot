﻿using System;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Data;

namespace GraceBot
{
    internal class App : IApp
    {
        #region Fields
        private readonly IFactory _factory;
        private readonly IFilter _filter;
        private readonly IDefinition _definition;
        private readonly IDbManager _dbManager;
        private readonly ILuisManager _luisManager;
        private readonly ISlackManager _slackManager;
        private readonly IBotManager _botManager;
        private readonly ICommandManager _commandManager;

        private Activity _activity;
        private UserRole _userRole;

        #endregion


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
            _commandManager = _factory.GetCommandManager();
            _userRole = UserRole.User;
        }

        #region Methods

        // Determine activity (message) type and process accordingly as an asynchronous operation.
        public async Task RunAsync(Activity activity)
        {
            // Set field activity to argument activity 
            _activity = activity;

            // Get user role
            try
            {
                _userRole = _dbManager.GetUserRole(_activity.From.Id);
            } catch (RowNotInTableException ex)
            {
                _userRole = UserRole.User;
            }

            // Check activity type, if not message, handle system message
            var isMessageType = _activity.Type != ActivityTypes.Message;
            if (isMessageType)
            {
                await HandleSystemMessage();
                return;
            }

            // Check if this activity is replying message
            var isReplyingMessage = await _botManager.GetUserDataPropertyAsync<bool>("replying", _activity);
            if (isReplyingMessage)
            {
                await ProcessReplyAsync();
                return;
            }

            // Filter activity text
            var isPassedFilter = await _filter.FilterAsync(_activity);
            if (!isPassedFilter)
            {
                await _botManager.ReplyToActivityAsync("Sorry, bad words detected, please try again.", _activity);
                return;
            }

            // Respond to triggering words
            switch (_activity.Text.Split(' ')[0].Substring(0, CommandString.CMD_PREFIX.Length))
            {
                case CommandString.CMD_PREFIX:
                    {
                        // Execute Command 
                        var cmd = _activity.Text.Split(' ')[0];
                        await _commandManager.GetCommand(cmd, _userRole).Execute(_activity);
                        break;
                    }
                default:
                    {
                        await ProcessActivityAsync();
                        break;
                    }
            }

        }


        // Receive a new activity (user's message), analyse the intent with LUIS and process accordingly
        // as an asynchronous operation.
        private async Task ProcessActivityAsync()
        {
            // save the activity to db
            await _dbManager.AddActivity(_activity, ProcessStatus.Unprocessed);

            // get response from Luis
            var response = await _luisManager.GetResponse(_activity.Text);

            // Check Luis response
            if (response == null)
                return;

            switch (response.topScoringIntent.intent)
            {
                case "GetDefinition":
                    {
                        await ReplyDefinition(response);
                        break;
                    }

                default:
                    {
                        await SlackForwardAsync(_activity.Text);
                        break;
                    }
            }

        }

        private async Task ReplyDefinition(LuisResponse response)
        {
            // Get the highest score subject
            var subjectEntity =
                response.entities.Where(e => e.type == "subject").OrderByDescending(o => o.score).FirstOrDefault();

            // Get definition
            var result = _definition.FindDefinition(subjectEntity.entity);

            // If definition is null , forward...
            if (result == null)
            {
                await SlackForwardAsync(_activity.Text);
            }
            else
            {
                // Reply definition to user
                var replyActivity = await _botManager.ReplyToActivityAsync(result, _activity);

                // Save to and update status in database
                await _dbManager.AddActivity(replyActivity);
                await _dbManager.UpdateActivityProcessStatus(_activity.Id, ProcessStatus.BotReplied);
            }
        }


        // Save the answer to database and give notification as an asynchronous operation.
        private async Task ProcessReplyAsync()
        {
            // get the userQuestion activity in order to update the process status
            var userQuestionActivity = _dbManager.FindActivity(await _botManager.GetUserDataPropertyAsync<string>("replyingToQuestionID", _activity));

            // set the ReplyToID of the answer acitivity to the AcitivityID of userQuestionAcitivity
            var rangerAnswerActivity = _activity;
            rangerAnswerActivity.ReplyToId = userQuestionActivity.Id;

            // save the rangerAnswerAcitivty to database.
            await _dbManager.AddActivity(rangerAnswerActivity, ProcessStatus.BotReplied);

            // update the process status of userQuestionAcitivity
            await _dbManager.UpdateActivityProcessStatus(userQuestionActivity.Id, ProcessStatus.Processed);

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
    #endregion
}
