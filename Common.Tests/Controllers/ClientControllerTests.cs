using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Common.WebApi.Application.Models.Client;
using Common.Core.Data.Interfaces;
using Common.Core.Data.Wrappers;
using Common.Domain.Entities;
using System.Net;
using Common.WebApi.Application.Controllers;
using Common.Core.CustomExceptions;
using Common.Tests.Infrastructure.AutoMoq;
using Common.Core.Data.Identity.Enums;
using Common.Core.Generic.Controllers.Response;

namespace Common.Tests.Controllers
{
    public class ClientControllerTests
    {
        [Theory, AutoMoq]
        public async Task GetAsync_ById_OK(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            ClientResponseDto dto)
        {
            service.Setup(service => service.GetByPKAsync<ClientResponseDto>(It.IsAny<int>())).ReturnsAsync(() => dto);

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Get(dto.Id);

            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();

            var result = response.As<OkObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeOfType<Response<ClientResponseDto>>();

            var body = result.Value.As<Response<ClientResponseDto>>();
            body.Data.Should().NotBeNullOrEmpty();
            body.Data.Count.Should().Be(1);
            body.Error.Should().BeNull();

        }

        [Theory, AutoMoq]
        public async Task GetAsync_ById_NotFound_404(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            int id)
        {
            service.Setup(service => service.GetByPKAsync<ClientResponseDto>(id)).ReturnsAsync(() => null);

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Get(id);

            response.Should().NotBeNull();
            response.Should().BeOfType<ObjectResult>();

            var result = response.As<ObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            result.Value.Should().BeOfType<BaseResponse>();

        }

        [Theory, AutoMoq]
        public async Task GetAll_ReturnsEntities(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            ClientQueryFilter filter,
            PaginatedResult<ClientResponseDto> expectedEntities)
        {
            expectedEntities.Page = 1;
            expectedEntities.PageSize = 50;
            expectedEntities.TotalCount = expectedEntities.Items.Count();

            service.Setup(service => service.Get<ClientResponseDto, ClientQueryFilter>(filter)).ReturnsAsync(expectedEntities);

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Query(filter);

            response.Should().NotBeNull();
            response.Should().BeOfType<OkObjectResult>();

            var result = response.As<OkObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeOfType<Response<ClientResponseDto>>();

            var body = result.Value.As<Response<ClientResponseDto>>();
            body.Data.Should().NotBeEmpty();
            body.Error.Should().BeNull();
            body.TotalRecords.Should().Be(expectedEntities.TotalCount);
        }

        [Theory, AutoMoq]
        public async Task Create_OK(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            ClientRequestDto dto,
            Client entity)
        {
            service.Setup(service => service.AddAsync(It.IsAny<ClientRequestDto>())).ReturnsAsync(() => entity);

            var sut = new ClientController(log.Object, service.Object);
            // Mock ControllerContext and ActionDescriptor
            var controllerContext = new ControllerContext
            {
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor
                {
                    ControllerName = "Client"
                }
            };
            sut.ControllerContext = controllerContext;

            var response = await sut.Create(dto);

            response.Should().NotBeNull();
            response.Should().BeOfType<CreatedAtRouteResult>();

            var result = response.As<CreatedAtRouteResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.Created);
            result.Value.Should().BeOfType<Response<ClientResponseDto>>();

            var body = result.Value.As<Response<ClientResponseDto>>();
            body.Data.Should().NotBeNullOrEmpty();
            body.Data.Count.Should().Be(1);
            body.Error.Should().BeNull();

        }

        [Theory, AutoMoq]
        public async Task Update_OK(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            ClientRequestDto dto)
        {
            service.Setup(s => s.ValidateDto<ClientRequestDto, ClientRequestDtoValidator>(CrudAction.UPDATE_PATCH, dto)).Verifiable();

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Update(dto);

            response.Should().NotBeNull();
            response.Should().BeOfType<NoContentResult>();

            var result = response.As<NoContentResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

        }

        [Theory, AutoMoq]
        public async Task Update_NotFound_404(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            ClientRequestDto dto)
        {
            service.Setup(service => service.UpdateAsync(dto))
                .ThrowsAsync(new NoDbRecordException(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Update(dto);

            response.Should().NotBeNull();
            response.Should().BeOfType<ObjectResult>();

            var result = response.As<ObjectResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            result.Value.Should().BeOfType<BaseResponse>();

            var body = result.Value.As<BaseResponse>();
            body.Error.Should().NotBeNull();
            body.Error.Code.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Theory, AutoMoq]
        public async Task Delete_OK(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            Client entity)
        {
            service.Setup(service => service.GetByPKAsync(entity.Id)).ReturnsAsync(() => null);
            service.Setup(service => service.DeleteAsync(entity.Id)).Verifiable();

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Delete(1);

            response.Should().NotBeNull();
            response.Should().BeOfType<NoContentResult>();

            var result = response.As<NoContentResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);

        }

        [Theory, AutoMoq]
        public async Task Modify_OK(
            [Frozen] Mock<ILogger<ClientController>> log,
            [Frozen] Mock<IBaseService<Client, int>> service,
            ClientRequestDto dto)
        {
            service.Setup(s => s.ValidateDto<ClientRequestDto, ClientRequestDtoValidator>(CrudAction.UPDATE_PATCH, dto)).Verifiable();

            var sut = new ClientController(log.Object, service.Object);

            var response = await sut.Patch(dto);

            response.Should().NotBeNull();
            response.Should().BeOfType<NoContentResult>();

            var result = response.As<NoContentResult>();
            result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }
    }
}
