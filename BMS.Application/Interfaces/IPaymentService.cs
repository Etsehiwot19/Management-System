using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentResponseDto>> GetAllPayments();
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByInvoice(int invoiceId);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByLease(int leaseId);
        Task<PaymentResponseDto> RecordPayment(RecordPaymentDto dto);
    }
}
