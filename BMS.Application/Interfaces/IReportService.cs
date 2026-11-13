using BMS.Application.DTOs;

namespace BMS.Application.Interfaces
{
    public interface IReportService
    {
        Task<DashboardSummaryDto> GetDashboardSummary();
        Task<ReportsResponseDto> GetReports(ReportFilterDto filters);
        Task<byte[]> ExportReports(ReportFilterDto filters);
    }
}
