using System.Threading.Tasks;
using NUnit.Framework;
using Moq;

namespace GraceBot.Tests
{
    [TestFixture]
    internal class AppTests
    {
        [Test]
        internal async Task RunTest()
        {
            var mockActivity = new Mock<IExtendedActivity>();
            var mockFactory = new Mock<IFactory>();
            await new App(mockFactory.Object, mockActivity.Object).Run();
        }
    }
}