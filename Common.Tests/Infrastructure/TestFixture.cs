using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Options;
using Common.WebApi.Application.Mappings;
using Common.Core.Data.Interfaces;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;
using Common.Core.Generic.QueryLanguage.Interfaces;
using MockQueryable.Moq;

namespace Common.Tests.Infrastructure
{
    public class TestFixture
    {
        private readonly Random _random;

        public TestFixture()
        {
            _random = new Random(Environment.TickCount);
        }

        public IMapper CreateMapper()
        {
            var provider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

            return provider.CreateMapper();
        }

        public string GetString(int size = 100)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, size)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public string GetEmail()
        {
            return $"{GetString(10)}@mail.com".ToLower();
        }

        public Uri GetUri()
        {
            return new Uri($"https://{GetString(5)}domain.com");
        }

        public int GetInt(int from, int to)
        {
            return _random.Next(from, to);
        }

        public int GetInt(int max)
        {
            return _random.Next(max);
        }

        public bool GetBool()
        {
            return _random.Next(0, 1) == 1;
        }

        public DateTime GetFutureDateTime(int from = 0, int days = 100)
        {
            return DateTime.UtcNow.AddDays(from).AddDays(_random.Next(days));
        }

        public DateTime GetPastDateTime(int days = 100)
        {
            return DateTime.UtcNow.AddDays(-1 * _random.Next(days));
        }

        public IEnumerable<T> GetMockEntities<T>(int setSize) where T : class, IEntity
        {
            var entities = new List<T>();

            for (int i = 10; i < setSize + 10; i++)
            {
                var entity = Activator.CreateInstance<T>();

                entity.Id = i;
                entities.Add(entity);
            }

            return entities;
        }

        public static Func<IDynamicExpression<T>, IQueryable<T>> MockDynamicFilter<T>(IQueryable<T> query)
            where T : class
        {
            return (IDynamicExpression<T> exp) =>
            {
                var filteredEntities = query.Where(exp.Predicate()).AsQueryable();
                var filteredEntitiesMockl = filteredEntities.BuildMock();
                return filteredEntitiesMockl;
            };
        }
    }
}

