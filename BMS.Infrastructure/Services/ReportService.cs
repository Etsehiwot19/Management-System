using System.IO.Compression;
using System.Globalization;
using System.Xml;
using BMS.Application.DTOs;
using BMS.Application.Enums;
using BMS.Application.Interfaces;
using BMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BMS.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummary()
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);
            var sixMonthStart = monthStart.AddMonths(-5);

            var units = await _context.Units.AsNoTracking().ToListAsync();
            var invoiceBalances = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status != InvoiceStatus.Cancelled)
                .Select(i => new
                {
                    i.Status,
                    Balance = i.AmountDue - i.Payments.Sum(p => (decimal?)p.AmountPaid) ?? i.AmountDue
                })
                .ToListAsync();

            var monthlyRevenue = await _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= sixMonthStart && p.PaymentDate < nextMonth)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new RevenueReportItemDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(p => p.AmountPaid)
                })
                .ToListAsync();

            var revenueByMonth = monthlyRevenue.ToDictionary(x => (x.Year, x.Month), x => x.Amount);
            var revenue = Enumerable.Range(0, 6)
                .Select(offset => monthStart.AddMonths(offset - 5))
                .Select(date => new RevenueReportItemDto
                {
                    Year = date.Year,
                    Month = date.Month,
                    Amount = revenueByMonth.TryGetValue((date.Year, date.Month), out var amount) ? amount : 0
                })
                .ToList();

            return new DashboardSummaryDto
            {
                TotalUnits = units.Count,
                AvailableUnits = units.Count(u => u.Status == UnitStatus.Available),
                OccupiedUnits = units.Count(u => u.Status == UnitStatus.Occupied),
                ActiveLeases = await _context.Leases.CountAsync(l => l.Status == LeaseStatus.Active),
                ActiveTenants = await _context.Tenants.CountAsync(t => t.IsActive),
                RevenueThisMonth = monthlyRevenue
                    .Where(x => x.Year == today.Year && x.Month == today.Month)
                    .Sum(x => x.Amount),
                OutstandingBalance = invoiceBalances.Sum(x => Math.Max(x.Balance, 0)),
                OverdueInvoiceCount = invoiceBalances.Count(x => x.Status == InvoiceStatus.Overdue && x.Balance > 0),
                Revenue = revenue
            };
        }

        public async Task<ReportsResponseDto> GetReports(ReportFilterDto filters)
        {
            var today = DateTime.UtcNow.Date;
            var fromDate = (filters.FromDate ?? today.AddMonths(-11)).Date;
            var toDate = (filters.ToDate ?? today).Date;
            var expiryStart = today;
            var expiryEnd = today.AddDays(filters.ExpiryWithinDays);
            var toExclusive = toDate.AddDays(1);

            var totalUnits = await _context.Units.CountAsync();
            var occupiedUnits = await _context.Units.CountAsync(u => u.Status == UnitStatus.Occupied);
            var availableUnits = await _context.Units.CountAsync(u => u.Status == UnitStatus.Available);
            var revenue = await _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate < toExclusive)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new RevenueReportItemDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(p => p.AmountPaid)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var arrears = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.Status != InvoiceStatus.Cancelled
                    && i.DueDate < today
                    && i.DueDate < toExclusive
                    && i.DueDate >= fromDate)
                .Select(i => new ArrearsReportItemDto
                {
                    InvoiceId = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    TenantName = i.Lease.Tenant.OrganizationName,
                    UnitNumber = i.Lease.Unit.UnitNumber,
                    DueDate = i.DueDate,
                    AmountDue = i.AmountDue,
                    AmountPaid = i.Payments.Sum(p => (decimal?)p.AmountPaid) ?? 0,
                    OutstandingBalance = i.AmountDue - (i.Payments.Sum(p => (decimal?)p.AmountPaid) ?? 0)
                })
                .Where(i => i.OutstandingBalance > 0)
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            var leaseExpiries = await _context.Leases
                .AsNoTracking()
                .Where(l => l.Status == LeaseStatus.Active
                    && l.EndDate >= expiryStart
                    && l.EndDate < expiryEnd.AddDays(1))
                .Select(l => new LeaseExpiryReportItemDto
                {
                    LeaseId = l.Id,
                    TenantName = l.Tenant.OrganizationName,
                    UnitNumber = l.Unit.UnitNumber,
                    EndDate = l.EndDate,
                    MonthlyRent = l.MonthlyRent
                })
                .OrderBy(l => l.EndDate)
                .ToListAsync();

            var documentExpiries = await _context.LegalDocuments
                .AsNoTracking()
                .Where(d => d.ExpiryDate.HasValue
                    && d.ExpiryDate.Value >= expiryStart
                    && d.ExpiryDate.Value < expiryEnd.AddDays(1))
                .Select(d => new DocumentExpiryReportItemDto
                {
                    DocumentId = d.Id,
                    TenantName = d.Tenant.OrganizationName,
                    DocumentType = d.DocumentType.ToString(),
                    ExpiryDate = d.ExpiryDate!.Value,
                    IsVerified = d.IsVerified
                })
                .OrderBy(d => d.ExpiryDate)
                .ToListAsync();

            return new ReportsResponseDto
            {
                Occupancy = new OccupancyReportDto
                {
                    TotalUnits = totalUnits,
                    OccupiedUnits = occupiedUnits,
                    AvailableUnits = availableUnits,
                    OccupancyRate = totalUnits == 0 ? 0 : decimal.Round(occupiedUnits * 100m / totalUnits, 2)
                },
                Revenue = revenue,
                Arrears = arrears,
                LeaseExpiries = leaseExpiries,
                DocumentExpiries = documentExpiries
            };
        }

        public async Task<byte[]> ExportReports(ReportFilterDto filters)
        {
            var reports = await GetReports(filters);
            using var stream = new MemoryStream();
            var sheets = new (string Name, IEnumerable<object[]> Rows)[]
            {
                ("Occupancy", new[]
                {
                    new object[] { "Total units", "Occupied units", "Available units", "Occupancy rate (%)" },
                    new object[] { reports.Occupancy.TotalUnits, reports.Occupancy.OccupiedUnits,
                        reports.Occupancy.AvailableUnits, reports.Occupancy.OccupancyRate }
                }),
                ("Revenue", new[] { new object[] { "Year", "Month", "Revenue" } }
                    .Concat(reports.Revenue.Select(x => new object[]
                    {
                        x.Year, x.Month, x.Amount
                    }))),
                ("Arrears", new[] { new object[] { "Invoice", "Tenant", "Unit", "Due date", "Amount due", "Amount paid", "Balance" } }
                    .Concat(reports.Arrears.Select(x => new object[]
                    {
                        x.InvoiceNumber, x.TenantName, x.UnitNumber, x.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        x.AmountDue, x.AmountPaid, x.OutstandingBalance
                    }))),
                ("Lease expiry", new[] { new object[] { "Lease", "Tenant", "Unit", "End date", "Monthly rent" } }
                    .Concat(reports.LeaseExpiries.Select(x => new object[]
                    {
                        x.LeaseId, x.TenantName, x.UnitNumber,
                        x.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), x.MonthlyRent
                    }))),
                ("Document expiry", new[] { new object[] { "Document", "Tenant", "Type", "Expiry date", "Verified" } }
                    .Concat(reports.DocumentExpiries.Select(x => new object[]
                    {
                        x.DocumentId, x.TenantName, x.DocumentType,
                        x.ExpiryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), x.IsVerified
                    })))
            };

            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
            {
                WriteContentTypes(archive, sheets.Length);
                WritePackageRelationships(archive);
                WriteWorkbook(archive, sheets);
                for (var index = 0; index < sheets.Length; index++)
                    WriteWorksheet(archive, index + 1, sheets[index].Rows);
            }

            return stream.ToArray();
        }

        private static void WriteContentTypes(ZipArchive archive, int sheetCount)
        {
            WriteEntry(archive, "[Content_Types].xml", writer =>
            {
                const string contentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";
                writer.WriteStartElement("Types", contentTypesNamespace);
                writer.WriteStartElement("Default", contentTypesNamespace);
                writer.WriteAttributeString("Extension", "rels");
                writer.WriteAttributeString("ContentType", "application/vnd.openxmlformats-package.relationships+xml");
                writer.WriteEndElement();
                writer.WriteStartElement("Default", contentTypesNamespace);
                writer.WriteAttributeString("Extension", "xml");
                writer.WriteAttributeString("ContentType", "application/xml");
                writer.WriteEndElement();
                WriteContentTypeOverride(writer, "/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
                for (var index = 1; index <= sheetCount; index++)
                    WriteContentTypeOverride(writer, $"/xl/worksheets/sheet{index}.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
                writer.WriteEndElement();
            });
        }

        private static void WritePackageRelationships(ZipArchive archive)
        {
            WriteEntry(archive, "_rels/.rels", writer =>
            {
                const string relationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
                writer.WriteStartElement("Relationships", relationshipsNamespace);
                writer.WriteStartElement("Relationship", relationshipsNamespace);
                writer.WriteAttributeString("Id", "rId1");
                writer.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
                writer.WriteAttributeString("Target", "xl/workbook.xml");
                writer.WriteEndElement();
                writer.WriteEndElement();
            });
        }

        private static void WriteWorkbook(ZipArchive archive, (string Name, IEnumerable<object[]> Rows)[] sheets)
        {
            const string spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            const string documentRelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
            WriteEntry(archive, "xl/workbook.xml", writer =>
            {
                writer.WriteStartElement("workbook", spreadsheetNamespace);
                writer.WriteAttributeString("xmlns", "r", null, documentRelationshipsNamespace);
                writer.WriteStartElement("sheets", spreadsheetNamespace);
                for (var index = 0; index < sheets.Length; index++)
                {
                    writer.WriteStartElement("sheet", spreadsheetNamespace);
                    writer.WriteAttributeString("name", sheets[index].Name);
                    writer.WriteAttributeString("sheetId", (index + 1).ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("r", "id", documentRelationshipsNamespace, $"rId{index + 1}");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            });

            WriteEntry(archive, "xl/_rels/workbook.xml.rels", writer =>
            {
                const string relationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
                writer.WriteStartElement("Relationships", relationshipsNamespace);
                for (var index = 0; index < sheets.Length; index++)
                {
                    writer.WriteStartElement("Relationship", relationshipsNamespace);
                    writer.WriteAttributeString("Id", $"rId{index + 1}");
                    writer.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet");
                    writer.WriteAttributeString("Target", $"worksheets/sheet{index + 1}.xml");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            });
        }

        private static void WriteWorksheet(ZipArchive archive, int sheetNumber, IEnumerable<object[]> rows)
        {
            const string spreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            WriteEntry(archive, $"xl/worksheets/sheet{sheetNumber}.xml", writer =>
            {
                writer.WriteStartElement("worksheet", spreadsheetNamespace);
                writer.WriteStartElement("sheetData", spreadsheetNamespace);
                var rowNumber = 0;
                foreach (var row in rows)
                {
                    rowNumber++;
                    writer.WriteStartElement("row", spreadsheetNamespace);
                    writer.WriteAttributeString("r", rowNumber.ToString(CultureInfo.InvariantCulture));
                    for (var column = 0; column < row.Length; column++)
                    {
                        var value = row[column];
                        writer.WriteStartElement("c", spreadsheetNamespace);
                        writer.WriteAttributeString("r", $"{ColumnName(column + 1)}{rowNumber}");
                        if (value is bool boolean)
                        {
                            writer.WriteAttributeString("t", "b");
                            writer.WriteElementString("v", spreadsheetNamespace, boolean ? "1" : "0");
                        }
                        else if (value is byte or short or int or long or decimal or float or double)
                        {
                            writer.WriteAttributeString("t", "n");
                            writer.WriteElementString("v", spreadsheetNamespace, Convert.ToString(value, CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            writer.WriteAttributeString("t", "inlineStr");
                            writer.WriteStartElement("is", spreadsheetNamespace);
                            writer.WriteStartElement("t", spreadsheetNamespace);
                            writer.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                            writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                            writer.WriteEndElement();
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            });
        }

        private static void WriteContentTypeOverride(XmlWriter writer, string partName, string contentType)
        {
            const string contentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";
            writer.WriteStartElement("Override", contentTypesNamespace);
            writer.WriteAttributeString("PartName", partName);
            writer.WriteAttributeString("ContentType", contentType);
            writer.WriteEndElement();
        }

        private static void WriteEntry(ZipArchive archive, string path, Action<XmlWriter> writeContent)
        {
            var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
            using var stream = entry.Open();
            using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                Indent = true
            });
            writer.WriteStartDocument();
            writeContent(writer);
            writer.WriteEndDocument();
        }

        private static string ColumnName(int column)
        {
            var name = string.Empty;
            while (column > 0)
            {
                column--;
                name = (char)('A' + column % 26) + name;
                column /= 26;
            }
            return name;
        }
    }
}
