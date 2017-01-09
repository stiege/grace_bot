using NUnit.Framework;
using Moq;

namespace GraceBot.Tests
{
    [TestFixture]
    public class ActivityFilterTests
    {
        [Test]
        public void InitTest()
        {
            var mockFactory = new Mock<IFactory>();
            new ActivityFilter(new [] {"bad", "word", "list"});
        }
    }
}
