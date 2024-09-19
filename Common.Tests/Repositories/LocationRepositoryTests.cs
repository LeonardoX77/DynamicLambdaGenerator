using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using MockQueryable.Moq;
using Common.Tests.Infrastructure;
using Common.Core.Data.Context;
using Common.Domain.Entities;
using Common.Core.Generic.Repository;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;
using Common.WebApi.Application.Models.Location;
using Common.Tests.Infrastructure.AutoMoq;
using Microsoft.Extensions.Options;
using Common.Business.Repositories;
using Common.WebApi.Application.Models.Client;

namespace Common.Tests.Repositories
{
    public class LocationRepositoryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        public LocationRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoMoq]
        public void Get(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Location>> dbSetMock,
            Location entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(set => set.Find(It.IsAny<int>())).Returns(entity);

            var sut = new LocationRepository(ctxMock.Object);

            var result = sut.Get(entity.Id);

            result.Should().NotBeNull();

            result.Should().BeOfType<Location>();
        }

        [Theory, AutoMoq]
        public void Get_ByName_KO(
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<AppDbContext> ctxMock, string name)
        {

            var dbSetMock = new List<Location>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            LocationQueryFilter filter = new LocationQueryFilter() { Name = name };

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);
            var sut = new LocationRepository(ctxMock.Object);

            IQueryable<Location> result = sut.Get(
                new DynamicExpression<Location, LocationQueryFilter>(filter, config.Value));

            result.Count().Should().Be(0);
        }


        [Theory, AutoMoq]
        public void Add([Frozen] Mock<AppDbContext> ctxMock, Location entity)
        {
            var dbSetMock = new Mock<DbSet<Location>>();

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            var result = sut.Add(entity);

            dbSetMock.Verify(x => x.Add(It.IsAny<Location>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<Location>();
        }

        [Theory, AutoMoq]
        public void Delete([Frozen] Mock<AppDbContext> ctxMock, Location entity)
        {

            var setMock = new List<Location> { entity }.AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(setMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            sut.Delete(entity);

            setMock.Verify(x => x.Remove(It.IsAny<Location>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void Update([Frozen] Mock<AppDbContext> ctxMock)
        {
            var setMock = new List<Location> { new Location { Id = 1 } }
                .AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(setMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            sut.Update(new Location { Id = 1 });

            setMock.Verify(x => x.Update(It.IsAny<Location>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void Get_ByDynamicFilter_Equal(
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<AppDbContext> ctxMock)
        {
            Mock<DbSet<Location>> setMock = _fixture.GetMockEntities<Location>(10)
                .AsQueryable().BuildMockDbSet();
            LocationQueryFilter filter = new LocationQueryFilter() { Id = 10 };

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(setMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            IQueryable<Location> result = sut.Get(new DynamicExpression<Location, LocationQueryFilter>(filter, config.Value));

            result.Should().NotBeNull();
            result.Count().Should().Be(1);
        }

        [Theory, AutoMoq]
        public async Task GetAsync(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Location>> dbSetMock,
            Location entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(set => set.FindAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            var sut = new LocationRepository(ctxMock.Object);

            var result = await sut.GetAsync(entity.Id);

            result.Should().NotBeNull();

            result.Should().BeOfType<Location>();
        }

        [Theory, AutoMoq]
        public async Task AddAsync(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Location>> dbSetMock,
            Location entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            dbSetMock.Setup(set => set.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>())).Returns(() => new ValueTask<EntityEntry<Location>>());

            var result = await sut.AddAsync(entity);

            result.Should().NotBeNull();

            result.Should().BeOfType<Location>();

            dbSetMock.Verify(x => x.AddAsync(It.IsAny<Location>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void GetAll_KO([Frozen] Mock<AppDbContext> ctxMock)
        {
            var dbSetMock = new List<Location>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            var result = sut.GetAll();

            result.Should().BeEmpty();
        }
        [Theory, AutoMoq]
        public void Get_ById_KO([Frozen] Mock<AppDbContext> ctxMock, int id)
        {
            var dbSetMock = new List<Location>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Location>()).Returns(dbSetMock.Object);

            var sut = new LocationRepository(ctxMock.Object);

            var result = sut.Get(id);

            result.Should().BeNull();
        }

    }
}
