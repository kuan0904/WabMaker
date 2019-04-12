using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools
{
    public class ExcelTool
    {
        public static byte[] Create<T>(List<T> exportDatas)
        {
            if (exportDatas == null)
            {
                return null;
            }

            using (ExcelPackage ep = new ExcelPackage())
            {
                //加入一個Sheet
                ExcelWorksheet sheet1 = ep.Workbook.Worksheets.Add("Sheet1");
                int x = 1;
                int y = 1;


                // DisplayName
                foreach (var property in typeof(T).GetProperties())
                {
                    var attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                                                    .Cast<DisplayNameAttribute>()
                                                    .FirstOrDefault();
                    if (attribute != null)
                    {
                        var name = attribute.DisplayName;
                        sheet1.Cells[y, x].Value = name;
                        sheet1.Cells[y, x].Style.Font.Bold = true;
                    }
                    x++;
                }
                y++;


                //起始放置列數
                foreach (var item in exportDatas)
                {
                    x = 1;
                    foreach (var field in item.GetType().GetProperties())
                    {
                        sheet1.Cells[y, x].Value = field.GetValue(item);
                        x++;
                    }

                    y++;
                }

                //自動欄位大小
                x = 1;
                foreach (var field in exportDatas.FirstOrDefault().GetType().GetProperties())
                {
                    sheet1.Column(x).AutoFit();
                    x++;
                }

                y = 1;
                foreach (var item in exportDatas)
                {
                    x = 1;
                    foreach (var field in item.GetType().GetProperties())
                    {
                        sheet1.Cells[y, x].Style.WrapText = true;
                        //if (y % 2 == 0)
                        //{
                        //    sheet1.Cells[y, x].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        //    sheet1.Cells[y, x].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(219, 229, 241));
                        //}
                        //x++;
                    }

                    y++;
                }
                sheet1.Cells[1, 1, y, x].Style.VerticalAlignment = ExcelVerticalAlignment.Top;


                //匯出資料
                byte[] file = ep.GetAsByteArray();

                return file;
            }

        }


        /// <summary>
        /// Datatables to CSV.
        /// </summary>
        /// <returns></returns>
        public static bool SaveDatatableToCsv(DataTable table, string filePath)
        {
            try
            {
                // 輸出檔案
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Default))
                    {

                        // Datatable 組成csv格式
                        foreach (DataColumn column in table.Columns)
                        {
                            sw.Write(column.ColumnName + ",");
                        }
                        sw.Write(sw.NewLine);

                        foreach (DataRow row in table.Rows)
                        {
                            foreach (DataColumn column in table.Columns)
                            {
                                sw.Write(row[column] + ",");
                            }
                            sw.Write(sw.NewLine);
                        }


                        sw.Close();
                        sw.Dispose();
                    }
                    fs.Close();
                    fs.Dispose();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
