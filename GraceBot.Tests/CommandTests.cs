using Microsoft.Bot.Connector;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraceBot.Tests
{
    [TestFixture]
    class CommandTests
    {
        private Activity _activity;
        private Mock<IFactory> _factory;

        [SetUp]
        public void SetupActivity()
        {
            _activity = new Activity();
            _factory = new Mock<IFactory>();
        }

        [Test]
        public async Task ReplyToQuestionsTest()
        {
            _activity.Text = $"{CommandString.REPLYING_TO_QUESTION} 1234";
            
            var questionActivity = new Activity()
            {
                Id = "1234",
                Text = "what is test?",
                From = new ChannelAccount()
                {
                    Name = "FromName"
                }
            };

            var expectedResult = $"You are answering ***{questionActivity.From.Name}***'s question:\n";
            expectedResult += "***\n";
            expectedResult += $"{questionActivity.Text}\n";
            expectedResult += "***\n";
            expectedResult += "**Please give your answer in the next message.**\n";

            var mFactory = new Mock<IFactory>();

            mFactory.Setup(o => o.GetDbManager().FindActivity(It.IsAny<string>())).Returns(questionActivity);

            mFactory.Setup(o => o.GetBotManager().SetUserDataPropertyAsync("replying", true, _activity)).Returns(Task.CompletedTask);
            mFactory.Setup(o => o.GetBotManager().SetUserDataPropertyAsync("replyingToQuestionID", questionActivity.Id, _activity)).Returns(Task.CompletedTask);

            var command = new CommandReplyQuestion(mFactory.Object);

            await command.Execute(_activity);

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync(expectedResult, _activity, null));

        }

        [Test]
        public async Task RetrieveQuestions_No_Questions_Test()
        {
            var mFactory = new Mock<IFactory>();
            mFactory.Setup((o => o.GetDbManager()
                .FindUnprocessedQuestions(It.IsAny<int>(), It.IsAny<List<string>>())))
                .Returns(new List<Activity>());

            // set up a bot manager
            mFactory.Setup(o => o.GetBotManager().SetUserDataPropertyAsync("replying", true, _activity)).Returns(Task.CompletedTask);

            var command = new CommandGetQuestion(mFactory.Object);

            await command.Execute(_activity);

            var expectedReply = "No Unprocessed Questions Found.";

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync(expectedReply, _activity, null));

        }

        [Test]
        public async Task RetrieveQuestionsTest()
        {
            _activity.Text = "//get";
            _activity.Type = ActivityTypes.Message;
            var activitiesList = new List<Activity>()
            {
                new Activity(){}
            };

            var attachementsList = new List<Attachment>();

            var mFactory = new Mock<IFactory>();
            mFactory.Setup((o => o.GetDbManager()
               .FindUnprocessedQuestions(It.IsAny<int>(), It.IsAny<List<string>>())))
               .Returns(activitiesList);
            mFactory.Setup(o => o.GetBotManager().GetUserDataPropertyAsync<bool>(It.IsAny<string>(), It.IsAny<Activity>()))
    .Returns(Task.FromResult(false));
            mFactory.Setup(o => o.GetActivityFilter().FilterAsync(_activity)).Returns(Task.FromResult("Passed"));
            mFactory.Setup(o => o.GetBotManager().GenerateQuestionsAttachments(activitiesList)).Returns(attachementsList);

            var command = new CommandGetQuestion(mFactory.Object);

            await command.Execute(_activity);

            var expectedReply = "Unprocessed Questions:";

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync(expectedReply, _activity, attachementsList));
        }

        [TearDown]
        public void ResetActivity()
        {
            _activity = null;
        }
    }
}
