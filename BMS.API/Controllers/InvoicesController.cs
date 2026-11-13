using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var invoices = await _invoiceService.GetAllInvoices();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceById(id);
                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("lease/{leaseId}")]
        public async Task<IActionResult> GetByLease(int leaseId)
        {
            var invoices = await _invoiceService.GetInvoicesByLease(leaseId);
            return Ok(invoices);
        }

        [HttpGet("outstanding-balances")]
        public async Task<IActionResult> GetOutstandingBalances()
        {
            var balances = await _invoiceService.GetOutstandingBalances();
            return Ok(balances);
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GenerateMonthly([FromBody] GenerateInvoicesDto dto)
        {
            try
            {
                var invoices = await _invoiceService.GenerateMonthlyInvoices(dto);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("mark-overdue")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> MarkOverdue()
        {
            await _invoiceService.MarkOverdueInvoices();
            return Ok(new { message = "Overdue invoices updated successfully" });
        }
    }
}
