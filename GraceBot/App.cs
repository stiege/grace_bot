using System;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Data;
using System.IO;
using Microsoft.Bot.Builder.Dialogs;
using GraceBot.Dialogs;
using Microsoft.Rest;

namespace GraceBot
{
    internal class App : IApp
    {
        #region Fields
        private readonly IFactory _factory;
        private readonly IFilter _filter;
        private static IDefinition _definition;
        private readonly IResponseManager _responseHomeManager;

        #endregion

        #region FeatureToggle
        // Using Dialogs
        // If set to true, then some features will be implemented with dialogs 
        private const bool USING_DIALOG = true;

        // If enabled then some Ranger exclusive features will only be available to 
        // registered rangers (in database). This should ONLY be set to false for 
        // locally debuging purpose.
        private const bool AUTHORISATION_RANGER = false;
        #endregion

        private ActivityData ActivityData { get; set; }
        ActivityData IApp.ActivityData
        {
            get { return ActivityData; }
            set { ActivityData = value; }
        }

        // constructor 
        public App(IFactory factory)
        {
            _factory = factory;
            _filter = _factory.GetActivityFilter();

            _definition = _factory.GetActivityDefinition();
            _responseHomeManager = _factory.GetResponseManager(DialogTypes.Home.ToString());

        }

        #region Methods

        // Determine activity (message) type and process accordingly as an asynchronous operation.
        public async Task RunAsync(Activity activity)
        {
            activity.Locale = "en-UK";
            UserRole userRole;
            try
            {
                userRole = _factory.GetDbManager().GetUserRole(activity.From.Id);
            }
            catch (RowNotInTableException ex)
            {
                userRole = UserRole.User;
            }

            ActivityData = new ActivityData(activity, userRole);

            // Check activity type, if not message, handle system message
            var isMessageType = activity.Type.Equals(ActivityTypes.Message);
            if (!isMessageType)
            {
                await HandleSystemMessage();
                return;
            }

            // Show typing indicator to user
            await _factory.GetBotManager().ReplyIsTypingActivityAsync(ActivityData.Activity);

            // Filter activity text
            var isPassedFilter = await _filter.FilterAsync(activity);
            if (!isPassedFilter)
            {
                await _factory.GetBotManager().ReplyToActivityAsync(
                    "Sorry, bad words detected, please try again.", activity);
                return;
            }

            #region InDialog Status Check
            if (USING_DIALOG)
            {
                // Only root-level dialogs need to be forwarded from here, 
                // non-root level dialogs should not set the private conversation data anyway
                var inDialog = _factory.GetBotManager()
                    .GetPrivateConversationDataProperty<DialogTypes>("InDialog");
                if (inDialog != DialogTypes.NonDialog)
                {
                    await Conversation.SendAsync(activity,
                        () => _factory.MakeIDialog<object>(inDialog));
                    return;
                }
            }
            #endregion

            // Check if this activity is replying message
            var isReplyingMessage = await _factory.GetBotManager()
                .GetUserDataPropertyAsync<bool>("replying", activity);
            if (isReplyingMessage)
            {
                await ProcessReplyAsync();
                return;
            }

            // Respond to triggering words
            switch (activity.Text.Trim().Split(' ')[0].Substring(0, CommandString.CMD_PREFIX.Length).ToLower())
            {
                case CommandString.CMD_PREFIX:
                    {
                        if (USING_DIALOG)
                        {
                            if (AUTHORISATION_RANGER && _factory.GetApp().ActivityData.UserRole != UserRole.Ranger)
                                goto default;
                            await Conversation.SendAsync(activity, 
                                () => _factory.MakeIDialog<object>(DialogTypes.Ranger));
                        }
                        else
                        {
                            // Execute Command 
                            var cmd = activity.Text.Split(' ')[0].ToLower();
                            await _factory.GetCommandManager().GetCommand(
                                cmd, ActivityData.UserRole).Execute(activity);
                        }
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

            // get response from Luis
            var response = await _factory.GetLuisManager()
                .GetResponse(ActivityData.Activity.Text);

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

                case "Greeting":
                    {
                        await ReplyGreeting();
                        break;
                    }

                case "Help":
                {
                    await ReplyHelp();
                    break;
                }

                default:
                    {
                        await _factory.GetBotManager().ReplyToActivityAsync(
                            "Sorry I cannot understand.", ActivityData.Activity);
                        break;
                    }
            }

        }

        private async Task ReplyHelp()
        {
                await Conversation.SendAsync(ActivityData.Activity,() => _factory.MakeIDialog<object>(DialogTypes.Help));
        }

        private async Task ReplyGreeting()
        {
            List<Attachment> attachments = null;

            string replyText = _responseHomeManager.GetResponseByKey("greeting");
            replyText += "!\n\nI'm Gracebot!";
            replyText += "\n\nPlease ask me questions OR type [//HELP] for help.";

            string imgUrl = "https://static1.squarespace.com/static/556e9677e4b099ded4a2e757/t/556fd5c8e4b063cd79bfe840/1485289348665";

            var attachment = _factory.GetBotManager()
                .GenerateImageCard("OMGTech", replyText, imgUrl);
            attachments = new List<Attachment> { attachment };

            await _factory.GetBotManager().ReplyToActivityAsync(null, ActivityData.Activity,attachments);
        }

        private async Task ReplyDefinition(LuisResponse response)
        {
            // save the activity to db
            await _factory.GetDbManager().AddActivity(ActivityData.Activity, ProcessStatus.Unprocessed);

            var subjectEntities = response.entities.Where(e => e.type == "subject").ToList();

            if (subjectEntities.Count > 1)
            {
                var replyText = "Please ask only one question at a time";
                await _factory.GetBotManager().ReplyToActivityAsync(replyText, ActivityData.Activity);
                return;
            }

            // Get definition
            var result = _definition.FindDefinition(subjectEntities.FirstOrDefault()?.entity);


            if (result == null)
            {
                await SlackForwardAsync(ActivityData.Activity.Text);
                return;
            }

            // Reply definition to user
            var replyActivity = await _factory.GetBotManager().ReplyToActivityAsync(result, ActivityData.Activity);

            // Save to and update status in database
            await _factory.GetDbManager().AddActivity(replyActivity);
            await _factory.GetDbManager().UpdateActivityProcessStatus(ActivityData.Activity.Id, ProcessStatus.BotReplied);

        }


        // Save the answer to database and give notification as an asynchronous operation.
        private async Task ProcessReplyAsync()
        {
            // get the userQuestion activity in order to update the process status
            var userQuestionActivity = _factory.GetDbManager().FindActivity(
                await _factory.GetBotManager().GetUserDataPropertyAsync<string>(
                    "replyingToQuestionID", ActivityData.Activity));

            // set the ReplyToID of the answer acitivity to the AcitivityID of userQuestionAcitivity
            var rangerAnswerActivity = ActivityData.Activity;
            rangerAnswerActivity.ReplyToId = userQuestionActivity.Id;

            // save the rangerAnswerAcitivty to database.
            await _factory.GetDbManager().AddActivity(rangerAnswerActivity, ProcessStatus.BotReplied);

            // update the process status of userQuestionAcitivity
            await _factory.GetDbManager().UpdateActivityProcessStatus(userQuestionActivity.Id, ProcessStatus.Processed);

            // reset replying state of the user
            await _factory.GetBotManager().SetUserDataPropertyAsync(
                "replying", false, ActivityData.Activity);

            await _factory.GetBotManager().ReplyToActivityAsync(
                "Thanks, your answer has been received.", ActivityData.Activity);
        }


        // ForwardMessageAsync an unprocessed question to a Slack channel and notify the user as an asynchronous operation.
        private async Task SlackForwardAsync(string msg)
        {
            var forwardResult = await _factory.GetQuestionSlackManager().ForwardMessageAsync(msg);

            var reply = "Sorry, we currently don't have an answer for your question.";
            if (forwardResult)
            {
                reply += " Your question has been sent to OMGTech! team, we will get back to you ASAP.";
            }

            await _factory.GetBotManager()
                .ReplyToActivityAsync(reply, ActivityData.Activity);
        }


        // Handle various system messages as an asynchronous operation.
        private async Task HandleSystemMessage()
        {
            switch (ActivityData.Activity.Type)
            {
                case ActivityTypes.DeleteUserData:
                    {
                        await _factory.GetBotManager()
                            .DeleteStateForUserAsync(ActivityData.Activity);
                        await _factory.GetBotManager().ReplyToActivityAsync(
                            $"The data of User {ActivityData.Activity.From.Id} has been deleted.",
                            ActivityData.Activity);
                        break;
                    }
                case ActivityTypes.ConversationUpdate:
                    // Handle conversation state changes, like members being added and removed
                    // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                    // Not available in all channels

                    await ReplyGreeting();
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
