using Jude.Server.Core.Helpers;
using Jude.Server.Data.Models;
using OfficeOpenXml;

namespace Jude.Server.Domains.Claims;

public interface IExcelClaimParser
{
    Task<Result<List<ClaimModel>>> ParseExcelAsync(Stream excelStream);
}

public class ExcelClaimParser : IExcelClaimParser
{
    private readonly ILogger<ExcelClaimParser> _logger;

    public ExcelClaimParser(ILogger<ExcelClaimParser> logger)
    {
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<Result<List<ClaimModel>>> ParseExcelAsync(Stream excelStream)
    {
        try
        {
            using var package = new ExcelPackage(excelStream);
            var worksheet = package.Workbook.Worksheets[0];

            if (worksheet == null || worksheet.Dimension == null)
            {
                return Result.Fail("Excel file is empty or invalid");
            }

            var claims = new List<ClaimModel>();
            var headers = GetHeaders(worksheet);

            if (!ValidateHeaders(headers, out var missingColumns))
            {
                return Result.Fail(
                    $"Missing required columns: {string.Join(", ", missingColumns)}"
                );
            }

            var rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var claim = ParseRow(worksheet, row, headers);
                    if (claim != null)
                    {
                        claims.Add(claim);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Error parsing row {Row}. Skipping this row.",
                        row
                    );
                }
            }

            _logger.LogInformation("Successfully parsed {Count} claims from Excel", claims.Count);
            return Result.Ok(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Excel file");
            return Result.Fail($"Error parsing Excel file: {ex.Message}");
        }
    }

    private Dictionary<string, int> GetHeaders(ExcelWorksheet worksheet)
    {
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var colCount = worksheet.Dimension.End.Column;

        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim();
            if (!string.IsNullOrEmpty(header))
            {
                headers[header] = col;
            }
        }

        return headers;
    }

    private bool ValidateHeaders(
        Dictionary<string, int> headers,
        out List<string> missingColumns
    )
    {
        var requiredColumns = new[]
        {
            "MEMBER NO",
            "CLAIM NO",
            "SERVICE DATE",
            "AMOUNT CLAIMED",
        };

        missingColumns = requiredColumns.Where(col => !headers.ContainsKey(col)).ToList();
        return missingColumns.Count == 0;
    }

    private ClaimModel? ParseRow(
        ExcelWorksheet worksheet,
        int row,
        Dictionary<string, int> headers
    )
    {
        var claim = new ClaimModel
        {
            Id = Guid.NewGuid(),
            IngestedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ClaimStatus.Pending,
            Source = ClaimSource.Upload,
        };

        claim.MemberNumber = GetCellValue(worksheet, row, headers, "MEMBER NO");
        claim.ClaimNumber = GetCellValue(worksheet, row, headers, "CLAIM NO");

        var providerText = GetCellValue(worksheet, row, headers, "PROVIDER");
        ExtractPatientNameFromProvider(providerText, out var firstName, out var surname);
        claim.PatientFirstName = firstName;
        claim.PatientSurname = surname;

        claim.MedicalSchemeName =
            GetCellValue(worksheet, row, headers, "MEDICAL SCHEME") ?? "CIMAS";
        claim.OptionName = GetCellValue(worksheet, row, headers, "OPTION NAME");
        claim.PayerName = GetCellValue(worksheet, row, headers, "PAYER NAME");

        claim.ProviderName = providerText;
        claim.PracticeNumber = GetCellValue(worksheet, row, headers, "PRACTICE NO");
        claim.InvoiceReference = GetCellValue(worksheet, row, headers, "INV REF");

        claim.ServiceDate = GetDateValue(worksheet, row, headers, "SERVICE DATE");
        claim.AssessmentDate = GetDateValue(worksheet, row, headers, "ASSESS DATE");
        claim.DateReceived = GetDateValue(worksheet, row, headers, "DATE RECEIVED");

        claim.ClaimCode = GetCellValue(worksheet, row, headers, "CLM CODE");
        claim.CodeDescription = GetCellValue(worksheet, row, headers, "CODE DESCRIPTION");
        claim.Units = GetIntValue(worksheet, row, headers, "UNITS");

        claim.TotalClaimAmount = GetDecimalValue(worksheet, row, headers, "AMOUNT CLAIMED");
        claim.TotalAmountPaid = GetDecimalValue(worksheet, row, headers, "TOTAL AMOUNT PAID");
        claim.CoPayAmount = GetDecimalValue(worksheet, row, headers, "CO-PAY");

        claim.AssessorName = GetCellValue(worksheet, row, headers, "ASSESSOR NAME");
        claim.ClaimTypeCode = GetCellValue(worksheet, row, headers, "CLAIM TYPE CODE") ?? "";

        claim.PatientBirthDate = GetDateValue(worksheet, row, headers, "BIRTHDATE");
        claim.PatientCurrentAge = GetIntValue(worksheet, row, headers, "CURRENT AGE");

        if (string.IsNullOrEmpty(claim.ClaimNumber))
        {
            _logger.LogWarning("Row {Row} has no claim number, skipping", row);
            return null;
        }

        return claim;
    }

    private string GetCellValue(
        ExcelWorksheet worksheet,
        int row,
        Dictionary<string, int> headers,
        string columnName
    )
    {
        if (!headers.TryGetValue(columnName, out var col))
        {
            return string.Empty;
        }

        return worksheet.Cells[row, col].Text?.Trim() ?? string.Empty;
    }

    private DateTime GetDateValue(
        ExcelWorksheet worksheet,
        int row,
        Dictionary<string, int> headers,
        string columnName
    )
    {
        if (!headers.TryGetValue(columnName, out var col))
        {
            return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        }

        var cell = worksheet.Cells[row, col];

        if (cell.Value is DateTime dateValue)
        {
            return DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
        }

        if (DateTime.TryParse(cell.Text, out var parsedDate))
        {
            return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }

        return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
    }

    private decimal GetDecimalValue(
        ExcelWorksheet worksheet,
        int row,
        Dictionary<string, int> headers,
        string columnName
    )
    {
        if (!headers.TryGetValue(columnName, out var col))
        {
            return 0m;
        }

        var cell = worksheet.Cells[row, col];

        if (cell.Value is double doubleValue)
        {
            return (decimal)doubleValue;
        }

        if (cell.Value is decimal decimalValue)
        {
            return decimalValue;
        }

        var text = cell.Text?.Replace(",", "").Replace(" ", "").Trim();
        if (decimal.TryParse(text, out var parsedValue))
        {
            return parsedValue;
        }

        return 0m;
    }

    private int GetIntValue(
        ExcelWorksheet worksheet,
        int row,
        Dictionary<string, int> headers,
        string columnName
    )
    {
        if (!headers.TryGetValue(columnName, out var col))
        {
            return 0;
        }

        var cell = worksheet.Cells[row, col];

        if (cell.Value is int intValue)
        {
            return intValue;
        }

        if (cell.Value is double doubleValue)
        {
            return (int)doubleValue;
        }

        if (int.TryParse(cell.Text, out var parsedValue))
        {
            return parsedValue;
        }

        return 0;
    }

    private void ExtractPatientNameFromProvider(
        string providerText,
        out string firstName,
        out string surname
    )
    {
        firstName = "Unknown";
        surname = "Patient";

        if (string.IsNullOrWhiteSpace(providerText))
        {
            return;
        }
    }
}

