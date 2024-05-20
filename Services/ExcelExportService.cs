using ContentfulApp.Models.DTO.ExportDto;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace ContentfulApp.Services
{
    public class ExcelExportService : IExcelExportService
    {
        /// <summary>
        /// Export data to an Excel file.
        /// </summary>
        /// <param name="data">The data to be exported.</param>
        /// <param name="path">The path where the Excel file will be saved.</param>
        public void ExportToExcel(Dictionary<string, IEnumerable<object>> data, string path)
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

                    
                    for (int i = 1; i <= ws.Dimension.End.Column; i++)
                    {
                        if (ws.Column(i).Width > 40)
                        {
                            ws.Column(i).Width = 40;
                        }
                    }
                    

                    // Format CreatedAt column as date
                    var createdAtColumn = ws.Cells[1, 1, 1, ws.Dimension.End.Column].FirstOrDefault(c => c.Text == "CreatedAt");
                    if (createdAtColumn != null)
                    {
                        var columnNumber = createdAtColumn.Start.Column;
                        var columnRange = ws.Cells[2, columnNumber, ws.Dimension.End.Row, columnNumber];
                        columnRange.Style.Numberformat.Format = "yyyy-MM-dd";

                    }

                }
                package.SaveAs(new FileInfo(path));
            }
        }
        
        /// <summary>
        /// Generate an Excel file with the given data and environment.
        /// </summary>
        /// <param name="allentries">The data to be exported.</param>
        /// <param name="environment">The environment name.</param>
        /// <returns>The file path of the generated Excel file.</returns>
        public string GenerateExcelFile(Dictionary<string, IEnumerable<object>> allentries, string environment)
        {
            var environmentName = environment == "master" ? "" : environment;
            var currentDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

            var excelFileName = $"export-{environmentName}-{currentDateTime}.xlsx";
            var excelfilePath = Path.Combine(Path.GetTempPath(), excelFileName);

            ExportToExcel(allentries, excelfilePath);

            return excelfilePath;
        }
    }


    public interface IExcelExportService
    {
        void ExportToExcel(Dictionary<string, IEnumerable<object>> data, string path);
        string GenerateExcelFile(Dictionary<string, IEnumerable<object>> allentries, string environment);
    }

}


