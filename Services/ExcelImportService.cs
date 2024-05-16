
using OfficeOpenXml;
using System.Data;

namespace ContentfulApp.Services
{
    public class ExcelImportService : IExcelImportService
    {
        public Dictionary<string, DataTable> ImportFromExcel(IFormFile file)
        {
            var data = new Dictionary<string, DataTable>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        var table = new DataTable();
                        bool hasHeader = true; // adjust it accordingly
                        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        {
                            table.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                        }

                        var startRow = hasHeader ? 2 : 1;

                        for (int rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                        {
                            var row = table.NewRow();
                            int i = 0;
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                row[i++] = worksheet.Cells[rowNum, col].Text;
                            }

                            table.Rows.Add(row);
                        }

                        data.Add(worksheet.Name, table);
                    }
                }
            }

            return data;
        }

    }


    public interface IExcelImportService
    {
        Dictionary<string, DataTable> ImportFromExcel(IFormFile file);
    }
}
