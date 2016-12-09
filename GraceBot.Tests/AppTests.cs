using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using NUnit.Framework;
using Moq;

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
    }
}