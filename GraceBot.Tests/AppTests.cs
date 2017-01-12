using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using GraceBot.Controllers;
using GraceBot.Models;

namespace GraceBot.Tests
{
    [TestFixture]
    public class AppTests
    {
        private Activity _activity;
        private Mock<IFactory> _factory;

        [SetUp]
        public void SetupActivity()
        {
            _activity = new Activity();
            _factory = new Mock<IFactory>();
        }

        /// <summary>
        /// Test known bad words get a response of "...".
        /// </summary>
        /// <returns>Task</returns>
        [Test]
        public async Task RunWithBadWordTest()
        {
            _activity.Text = "What does badword mean?";
            _activity.Type = ActivityTypes.Message;

            var mFactory = new Mock<IFactory>();
            mFactory.Setup(f => f.GetActivityFilter()).Returns(new ActivityFilter(new string[] { "badword" }));
            mFactory.Setup(f => f.GetBotManager().GetUserDataPropertyAsync<bool>(It.IsAny<string>(), It.IsAny<Activity>())).Returns(Task.FromResult(false));
            var app = new App(mFactory.Object);
            await app.RunAsync(_activity);

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync("Sorry, bad words detected, please try again.", _activity,null));
        }

        [Test]
        public async Task Db_ActivityIsAdded()
        {
            var mFactory = new Mock<IFactory>();
            mFactory.Setup(f => f.GetBotManager().GetUserDataPropertyAsync<bool>(It.IsAny<string>(), It.IsAny<Activity>())).Returns(Task.FromResult(false));
            mFactory.Setup(f => f.GetActivityFilter()).Returns(new ActivityFilter(new string[] { }));
            mFactory.Setup(f => f.GetActivityDefinition()).Returns(new ActivityDefinition(new Dictionary<string, string>() { { "test", "this is a test definition" } }));

            _activity.Text = It.IsAny<string>();

            var mDbManager = new Mock<IDbManager>();
            Activity persistedActivity = null;
        }

        /// <summary>
        /// The app should get an activity filter as soon as it is created so that this
        /// is not done every time a new request is received.
        /// </summary>
        [Test]
        public void CreateFilterOnStartup()
        {
            var mockFactory = new Mock<IFactory>();
            new App(mockFactory.Object);
            mockFactory.Verify(f => f.GetActivityFilter());
            mockFactory.Verify(f => f.GetActivityDefinition());
        }

        [Test]
        public async Task SlackForwardTest()
        {
            _activity.Text = "What is Forwarding Question?";
            _activity.Type = ActivityTypes.Message;

            var mFactory = new Mock<IFactory>();

            var response = new LuisResponse()
            {
                topScoringIntent = new Intent()
                {
                    intent = ""
                }
            };

            mFactory.Setup(f => f.GetActivityFilter().FilterAsync(It.IsAny<Activity>())).Returns(Task.FromResult(true));
            mFactory.Setup(o => o.GetDbManager().AddActivity(It.IsAny<Activity>(), It.IsAny<ProcessStatus>())).Returns(Task.CompletedTask);
            mFactory.Setup(o=>o.GetLuisManager().GetResponse(It.IsAny<string>())).Returns(Task.FromResult(response));
            mFactory.Setup(o => o.GetSlackManager().Forward(It.IsAny<string>())).Returns(Task.FromResult(true));

            var app = new App(mFactory.Object);

            await app.RunAsync(_activity);

            var expectedReply = "Sorry, we currently don't have an answer for your question";
            expectedReply += "Your question has been sent to OMGTech! team, we will get back to you ASAP.";

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync(expectedReply, _activity,null));
        }

        [Test]
        public async Task ProcessReplyActivityTest()
        {
            _activity.Text = "What is Forwarding Question?";
            _activity.Type = ActivityTypes.Message;

            var mFactory = new Mock<IFactory>();

            mFactory.Setup(o => o.GetBotManager().GetUserDataPropertyAsync<bool>(It.IsAny<string>(), It.IsAny<Activity>()))
                .Returns(Task.FromResult(true));
            mFactory.Setup((o => o.GetDbManager().FindActivity(It.IsAny<string>())))
                .Returns((new Activity() {Id = "1234"}));
            mFactory.Setup(o => o.GetDbManager().AddActivity(It.IsAny<Activity>(), It.IsAny<ProcessStatus>()))
                .Returns(Task.CompletedTask);
            mFactory.Setup(o => o.GetDbManager().UpdateActivity(It.IsAny<Activity>(), It.IsAny<ProcessStatus>()))
                .Returns(Task.CompletedTask);
            mFactory.Setup(o => o.GetBotManager().SetUserDataPropertyAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Activity>()))
                .Returns(Task.CompletedTask);

            var app = new App(mFactory.Object);

            await app.RunAsync(_activity);

            var expectedReply = "Thanks, your answer has been received.";

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync(expectedReply, _activity,null));
        }

        [Test]
        public async Task RetrieveQuestions_No_Questions_Test()
        {
            _activity.Text = "/get";
            _activity.Type = ActivityTypes.Message;

            var mFactory = new Mock<IFactory>();
            mFactory.Setup((o => o.GetDbManager().FindUnprocessedQuestions()))
               .Returns(new List<Activity>());
            mFactory.Setup(o => o.GetBotManager().GetUserDataPropertyAsync<bool>(It.IsAny<string>(), It.IsAny<Activity>()))
    .Returns(Task.FromResult(false));
            mFactory.Setup(o => o.GetActivityFilter().FilterAsync(_activity)).Returns(Task.FromResult(true));

            var app = new App(mFactory.Object);

            await app.RunAsync(_activity);

            var expectedReply = "No Unprocessed Questions Found.";

            mFactory.Verify(f => f.GetBotManager().ReplyToActivityAsync(expectedReply, _activity, null));

        }



        [TearDown]
        public void ResetActivity()
        {
            _activity = null;
        }
    }

}
