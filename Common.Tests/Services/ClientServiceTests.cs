using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using MockQueryable.Moq;
using Microsoft.Extensions.Options;
using Common.Tests.Infrastructure;
using Common.Core.Data.Interfaces;
using Common.Domain.Entities;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;
using Common.Business.Services;
using Common.WebApi.Application.Models.Client;
using Common.Core.Generic.QueryLanguage.Interfaces;
using Common.Core.Data.Wrappers;
using Common.Core.CustomExceptions;
using Common.Core.Data.Identity.Enums;
using Common.Tests.Infrastructure.AutoMoq;

namespace Common.Tests.Services
{
    public class ClientServiceTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        public ClientServiceTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoMoq]
        public async Task Get_ById_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client expectedEntity)
        {

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => expectedEntity);
            var mapper = _fixture.CreateMapper();
            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            Client result = await sut.GetByPKAsync(expectedEntity.Id);

            result.Should().NotBeNull();
            result.Should().BeOfType<Client>();
            result.Id.Should().Be(expectedEntity.Id);
        }

        [Theory, AutoMoq]
        public async Task GetDto_ById_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client expectedEntity)
        {

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => expectedEntity);
            var mapper = _fixture.CreateMapper();
            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            ClientRequestDto dto = await sut.GetByPKAsync<ClientRequestDto>(expectedEntity.Id);

            dto.Should().NotBeNull();
            dto.Should().BeOfType<ClientRequestDto>();
            dto.Id.Should().Be(expectedEntity.Id);
        }

        [Theory, AutoMoq]
        public async Task Get_ById_NotFound_404(
            [Frozen] Mock<IRepository<Client>> repo,
            Client entity,
            ClientService sut)
        {
            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => null);
            Client result = await sut.GetByPKAsync(entity.Id);

            result.Should().BeNull();
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_Equal(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity)
        {

            var mapper = _fixture.CreateMapper();
            ClientQueryFilter filter = new ClientQueryFilter() { Name = entity.Name };
            var query = new List<Client>() { entity }.AsQueryable();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>())).Returns(TestFixture.MockDynamicFilter(query));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            IPaginatedResult<ClientRequestDto> result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(1);
            result.Items.Single().Id.Should().Be(entity.Id);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_Contains(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity)
        {

            var mapper = _fixture.CreateMapper();
            string searchValue = entity.Name;
            entity.Name = $"--- {entity.Name} ---";
            var query = new List<Client>() { entity }.AsQueryable();

            ClientQueryFilter filter = new ClientQueryFilter() { ContainsName = searchValue };
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>())).Returns(TestFixture.MockDynamicFilter(query));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            IPaginatedResult<ClientRequestDto> result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(1);
            result.Items.Single().Id.Should().Be(entity.Id);
            result.Items.Single().Name.Should().Contain(searchValue);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_NoPaginationData_ReturnsAll(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {


            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>()))
                .Returns(() => entities.BuildMock());

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            var filter = new ClientQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Disabled = true
            };

            IPaginatedResult<ClientRequestDto> result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.ListId.Count);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(50);
            result.TotalCount.Should().Be(filter.ListId.Count);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_WithPagination_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);


            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>()))
                .Returns(() => entities.BuildMock());

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            var filter = new ClientQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.PageSize);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(filter.ListId.Count);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_GreaterThanOrEqualAndLessThanOrEqualNumber(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            var mapper = _fixture.CreateMapper();
            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>())).Returns(TestFixture.MockDynamicFilter(entities.AsQueryable()));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            ClientDynamicFieldsQueryFilter filter = new ClientQueryFilter
            {
                GreaterThanOrEqualId = 10,
                LessThanOrEqualId = 13,
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<ClientRequestDto, ClientDynamicFieldsQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.First().Id.Should().Be(10);
            result.Items.Last().Id.Should().Be(13);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
        }
        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_GreaterThanOrEqualAndLessThanOrEqualDateTime(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);
            var dateStart = DateTime.Now.AddYears(-20);
            var dateEnd = DateTime.Now.AddYears(20);
            entities.First().BirthDate = dateStart;

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>())).Returns(TestFixture.MockDynamicFilter(entities.AsQueryable()));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            ClientDynamicFieldsQueryFilter filter = new ClientQueryFilter
            {
                GreaterThanOrEqualBirthDate = dateStart,
                LessThanOrEqualBirthDate = dateEnd,
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<ClientRequestDto, ClientDynamicFieldsQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.First().Id.Should().Be(10);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
        }
        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_GreaterThanLessThan(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            var mapper = _fixture.CreateMapper();
            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>())).Returns(TestFixture.MockDynamicFilter(entities.AsQueryable()));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            ClientDynamicFieldsQueryFilter filter = new ClientQueryFilter
            {
                GreaterThanId = 10,
                LessThanId = 13,
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<ClientRequestDto, ClientDynamicFieldsQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(2);
            result.Items.First().Id.Should().Be(filter.GreaterThanId+1);
            result.Items.Last().Id.Should().Be(filter.LessThanId-1);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(2);
        }
        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_List(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            var mapper = _fixture.CreateMapper();
            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>())).Returns(TestFixture.MockDynamicFilter(entities.AsQueryable()));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            ClientDynamicFieldsQueryFilter filter = new ClientQueryFilter
            {
                ListId = entities.Where(c => c.Id % 2 == 0).Take(10).Select(x => x.Id).ToList(),
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<ClientRequestDto, ClientDynamicFieldsQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(10);
            result.Items.All(x => filter.ListId.Contains(x.Id));
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(10);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_WithPagination_SortByClientName_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            List<Client> entities = _fixture.GetMockEntities<Client>(60).ToList();
            // Loop to reassign names in reverse, that is, from id=10 to 70 they are assigned the reverse of how they are created to force an ascending sort
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Name = $"test_{entities[entities.Count - 1 - i].Id}";
            }


            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>()))
                .Returns(() => entities.BuildMock());

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            var filter = new ClientQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Page = null,
                PageSize = null,
                SortingFields = nameof(Client.Name),
            };

            IPaginatedResult<ClientRequestDto> result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.PageSize);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(filter.ListId.Count);
            result.Items.ToArray()[0].Name.Should().Be(entities.ToList()[entities.Count - 1].Name);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_WithPagination_SortByClientNameDesc_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(60);


            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>()))
                .Returns(() => entities.BuildMock());

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            var filter = new ClientQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Page = 1,
                PageSize = 10,
                SortingFields = "Name desc",
            };

            IPaginatedResult<ClientRequestDto> result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.PageSize);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(filter.ListId.Count);
            result.Items.ToArray()[0].Name.Should().Be(entities.ToList()[entities.Count() - 1].Name);
        }

        [Theory, AutoMoq]
        public async Task Create_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity,
            ClientRequestDto dto)
        {
            //
            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.AddAsync(It.IsAny<Client>())).ReturnsAsync(() => entity);

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            Client result = await sut.AddAsync(dto);

            result.Should().NotBeNull();
            result.Should().BeOfType<Client>();
            repo.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Delete_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            await sut.DeleteAsync(entity.Id);

            repo.Verify(repo => repo.Delete(It.IsAny<Client>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Delete_SaveChangesException_Ko(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new CrudOperationException(CrudAction.DELETE));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.DeleteAsync(entity.Id);

            var result = await Assert.ThrowsAsync<CrudOperationException>(Actual);

            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<CrudOperationException>();
            result.Message.Should().Be(CrudOperationException.ERROR_DELETE);
        }

        [Theory, AutoMoq]
        public async Task Delete_NotFoundException_Ko(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => null);

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.DeleteAsync(entity.Id);

            var result = await Assert.ThrowsAsync<NoDbRecordException>(Actual);

            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            result.Should().NotBeNull();
            result.Should().BeOfType<NoDbRecordException>();
            result.Message.Should().Contain($"{entity.Id}");

        }
        [Theory, AutoMoq]
        public async Task Update_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity,
            ClientRequestDto dto)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);
            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            await sut.UpdateAsync(dto);

            repo.Verify(repo => repo.Update(It.IsAny<Client>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Update_SaveChangesException_Ko(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity,
            ClientRequestDto dto)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new CrudOperationException(CrudAction.UPDATE));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.UpdateAsync(dto);

            var result = await Assert.ThrowsAsync<CrudOperationException>(Actual);

            repo.Verify(repo => repo.Update(It.IsAny<Client>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<CrudOperationException>();
            result.Message.Should().Be(CrudOperationException.ERROR_UPDATE);
        }

        [Theory, AutoMoq]
        public async Task Modify_Ok(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity,
            ClientRequestDto dto)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            await sut.UpdateAsync(dto);

            repo.Verify(repo => repo.Update(It.IsAny<Client>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Modify_SaveChangesException_Ko(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Client entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new CrudOperationException(CrudAction.UPDATE));

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.UpdateAsync(entity);

            var result = await Assert.ThrowsAsync<CrudOperationException>(Actual);

            repo.Verify(repo => repo.Update(It.IsAny<Client>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<CrudOperationException>();
            result.Message.Should().Be(CrudOperationException.ERROR_UPDATE);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_NoPaginationAsync(
            [Frozen] Mock<IRepository<Client>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {

            var mapper = _fixture.CreateMapper();

            IEnumerable<Client> entities = _fixture.GetMockEntities<Client>(10);

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Client>>()))
                .Returns(() => entities.BuildMock());

            var sut = new ClientService(repo.Object, validationService.Object, mapper, config);

            var filter = new ClientQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Disabled = true
            };

            IPaginatedResult<ClientRequestDto> result = await sut.Get<ClientRequestDto, ClientQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<ClientRequestDto>>();
            result.Items.Count().Should().Be(entities.Count());
            result.TotalCount.Should().Be(entities.Count());
        }

    }
}

