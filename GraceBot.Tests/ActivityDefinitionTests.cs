using NUnit.Framework;
using System.Collections.Generic;

namespace GraceBot.Tests
{
    [TestFixture]
    public class ActivityDefinitionTests
    {
        [Test]
        public void RunWithBadWordTest()
        {
            var testLookup = new Dictionary<string, string> { { "KEY", "value"} };
            var dut = new ActivityDefinition(testLookup);
            Assert.That(dut.FindDefinition("KEY"), Is.EqualTo("value"));
        }
    }
}