using Moq;
using Microsoft.Extensions.Logging;
using Common.WebApi.Application.Controllers;
using Common.Domain.Entities;
using AutoFixture;
using AutoFixture.AutoMoq;
using Common.Core.Data.Interfaces;

namespace Common.Tests
{
    public class ClientControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IBaseService<Client, int>> _mockClientService;
        private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockClientService = _fixture.Freeze<Mock<IBaseService<Client, int>>>();
            var mockLogger = _fixture.Freeze<Mock<ILogger<ClientController>>>();
            _controller = new ClientController(mockLogger.Object, _mockClientService.Object);
        }
    }

}