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
        private readonly IResponseManager _responseHomeManager;

        #endregion

        #region FeatureToggle
        // Using Dialogs
        // If set to true, then some features will be implemented with dialogs 
        private readonly bool USING_DIALOG;

        // If enabled then some Ranger exclusive features will only be available to 
        // registered rangers (in database). This should ONLY be set to false for 
        // locally debuging purpose.
        private readonly bool AUTHORISATION_RANGER;

        // A LUIS intent is used when its score is above the threshold.
        private readonly double INTENT_SCORE_THRESHOLD;

        // If enabled, then the locale of all activities will be recognised as "en-UK"
        // regardless of the setup of users' clients.
        private readonly bool FORCE_LOCALE_EN;
        #endregion

        private ActivityData ActivityData { get; set; }
        ActivityData IApp.ActivityData
        {
            get { return ActivityData; }
            set { ActivityData = value; }
        }

        // constructor 
        public App(IFactory factory, 
            bool using_dialog = true, 
            bool authorisation_ranger = false,
            double intent_score_threshold = 0.7,
            bool force_locale_en = true)
        {
            if (intent_score_threshold > 1.0 || intent_score_threshold < 0.0)
                throw new InvalidOperationException("The range of intent_score_threshold must be between 0.0 and 1.0");
            _factory = factory;
            _filter = _factory.GetActivityFilter();
            USING_DIALOG = using_dialog;
            AUTHORISATION_RANGER = authorisation_ranger;
            INTENT_SCORE_THRESHOLD = intent_score_threshold;
            FORCE_LOCALE_EN = force_locale_en;

            _responseHomeManager = _factory.GetResponseManager(DialogTypes.Home.ToString());
        }

        #region Methods
        // Determine activity (message) type and process accordingly as an asynchronous operation.
        public async Task RunAsync(Activity activity)
        {
            if(FORCE_LOCALE_EN)
                activity.Locale = "en-UK";

            UserRole userRole;
            try
            {
                userRole = _factory.GetDbManager().GetUserRole(activity.From.Id);
            }
            catch (RowNotInTableException)
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
            string filterResult = await _filter.FilterAsync(activity);
            if (!filterResult.Equals("Passed"))
            {
                await _factory.GetBotManager().ReplyToActivityAsync(filterResult, activity);
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
                        () => _factory.MakeIDialog<object>(DialogTypes.Root));
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
            var cmd = activity.Text.Trim().Split(' ');
            switch (cmd[0].Substring(0, CommandString.CMD_PREFIX.Length))
            {
                case CommandString.CMD_PREFIX:
                    {
                        if (USING_DIALOG)
                        {
                            if (AUTHORISATION_RANGER && _factory.GetApp().ActivityData.UserRole != UserRole.Ranger)
                                goto default;
                            DialogTypes inDialog = DialogTypes.Ranger;
                            if (cmd[0] == CommandString.RATE_ANSWER)
                                inDialog = DialogTypes.RateAnswer;
                            _factory.GetBotManager().SetPrivateConversationDataProperty("InDialog", inDialog);
                            _factory.GetBotManager().SetPrivateConversationDataProperty("Command", cmd);
                            await Conversation.SendAsync(activity, 
                                () => _factory.MakeIDialog<object>(DialogTypes.Root));
                        }
                        else
                        {
                            // Rate Answer can only be processed in Dialog
                            if (cmd[0] == CommandString.RATE_ANSWER)
                                return;
                            await _factory.GetCommandManager().GetCommand(
                                cmd[0], ActivityData.UserRole).Execute(activity);
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
            ActivityData.LuisResponse = await _factory.GetLuisManager()
                .GetResponse(ActivityData.Activity.Text);

            // TODO: it's probably better that posting back an error message to user
            if (ActivityData.LuisResponse == null)
                return;

            var intent = ActivityData.LuisResponse.TopScoringIntent.Score > INTENT_SCORE_THRESHOLD ?
                ActivityData.LuisResponse.TopScoringIntent.Name :
                "no intent";

            switch (intent)
            {
                case "GetDefinition":
                    {
                        await ReplyDefinition(ActivityData.LuisResponse);
                        break;
                    }

                case "Greeting":
                    {
                        await ReplyGreeting();
                        break;
                    }

                case "Help":
                    {
                        _factory.GetBotManager().SetPrivateConversationDataProperty("InDialog", DialogTypes.Help);
                        await Conversation.SendAsync(
                            ActivityData.Activity, 
                            () => _factory.MakeIDialog<object>(DialogTypes.Root));
                        break;
                    }

                case "no intent":
                default:
                    {
                        await _factory.GetBotManager().ReplyToActivityAsync(
                            "Sorry I cannot understand.", ActivityData.Activity);
                        break;
                    }
            }

        }

        private async Task ReplyGreeting()
        {
            string replyText = _responseHomeManager.GetResponseByKey("greeting");
            replyText += "!\n\nI'm Gracebot!";
            replyText += "\n\nPlease ask me questions OR type \"help\" for more information.";

            string imgUrl = "https://static1.squarespace.com/static/556e9677e4b099ded4a2e757/t/556fd5c8e4b063cd79bfe840/1485289348665";

            var attachment = _factory.GetBotManager()
                .GenerateHeroCard("OMGTech", replyText, imgUrl);

            await _factory.GetBotManager().ReplyToActivityAsync(null,
                null, a => new List<Attachment> { attachment });
        }

        private async Task ReplyDefinition(LuisResponse response)
        {
            // save the activity to db
            await _factory.GetDbManager().AddActivity(ActivityData.Activity, ProcessStatus.Unprocessed);

            var subjectEntities = response.Entities.Where(e => e.Type == "subject").ToList();

            if (subjectEntities.Count > 1)
            {
                var replyText = "Please ask only one question at a time";
                await _factory.GetBotManager().ReplyToActivityAsync(replyText, ActivityData.Activity);
                return;
            }

            var subject = subjectEntities.FirstOrDefault()?.Name;
            var result = _factory.GetAnswerManager().GetAnswerTo(subject);

            if (result == null)
            {
                await SlackForwardAsync(ActivityData.Activity.Text);
                return;
            }

            await _factory.GetBotManager()
                .ReplyToActivityAsync(result, null,
                a => new List<Attachment> { GenerateRateAnswerCard(subject, a.Id) },
                a =>
                {
                    _factory.GetDbManager().AddActivity(a);
                    try
                    {
                        _factory.GetAnswerManager().AddAnswer(subject, a);
                    }
                    catch (Exception e)
                    {
                        var s = e.Message;
                    }

                });
            
            await _factory.GetDbManager()
                .UpdateActivityProcessStatus(ActivityData.Activity.Id, ProcessStatus.BotReplied);
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

            var reply = "Sorry, we currently don't have an answer to your question.";
            if (forwardResult)
            {
                reply += " Your question has been forwarded to OMGTech! team. We will get back to you ASAP.";
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

        private Attachment GenerateRateAnswerCard(string subject, string answerActivityId)
        {
            var cardButtons = new List<CardAction>();
            foreach(AnswerGrade grade in Enum.GetValues(typeof(AnswerGrade)))
            {
                if (grade == AnswerGrade.NotRated)
                    continue;
                cardButtons.Add(new CardAction()
                {
                    Title = grade.ToString().Replace('_', ' '),
                    Type = ActionTypes.PostBack,
                    Value = CommandStringFactory.GenerateRateAnswerCmd(subject, grade, answerActivityId)
                });
            }

            HeroCard plCard = new HeroCard()
            {
                Text = "Rate this answer",
                Buttons = cardButtons,
            };
            return plCard.ToAttachment();
        }
    }
    #endregion
}
