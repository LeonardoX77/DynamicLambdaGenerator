using Common.Domain.Entities;
using AutoMapper;
using Common.Core.Generic.Services;
using Common.Core.Data.Interfaces;
using Microsoft.Extensions.Options;
using Common.Core.Generic.DynamicQueryFilter.DynamicExpressions;

namespace Common.Business.Services
{
    public class LocationService : BaseService<Location, int>
    {
        public LocationService(
            IRepository<Location> repository,
            IValidationService validationService,
            IMapper mapper,
            IOptions<DynamicFiltersConfiguration> config) : base(repository, validationService, mapper, config)
        {
        }
    }


}
