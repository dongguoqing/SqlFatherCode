using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPOI;
using NPOI.XSSF;
using System.Reflection;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using System.IO;

namespace Daemon.Common
{
    public static class NpoiHelper
    {
        public static byte[] ExportExcel<T>(Dictionary<string, string> columnsHeader, List<T> dataSource, int defaultColumnWidth = 50, string title = null, string footer = null)
        {
            IWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Sheet1");
            sheet.DefaultColumnWidth = defaultColumnWidth;

            IRow row;
            ICell cell;

            #region excel标题头
            int rowIndex = 0;
            if (!string.IsNullOrEmpty(title))
            {
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.Alignment = HorizontalAlignment.Center;
                IFont font = workbook.CreateFont();
                font.FontHeightInPoints = 10;
                font.IsBold = true;
                cellStyle.SetFont(font);
                var region = new CellRangeAddress(1, 1, 0, columnsHeader.Keys.Count > 0 ? columnsHeader.Keys.Count - 1 : 0);
                sheet.AddMergedRegion(region);
                //合并单元格后样式
                ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(region, BorderStyle.Thin, NPOI.HSSF.Util.HSSFColor.Black.Index);

                row = sheet.CreateRow(rowIndex);
                cell = row.CreateCell(0);
                cell.SetCellValue(title);
                cell.CellStyle = cellStyle;
                rowIndex++;
            }
            #endregion

            #region 列头
            row = sheet.CreateRow(rowIndex);
            row.HeightInPoints = 20;
            int cellIndex = 0;
            foreach (var value in columnsHeader.Values)
            {
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle = workbook.CreateCellStyle();
                cellStyle.BorderBottom = BorderStyle.Thin;
                cellStyle.BorderLeft = BorderStyle.Thin;
                cellStyle.BorderRight = BorderStyle.Thin;
                cellStyle.BorderTop = BorderStyle.Thin;
                //背景色
                cellStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                cellStyle.FillPattern = FillPattern.SolidForeground;
                //水平垂直居中
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.Alignment = HorizontalAlignment.Center;
                IFont font = workbook.CreateFont();
                font.FontHeightInPoints = 10;
                font.IsBold = true;
                cellStyle.SetFont(font);

                cell = row.CreateCell(cellIndex);
                cell.CellStyle = cellStyle;
                cell.SetCellValue(value);
                cellIndex++;
            }
            rowIndex++;
            #endregion

            #region 主题内容

            //单元格样式 注：不要放循环里面，NPOI中调用workbook.CreateCellStyle()方法超过4000次会报错
            ICellStyle contentStyle = workbook.CreateCellStyle();
            contentStyle.BorderBottom = BorderStyle.Thin;
            contentStyle.BorderLeft = BorderStyle.Thin;
            contentStyle.BorderRight = BorderStyle.Thin;
            contentStyle.BorderTop = BorderStyle.Thin;
            contentStyle.VerticalAlignment = VerticalAlignment.Center;
            IFont contentFont = workbook.CreateFont();
            contentFont.FontHeightInPoints = 10;
            contentStyle.SetFont(contentFont);

            //日期格式样式
            ICellStyle dateStyle = workbook.CreateCellStyle();
            dateStyle.BorderBottom = BorderStyle.Thin;
            dateStyle.BorderLeft = BorderStyle.Thin;
            dateStyle.BorderRight = BorderStyle.Thin;
            dateStyle.BorderTop = BorderStyle.Thin;
            dateStyle.VerticalAlignment = VerticalAlignment.Center;
            dateStyle.SetFont(contentFont);
            IDataFormat format = workbook.CreateDataFormat();
            dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");
            foreach (T item in dataSource)
            {
                row = sheet.CreateRow(rowIndex);
                row.HeightInPoints = 20;
                rowIndex++;
                Type type = item.GetType();
                PropertyInfo[] properties = type.GetProperties();
                if (properties.Length > 0)
                {
                    cellIndex = 0;
                    foreach (var key in columnsHeader.Keys)
                    {
                        cell = row.CreateCell(cellIndex);
                        cell.CellStyle = contentStyle;

                        if (properties.Select(x => x.Name.ToLower()).Contains(key.ToLower()))
                        {
                            var property = properties.Where(x => x.Name.ToLower() == key.ToLower()).FirstOrDefault();
                            string drValue = property.GetValue(item) == null ? "" : property.GetValue(item).ToString();
                            //当类型类似DateTime?时
                            var fullType = property.PropertyType.Name == "Nullable`1" ? property.PropertyType.GetGenericArguments()[0].FullName : property.PropertyType.FullName;
                            switch (fullType)
                            {
                                case "System.String": //字符串类型
                                    cell.SetCellValue(drValue);
                                    break;
                                case "System.DateTime": //日期类型
                                    if (string.IsNullOrEmpty(drValue) || drValue == "0001/1/1 0:00:00")
                                    {
                                        cell.SetCellValue("");
                                    }
                                    else
                                    {
                                        DateTime dateV;
                                        DateTime.TryParse(drValue, out dateV);
                                        cell.SetCellValue(dateV);

                                        cell.CellStyle = dateStyle; //格式化显示
                                    }
                                    break;
                                case "System.Boolean": //布尔型
                                    bool boolV = false;
                                    bool.TryParse(drValue, out boolV);
                                    cell.SetCellValue(boolV);
                                    break;
                                case "System.Int16": //整型
                                case "System.Int32":
                                case "System.Int64":
                                case "System.Byte":
                                    int intV = 0;
                                    int.TryParse(drValue, out intV);
                                    cell.SetCellValue(intV);
                                    break;
                                case "System.Decimal": //浮点型
                                case "System.Double":
                                    double doubV = 0;
                                    double.TryParse(drValue, out doubV);
                                    cell.SetCellValue(doubV);
                                    break;
                                case "System.DBNull": //空值处理
                                    cell.SetCellValue("");
                                    break;
                                default:
                                    cell.SetCellValue("");
                                    break;
                            }
                        }
                        cellIndex++;
                    }
                }

            }
            #endregion

            #region 结尾行
            if (!string.IsNullOrEmpty(footer))
            {
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.VerticalAlignment = VerticalAlignment.Center;
                cellStyle.Alignment = HorizontalAlignment.Center;
                IFont font = workbook.CreateFont();
                font.FontHeightInPoints =10 ;
                font.IsBold = true;
                cellStyle.SetFont(font);
                var region = new CellRangeAddress(rowIndex, rowIndex, 0, columnsHeader.Keys.Count > 0 ? columnsHeader.Keys.Count -1  : 0);
                sheet.AddMergedRegion(region);
                //合并单元格后样式
                ((HSSFSheet)sheet).SetEnclosedBorderOfRegion(region, BorderStyle.Thin, NPOI.HSSF.Util.HSSFColor.Black.Index);

                row = sheet.CreateRow(rowIndex);
                row.HeightInPoints = 20 ;
                cell = row.CreateCell(0);
                cell.SetCellValue(footer);
                cell.CellStyle = cellStyle;
            }
            #endregion

            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms, false);
                ms.Flush();
                ms.Seek(ms.Length, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }
    }
}