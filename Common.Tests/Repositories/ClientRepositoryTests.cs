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
using Common.WebApi.Application.Models.Client;
using Common.Tests.Infrastructure.AutoMoq;
using Microsoft.Extensions.Options;
using Common.Business.Repositories;

namespace Common.Tests.Repositories
{
    public class ClientRepositoryTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        public ClientRepositoryTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, AutoMoq]
        public void Get(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Client>> dbSetMock,
            Client entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(set => set.Find(It.IsAny<int>())).Returns(entity);

            var sut = new ClientRepository(ctxMock.Object);

            var result = sut.Get(entity.Id);

            result.Should().NotBeNull();

            result.Should().BeOfType<Client>();
        }

        [Theory, AutoMoq]
        public void Get_ByName_KO(
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<AppDbContext> ctxMock, string name)
        {

            var dbSetMock = new List<Client>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            ClientQueryFilter filter = new ClientQueryFilter() { Name = name };

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);
            var sut = new ClientRepository(ctxMock.Object);

            IQueryable<Client> result = sut.Get(
                new DynamicExpression<Client, ClientQueryFilter>(filter, config.Value));

            result.Count().Should().Be(0);
        }


        [Theory, AutoMoq]
        public void Add([Frozen] Mock<AppDbContext> ctxMock, Client entity)
        {
            var dbSetMock = new Mock<DbSet<Client>>();

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            var result = sut.Add(entity);

            dbSetMock.Verify(x => x.Add(It.IsAny<Client>()), Times.Once());
            result.Should().NotBeNull();
            result.Should().BeOfType<Client>();
        }

        [Theory, AutoMoq]
        public void Delete([Frozen] Mock<AppDbContext> ctxMock, Client entity)
        {

            var setMock = new List<Client> { entity }.AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(setMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            sut.Delete(entity);

            setMock.Verify(x => x.Remove(It.IsAny<Client>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void Update([Frozen] Mock<AppDbContext> ctxMock)
        {
            var setMock = new List<Client> { new Client { Id = 1 } }
                .AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(setMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            sut.Update(new Client { Id = 1 });

            setMock.Verify(x => x.Update(It.IsAny<Client>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void Get_ByDynamicFilter_Equal(
            [Frozen] IOptions<DynamicFiltersConfiguration> config,
            [Frozen] Mock<AppDbContext> ctxMock)
        {
            //setup a list of 10 Clients
            Mock<DbSet<Client>> setMock = _fixture.GetMockEntities<Client>(10).AsQueryable().BuildMockDbSet();
            ClientQueryFilter filter = new ClientQueryFilter() { Id = 10 };

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(setMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            IQueryable<Client> result = sut.Get(new DynamicExpression<Client, ClientQueryFilter>(filter, config.Value));

            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().Id.Should().Be(10);
        }

        [Theory, AutoMoq]
        public async Task GetAsync(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Client>> dbSetMock,
            Client entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            dbSetMock.Setup(set => set.FindAsync(It.IsAny<int>())).ReturnsAsync(() => entity);

            var sut = new ClientRepository(ctxMock.Object);

            var result = await sut.GetAsync(entity.Id);

            result.Should().NotBeNull();

            result.Should().BeOfType<Client>();
        }

        [Theory, AutoMoq]
        public async Task AddAsync(
            [Frozen] Mock<AppDbContext> ctxMock,
            [Frozen] Mock<DbSet<Client>> dbSetMock,
            Client entity)
        {
            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            dbSetMock.Setup(set => set.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>())).Returns(() => new ValueTask<EntityEntry<Client>>());

            var result = await sut.AddAsync(entity);

            result.Should().NotBeNull();

            result.Should().BeOfType<Client>();

            dbSetMock.Verify(x => x.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Theory, AutoMoq]
        public void GetAll_KO([Frozen] Mock<AppDbContext> ctxMock)
        {
            var dbSetMock = new List<Client>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            var result = sut.GetAll();

            result.Should().BeEmpty();
        }
        [Theory, AutoMoq]
        public void Get_ById_KO([Frozen] Mock<AppDbContext> ctxMock, int id)
        {
            var dbSetMock = new List<Client>().AsQueryable().BuildMockDbSet();

            ctxMock.Setup(ctx => ctx.Set<Client>()).Returns(dbSetMock.Object);

            var sut = new ClientRepository(ctxMock.Object);

            var result = sut.Get(id);

            result.Should().BeNull();
        }

    }
}
