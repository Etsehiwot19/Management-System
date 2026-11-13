using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeasesController : ControllerBase
    {
        private readonly ILeaseService _leaseService;

        public LeasesController(ILeaseService leaseService)
        {
            _leaseService = leaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var leases = await _leaseService.GetAllLeases();
            return Ok(leases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var lease = await _leaseService.GetLeaseById(id);
                return Ok(lease);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("tenant/{tenantId}")]
        public async Task<IActionResult> GetByTenant(int tenantId)
        {
            var leases = await _leaseService.GetLeasesByTenant(tenantId);
            return Ok(leases);
        }

        [HttpGet("unit/{unitId}")]
        public async Task<IActionResult> GetByUnit(int unitId)
        {
            var leases = await _leaseService.GetLeasesByUnit(unitId);
            return Ok(leases);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreateLeaseDto dto)
        {
            try
            {
                var lease = await _leaseService.CreateLease(dto);
                return Ok(lease);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/renew")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Renew(int id, [FromBody] RenewLeaseDto dto)
        {
            try
            {
                var lease = await _leaseService.RenewLease(id, dto);
                return Ok(lease);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/terminate")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Terminate(int id, [FromBody] TerminateLeaseDto dto)
        {
            try
            {
                var lease = await _leaseService.TerminateLease(id, dto);
                return Ok(lease);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
