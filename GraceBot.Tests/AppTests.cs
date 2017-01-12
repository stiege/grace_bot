using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using NUnit.Framework;
using Moq;
using System.Collections.Generic;

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
            mFactory.Setup(f => f.GetUserDataPropertyAsync<bool>(It.IsAny<string>(), It.IsAny<Activity>())).Returns(Task.FromResult(false));
            var app = new App(mFactory.Object);
            await app.RunAsync(_activity);

            mFactory.Verify(f => f.RespondAsync("Sorry, bad words detected, please try again.", _activity));
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
         
        [TearDown]
        public void ResetActivity()
        {
            _activity = null;
        }
    }
}
