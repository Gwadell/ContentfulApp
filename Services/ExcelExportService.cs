using ContentfulApp.Models.DTO.ExportDto;
using OfficeOpenXml;
using System.Drawing;

namespace ContentfulApp.Services
{
    public class ExcelExportService
    {
        public static void ExportToExcel(Dictionary<string, IEnumerable<object>> data, string path)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                foreach (var sheet in data.Keys)
                {
                    var collection = data[sheet];

                    if (!collection.Any())
                        continue;

                    var type = collection.First().GetType();

                    // Convert the collection to a list of the specific type
                    var typedCollection = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(type).Invoke(null, new object[] { collection });

                    var ws = package.Workbook.Worksheets.Add(sheet);
                    ws.Cells["A1"].LoadFromCollection((dynamic)typedCollection, true);


                    // Format header row
                    var headerRange = ws.Cells[1, 1, 1, ws.Dimension.End.Column];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                    // Autofit columns
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    // Format CreatedAt column as date
                    var createdAtColumn = ws.Cells[1, 1, 1, ws.Dimension.End.Column].FirstOrDefault(c => c.Text == "CreatedAt");
                    if (createdAtColumn != null)
                    {
                        var columnNumber = createdAtColumn.Start.Column;
                        var columnRange = ws.Cells[2, columnNumber, ws.Dimension.End.Row, columnNumber];
                        columnRange.Style.Numberformat.Format = "yyyy-mm-dd-hh-ss";

                    }

                }
                package.SaveAs(new FileInfo(path));
            }
        }

        public static Dictionary<string, Type> ContentTypeToExportDtoTypeMap = new Dictionary<string, Type>
        {
            { "productListingPage", typeof(FullExport) },
            { "brand", typeof(FullExport) },
            { "collection", typeof(FullExport) },
            { "designer", typeof(FullExport) },
            { "_default", typeof(RegularExport) }
        };
    }
}
