using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboard()
        {
            return Ok(await _reportService.GetDashboardSummary());
        }

        [HttpGet]
        public async Task<ActionResult<ReportsResponseDto>> GetReports([FromQuery] ReportFilterDto filters)
        {
            if (filters.FromDate.HasValue && filters.ToDate.HasValue
                && filters.FromDate.Value.Date > filters.ToDate.Value.Date)
                return BadRequest(new { message = "FromDate must be on or before ToDate." });
            if (filters.ToDate.HasValue && filters.ToDate.Value.Date == DateTime.MaxValue.Date)
                return BadRequest(new { message = "ToDate is outside the supported report range." });
            if (filters.ExpiryWithinDays < 0 || filters.ExpiryWithinDays > 3650)
                return BadRequest(new { message = "ExpiryWithinDays must be between 0 and 3650." });

            return Ok(await _reportService.GetReports(filters));
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] ReportFilterDto filters)
        {
            if (filters.FromDate.HasValue && filters.ToDate.HasValue
                && filters.FromDate.Value.Date > filters.ToDate.Value.Date)
                return BadRequest(new { message = "FromDate must be on or before ToDate." });
            if (filters.ToDate.HasValue && filters.ToDate.Value.Date == DateTime.MaxValue.Date)
                return BadRequest(new { message = "ToDate is outside the supported report range." });
            if (filters.ExpiryWithinDays < 0 || filters.ExpiryWithinDays > 3650)
                return BadRequest(new { message = "ExpiryWithinDays must be between 0 and 3650." });

            var file = await _reportService.ExportReports(filters);
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"bms-reports-{DateTime.UtcNow:yyyyMMdd}.xlsx");
        }
    }
}
