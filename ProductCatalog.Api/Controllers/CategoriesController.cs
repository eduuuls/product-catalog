using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Application.ViewModels;
using ProductCatalog.Domain.Enums;

namespace ProductCatalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoriesController : ApiController
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly ICategoryService _categoryService;

        public CategoriesController(ILogger<CategoriesController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CategoryViewModel categoryViewModel)
        {
            try
            {
                var result = await _categoryService.Create(categoryViewModel);

                return CustomResponse(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet("provider/{dataProvider}")]
        public async Task<IActionResult> GetByDataProvider(DataProvider dataProvider, [FromQuery] bool onlyActive)
        {
            try
            {
                var result = await _categoryService.GetByDataProvider(dataProvider, onlyActive);

                return CustomResponse(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, [FromBody] CategoryViewModel categoryViewModel)
        {
            categoryViewModel.Id = id;
            var result = await _categoryService.Update(categoryViewModel);

            return CustomResponse(result);
        }
    }
}
