using Microsoft.AspNetCore.Mvc;
using Tutorial6.Repositories;
using Tutorial6.Models.DTOs;

namespace Tutorial6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        [HttpPost]
        [Route("add-product")]
        public async Task<IActionResult> AddProduct([FromBody] ProductWarehouseRequest request)
        {
            var result = await _warehouseRepository.AddProductAsync(request);
            if (result is null)
                return BadRequest("Unable to add product to warehouse.");

            return Ok(result);
        }

        [HttpPost]
        [Route("add-product-using-procedure")]
        public async Task<IActionResult> AddProductUsingProcedure([FromBody] ProductWarehouseRequest request)
        {
            var result = await _warehouseRepository.AddProductUsingProcedureAsync(request);
            if (result is null)
                return BadRequest("Unable to add product to warehouse using procedure.");

            return Ok(result);
        }
    }
}