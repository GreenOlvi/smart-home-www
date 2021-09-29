using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using SmartHomeWWW.Controllers;
using NUnit.Framework;
using FluentAssertions;
using Moq;
using SmartHomeCore.Firmwares;

namespace SmartHomeWWWTests
{
    [TestFixture]
    public class UpdateControllerTests
    {
        [SetUp]
        public void Setup()
        {
            var _firmwareRepository = new Mock<IFirmwareRepository>();
            _controller = new UpdateController(NullLogger<UpdateController>.Instance, _firmwareRepository.Object);
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