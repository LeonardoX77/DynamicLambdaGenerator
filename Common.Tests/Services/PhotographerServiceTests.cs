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
using Common.WebApi.Application.Models.Photographer;
using Common.Core.Generic.QueryLanguage.Interfaces;
using Common.Core.Data.Wrappers;
using Common.Core.CustomExceptions;
using Common.Core.Data.Identity.Enums;
using Common.Tests.Infrastructure.AutoMoq;

namespace Common.Tests.Services
{
    public class PhotographerServiceTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        public PhotographerServiceTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoMoq]
        public async Task Get_ById_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer expectedEntity)
        {

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => expectedEntity);
            var mapper = _fixture.CreateMapper();
            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            Photographer result = await sut.GetByPKAsync(expectedEntity.Id);

            result.Should().NotBeNull();
            result.Should().BeOfType<Photographer>();
            result.Id.Should().Be(expectedEntity.Id);
        }

        [Theory, AutoMoq]
        public async Task GetDto_ById_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer expectedEntity)
        {

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => expectedEntity);
            var mapper = _fixture.CreateMapper();
            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            PhotographerDto dto = await sut.GetByPKAsync<PhotographerDto>(expectedEntity.Id);

            dto.Should().NotBeNull();
            dto.Should().BeOfType<PhotographerDto>();
            dto.Id.Should().Be(expectedEntity.Id);
        }

        [Theory, AutoMoq]
        public async Task Get_ById_NotFound_404(
            [Frozen] Mock<IRepository<Photographer>> repo,
            Photographer entity,
            PhotographerService sut)
        {
            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => null);
            Photographer result = await sut.GetByPKAsync(entity.Id);

            result.Should().BeNull();
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_Equal(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity)
        {
            var mapper = _fixture.CreateMapper();
            PhotographerQueryFilter filter = new PhotographerQueryFilter() { Name = entity.Name };
            var query = new List<Photographer>() { entity }.AsQueryable();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>())).Returns(TestFixture.MockDynamicFilter(query));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            IPaginatedResult<PhotographerDto> result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(1);
            result.Items.Single().Id.Should().Be(entity.Id);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_Contains(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity)
        {

            var mapper = _fixture.CreateMapper();
            string searchValue = entity.Name;
            entity.Name = $"--- {entity.Name} ---";

            PhotographerQueryFilter filter = new PhotographerQueryFilter() { ContainsName = searchValue };
            var query = new List<Photographer>() { entity }.AsQueryable();
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>())).Returns(TestFixture.MockDynamicFilter(query));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            IPaginatedResult<PhotographerDto> result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(1);
            result.Items.Single().Id.Should().Be(entity.Id);
            result.Items.Single().Name.Should().Contain(searchValue);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_NoPaginationData_ReturnsAll(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {


            IEnumerable<Photographer> entities = _fixture.GetMockEntities<Photographer>(60);

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>()))
                .Returns(() => entities.BuildMock());

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            var filter = new PhotographerQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Disabled = true
            };

            IPaginatedResult<PhotographerDto> result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.ListId.Count);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(50);
            result.TotalCount.Should().Be(filter.ListId.Count);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_WithPagination_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            IEnumerable<Photographer> entities = _fixture.GetMockEntities<Photographer>(60);


            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>()))
                .Returns(() => entities.BuildMock());

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            var filter = new PhotographerQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.PageSize);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(filter.ListId.Count);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_MaxMin(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            var mapper = _fixture.CreateMapper();
            IEnumerable<Photographer> entities = _fixture.GetMockEntities<Photographer>(60);
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>())).Returns(TestFixture.MockDynamicFilter(entities.AsQueryable()));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            PhotographerDynamicFieldsQueryFilter filter = new PhotographerQueryFilter
            {
                GreaterThanOrEqualId = 10,
                LessThanOrEqualId = 13,
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<PhotographerDto, PhotographerDynamicFieldsQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(4);
            result.Items.First().Id.Should().Be(10);
            result.Items.Last().Id.Should().Be(13);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(4);
        }
        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_List(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            var mapper = _fixture.CreateMapper();
            IEnumerable<Photographer> entities = _fixture.GetMockEntities<Photographer>(60);
            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>())).Returns(TestFixture.MockDynamicFilter(entities.AsQueryable()));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            PhotographerDynamicFieldsQueryFilter filter = new PhotographerQueryFilter
            {
                ListId = entities.Where(c => c.Id % 2 == 0).Take(10).Select(x => x.Id).ToList(),
                Page = 1,
                PageSize = 10,
            };

            var result = await sut.Get<PhotographerDto, PhotographerDynamicFieldsQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(10);
            result.Items.All(x => filter.ListId.Contains(x.Id));
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(10);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_WithPagination_SortByPhotographerName_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            List<Photographer> entities = _fixture.GetMockEntities<Photographer>(60).ToList();
            // Loop to reassign names in reverse, that is, from id=10 to 70 they are assigned the reverse of how they are created to force an ascending sort
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Name = $"test_{entities[entities.Count - 1 - i].Id}";
            }


            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>()))
                .Returns(() => entities.BuildMock());

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            var filter = new PhotographerQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Page = null,
                PageSize = null,
                SortingFields = "Name",
            };

            IPaginatedResult<PhotographerDto> result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.PageSize);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(filter.ListId.Count);
            result.Items.ToArray()[0].Name.Should().Be(entities.ToList()[entities.Count - 1].Name);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_WithPagination_SortByPhotographerNameDesc_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {
            IEnumerable<Photographer> entities = _fixture.GetMockEntities<Photographer>(60);


            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>()))
                .Returns(() => entities.BuildMock());

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            var filter = new PhotographerQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Page = 1,
                PageSize = 10,
                SortingFields = "Name desc",
            };

            IPaginatedResult<PhotographerDto> result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Should().NotBeNull();
            result.Items.Count().Should().Be(filter.PageSize);
            result.Page.Should().Be(filter.Page);
            result.PageSize.Should().Be(filter.PageSize);
            result.TotalCount.Should().Be(filter.ListId.Count);
            result.Items.ToArray()[0].Name.Should().Be(entities.ToList()[entities.Count() - 1].Name);
        }

        [Theory, AutoMoq]
        public async Task Create_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity,
            PhotographerDto dto)
        {
            //
            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.AddAsync(It.IsAny<Photographer>())).ReturnsAsync(() => entity);

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            Photographer result = await sut.AddAsync(dto);

            result.Should().NotBeNull();
            result.Should().BeOfType<Photographer>();
            repo.Verify(repo => repo.AddAsync(It.IsAny<Photographer>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Delete_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            await sut.DeleteAsync(entity.Id);

            repo.Verify(repo => repo.Delete(It.IsAny<Photographer>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Delete_SaveChangesException_Ko(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new CrudOperationException(CrudAction.DELETE));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.DeleteAsync(entity.Id);

            var result = await Assert.ThrowsAsync<CrudOperationException>(Actual);

            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<CrudOperationException>();
            result.Message.Should().Be(CrudOperationException.ERROR_DELETE);
        }

        [Theory, AutoMoq]
        public async Task Delete_NotFoundException_Ko(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => null);

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.DeleteAsync(entity.Id);

            var result = await Assert.ThrowsAsync<NoDbRecordException>(Actual);

            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());
            result.Should().NotBeNull();
            result.Should().BeOfType<NoDbRecordException>();
            result.Message.Should().Contain($"{entity.Id}");

        }
        [Theory, AutoMoq]
        public async Task Update_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity,
            PhotographerDto dto)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);
            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            await sut.UpdateAsync(dto);

            repo.Verify(repo => repo.Update(It.IsAny<Photographer>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Update_SaveChangesException_Ko(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity,
            PhotographerDto dto)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new CrudOperationException(CrudAction.UPDATE));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.UpdateAsync(dto);

            var result = await Assert.ThrowsAsync<CrudOperationException>(Actual);

            repo.Verify(repo => repo.Update(It.IsAny<Photographer>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<CrudOperationException>();
            result.Message.Should().Be(CrudOperationException.ERROR_UPDATE);
        }

        [Theory, AutoMoq]
        public async Task Modify_Ok(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity,
            PhotographerDto dto)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => 1);

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            await sut.UpdateAsync(dto);

            repo.Verify(repo => repo.Update(It.IsAny<Photographer>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public async Task Modify_SaveChangesException_Ko(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] Mock<IValidationService> validationService,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            Photographer entity)
        {

            var mapper = _fixture.CreateMapper();

            repo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            repo.Setup(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new CrudOperationException(CrudAction.UPDATE));

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            async Task Actual() => await sut.UpdateAsync(entity);

            var result = await Assert.ThrowsAsync<CrudOperationException>(Actual);

            repo.Verify(repo => repo.Update(It.IsAny<Photographer>()), Times.Once());
            repo.Verify(repo => repo.UnitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<CrudOperationException>();
            result.Message.Should().Be(CrudOperationException.ERROR_UPDATE);
        }

        [Theory, AutoMoq]
        public async Task Get_ByDynamicFilter_NoPaginationAsync(
            [Frozen] Mock<IRepository<Photographer>> repo,
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<IValidationService> validationService)
        {

            var mapper = _fixture.CreateMapper();

            IEnumerable<Photographer> entities = _fixture.GetMockEntities<Photographer>(10);

            repo.Setup(repo => repo.Get(It.IsAny<IDynamicExpression<Photographer>>()))
                .Returns(() => entities.BuildMock());

            var sut = new PhotographerService(repo.Object, validationService.Object, mapper, config);

            var filter = new PhotographerQueryFilter
            {
                ListId = entities.Select(c => c.Id).ToList(),
                Disabled = true
            };

            IPaginatedResult<PhotographerDto> result = await sut.Get<PhotographerDto, PhotographerQueryFilter>(filter);

            result.Should().NotBeNull();
            result.Should().BeOfType<PaginatedResult<PhotographerDto>>();
            result.Items.Count().Should().Be(entities.Count());
            result.TotalCount.Should().Be(entities.Count());
        }

    }
}

