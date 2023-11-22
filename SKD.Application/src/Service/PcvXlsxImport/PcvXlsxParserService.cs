using OfficeOpenXml;
using SKD.Application.Common;

namespace SKD.Service;

public class PcvXlsxParserService : IAppService {
    private const int PcvStartColumn = 3;
    private const int PcvMaxColumn = 100;
    private const int PcvPropertiesStartRow = 1;
    private const int PcvPropertiesEndRow = 10;
    private const int ComponentCodesRow = 10;
    private const int ComponentCodesColumn = 2;
    private readonly SkdContext _context;

    public PcvXlsxParserService(SkdContext context) {
        _context = context;
    }

    public async Task<MutationResult<ParsePcvsXlxsResult>> ParsePcvsXlsx(MemoryStream parsePcvMemoryStream) {
        await Task.Delay(100);

        var result = new MutationResult<ParsePcvsXlxsResult> {
            Payload = new ParsePcvsXlxsResult()
        };

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage excel = new(parsePcvMemoryStream);
        ExcelWorksheet worksheet = excel.Workbook.Worksheets[0];

        var componentCodes = GetComponentCodes(worksheet);

        var pcvs = new List<PcvDataFromXlsx>();
        for (int col = PcvStartColumn; col < PcvMaxColumn; col++) {
            if (worksheet.Cells[PcvPropertiesStartRow, col].Value is null) break;

            var pcv = CreatePCVMetaFromWorksheetColumn(worksheet, col, componentCodes);

            pcv.Exists = await _context.Pcvs.AnyAsync(t => t.Code == pcv.PCV);

            pcvs.Add(pcv);
        }

        result.Payload.PcvData = pcvs;
        result.Payload.ComponentCodes = componentCodes;
        return result;
    }

    private static PcvDataFromXlsx CreatePCVMetaFromWorksheetColumn(ExcelWorksheet worksheet, int col, List<string> componentCodes) {
        PcvDataFromXlsx pcv = new();
        for (int row = PcvPropertiesStartRow; row <= PcvPropertiesEndRow; row++) {
            ExcelRange cell = worksheet.Cells[row, col];
            if (cell is null) {
                continue;
            }

            string value = cell.Value?.ToString() ?? "";
            switch (row) {
                case PcvPropertiesStartRow:
                    pcv.PCV = value;
                    break;
                case 2:
                    pcv.Model = value;
                    break;
                case 3:
                    pcv.Submodel = value;
                    break;
                case 4:
                    pcv.ModelYear = value;
                    break;
                case 5:
                    pcv.Series = value;
                    break;
                case 6:
                    pcv.Engine = value;
                    break;
                case 7:
                    pcv.Transmission = value;
                    break;
                case 8:
                    pcv.Drive = value;
                    break;
                case 9:
                    pcv.Paint = value;
                    break;
                case PcvPropertiesEndRow:
                    pcv.Trim = value;
                    break;
                default:
                    break;
            }
        }
        for (int row = PcvPropertiesEndRow; row < PcvPropertiesEndRow + componentCodes.Count; row++) {
            ExcelRange cell = worksheet.Cells[row, col];
            if (cell is null) continue;

            if (int.TryParse(cell.Value?.ToString(), out var cellValue) && cellValue == 1) {
                pcv.ComponentCodes.Add(componentCodes[row - PcvPropertiesEndRow]);
            }
        }
        return pcv;
    }
    
    public static List<string> GetComponentCodes(ExcelWorksheet worksheet) {
        List<string> componentCodes = new();
        for (int row = ComponentCodesRow; row < PcvMaxColumn; row++) {
            ExcelRange cell = worksheet.Cells[row, ComponentCodesColumn];
            if (cell is null || cell.Value is null) break;

            componentCodes.Add(cell.Value?.ToString() ?? "");
        }
        return componentCodes;
    }
}
