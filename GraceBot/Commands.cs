using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;
using GraceBot.Models;

namespace GraceBot
{
    internal static class CommandString
    {
        internal const string CMD_PREFIX = "//";
        internal const string GET_UNPROCESSED_QUESTIONS = CMD_PREFIX + "get";
        internal const string REPLYING_TO_QUESTION = CMD_PREFIX + "replyActivity";
        internal const string RATE_ANSWER = CMD_PREFIX + "rateAnswer";
    }

    internal static class CommandStringFactory
    {
        internal static string GenerateRateAnswerCmd(string subject, AnswerGrade answerGrade, string answerActivityId)
        {
            var cmd = CommandString.RATE_ANSWER;
            cmd += $" {subject} {answerGrade.ToString()} {answerActivityId}";
            return cmd;
        }
    }

    internal class CommandGetQuestion:ICommand
    {
        private IFactory _factory;
        private IBotManager _botManager;
        private IDbManager _dbManager;

        public CommandGetQuestion(IFactory factory)
        {
            _factory = factory;
            _botManager = _factory.GetBotManager();
            _dbManager = _factory.GetDbManager();
        }

        public async Task Execute(Activity activity)
        {
            // Get all unprocessd activities from database
            var unprocessedActivities = _dbManager.FindUnprocessedQuestions(5);

            // Create reply activity          
            if (unprocessedActivities.Any())
            {
                await _botManager.ReplyToActivityAsync("Unprocessed Questions:", activity, 
                    a => _botManager.GenerateQuestionsAttachments(unprocessedActivities));
            }
            else
            {
                await _botManager.ReplyToActivityAsync("No Unprocessed Questions Found.", activity);
            }
        }
    }

    internal class CommandReplyQuestion : ICommand
    {
        private IFactory _factory;
        private readonly IBotManager _botManager;
        private readonly IDbManager _dbManager;

        public CommandReplyQuestion(IFactory factory)
        {
            _factory = factory;
            _botManager = _factory.GetBotManager();
            _dbManager = _factory.GetDbManager();
        }

        public async Task Execute(Activity activity)
        {
            // Get question activity from database
            var questionActivity = _dbManager.FindActivity(activity.Text.Split(' ')[1]);

            if (questionActivity == null)
            {
                //TODO: do something to handle when the questionActivity is null.
            }

            // Set this activity is a replying activity and the question id
            await _botManager.SetUserDataPropertyAsync("replying", true, activity);
            await _botManager.SetUserDataPropertyAsync("replyingToQuestionID", questionActivity.Id, activity);

            // Return text
            var markdown = $"You are answering ***{questionActivity.From.Name}***'s question:\n";
            markdown += "***\n";
            markdown += $"{questionActivity.Text}\n";
            markdown += "***\n";
            markdown += "**Please give your answer in the next message.**\n";

            await _botManager.ReplyToActivityAsync(markdown, activity);
        }
    }
}
