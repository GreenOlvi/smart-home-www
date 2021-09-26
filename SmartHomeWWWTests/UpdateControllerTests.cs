using NUnit.Framework;
using FluentAssertions;
using SmartHomeWWW.Controllers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace SmartHomeWWWTests
{
    [TestFixture]
    public class UpdateControllerTests
    {
        [SetUp]
        public void Setup()
        {
            _controller = new UpdateController(NullLogger<UpdateController>.Instance);
        }

        private UpdateController _controller;

        [Test]
        public void GetUpdatesTest()
        {
            var result = _controller.Index() as ViewResult;
            result.Should().NotBeNull();
        }
    }
}