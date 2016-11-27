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
            mockActivity.Setup(a => a.Text).Returns("What does xxx mean?");
            mockActivity.Setup(a => a.Type).Returns(ActivityTypes.Message);
            mockFactory.Setup(f => f.GetActivityFilter()).Returns(new ActivityFilter(mockFactory.Object));
            await new App(mockFactory.Object, mockActivity.Object).RunAsync();
            mockFactory.Verify(f => f.RespondAsync("...", mockActivity.Object));
        }
    }
}