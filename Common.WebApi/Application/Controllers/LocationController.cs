using Common.Domain.Entities;
using Common.WebApi.Application.Models.Location;
using Microsoft.AspNetCore.Mvc;
using Common.Core.Data.Interfaces;
using Common.WebApi.Application.Models.Photographer;
using Common.Core.Generic.Controllers;

namespace Common.WebApi.Application.Controllers
{
    /// <summary>
    /// Controller for managing Locations.
    /// </summary>
    public class LocationController : GenericControllerBase<Location, LocationDto, LocationDto, LocationQueryFilter, LocationDtoValidator>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="LocationService">Location service.</param>
        public LocationController(
            ILogger<LocationController> logger,
            IBaseService<Location, int> LocationService) : base(logger, LocationService)
        {
        }
    }
}
