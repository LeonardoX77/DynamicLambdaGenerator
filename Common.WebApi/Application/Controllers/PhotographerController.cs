using Common.Domain.Entities;
using Common.WebApi.Application.Models.Photographer;
using Microsoft.AspNetCore.Mvc;
using Common.Core.Data.Interfaces;
using Common.Core.Generic.Controllers;

namespace Common.WebApi.Application.Controllers
{
    /// <summary>
    /// Controller for managing Photographers.
    /// </summary>
    public class PhotographerController : GenericControllerBase<Photographer, PhotographerDto, PhotographerDto, PhotographerQueryFilter, PhotographerDtoValidator>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="PhotographerService">Photographer service.</param>
        public PhotographerController(
            ILogger<PhotographerController> logger,
            IBaseService<Photographer, int> PhotographerService) : base(logger, PhotographerService)
        {
        }
    }
}
