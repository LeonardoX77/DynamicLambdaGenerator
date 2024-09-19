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
using Common.WebApi.Application.Models.Session;
using Common.Tests.Infrastructure.AutoMoq;
using Microsoft.Extensions.Options;
using Common.Business.Repositories;
using Common.WebApi.Application.Models.Photographer;

namespace Common.Tests.Repositories
{
    public class SessionRepositoryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        public SessionRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoMoq]
        public void Get(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Session>> dbSetMock,
            Session entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(set => set.Find(It.IsAny<int>())).Returns(entity);

            var sut = new SessionRepository(ctxMock.Object);

            var result = sut.Get(entity.Id);

            result.Should().NotBeNull();

            result.Should().BeOfType<Session>();
        }

        [Theory, AutoMoq]
        public void Get_ByPhotographerId_KO(
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<AppDbContext> ctxMock, int photographerId)
        {

            var dbSetMock = new List<Session>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            SessionQueryFilter filter = new SessionQueryFilter() { PhotographerId = photographerId };

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);
            var sut = new SessionRepository(ctxMock.Object);

            IQueryable<Session> result = sut.Get(
                new DynamicExpression<Session, SessionQueryFilter>(filter, config.Value));

            result.Count().Should().Be(0);
        }


        [Theory, AutoMoq]
        public void Add([Frozen] Mock<AppDbContext> ctxMock, Session entity)
        {
            var dbSetMock = new Mock<DbSet<Session>>();

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            var result = sut.Add(entity);

            dbSetMock.Verify(x => x.Add(It.IsAny<Session>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<Session>();
        }

        [Theory, AutoMoq]
        public void Delete([Frozen] Mock<AppDbContext> ctxMock, Session entity)
        {

            var setMock = new List<Session> { entity }.AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(setMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            sut.Delete(entity);

            setMock.Verify(x => x.Remove(It.IsAny<Session>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void Update([Frozen] Mock<AppDbContext> ctxMock)
        {
            var setMock = new List<Session> { new Session { Id = 1 } }
                .AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(setMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            sut.Update(new Session { Id = 1 });

            setMock.Verify(x => x.Update(It.IsAny<Session>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void Get_ByDynamicFilter_Equal(
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<AppDbContext> ctxMock)
        {
            Mock<DbSet<Session>> setMock = _fixture.GetMockEntities<Session>(10)
                .AsQueryable().BuildMockDbSet();
            SessionQueryFilter filter = new SessionQueryFilter() { Id = 10 };

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(setMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            IQueryable<Session> result = sut.Get(new DynamicExpression<Session, SessionQueryFilter>(filter, config.Value));

            result.Should().NotBeNull();
            result.Count().Should().Be(1);
        }

        [Theory, AutoMoq]
        public async Task GetAsync(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Session>> dbSetMock,
            Session entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(set => set.FindAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            var sut = new SessionRepository(ctxMock.Object);

            var result = await sut.GetAsync(entity.Id);

            result.Should().NotBeNull();

            result.Should().BeOfType<Session>();
        }

        [Theory, AutoMoq]
        public async Task AddAsync(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Session>> dbSetMock,
            Session entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            dbSetMock.Setup(set => set.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>())).Returns(() => new ValueTask<EntityEntry<Session>>());

            var result = await sut.AddAsync(entity);

            result.Should().NotBeNull();

            result.Should().BeOfType<Session>();

            dbSetMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void GetAll_KO([Frozen] Mock<AppDbContext> ctxMock)
        {
            var dbSetMock = new List<Session>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            var result = sut.GetAll();

            result.Should().BeEmpty();
        }
        [Theory, AutoMoq]
        public void Get_ById_KO([Frozen] Mock<AppDbContext> ctxMock, int id)
        {
            var dbSetMock = new List<Session>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Session>()).Returns(dbSetMock.Object);

            var sut = new SessionRepository(ctxMock.Object);

            var result = sut.Get(id);

            result.Should().BeNull();
        }

    }
}
