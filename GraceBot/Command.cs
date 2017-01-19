using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;

namespace GraceBot
{

        internal class CommandGetQuestion:ICommand
        {
            private IFactory _factory;
            private IBotManager _botManager;
            private IDbManager _dbManager;

            public CommandGetQuestion()
            {
                _factory = Factory.GetFactory();
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
                    var attachments = _botManager.GenerateQuestionsAttachments(unprocessedActivities);
                    await _botManager.ReplyToActivityAsync("Unprocessed Questions:", activity, attachments);
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
        public CommandReplyQuestion()
        {
            _factory = Factory.GetFactory(); ;
            _botManager = _factory.GetBotManager();
            _dbManager = _factory.GetDbManager();
        }

        public async Task Execute(Activity activity)
            {
            // Get question activity from database
            var questionActivity = _dbManager.FindActivity(activity.Text.Split(' ')[1]);

            if (questionActivity == null)
            {
                //do something to handle
            }

            // Set this activity is a replying activity and the question id
            await _botManager.SetUserDataPropertyAsync("replying", true, activity);
            await _botManager.SetUserDataPropertyAsync("replyingToQuestionID", questionActivity.Id, activity);

            // Return text
            var markdown = $"You are answering ***{questionActivity.From.Name}***'s question:\n";
            markdown += "***\n";
            markdown += $"{questionActivity.Text}\n";
            markdown += "***\n";
            markdown += "**Please give your answer in the next replyActivity.**\n";

            await _botManager.ReplyToActivityAsync(markdown, activity);
        }
        }

    }
