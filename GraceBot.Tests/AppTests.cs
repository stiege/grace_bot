using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using NUnit.Framework;
using Moq;
using Newtonsoft.Json;

namespace GraceBot.Tests
{
    [TestFixture]
    public class AppTests
    {
        /// <summary>
        /// Test known bad words get a response of "...".
        /// </summary>
        /// <returns>Task</returns>
        [Test]
        public async Task RunWithBadWordTest()
        {
            var mockActivity = new Mock<IExtendedActivity>();
            var mockFactory = new Mock<IFactory>();
            mockActivity.Setup(a => a.Text).Returns("What does badword mean?");
            mockActivity.Setup(a => a.Type).Returns(ActivityTypes.Message);
            mockFactory.Setup(f => f.GetActivityFilter()).Returns(new ActivityFilter(mockFactory.Object, new [] {"badword"}));
            var app = new App(mockFactory.Object);
            await app.RunAsync(mockActivity.Object);
            mockFactory.Verify(f => f.RespondAsync("...", mockActivity.Object));
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
        public async Task ActivityIsPersisted()
        {
            var mockActivity = new Mock<IExtendedActivity>();
            var mockFactory = new Mock<IFactory>();
            var mockHttpClient = new Mock<IHttpClient>();
            IExtendedActivity persistedActivity = null;

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            var luisResponse = new LuisResponse
            {
                topScoringIntent = new Intent { intent = ""}
            };
            httpResponseMessage.Content = new StringContent(
                JsonConvert.SerializeObject(luisResponse));
            mockHttpClient.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(httpResponseMessage));
            mockHttpClient.Setup(c => c.PostMessageAsync(It.IsAny<string>(), It.IsAny<Payload>())).Returns(Task.FromResult(httpResponseMessage));
            mockActivity.Setup(a => a.Text).Returns("");
            mockActivity.Setup(a => a.Type).Returns(ActivityTypes.Message);
            mockFactory.Setup(f => f.GetActivityFilter()).Returns(
                new ActivityFilter(mockFactory.Object, new string[] {}));
            mockFactory.Setup(f => f.GetActivityDefinition()).Returns(
                new ActivityDefinition(new Dictionary<string, string>()));
            mockFactory.Setup(f => f.GetHttpClient()).Returns(mockHttpClient.Object);
            mockFactory.Setup(f => f.GetActivityPersistor()).Returns(activity => { persistedActivity = activity; });

            var app = new App(mockFactory.Object);
            await app.RunAsync(mockActivity.Object);

            Assert.That(persistedActivity, Is.SameAs(mockActivity.Object));
        }
    }
}