using BMS.Application.DTOs;
using BMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentService.GetAllPayments();
            return Ok(payments);
        }

        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetByInvoice(int invoiceId)
        {
            var payments = await _paymentService.GetPaymentsByInvoice(invoiceId);
            return Ok(payments);
        }

        [HttpGet("lease/{leaseId}")]
        public async Task<IActionResult> GetByLease(int leaseId)
        {
            var payments = await _paymentService.GetPaymentsByLease(leaseId);
            return Ok(payments);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.RecordPayment(dto);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
