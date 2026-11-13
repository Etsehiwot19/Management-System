using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoices();
        Task<InvoiceResponseDto> GetInvoiceById(int id);
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByLease(int leaseId);
        Task<IEnumerable<InvoiceResponseDto>> GenerateMonthlyInvoices(GenerateInvoicesDto dto);
        Task<IEnumerable<OutstandingBalanceDto>> GetOutstandingBalances();
        Task MarkOverdueInvoices();
    }
}
