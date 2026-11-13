using BMS.Application.DTOs;
using BMS.Application.Enums;
using BMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var units = await _unitService.GetAllUnits();
            return Ok(units);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var unit = await _unitService.GetUnitById(id);
                return Ok(unit);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("floor/{floorNumber}")]
        public async Task<IActionResult> GetByFloor(int floorNumber)
        {
            var units = await _unitService.GetUnitsByFloor(floorNumber);
            return Ok(units);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUnitDto dto)
        {
            try
            {
                var unit = await _unitService.CreateUnit(dto);
                return Ok(unit);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitDto dto)
        {
            try
            {
                var unit = await _unitService.UpdateUnit(id, dto);
                return Ok(unit);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _unitService.DeleteUnit(id);
                return Ok(new { message = "Unit deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] UnitStatus status)
        {
            try
            {
                await _unitService.ChangeUnitStatus(id, status);
                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
